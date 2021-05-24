#include "stdafx.h"

extern "C" {
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
#include <libavutil/imgutils.h>

}


#using <system.dll>
using namespace System;
using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace MediaToolkit::Core;
using namespace System::Collections::Generic;
namespace FFmpegLib {
	public ref class Utils {
	public:

		static array<VideoEncoderDescription^>^GetH264Encoders() {

			List<VideoEncoderDescription^>^ descriptions = gcnew List<VideoEncoderDescription^>(16);

			const AVCodec *current_codec = nullptr;
			void *i = 0;
			while ((current_codec = av_codec_iterate(&i))) {
				if (av_codec_is_encoder(current_codec)) {

					if (current_codec->type == AVMediaType::AVMEDIA_TYPE_VIDEO && 
						current_codec->id == AVCodecID::AV_CODEC_ID_H264) {
						
						AVCodecContext* context = avcodec_alloc_context3(current_codec);
						context->width = 640;
						context->height = 480;
						context->time_base = { 1, 1 };
						context->pix_fmt = AV_PIX_FMT_NV12;

						int res = -1;
						try {
							res = avcodec_open2(context, NULL, NULL);
						}
						finally{
							if (context) {
								pin_ptr<AVCodecContext*> p_ectx = &context;
								avcodec_free_context(p_ectx);
								context = NULL;
							}
						}

						bool hardware = current_codec->capabilities & AV_CODEC_CAP_HARDWARE;
						String^ wrapperName = gcnew String(current_codec->wrapper_name);
						String^ name = gcnew String(current_codec->name);

						VideoEncoderDescription^ descr = gcnew VideoEncoderDescription();
						descr->Id = name;
						descr->Name = name;
						descr->Description = gcnew String(current_codec->long_name);
						descr->Format = VideoCodingFormat::H264;
						descr->IsHardware = false;
						descr->Activatable = (res >= 0);
						descriptions->Add(descr);

					}
				}
			}

			return descriptions->ToArray();
		}

		static int AllocImageData(System::Drawing::Size size, MediaToolkit::Core::PixFormat pixFormat, int align,
			[Out] array<IntPtr>^% destData, [Out] array<int>^% destLinesize) {

			AVPixelFormat format = GetAVPixelFormat(pixFormat);
			int width = size.Width;
			int height = size.Height;

			uint8_t* data[4];
			int linesize[4];

			int destSize = av_image_alloc(data, linesize, width, height, format, align);

			destData = gcnew array<IntPtr>(4);
			destLinesize = gcnew array<int>(4);

			for (int i = 0; i < 4; i++) {
				destData[i] = (IntPtr)data[i];
				destLinesize[i] = linesize[i];
			}

			return destSize;
		}

		static int AllocImageData(System::Drawing::Size size, MediaToolkit::Core::PixFormat pixFormat, int align,
			[Out] array<IFrameBuffer^>^% destData) {

			AVPixelFormat format = GetAVPixelFormat(pixFormat);
			int width = size.Width;
			int height = size.Height;

			uint8_t* data[4];
			int linesize[4];

			int destSize = av_image_alloc(data, linesize, width, height, format, align);

			destData = gcnew array<FrameBuffer^>(4);
			for (int i = 0; i < 4; i++) {
				destData[i] = gcnew FrameBuffer((IntPtr)data[i], linesize[i]);
			}

			return destSize;
		}

		static void FreeImageData(array<IFrameBuffer^>^% data) {

			IntPtr ptr = data[0]->Data;
			if (ptr != IntPtr::Zero) {
				void* _ptr = ptr.ToPointer();
				pin_ptr<void*> p_ptr = &_ptr;
				av_freep(p_ptr);
				//av_free(ptr.ToPointer());
			}
		}

		static int FillImageData(IntPtr srcData, System::Drawing::Size size, MediaToolkit::Core::PixFormat pixFormat, int align,
			[Out] array<IntPtr>^% destData, [Out] array<int>^% destLinesize) {

			AVPixelFormat format = GetAVPixelFormat(pixFormat);
			const uint8_t* src = reinterpret_cast<uint8_t*>(srcData.ToPointer());
			int width = size.Width;
			int height = size.Height;

			uint8_t* data[4];
			int linesize[4];

			int destSize = av_image_fill_arrays(data, linesize, src, format, width, height, align);

			destData = gcnew array<IntPtr>(4);
			destLinesize = gcnew array<int>(4);

			for (int i = 0; i < 4; i++) {
				destData[i] = (IntPtr)data[i];
				destLinesize[i] = linesize[i];
			}

			return destSize;
		}

		static String^ GetErrorStr(int errnum)
		{
			char errbuf[AV_ERROR_MAX_STRING_SIZE] = { 0 };
			av_strerror(errnum, errbuf, AV_ERROR_MAX_STRING_SIZE);
			return gcnew String(errbuf);
		}

		static void ThrowIfError(int errnum, String^ callerName) {

			if (errnum < 0) {
				String^ description = GetErrorStr(errnum);
				String^ message = callerName + " returned result: " + errnum + ", " + description;

				throw gcnew Exception(message);
			}
		}

	internal:
		static AVPixelFormat GetAVPixelFormat(MediaToolkit::Core::PixFormat pixFormat)
		{
			AVPixelFormat pix_fmt = AV_PIX_FMT_NONE;
			switch (pixFormat) {

			case PixFormat::NV12:
				pix_fmt = AV_PIX_FMT_NV12;
				break;
			case PixFormat::I420:
				pix_fmt = AV_PIX_FMT_YUV420P;
				break;
			case PixFormat::RGB32:
				pix_fmt = AV_PIX_FMT_BGRA;//AV_PIX_FMT_BGRA;
				break;
			case PixFormat::RGB24:
				pix_fmt = AV_PIX_FMT_BGR24;
				break;
			case PixFormat::RGB16:
				pix_fmt = AV_PIX_FMT_RGB565LE;
				break;
			case PixFormat::I444:
				pix_fmt = AV_PIX_FMT_YUV444P;
				break;
			case PixFormat::I422:
				pix_fmt = AV_PIX_FMT_YUV422P;
				break;
			default:
				break;
			}

			return pix_fmt;
		}

		static MediaToolkit::Core::PixFormat GetPixelFormat(AVPixelFormat pix_fmt)
		{
			MediaToolkit::Core::PixFormat pixFormat = MediaToolkit::Core::PixFormat::Unknown;
			switch (pix_fmt) {

			case AV_PIX_FMT_YUV420P:
				pixFormat = PixFormat::I420;
				break;
			case AV_PIX_FMT_YUV422P:
				pixFormat = PixFormat::I422;
				break;
			case AV_PIX_FMT_YUV444P:
				pixFormat = PixFormat::I444;
				break;
			case AV_PIX_FMT_NV12:
				pixFormat = PixFormat::NV12;
				break;
			case AV_PIX_FMT_BGRA:
				pixFormat = PixFormat::RGB32;
				break;
			case AV_PIX_FMT_BGR24:
				pixFormat = PixFormat::RGB24;
				break;
			case AV_PIX_FMT_RGB565LE:
				pixFormat = PixFormat::RGB16;
				break;			
			default:
				break;
			}

			return pixFormat;
		}

		static AVPixelFormat GetAVPixelFormat(System::Drawing::Imaging::PixelFormat pixFormat)
		{
			AVPixelFormat pix_fmt = AV_PIX_FMT_NONE;
			switch (pixFormat) {
			case System::Drawing::Imaging::PixelFormat::Format32bppRgb:
			case System::Drawing::Imaging::PixelFormat::Format32bppArgb:
				pix_fmt = AV_PIX_FMT_BGRA;
				break;
			case System::Drawing::Imaging::PixelFormat::Format24bppRgb:
				pix_fmt = AV_PIX_FMT_BGR24;
				break;
			case System::Drawing::Imaging::PixelFormat::Format16bppRgb565:
				pix_fmt = AV_PIX_FMT_RGB565LE;
				break;
			default:
				break;
			}

			return pix_fmt;
		}

		static int GetSwsFilter(MediaToolkit::Core::ScalingFilter scalingFilter)
		{
			int sws_filter = SWS_FAST_BILINEAR;
			switch (scalingFilter) {
			case ScalingFilter::Point:
				sws_filter = SWS_POINT;
				break;
			case ScalingFilter::FastLinear:
				sws_filter = SWS_FAST_BILINEAR;
				break;
			case ScalingFilter::Linear:
				sws_filter = SWS_BILINEAR;
				break;
			case ScalingFilter::Bicubic:
				sws_filter = SWS_BICUBIC;
				break;
			case ScalingFilter::Lanczos:
				sws_filter = SWS_LANCZOS;
				break;
			case ScalingFilter::Spline:
				sws_filter = SWS_SPLINE;
				break;
			default:
				break;
			}

			return sws_filter;
		}


	};

	public ref class FFVideoFrame : public VideoFrameBase {
	public:
		FFVideoFrame(array<IFrameBuffer^>^ buffer, double time, int width, int height, PixFormat format) {
			Width = width;
			Height = height;
			Format = format;
			Buffer = buffer;
			Time = time;
		}
		virtual property VideoDriverType DriverType{
			VideoDriverType get() override { return VideoDriverType::CPU; }
		}
	};
}