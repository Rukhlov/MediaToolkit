#include "stdafx.h"
#include "Utils.cpp"

extern "C" {
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
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


	public ref class FFmpegPixelConverter {
	public:
		FFmpegPixelConverter() { }

		~FFmpegPixelConverter() {
			Close();
		}

		void Init(System::Drawing::Size srcSize, MediaToolkit::Core::PixFormat srcPixFormat,
				System::Drawing::Size destSize, MediaToolkit::Core::PixFormat destPixFormat, 
				MediaToolkit::Core::ScalingFilter filter) {

			logger->TraceEvent(TraceEventType::Verbose, 0, "FFmpegPixelConverter::Init(...) " 
				+ srcSize.ToString() + " " + srcPixFormat.ToString() + " >> " 
				+ destSize.ToString() + " " + destPixFormat.ToString() + " " + filter.ToString());

			try {

				initialized = false;
				srcWidth = srcSize.Width;
				srcHeight = srcSize.Height;

				srcFormat = Utils::GetAVPixelFormat(srcPixFormat);
				if (srcFormat == AV_PIX_FMT_NONE) {
					throw gcnew Exception("Unsupported pixel format " + srcPixFormat.ToString());
				}

				destFrame = av_frame_alloc();
				destFrame->width = destSize.Width;
				destFrame->height = destSize.Height;
				//frame->color_range = AVColorRange::AVCOL_RANGE_MPEG;
				//destFrame->colorspace = AVColorSpace::AVCOL_SPC_BT470BG;

				int frameFormat = Utils::GetAVPixelFormat(destPixFormat);
				if(frameFormat == AV_PIX_FMT_NONE){
					throw gcnew Exception("Unsupported pixel format " + destPixFormat.ToString());
				}

				destFrame->format = frameFormat;
				int res = av_frame_get_buffer(destFrame, 0);
				if (res < 0) {
					throw gcnew Exception("Could not allocate frame data " + res);
				}

				swsFilter = Utils::GetSwsFilter(filter);

				sws_ctx = sws_getCachedContext(sws_ctx,
					srcWidth, srcHeight, srcFormat, // input
					destFrame->width, destFrame->height, (AVPixelFormat)destFrame->format,  // output
					swsFilter,
					NULL, NULL, NULL);

				if (sws_ctx == NULL) {

					throw gcnew Exception("Could not allocate convert context");
				}

				initialized = true;

			}
			catch (Exception^ ex) {

				Close();
				throw;
			}
		}

		//void Convert(IntPtr srcData, int srcDataSize, [Out] array<IntPtr>^% destData, [Out] array<int>^% destLinesize) {

		//	Convert(srcData, srcDataSize, 16, destData, destLinesize);
		//}

		//void Convert(IntPtr srcData, int srcLinesize, int srcAlign, [Out] array<IntPtr>^% destData, [Out] array<int>^% destLinesize) {
		void Convert(IntPtr srcData, int srcLinesize, int srcAlign, [Out] array<IFrameBuffer^>^% destData) {

			if (!initialized) {

				throw gcnew InvalidOperationException("Not initialized");
			}

			AVFrame* srcFrame = NULL;
			try {

				srcFrame = av_frame_alloc();
				srcFrame->width = srcWidth;
				srcFrame->height = srcHeight;
				srcFrame->format = srcFormat;

				int srcSize = av_image_fill_arrays(srcFrame->data, srcFrame->linesize,
					reinterpret_cast<uint8_t*>(srcData.ToPointer()), srcFormat, srcWidth, srcHeight, srcAlign);

				if (srcSize < 0) {
					throw gcnew InvalidOperationException("Could not fill source frame " + srcSize);
				}
				//  конвертируем в новый формат
				int res = sws_scale(sws_ctx, srcFrame->data, srcFrame->linesize, 0, srcHeight, destFrame->data, destFrame->linesize);


				//const uint8_t* src_data[1] =
				//{
				//	reinterpret_cast<uint8_t*>(srcData.ToPointer())
				//};
				//const int scr_size[1] =
				//{
				//	srcLinesize
				//};
				////  конвертируем в новый формат
				//int res = sws_scale(sws_ctx, src_data, scr_size, 0, srcHeight, destFrame->data, destFrame->linesize);
				//

				destData = gcnew array<IFrameBuffer^>(4);

				for (int i = 0; i < 4; i++) {
					destData[i] = gcnew FFmpegFrameBuffer((IntPtr)destFrame->data[i], destFrame->linesize[i]);
				}			

			}
			catch (Exception^ ex) {

				logger->TraceEvent(TraceEventType::Error, 0, ex->Message);
				throw;
			}
			finally{

				if (srcFrame != NULL) {
					if (srcFrame) {
						pin_ptr<AVFrame*> pSrcFrame = &srcFrame;
						av_frame_free(pSrcFrame);
						srcFrame = NULL;
					}
				}
			}

		}


		void Close() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "Close()");

			initialized = false;

			if (destFrame) {
				pin_ptr<AVFrame*> p_frame = &destFrame;
				av_frame_free(p_frame);
				destFrame = NULL;
			}

			if (sws_ctx != NULL) {
				sws_freeContext(sws_ctx);
				sws_ctx = NULL;
			}
		}

	private:


		bool initialized;
		int srcWidth;
		int srcHeight;
		AVPixelFormat srcFormat;

		int swsFilter = SWS_FAST_BILINEAR;
		struct SwsContext* sws_ctx;
		AVFrame* destFrame;
		AVFrame* srcFrame;

		static TraceSource^ logger = TraceManager::GetTrace("MediaToolkit.FFmpeg");

	};

}