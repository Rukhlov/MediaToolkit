#include "stdafx.h"
#include "Utils.cpp"

extern "C" {

#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>

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
					throw gcnew LibAvException("Could not find video codec");
				}

				decoder_ctx = avcodec_alloc_context3(decoder);
				decoder_ctx->width = encodingSettings->Resolution.Width;
				decoder_ctx->height = encodingSettings->Resolution.Height;
				decoder_ctx->flags |= AV_CODEC_FLAG_LOW_DELAY;

				//MediaRatio^ frameRate = encodingSettings->FrameRate;
				//decoder_ctx->framerate = { frameRate->Den, frameRate->Num };
				//decoder_ctx->pix_fmt = AVPixelFormat::AV_PIX_FMT_NV12;

				int res = avcodec_open2(decoder_ctx, decoder, NULL);
				LibAvException::ThrowIfError(res, "avcodec_open2");

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
				LibAvException::ThrowIfError(res, "avcodec_send_packet");

				while (res >= 0) {

					if (frame == NULL) {

						frame = av_frame_alloc();
						//frame->width = decoder_ctx->width;
						//frame->height = decoder_ctx->height;
						//frame->format = decoder_ctx->pix_fmt;

						//res = av_frame_get_buffer(frame, 0);
						//LibAvException::ThrowIfError(res, "av_frame_get_buffer");
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

						LibAvException::ThrowIfError(res, "avcodec_receive_frame");
					}

					double destTime = 0;
					__int64 frame_pts = frame->pts;
					if (frame_pts != AV_NOPTS_VALUE) {
						destTime = (frame->pts / (double)AV_TIME_BASE);
					}

					AVFrame* destFrame = frame;
					bool needConvert = false;
					if (needConvert) {

						AVPixelFormat destAVPixelFormat = AVPixelFormat::AV_PIX_FMT_BGRA;
						int destWidth = decoder_ctx->width;
						int destHeight = decoder_ctx->height;

						if (sws_ctx == NULL) {

							sws_ctx = sws_getCachedContext(sws_ctx,
								decoder_ctx->width, decoder_ctx->height, decoder_ctx->pix_fmt, // input
								destWidth, destHeight, destAVPixelFormat,  // output
								//SWS_POINT,
								//SWS_FAST_BILINEAR,
								//SWS_BILINEAR,
								//SWS_AREA,
								SWS_BICUBIC,
								//SWS_LANCZOS,
								//SWS_SPLINE,

								NULL, NULL, NULL);

							if (sws_ctx == NULL) {

								throw gcnew LibAvException("Could not allocate convert context");
							}

							if (scaledFrame) {
								pin_ptr<AVFrame*> p_frame = &scaledFrame;
								av_frame_free(p_frame);
								scaledFrame = NULL;
							}

							scaledFrame = av_frame_alloc();
							scaledFrame->width = destWidth;
							scaledFrame->height = destHeight;
							scaledFrame->format = destAVPixelFormat;

							res = av_frame_get_buffer(scaledFrame, 0);
							LibAvException::ThrowIfError(res, "av_frame_get_buffer");

						}

						sws_scale(sws_ctx, frame->data, frame->linesize, 0, destHeight, scaledFrame->data, scaledFrame->linesize);
						destFrame = scaledFrame;

					}

					array<IFrameBuffer^>^ destBuffer = gcnew array<IFrameBuffer^>(3);
					for (int i = 0; i < destBuffer->Length; i++) {
						destBuffer[i] = gcnew FrameBuffer((IntPtr)destFrame->data[i], destFrame->linesize[i]);
					}

					PixFormat destPixFormat = Utils::GetPixelFormat((AVPixelFormat)destFrame->format);
					FFVideoFrame^ videoFrame = gcnew FFVideoFrame(destBuffer, destTime, destFrame->width, destFrame->height, destPixFormat);
					decodeResult = 0;
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

			if (scaledFrame) {
				pin_ptr<AVFrame*> p_frame = &scaledFrame;
				av_frame_free(p_frame);
				scaledFrame = NULL;
			}

			initialized = false;
		}

		bool initialized;

		__int64 framePts;

		AVCodecContext* decoder_ctx;
		AVFrame* frame;
		AVFrame* scaledFrame;
		struct SwsContext* sws_ctx;

		static TraceSource^ logger = TraceManager::GetTrace("MediaToolkit.FFmpeg");

	};

}