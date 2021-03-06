#include "stdafx.h"


extern "C" {

#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>

}
#include "Utils.cpp"

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
//using namespace NLog;

namespace FFmpegLib {


	public ref class FFmpegVideoEncoder {
	public:
		FFmpegVideoEncoder() { }

		~FFmpegVideoEncoder() {
			CleanUp();
		}

		event Action<IntPtr, int, double>^ DataEncoded;

		void Open(VideoEncoderSettings^ encodingParams) {

			logger->TraceEvent(TraceEventType::Verbose, 0, "FFmpegVideoEncoder::Open(...) " +
				encodingParams->Resolution.Width + "x" + encodingParams->Resolution.Height + " " + encodingParams->EncoderId);

			try {

				cleanedup = false;

				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_qsv");

				/*

				//AVCodec* encoder = avcodec_find_encoder_by_name("libx264");
				AVCodec* encoder = avcodec_find_encoder_by_name("h264_nvenc"); // �������� ������� ����� ...

				if (!encoder) {
					// ���� ���� �������� Nvidia ����� �����
					encoder = avcodec_find_encoder(AV_CODEC_ID_H264);
				}
				*/


				//AVCodec* encoder = avcodec_find_encoder_by_name("libx264");
				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_nvenc");

				//AV_CODEC_ID_H264 AV_CODEC_ID_MJPEG AV_CODEC_ID_HEVC //AV_CODEC_ID_VP9 //(AV_CODEC_ID_MPEG4);//
				//AVCodec* encoder = avcodec_find_encoder(AV_CODEC_ID_H264 /*AV_CODEC_ID_MPEG4*/); 
				//AVCodec* encoder = avcodec_find_encoder_by_name("h264_qsv");
				//AVCodec* encoder = avcodec_find_encoder(AV_CODEC_ID_MJPEG);



				AVCodec* encoder = NULL;
				IntPtr pEncoderName = IntPtr::Zero;
				try {

					pEncoderName = Marshal::StringToHGlobalAnsi(encodingParams->EncoderId);

					encoder = avcodec_find_encoder_by_name((char*)pEncoderName.ToPointer());
				}
				finally{
					Marshal::FreeHGlobal(pEncoderName);
				}


				encoder_ctx = avcodec_alloc_context3(encoder);

				MediaRatio^ frameRate = encodingParams->FrameRate;

				//int den = (encodingParams->FrameRate > 0 && encodingParams->FrameRate <= 60) ? encodingParams->FrameRate : 30;
				encoder_ctx->time_base = { frameRate->Den, frameRate->Num };

				encoder_ctx->width = encodingParams->Resolution.Width;
				encoder_ctx->height = encodingParams->Resolution.Height;

				encoder_ctx->gop_size = 60;
				encoder_ctx->max_b_frames = 0;


				//encoder_ctx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;

				//encoder_ctx->rtp_payload_size = 1470;

				if (encoder->pix_fmts) {

					encoder_ctx->pix_fmt = encoder->pix_fmts[0];
				}
				else {

					encoder_ctx->pix_fmt = AV_PIX_FMT_YUV420P;
				}

				//encoder_ctx->pix_fmt = AV_PIX_FMT_ABGR;//AV_PIX_FMT_NV12;

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
				else if (encoder_ctx->codec_id == AV_CODEC_ID_MJPEG) {
					//...

					//encoder_ctx->pix_fmt = AV_PIX_FMT_YUV420P;

					encoder_ctx->flags |= AV_CODEC_FLAG_QSCALE;
					encoder_ctx->global_quality = 10;// (2-31)//FF_QP2LAMBDA * qscale;

					//TODO: set color range
					// https://stackoverflow.com/questions/23067722/swscaler-warning-deprecated-pixel-format-used
					// https://stackoverflow.com/questions/33932581/how-to-convert-yuv420p-image-to-jpeg-using-ffmpegs-libraries
					//encoder_ctx->color_range = AVColorRange::AVCOL_RANGE_JPEG;
				}

				encoder_ctx->profile = profile;

				AVDictionary *param = 0;
				if (encoder->id == AV_CODEC_ID_H264) {


					if (strcmp(encoder->name, "h264_nvenc") == 0) {

						av_dict_set(&param, "preset", "llhq", 0); //lowlatency HQ 
						//av_dict_set(&param, "rc", "ll_2pass_size", 1478); 
					}
					else
					{
						//av_dict_set(&param, "preset", "medium", 0);
						av_dict_set(&param, "preset", "ultrafast", 0);

					}
					//av_dict_set(&param, "preset", "llhq", 0);
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

				
				AVPixelFormat pixFormat = encoder_ctx->pix_fmt;
				switch (encoder_ctx->pix_fmt) {
				case AV_PIX_FMT_YUVJ420P:
					pixFormat = AV_PIX_FMT_YUV420P;
					break;
				case AV_PIX_FMT_YUVJ422P:
					pixFormat = AV_PIX_FMT_YUV422P;
					break;
				case AV_PIX_FMT_YUVJ444P:
					pixFormat = AV_PIX_FMT_YUV444P;
					break;
				case AV_PIX_FMT_YUVJ440P:
					pixFormat = AV_PIX_FMT_YUV440P;
					break;
				default:
					break;
				}

				frame->format = pixFormat;



				if (av_frame_get_buffer(frame, 0) < 0) {
					throw gcnew Exception("Could not allocate frame data.");
				}


				last_sec = -1;

			}
			catch (Exception^ ex) {

				CleanUp();
				throw;
			}
		}


		void _Encode(IVideoFrame^ videoFrame) {

			if (cleanedup) {

				return;
			}

			double sec = videoFrame->Time;

			//if (sec <= last_sec) {

			//	logger->TraceEvent(TraceEventType::Warning, 0, "Non monotone time: " + sec + " <= " + last_sec);

			//}

			bool lockTaken = false;
			try {

				lockTaken = videoFrame->Lock(Int32::MaxValue);

				int width = videoFrame->Width;
				int height = videoFrame->Height;
				PixFormat pixFormat = videoFrame->Format;
				AVPixelFormat pix_fmt = Utils::GetAVPixelFormat(pixFormat);

				if(pix_fmt == AV_PIX_FMT_NONE) {
					throw gcnew Exception("Unsupported pixel format " + pixFormat.ToString());
				}

				if (sws_ctx == NULL) {

					sws_ctx = sws_getCachedContext(sws_ctx,
						width, height, pix_fmt, // input
						frame->width, frame->height, (AVPixelFormat)frame->format,  // output
						//SWS_POINT,
						//SWS_FAST_BILINEAR,
						//SWS_BILINEAR,
						//SWS_AREA,
						SWS_BICUBIC,
						//SWS_LANCZOS,
						//SWS_SPLINE,

						NULL, NULL, NULL);

					if (sws_ctx == NULL) {

						throw gcnew Exception("Could not allocate convert context");
					}

					if (encoder_ctx->codec_id == AV_CODEC_ID_MJPEG) {
						// ��� jpeg-� ����� color full range
						int table[4];
						int inv_table[4];
						int srcRange, dstRange;
						int brightness, contrast, saturation;
						int res = sws_getColorspaceDetails(sws_ctx, (int**)&inv_table, &srcRange, (int**)&table, &dstRange, &brightness, &contrast, &saturation);
						if (res < 0) {
							throw gcnew Exception("getColorspaceDetails not supported");
						}

						//const int* coefs = sws_getCoefficients(SWS_CS_DEFAULT);
						////srcRange = 1;
						//dstRange = 1;
						//res = sws_setColorspaceDetails(sws_ctx, coefs, srcRange, coefs, dstRange, brightness, contrast, saturation);
						//if (res < 0) {
						//	throw gcnew Exception("Invalid color space");
						//}
					}
				}

				array<IFrameBuffer^>^ frameBuffer = videoFrame->Buffer;
				//IFrameBuffer^ frameBuffer0 = frameBuffer[0];

				const uint8_t* src_data[3] =
				{
					reinterpret_cast<uint8_t*>(frameBuffer[0]->Data.ToPointer()),
					reinterpret_cast<uint8_t*>(frameBuffer[1]->Data.ToPointer()),
				   reinterpret_cast<uint8_t*>(frameBuffer[2]->Data.ToPointer()),
				};

				//int bmpStride = 4 * ((width * 3 + 3) / 4);
				const int scr_size[3] =
				{
					frameBuffer[0]->Stride,
					frameBuffer[1]->Stride,
					frameBuffer[2]->Stride,
					//bmpStride
				};

				//  ������������ � ����� ������
				int res = sws_scale(sws_ctx, src_data, scr_size, 0, height, frame->data, frame->linesize);
			}
			finally{

				if (lockTaken) {
					videoFrame->Unlock();
				}
			}


			try {

				__int64 pts = sec * AV_TIME_BASE; // ��������� ������� � ������� ffmpeg-�

				AVRational av_time_base_q = { 1, AV_TIME_BASE };

				AVRational codec_time = encoder_ctx->time_base; //


				//__int64 framePts = frame->pts;

				//Console::WriteLine("framePts " + framePts + "");

				//frame->pts = av_rescale_q(pts, av_time_base_q, codec_time); // ������������� � ������ ������
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


		void Encode(VideoBuffer^ videoBuffer) {

			if (cleanedup) {

				return;
			}

			double sec = videoBuffer->time;

			if (sec <= last_sec) {

				logger->TraceEvent(TraceEventType::Warning, 0, "Non monotone time: " + sec + " <= " + last_sec);

			}

			Bitmap^ bmp = videoBuffer->bitmap;

			try {

				Object^ syncRoot = videoBuffer->syncRoot;
				bool lockTaken = false;

				try {

					Monitor::Enter(syncRoot, lockTaken);
					if (lockTaken) {

						if (bmp == nullptr) {
							return;
						}

						int width = bmp->Width;
						int height = bmp->Height;
						PixelFormat bmpFmt = bmp->PixelFormat;
						System::Drawing::Rectangle bmpRect = System::Drawing::Rectangle(0, 0, width, height);

						AVPixelFormat pix_fmt = AV_PIX_FMT_NONE;

						if (bmpFmt == PixelFormat::Format24bppRgb) {

							pix_fmt = AV_PIX_FMT_BGR24;

						}
						else if (bmpFmt == PixelFormat::Format32bppArgb || bmpFmt == PixelFormat::Format32bppRgb) {

							pix_fmt = AV_PIX_FMT_BGRA;

						}
						else if (bmpFmt == PixelFormat::Format16bppRgb565) {

							pix_fmt = AV_PIX_FMT_RGB565LE;
							
						}
						else {

							throw gcnew Exception("Unsupported pixel format " + bmpFmt.ToString());
						}

						BitmapData^ bmpData = bmp->LockBits(bmpRect, ImageLockMode::ReadOnly, bmpFmt);
						try {

							if (sws_ctx == NULL) {

								sws_ctx = sws_getCachedContext(sws_ctx,
									width, height, pix_fmt, // input
									frame->width, frame->height, (AVPixelFormat)frame->format,  // output
									//SWS_POINT,
									//SWS_FAST_BILINEAR,
									//SWS_BILINEAR,
									//SWS_AREA,
									SWS_BICUBIC,
									//SWS_LANCZOS,
									//SWS_SPLINE,

									NULL, NULL, NULL);

								if (sws_ctx == NULL) {

									throw gcnew Exception("Could not allocate convert context");
								}

								if (encoder_ctx->codec_id == AV_CODEC_ID_MJPEG) {
									// ��� jpeg-� ����� color full range
									int table[4];
									int inv_table[4];
									int srcRange, dstRange;
									int brightness, contrast, saturation;
									int res = sws_getColorspaceDetails(sws_ctx, (int**)&inv_table, &srcRange, (int**)&table, &dstRange, &brightness, &contrast, &saturation);									
									if (res < 0) {
										throw gcnew Exception("getColorspaceDetails not supported");
									}

									const int* coefs = sws_getCoefficients(SWS_CS_DEFAULT);
									//srcRange = 1;
									dstRange = 1;
									res = sws_setColorspaceDetails(sws_ctx, coefs, srcRange, coefs, dstRange, brightness, contrast, saturation);
									if (res < 0) {
										throw gcnew Exception("Invalid color space");
									}
								}
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

							//  ������������ � ����� ������
							sws_scale(sws_ctx, src_data, scr_size, 0, height, frame->data, frame->linesize);
						}
						finally{

							bmp->UnlockBits(bmpData);

						}
					}

				}
				finally{

					if (lockTaken) {
						Monitor::Exit(syncRoot);
					}
				}

				__int64 pts = sec * AV_TIME_BASE; // ��������� ������� � ������� ffmpeg-�

				AVRational av_time_base_q = { 1, AV_TIME_BASE };

				AVRational codec_time = encoder_ctx->time_base; //


				//__int64 framePts = frame->pts;

				//Console::WriteLine("framePts " + framePts + "");

				//frame->pts = av_rescale_q(pts, av_time_base_q, codec_time); // ������������� � ������ ������
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

			logger->TraceEvent(TraceEventType::Verbose, 0, "Close()");
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
				packet.data = NULL; // ��� ����� �� �������� � ���������� �������� (AV_CODEC_ID_MPEG4...)
				packet.size = 0;
				av_init_packet(&packet);

				//int got_output;
				int res = avcodec_send_frame(encoder_ctx, frame);
				//int res = avcodec_encode_video2(encoder_st->codec, &packet, frame, &got_output);

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

					av_packet_rescale_ts(&packet, codec_time, av_time_base_q); // ��������� ����� � ������ ����������
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

				//������������ ������ �� ������ ������
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
		//static Logger^ logger = LogManager::GetCurrentClassLogger();

	};

}