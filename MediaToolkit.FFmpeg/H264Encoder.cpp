#include "stdafx.h"


extern "C" {

#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
#include <libavutil/mathematics.h>
#include <libavutil/opt.h>
#include <libswscale/swscale.h>
#include <libswresample/swresample.h>


#include <libavutil/hwcontext_dxva2.h>

}

//#include <vcclr.h>

#using <system.dll>
using namespace System;
using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace MediaToolkit::Core;
using namespace MediaToolkit::Logging;
using namespace System::Threading;

namespace FFmpegLib {


	public ref class H264Encoder {
	public:
		H264Encoder() { }

		~H264Encoder() {
			CleanUp();
		}

		event Action<IntPtr, int, double>^ DataEncoded;

		void Setup(VideoEncoderSettings^ encodingParams) {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Encoder::Setup(...) " +
				encodingParams->Resolution.Width + "x" + encodingParams->Resolution.Height + " " + encodingParams->EncoderName);

			try {

				cleanedup = false;

				AVCodec* encoder = avcodec_find_encoder_by_name("libx264");
				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_nvenc");

				//AVCodec* encoder = avcodec_find_encoder(AV_CODEC_ID_H264 /*AV_CODEC_ID_MPEG4*/); 
				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_qsv");

				encoder_ctx = avcodec_alloc_context3(encoder);

				int den = (encodingParams->FrameRate > 0 && encodingParams->FrameRate <= 60) ? encodingParams->FrameRate : 30;
				encoder_ctx->time_base = { 1, den };

				encoder_ctx->width = encodingParams->Resolution.Width;
				encoder_ctx->height = encodingParams->Resolution.Height;

				encoder_ctx->gop_size = 60;
				encoder_ctx->max_b_frames = 0;

				//encoder_ctx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;

				//encoder_ctx->rtp_payload_size = 1470;

				//if (encoder->pix_fmts) {

				//	encoder_ctx->pix_fmt = encoder->pix_fmts[0];
				//}
				//else {

				//	encoder_ctx->pix_fmt = AV_PIX_FMT_YUV420P;
				//}

				encoder_ctx->pix_fmt = AV_PIX_FMT_NV12;//AV_PIX_FMT_YUV420P;

				int profile = 0;
				if (encoder_ctx->codec_id == AV_CODEC_ID_H264) {
					profile = FF_PROFILE_H264_HIGH; //FF_PROFILE_H264_BASELINE; FF_PROFILE_H264_MAIN

					//encoder_ctx->max_b_frames = 0;
				}

				encoder_ctx->profile = profile;

				AVDictionary *param = 0;
				if (encoder->id == AV_CODEC_ID_H264) {


					if (strcmp(encoder->name, "h264_nvenc") == 0) {

						av_dict_set(&param, "preset", "llhq", 0); //lowlatency HQ 
						//av_dict_set(&param, "rc", "ll_2pass_size", 1478); 
					}
					else if (strcmp(encoder->name, "libx264") == 0)
					{
						//av_dict_set(&param, "preset", "medium", 0);
						av_dict_set(&param, "preset", "ultrafast", 0);

					}

					logger->TraceEvent(TraceEventType::Verbose, 0, "zerolatency");
					av_dict_set(&param, "tune", "zerolatency", 0);

				}


				if (avcodec_open2(encoder_ctx, encoder, &param) < 0) {
					throw gcnew Exception("Unable to open video codec");
				}

				//int extaSize = encoder_ctx->extradata_size;

				//array<Byte>^ extraData = gcnew array<Byte>(extaSize);
				//Marshal::Copy((IntPtr)encoder_ctx->extradata, extraData, 0, extaSize);
				//System::IO::File::WriteAllBytes("d:\\extadata.txt", extraData);

				//byte[] extraData = gcnew byte[encoder_ctx->]
				//encoder_ctx->extradata



				frame = av_frame_alloc();
				frame->width = encoder_ctx->width;
				frame->height = encoder_ctx->height;
				frame->format = encoder_ctx->pix_fmt;
				//frame->quality = 1;

				frame->format = encoder_ctx->pix_fmt;

				if (av_frame_get_buffer(frame, 0) < 0) {
					//if (av_frame_get_buffer(frame, 32) < 0) {
					throw gcnew Exception("Could not allocate frame data.");
				}


				last_sec = -1;

			}
			catch (Exception^ ex) {

				CleanUp();
				throw;
			}
		}

		void Encode(IntPtr srcPtr, int srcSize, double sec) {

			if (cleanedup) {

				return;
			}

			//if (sec <= last_sec) {

			//	logger->TraceEvent(TraceEventType::Warning, 0, "Non monotone time: " + sec + " <= " + last_sec);

			//}


			try {

				avpicture_fill((AVPicture*)frame, reinterpret_cast<uint8_t*>(srcPtr.ToPointer()), AV_PIX_FMT_NV12, frame->width, frame->height);


				__int64 pts = sec * AV_TIME_BASE; // переводим секунды в отсчеты ffmpeg-а

				AVRational av_time_base_q = { 1, AV_TIME_BASE };

				AVRational codec_time = encoder_ctx->time_base; //


				//__int64 framePts = frame->pts;

				//Console::WriteLine("framePts " + framePts + "");

				//frame->pts = av_rescale_q(pts, av_time_base_q, codec_time); // пересчитываем в формат кодека
				frame->pts++;

				//if (framePts == frame->pts) {
				//	Console::WriteLine("framePts " + framePts + " frame->pts " + frame->pts);
				//}
				EncodeFrame(frame);

				last_sec = sec;
				//framePts = frame->pts;

				//Console::WriteLine("last_sec " + last_sec);

			}
			catch (Exception^ ex) {
				CleanUp();
				throw;
			}

		}


		void Close() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Encoder::Close()");
			try {

				FlushEncoder();
			}
			finally{

				CleanUp();
			}
		}

	private:

		bool EncodeFrame(AVFrame* frame) {

			if (cleanedup) {

				return false;
			}

			bool write_frame = false;
			AVPacket packet;
			try {
				packet.data = NULL;
				packet.size = 0;
				av_init_packet(&packet);

				int res = avcodec_send_frame(encoder_ctx, frame);

				//if (res < 0) {
				//	//...
				//	throw gcnew Exception("Error while encode video " + res);
				//}


				while (res >= 0) {
					res = avcodec_receive_packet(encoder_ctx, &packet);
					if (res) {
						write_frame = false;
						break;
					}

					AVRational codec_time = encoder_ctx->time_base;
					AVRational av_time_base_q = { 1, AV_TIME_BASE };

					av_packet_rescale_ts(&packet, codec_time, av_time_base_q); // переводим время в формат контейнера
					double sec = packet.pts / (double)AV_TIME_BASE;


					OnDataEncoded((IntPtr)packet.data, packet.size, sec);
				}
			}
			finally{

				//av_free_packet(&packet);

			}

			return write_frame;
		}

		void OnDataEncoded(IntPtr ptr, int size, double time) {
			DataEncoded(ptr, size, time);
		}
		void FlushEncoder() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "FlushEncoder()");

			if (cleanedup) {

				return;
			}

			if (!(encoder_ctx->codec->capabilities & AV_CODEC_CAP_DELAY)) {
				return;
			}

			while (true) {

				//обрабатываем пакеты из буфера кодера
				bool result = EncodeFrame(NULL);

				if (!result) {

					break;
				}

			}
		}

		void CleanUp() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "CleanUp()");

			cleanedup = true;

			if (encoder_ctx) {

				pin_ptr<AVCodecContext*> p_ectx = &encoder_ctx;
				avcodec_free_context(p_ectx);
				encoder_ctx = NULL;
			}

			if (frame) {
				pin_ptr<AVFrame*> p_frame = &frame;
				av_frame_free(p_frame);
				frame = NULL;
			}

			if (sws_ctx != NULL) {
				sws_freeContext(sws_ctx);
				sws_ctx = NULL;
			}
		}

		bool cleanedup;

		double last_sec;
		__int64 framePts;

		AVCodecContext* encoder_ctx;
		struct SwsContext* sws_ctx;
		AVFrame* frame;


		static TraceSource^ logger = TraceManager::GetTrace("MediaToolkit.FFmpeg");

	};

}