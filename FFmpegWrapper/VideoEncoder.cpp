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

using namespace System;
using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace CommonData;
using namespace System::Threading;

namespace FFmpegWrapper {


	public ref class FFmpegVideoEncoder {
	public:
		FFmpegVideoEncoder() { }

		~FFmpegVideoEncoder() {
			CleanUp();
		}

		event Action<IntPtr, int>^ DataEncoded;

		void Open(VideoEncodingParams^ encodingParams) {

			//logger->Debug("FFmpegVideoEncoder::Open(...) " + Width + " " + Height + " " + FrameRate);

			try {

				cleanedup = false;
				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_qsv");

				//AVCodec* encoder = avcodec_find_encoder_by_name("libx264");
				AVCodec* encoder = avcodec_find_encoder_by_name("h264_nvenc"); // Работает быстрее всего ...

				if (!encoder) {
					// Если нету энкодера Nvidia берем любой
					encoder = avcodec_find_encoder(AV_CODEC_ID_H264);
				}

				//AVCodec* encoder = avcodec_find_encoder_by_name("libx264");
				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_nvenc");

				//AV_CODEC_ID_H264 AV_CODEC_ID_MJPEG AV_CODEC_ID_HEVC //AV_CODEC_ID_VP9 //(AV_CODEC_ID_MPEG4);//
				//AVCodec* encoder = avcodec_find_encoder(AV_CODEC_ID_H264 /*AV_CODEC_ID_MPEG4*/); 
				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_qsv");

				encoder_ctx = avcodec_alloc_context3(encoder); 

				int den = (encodingParams->FrameRate > 0 && encodingParams->FrameRate <= 60) ? encodingParams->FrameRate : 30;
				encoder_ctx->time_base = { 1, den };

				encoder_ctx->width = encodingParams->Width;
				encoder_ctx->height = encodingParams->Height;
				encoder_ctx->gop_size = 30;
				encoder_ctx->max_b_frames = 0;
				

				//encoder_ctx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;

				//encoder_ctx->rtp_payload_size = 1470;

				if (encoder->pix_fmts) {

					encoder_ctx->pix_fmt = encoder->pix_fmts[0];
				}
				else {

					encoder_ctx->pix_fmt = AV_PIX_FMT_YUV420P;
				}

				int profile = 0;
				if (encoder_ctx->codec_id == AV_CODEC_ID_H264) {
					profile = FF_PROFILE_H264_HIGH;//FF_PROFILE_H264_BASELINE; FF_PROFILE_H264_MAIN

					//encoder_ctx->max_b_frames = 0;
				}
				else if (encoder_ctx->codec_id == AV_CODEC_ID_VP9) {
					profile = FF_PROFILE_VP9_0;
				}
				else if (encoder_ctx->codec_id == AV_CODEC_ID_MPEG4) {
					profile = FF_PROFILE_MPEG4_ADVANCED_SIMPLE;
				}
				encoder_ctx->profile = profile;

				AVDictionary *param = 0;
				if (encoder->id == AV_CODEC_ID_H264) {


					if (encoder->name == "h264_nvenc") {

						av_dict_set(&param, "preset", "llhq", 0); //lowlatency HQ 
						//av_dict_set(&param, "rc", "ll_2pass_size", 1478); 
					}
					else
					{
						//av_dict_set(&param, "preset", "medium", 0);
						//av_dict_set(&param, "preset", "ultrafast", 0);

					}
					av_dict_set(&param, "preset", "llhq", 0);
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

		void Encode(VideoBuffer^ videoBuffer, double sec) {

			if (cleanedup) {

				return;
			}

			if (sec <= last_sec) {

				//logger->Warn("Non monotone time: " + sec + " <= " + last_sec);

			}

			Bitmap^ bmp = videoBuffer->bitmap;

			try {

				Object^ syncRoot = videoBuffer->syncRoot;
				bool lockTaken = false;

				
				try {

					Monitor::Enter(syncRoot, lockTaken);
					int width = bmp->Width;
					int height = bmp->Height;
					PixelFormat bmpFmt = bmp->PixelFormat;
					System::Drawing::Rectangle bmpRect = System::Drawing::Rectangle(0, 0, width, height);

					AVPixelFormat pix_fmt = AV_PIX_FMT_NONE;

					if (bmpFmt == PixelFormat::Format24bppRgb) {

						pix_fmt = AV_PIX_FMT_BGR24;

					}
					else if (bmpFmt == PixelFormat::Format32bppArgb || bmpFmt == PixelFormat::Format32bppRgb) {

						pix_fmt = AV_PIX_FMT_BGRA;//AV_PIX_FMT_RGBA;

					}
					else if (bmpFmt == PixelFormat::Format16bppRgb565 ) {

						pix_fmt = AV_PIX_FMT_BGR565LE;//AV_PIX_FMT_RGBA;

					}
					else {

						throw gcnew Exception("Unsupported pixel format " + bmpFmt.ToString());
					}

					BitmapData^ bmpData = bmp->LockBits(bmpRect, ImageLockMode::ReadOnly, bmpFmt);
					try {

						sws_ctx = sws_getCachedContext(sws_ctx,
							width, height, pix_fmt, // input
							frame->width, frame->height, (AVPixelFormat)frame->format,  // output
							SWS_BICUBIC,
							NULL, NULL, NULL);

						if (sws_ctx == NULL) {

							throw gcnew Exception("Could not allocate convert context");
						}

						const uint8_t* src_data[1] =
						{
							reinterpret_cast<uint8_t*>(bmpData->Scan0.ToPointer())
						};
						//int bmpStride = 4 * ((width * 3 + 3) / 4);
						const int scr_size[1] =
						{
							bmpData->Stride
							//bmpStride
						};

						//  конвертируем в новый формат
						sws_scale(sws_ctx, src_data, scr_size, 0, height, frame->data, frame->linesize);
					}
					finally{

						if (lockTaken) {
							bmp->UnlockBits(bmpData);
						}
					}

				}
				finally{

					Monitor::Exit(syncRoot);
				}

				__int64 pts = sec * AV_TIME_BASE; // переводим секунды в отсчеты ffmpeg-а

				AVRational av_time_base_q = { 1, AV_TIME_BASE };

				AVRational codec_time = encoder_ctx->time_base; //
				frame->pts = av_rescale_q(pts, av_time_base_q, codec_time); // пересчитываем в формат кодека

				EncodeFrame(frame);

				last_sec = sec;

				//Console::WriteLine("last_sec " + last_sec);

			}
			catch (Exception^ ex) {
				CleanUp();
				throw;
			}

		}


		void Close() {

			//logger->Trace("Close()");
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
				packet.data = NULL; // без этого не работает с некоторыми кодеками (AV_CODEC_ID_MPEG4...)
				packet.size = 0;
				av_init_packet(&packet);

				int got_output;
				int res = avcodec_send_frame(encoder_ctx, frame);
				//int res = avcodec_encode_video2(encoder_st->codec, &packet, frame, &got_output);

				if (res < 0) {
					//...
					throw gcnew Exception("Error while encode video " + res);
				}


				while (res >= 0) {
					res = avcodec_receive_packet(encoder_ctx, &packet);
					if (res) {
						write_frame = false;
						break;
					}

					//AVRational codec_time = encoder_st->codec->time_base;
					//AVRational stream_time = encoder_st->time_base;
					//av_packet_rescale_ts(&packet, codec_time, stream_time); // переводим время в формат контейнера

					OnDataEncoded((IntPtr)packet.data, packet.size);
				}
			}
			finally{

				//av_free_packet(&packet);

			}

			return write_frame;
		}

		void OnDataEncoded(IntPtr ptr, int size) {
			DataEncoded(ptr, size);
		}
		void FlushEncoder() {

			//logger->Trace("FlushEncoder()");

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

			//logger->Trace("CleanUp()");

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

		AVCodecContext* encoder_ctx;
		struct SwsContext* sws_ctx;
		AVFrame* frame;



		//static NLog::Logger^ logger = NLog::LogManager::GetCurrentClassLogger();

	};

}