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


		int Convert(IVideoFrame^ srcFrame, IVideoFrame^ destFrame) {

			int res = 0;
			if (!initialized) {

				throw gcnew InvalidOperationException("Not initialized");
			}

			try {

				array<IFrameBuffer^>^ srcBuffer = srcFrame->Buffer;
				const uint8_t* src_data[4] =
				{
					reinterpret_cast<uint8_t*>(srcBuffer[0]->Data.ToPointer()),
					reinterpret_cast<uint8_t*>(srcBuffer[1]->Data.ToPointer()),
					reinterpret_cast<uint8_t*>(srcBuffer[2]->Data.ToPointer()),
					reinterpret_cast<uint8_t*>(srcBuffer[3]->Data.ToPointer())
				};

				const int scr_linesize[4] =
				{
					srcBuffer[0]->Stride,
					srcBuffer[1]->Stride,
					srcBuffer[2]->Stride,
					srcBuffer[3]->Stride,
				};

				array<IFrameBuffer^>^ destBuffer = destFrame->Buffer;
				uint8_t* dest_data[4] =
				{
					reinterpret_cast<uint8_t*>(destBuffer[0]->Data.ToPointer()),
					reinterpret_cast<uint8_t*>(destBuffer[1]->Data.ToPointer()),
					reinterpret_cast<uint8_t*>(destBuffer[2]->Data.ToPointer()),
					reinterpret_cast<uint8_t*>(destBuffer[3]->Data.ToPointer())
				};

				int dest_linesize[4] =
				{
					destBuffer[0]->Stride,
					destBuffer[1]->Stride,
					destBuffer[2]->Stride,
					destBuffer[3]->Stride,
				};

				res = sws_scale(sws_ctx, src_data, scr_linesize, 0, srcFrame->Height, dest_data, dest_linesize);

			}
			catch (Exception^ ex) {

				logger->TraceEvent(TraceEventType::Error, 0, ex->Message);
				throw;
			}

			return res;
		}

		int Convert(array<IFrameBuffer^>^% srcBuffer, array<IFrameBuffer^>^% destBuffer) {

			int res = 0;
			if (!initialized) {

				throw gcnew InvalidOperationException("Not initialized");
			}

			try {

				srcFrame = av_frame_alloc();
				srcFrame->width = srcWidth;
				srcFrame->height = srcHeight;
				srcFrame->format = srcFormat;

				int srcSize = av_image_fill_arrays(srcFrame->data, srcFrame->linesize,
					reinterpret_cast<uint8_t*>(srcBuffer[0]->Data.ToPointer()), srcFormat, srcWidth, srcHeight, 16);

				if (srcSize < 0) {
					throw gcnew InvalidOperationException("Could not fill source frame " + srcSize);
				}

			
				//const uint8_t* src_data[1] = {
				//	reinterpret_cast<uint8_t*>(srcBuffer[0]->Data.ToPointer())
				//};

				//int scr_linesize[1] = {
				//	srcBuffer[0]->Stride
				//};
				
	/*			uint8_t* src_data[1] = {};
				int scr_linesize[1] = {};
				for (int i = 0; i < 1; i++) {
					src_data[i] = reinterpret_cast<uint8_t*>(srcBuffer[i]->Data.ToPointer());
					scr_linesize[i] = srcBuffer[i]->Stride;
				}
*/
				//const uint8_t* src_data[4] =
				//{
				//	reinterpret_cast<uint8_t*>(srcBuffer[0]->Data.ToPointer()),
				//	reinterpret_cast<uint8_t*>(srcBuffer[1]->Data.ToPointer()),
				//	reinterpret_cast<uint8_t*>(srcBuffer[2]->Data.ToPointer()),
				//	reinterpret_cast<uint8_t*>(srcBuffer[3]->Data.ToPointer())
				//};

				//const int scr_linesize[4] =
				//{
				//	srcBuffer[0]->Stride,
				//	srcBuffer[1]->Stride,
				//	srcBuffer[2]->Stride,
				//	srcBuffer[3]->Stride,
				//};

				//uint8_t* dest_data[4] =
				//{
				//	reinterpret_cast<uint8_t*>(destBuffer[0]->Data.ToPointer()),
				//	reinterpret_cast<uint8_t*>(destBuffer[1]->Data.ToPointer()),
				//	reinterpret_cast<uint8_t*>(destBuffer[2]->Data.ToPointer()),
				//	reinterpret_cast<uint8_t*>(destBuffer[3]->Data.ToPointer())
				//};

				//int dest_linesize[4] =
				//{
				//	destBuffer[0]->Stride,
				//	destBuffer[1]->Stride,
				//	destBuffer[2]->Stride,
				//	destBuffer[3]->Stride,
				//};
				
				uint8_t* dest_data[4] = {};
				int dest_linesize[4] = {};
				for (int i = 0; i < destBuffer->Length; i++) {
					dest_data[i] = reinterpret_cast<uint8_t*>(destBuffer[i]->Data.ToPointer());
					dest_linesize[i] = destBuffer[i]->Stride;
				}

				res = sws_scale(sws_ctx, srcFrame->data, srcFrame->linesize, 0, srcHeight, dest_data, dest_linesize);

			}
			catch (Exception^ ex) {

				logger->TraceEvent(TraceEventType::Error, 0, ex->Message);
				throw;
			}

			return res;
		}

		void _Convert(IVideoFrame^ srcFrame, IVideoFrame^ destFrame) {

			if (!initialized) {

				throw gcnew InvalidOperationException("Not initialized");
			}

			AVFrame* src_frame = NULL;
			AVFrame* dest_frame = NULL;

			try {

				array<IFrameBuffer^>^ srcBuffer = srcFrame->Buffer;
				IntPtr srcPtr = srcBuffer[0]->Data;
				int srcWidth = srcFrame->Width;
				int srcHeight = srcFrame->Height;
				int srcAlign = srcFrame->Align;
				AVPixelFormat srcFormat = Utils::GetAVPixelFormat(srcFrame->Format);

				src_frame = av_frame_alloc();
				src_frame->width = srcWidth;
				src_frame->height = srcHeight;
				src_frame->format = srcFormat;
				
				int srcSize = av_image_fill_arrays(src_frame->data, src_frame->linesize,
					reinterpret_cast<uint8_t*>(srcPtr.ToPointer()), srcFormat, srcWidth, srcHeight, srcAlign);

				if (srcSize < 0) {
					throw gcnew InvalidOperationException("Could not fill source frame " + srcSize);
				}

				array<IFrameBuffer^>^ destBuffer = destFrame->Buffer;				
				IntPtr destPtr = destBuffer[0]->Data;
				int destWidth = destFrame->Width;
				int destHeight = destFrame->Height;
				int destAlign = destFrame->Align;
				AVPixelFormat destFormat = Utils::GetAVPixelFormat(destFrame->Format);

				dest_frame = av_frame_alloc();
				dest_frame->width = destWidth;
				dest_frame->height = destHeight;				
				dest_frame->format = destFormat;
				
				int destSize = av_image_fill_arrays(dest_frame->data, dest_frame->linesize,
					reinterpret_cast<uint8_t*>(destPtr.ToPointer()), destFormat, destWidth, destHeight, destAlign);

				if (destSize < 0) {
					throw gcnew InvalidOperationException("Could not fill source frame " + destSize);
				}

				//  конвертируем в новый формат
				int res = sws_scale(sws_ctx, src_frame->data, src_frame->linesize, 0, srcHeight, dest_frame->data, dest_frame->linesize);
			}
			catch (Exception^ ex) {

				logger->TraceEvent(TraceEventType::Error, 0, ex->Message);
				throw;
			}
			finally{

				if (dest_frame != NULL) {
					if (dest_frame) {
						pin_ptr<AVFrame*> pDestFrame = &dest_frame;
						av_frame_free(pDestFrame);
						dest_frame = NULL;
					}
				}

				if (src_frame != NULL) {
					if (src_frame) {
						pin_ptr<AVFrame*> pSrcFrame = &src_frame;
						av_frame_free(pSrcFrame);
						src_frame = NULL;
					}
				}
			}

		}

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
					destData[i] = gcnew FrameBuffer((IntPtr)destFrame->data[i], destFrame->linesize[i]);
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