#include "stdafx.h"


extern "C" {

#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
#include <libavutil/mathematics.h>
#include <libavutil/opt.h>
#include <libswscale/swscale.h>
#include <libswresample/swresample.h>


	//#include <libavutil/hwcontext_dxva2.h>

#include <libavutil/imgutils.h>
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


	public ref class H264Decoder {
	public:
		H264Decoder() { }

		~H264Decoder() {
			CleanUp();
		}

		event Action<array<IFrameBuffer^>^, double>^ DataDecoded;

		void Setup(VideoEncoderSettings^ encodingSettings) {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Decoder::Setup(...) " +
				encodingSettings->Resolution.Width + "x" + encodingSettings->Resolution.Height + " " + encodingSettings->EncoderId);

			try {

				closed = false;

				AVCodec* decoder = avcodec_find_decoder(AVCodecID::AV_CODEC_ID_H264);;

				if (decoder == NULL) {
					throw gcnew Exception("Could not find video codec");
				}


				decoder_ctx = avcodec_alloc_context3(decoder);

				MediaRatio^ frameRate = encodingSettings->FrameRate;

				//int den = (encodingSettings->FrameRate > 0 && encodingSettings->FrameRate <= 60) ? encodingSettings->FrameRate : 30;
				//decoder_ctx->time_base = { frameRate->Den, frameRate->Num }; // 1/fps

				decoder_ctx->width = encodingSettings->Resolution.Width;
				decoder_ctx->height = encodingSettings->Resolution.Height;
				//decoder_ctx->pix_fmt = AVPixelFormat::AV_PIX_FMT_NV12;

				int res = avcodec_open2(decoder_ctx, decoder, NULL);
				if (res < 0) {
					throw gcnew Exception("Unable to open video codec");
				}

				frame = av_frame_alloc();
				frame->width = decoder_ctx->width;
				frame->height = decoder_ctx->height;
				frame->format = AVPixelFormat::AV_PIX_FMT_YUV420P;

				res = av_frame_get_buffer(frame, 0);
				if (res < 0) {
					//if (av_frame_get_buffer(frame, 32) < 0) {
					throw gcnew Exception("Could not allocate frame data. " + res);
				}
			}
			catch (Exception^ ex) {

				CleanUp();
				throw;
			}
		}

		bool Decode(array<Byte>^ srcData, double sec) {

			bool Result = false;

			if (closed)
			{
				return false;
			}

			int res;
			AVPacket* packet = NULL;
			try {

				if (srcData != nullptr) {
					packet = av_packet_alloc();

					int srcSize = srcData->Length;
					res = av_new_packet(packet, srcSize);
					//packet->data = reinterpret_cast<uint8_t*>(Packet->data.ToPointer());
					packet->size = srcSize;

					Marshal::Copy(srcData, 0, (IntPtr)packet->data, srcSize);

					// устанавливаем врямя пакета в отсчетах ffmpeg-а
					//double packet_time_sec = sec / 1000.0;
					packet->dts = packet->pts = sec * AV_TIME_BASE;
				}


				res = avcodec_send_packet(decoder_ctx, packet);
	
				if (res < 0) {
					throw gcnew Exception(L"Error while decoding video");
				}

				while (res >= 0) {
					res = avcodec_receive_frame(decoder_ctx, frame);

					//if (res == AVERROR(EAGAIN) || res == AVERROR_EOF) {
					//	break;
					//}
					//else if (res < 0) {
					//	//throw gcnew Exception(L"Error while decoding video");
					//}

					if (res) {

						break;
					}
					
					array<IFrameBuffer^>^destData = gcnew array<IFrameBuffer^>(4);
					for (int i = 0; i < 4; i++) {
						destData[i] = gcnew FrameBuffer((IntPtr)frame->data[i], frame->linesize[i]);
					}
				    
					DataDecoded(destData, (frame->pts / (double)AV_TIME_BASE));


				}

				//if (got_frame > 0) {
				//	//double pts = frame->pkt_dts;

				//	//if (pts == AV_NOPTS_VALUE)
				//	//	pts = frame->pkt_pts;

				//	//if (pts == AV_NOPTS_VALUE)
				//	//	pts = 0;

				//	//pts *= 1 / (double)AV_TIME_BASE;
				//	//__int64 m_pts = pts * 1000;

				//	IntPtr frameDataPtr = (IntPtr)frame->data[0];
				//	int frameDataSize = frame->linesize[0];

				//	destData = gcnew array<Byte>(frameDataSize);

				//	Marshal::Copy(frameDataPtr, destData, 0, frameDataSize);

				//	Result = true;
				//}
			}
			finally{
				if (packet) {
					av_free_packet(packet);
				}
				
			}


			return Result;
		}


		void Close() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Decoder::Close()");
			try {

				Drain();
			}
			finally{

				CleanUp();
			}
		}

	private:
		void Drain() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Decoder::Drain()");

			if (closed) {

				return;
			}

			if (!(decoder_ctx->codec->capabilities & AV_CODEC_CAP_DELAY)) {
				return;
			}

			while (true) {

				//обрабатываем пакеты из буфера кодера
				bool result = Decode(nullptr, 0);

				if (!result) {

					break;
				}

			}
		}

		void CleanUp() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "CleanUp()");

			closed = true;

			if (decoder_ctx) {

				pin_ptr<AVCodecContext*> p_ectx = &decoder_ctx;
				avcodec_free_context(p_ectx);
				decoder_ctx = NULL;
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

		bool closed;

		double last_sec;
		__int64 framePts;

		AVCodecContext* decoder_ctx;
		struct SwsContext* sws_ctx;
		AVFrame* frame;


		static TraceSource^ logger = TraceManager::GetTrace("MediaToolkit.FFmpeg");

	};

}