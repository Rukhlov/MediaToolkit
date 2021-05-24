#include "stdafx.h"
#include "Utils.cpp"

extern "C" {

#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
#include <libavutil/mathematics.h>
#include <libavutil/opt.h>
#include <libswscale/swscale.h>
#include <libswresample/swresample.h>

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

		void Setup(VideoEncoderSettings^ encodingSettings) {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Decoder::Setup(...) " +
				encodingSettings->Resolution.Width + "x" + encodingSettings->Resolution.Height);

			try {

				AVCodec* decoder = avcodec_find_decoder(AVCodecID::AV_CODEC_ID_H264);;

				if (decoder == NULL) {
					throw gcnew Exception("Could not find video codec");
				}

				decoder_ctx = avcodec_alloc_context3(decoder);
				decoder_ctx->width = encodingSettings->Resolution.Width;
				decoder_ctx->height = encodingSettings->Resolution.Height;
				decoder_ctx->flags |= AV_CODEC_FLAG_LOW_DELAY;

				//MediaRatio^ frameRate = encodingSettings->FrameRate;
				//decoder_ctx->framerate = { frameRate->Den, frameRate->Num };
				//decoder_ctx->pix_fmt = AVPixelFormat::AV_PIX_FMT_NV12;

				int res = avcodec_open2(decoder_ctx, decoder, NULL);
				Utils::ThrowIfError(res, "avcodec_open2");

				initialized = true;

			}
			catch (Exception^ ex) {

				CleanUp();
				throw;
			}
		}

		int Decode(array<Byte>^ srcData, double sec, Action<IVideoFrame^>^ OnDataDecoded) {

			int decodeResult = 0;

			if (!initialized)
			{
				throw gcnew InvalidOperationException("Not initialized");
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

					packet->dts = packet->pts = sec * AV_TIME_BASE;
				}

				res = avcodec_send_packet(decoder_ctx, packet);
				Utils::ThrowIfError(res, "avcodec_send_packet");

				while (res >= 0) {

					if (frame == NULL) {

						frame = av_frame_alloc();
						frame->width = decoder_ctx->width;
						frame->height = decoder_ctx->height;
						frame->format = decoder_ctx->pix_fmt;

						res = av_frame_get_buffer(frame, 0);
						Utils::ThrowIfError(res, "av_frame_get_buffer");
					}

					res = avcodec_receive_frame(decoder_ctx, frame);

					if (res == AVERROR(EAGAIN)) {
						decodeResult = 1; //need more data...
						break;
					}
					else if (res == AVERROR_EOF) {
						decodeResult = 2;
						break;
					}
					else if (res < 0) {

						Utils::ThrowIfError(res, "avcodec_receive_frame");
					}

					decodeResult = 0;
					array<IFrameBuffer^>^destBuffer = gcnew array<IFrameBuffer^>(4);
					for (int i = 0; i < 4; i++) {
						destBuffer[i] = gcnew FrameBuffer((IntPtr)frame->data[i], frame->linesize[i]);
					}
			
					double destTime = (frame->pts / (double)AV_TIME_BASE);
					int destWidth = frame->width;
					int destHeight = frame->height;
					PixFormat destFormat = Utils::GetPixelFormat((AVPixelFormat)frame->format);
					FFVideoFrame^ videoFrame = gcnew FFVideoFrame(destBuffer, destTime, destWidth, destHeight, destFormat);
					OnDataDecoded(videoFrame);
				}
			}
			finally{

				if (packet) {
					av_free_packet(packet);
				}
			}

			return decodeResult;
		}

		bool Drain() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Decoder::Drain()");
;
			if (!initialized) {

				return false;
			}

			if (!(decoder_ctx->codec->capabilities & AV_CODEC_CAP_DELAY)) {
				return false;
			}

			return true;
		}


		void Close() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "H264Decoder::Close()");
			CleanUp();
		}

	private:

		void CleanUp() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "CleanUp()");

			initialized = false;

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

		}

		bool initialized;

		__int64 framePts;

		AVCodecContext* decoder_ctx;
		AVFrame* frame;

		static TraceSource^ logger = TraceManager::GetTrace("MediaToolkit.FFmpeg");

	};

}