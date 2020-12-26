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

namespace FFmpegLib {
	public ref class Utils {
	public:

		static int AllocImageData(System::Drawing::Size size, MediaToolkit::Core::PixFormat pixFormat, int align,
			[Out] array<IntPtr>^% destData, [Out] array<int>^% destLinesize) {

			AVPixelFormat format = GetAVPixelFormat(pixFormat);
			int width = size.Width;
			int height = size.Height;

			uint8_t* data[4];
			int linesize[4];

			int destSize = av_image_fill_arrays(data, linesize, NULL, format, width, height, align);

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

			int destSize = av_image_fill_arrays(data, linesize, NULL, format, width, height, align);

			destData = gcnew array<FrameBuffer^>(4);
			for (int i = 0; i < 4; i++) {
				destData[i] = gcnew FrameBuffer((IntPtr)data[i], linesize[i]);
			}

			return destSize;
		}

		//static int FillImageData(IntPtr srcData, System::Drawing::Size size, MediaToolkit::Core::PixFormat pixFormat, int align,
		//	[Out] array<IntPtr>^% destData, [Out] array<int>^% destLinesize) {

		//	AVPixelFormat format = GetAVPixelFormat(pixFormat);
		//	const uint8_t* src = reinterpret_cast<uint8_t*>(srcData.ToPointer());
		//	int width = size.Width;
		//	int height = size.Height;

		//	uint8_t* data[4];
		//	int linesize[4];

		//	int destSize = av_image_fill_arrays(data, linesize, src, format, width, height, align);

		//	destData = gcnew array<IntPtr>(4);
		//	destLinesize = gcnew array<int>(4);

		//	for (int i = 0; i < 4; i++) {
		//		destData[i] = (IntPtr)data[i];
		//		destLinesize[i] = linesize[i];
		//	}

		//	return destSize;
		//}

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
			case PixFormat::RGB565:
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

	public ref class FFmpegVideoFrame : public IVideoFrame
	{
	public:

		FFmpegVideoFrame() {

		}
		~FFmpegVideoFrame() {

		}

		property array<IFrameBuffer^>^ Buffer{
			virtual array<IFrameBuffer^>^ get() {
				return buffer;
			}
		}

		property double Time {
			virtual double get() { return time; }
			void set(double value) { time = value; }
		}

		property double Duration { 
			virtual double get() { return time; }
			void set(double value) { duration = value; }
		}

		property int Width {
			virtual int get() { return width; }
			void set(int value) { width = value; }
		}

		property int Height {
			virtual int get() { return height; }
			void set(int value) { height = value; }
		}

		property PixFormat Format {
			virtual PixFormat get() { return format; }
			void set(PixFormat value) { format = value; }
		}

		property int Align {
			virtual int get() { return align; }
			void set(int value) { align = value; }
		}

		property MediaToolkit::Core::ColorSpace ColorSpace {
			virtual  MediaToolkit::Core::ColorSpace get() { return colorSpace; }
			void set(MediaToolkit::Core::ColorSpace value) { colorSpace = value; }
		}

		property MediaToolkit::Core::ColorRange ColorRange {
			virtual  MediaToolkit::Core::ColorRange get() { return colorRange; }
			void set(MediaToolkit::Core::ColorRange value) { colorRange = value; }
		}

		property int Size {
			virtual int get() { return size; }
			void set(int value) { size = value; }
		}

		property MediaToolkit::Core::VideoDriverType DriverType {
			virtual  MediaToolkit::Core::VideoDriverType get() { return driverType; }
			void set(MediaToolkit::Core::VideoDriverType value) { driverType = value; }
		}

	private:

		array<FrameBuffer^>^ buffer = gcnew array<FrameBuffer^>(4);

		double time = 0;
		double duration = 0;
		int width = 0;
		int height = 0;
		PixFormat format = PixFormat::Unknown;
		int align = 0;
		MediaToolkit::Core::ColorSpace colorSpace = MediaToolkit::Core::ColorSpace::BT601;
		MediaToolkit::Core::ColorRange colorRange = MediaToolkit::Core::ColorRange::Partial;
		MediaToolkit::Core::VideoDriverType driverType = MediaToolkit::Core::VideoDriverType::CPU;

		int size = 0;
	};

	public ref class FFmpegFrameBuffer : public IFrameBuffer
	{
	public:
		FFmpegFrameBuffer(IntPtr _data, int _stride) {
			data = _data;
			stride = _stride;
		}

		property IntPtr Data {
			virtual IntPtr get() { return data; }
			void set(IntPtr value) { data = value; }
		}

		property int Stride {
			virtual int get() { return stride; }
			void set(int value) { stride = value; }
		}

	private:
		IntPtr data;
		int stride = 0;
	};
}