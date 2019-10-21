#include "stdafx.h"


extern "C" {

#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>

#include "libavutil/audio_fifo.h"

#include <libavutil/opt.h>
#include <libavutil/channel_layout.h>
#include <libavutil/samplefmt.h>
#include <libswresample/swresample.h>
}

using namespace System;
using namespace System::IO;

using namespace Collections::Generic;

using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace MediaToolkit::Common;
using namespace System::Threading;
using namespace NLog;

namespace FFmpegLib {

	public ref class AudioDecoder {
	public:
		AudioDecoder()
		{
			closing = false;
			CleanedUp = false;

			//isInitilized = false; 
		}
		~AudioDecoder() {

			CleanUp();
		}

		void Open(AudioEncodingParams^ srcParams) {
			//av_log(NULL, AV_LOG_DEBUG, "AudioDecoder::Open()");
			logger->Debug("AudioDecoder::Open()");
			int res;
			try {
				//av_register_all();

				//AVCodecID CodecID = AV_CODEC_ID_ADPCM_G726;

				AVCodecID CodecID = AVCodecID::AV_CODEC_ID_PCM_MULAW;
				codec = avcodec_find_decoder(CodecID);

				//IntPtr pDecoderName = IntPtr::Zero;
				//try {

				//	pDecoderName = Marshal::StringToHGlobalAnsi(srcParams->Encoding);

				//	codec = avcodec_find_decoder_by_name((char*)pDecoderName.ToPointer());
				//}
				//finally{
				//	Marshal::FreeHGlobal(pDecoderName);
				//}

				if (codec == NULL) {
					throw gcnew Exception("Unsupported audio codec format!");
				}

				codec_context = avcodec_alloc_context3(codec);

				//codec_context->codec_type = AVMEDIA_TYPE_AUDIO;
				//codec_context->sample_fmt = *codec->sample_fmts;   
				//codec_context->bit_rate = DecoderParams->Bitrate;

				codec_context->sample_rate = srcParams->SampleRate;
				codec_context->channels = srcParams->Channels;

				//if (CodecID == AV_CODEC_ID_ADPCM_G726) {
				//	codec_context->bits_per_coded_sample = OutputContext->BitsPerCodedSample / 8;
				//	codec_context->bit_rate = codec_context->bits_per_coded_sample * codec_context->sample_rate;
				//}

				res = avcodec_open2(codec_context, codec, NULL);
				if (res < 0) throw gcnew Exception("Unable to open audio codec");

				frame = av_frame_alloc();

				if (frame == NULL) throw gcnew Exception("Could not allocate audio frame");

				//isInitilized = true;
			}
			catch (Exception^ ex) {

				CleanUp();
				throw;
			}

		}

		//[System::Runtime::CompilerServices::MethodImpl(System::Runtime::CompilerServices::MethodImplOptions::Synchronized)]
		bool Decode(array<Byte>^ srcData, [Out] array<Byte>^% destData) {

			bool Result = false;

			if (closing)
			{
				return false;
			}

			if (CleanedUp) {
				throw gcnew ObjectDisposedException("FFmpegLib::AudioDecoder");
			}

			int res;
			AVPacket packet;
			try {

				int srcSize = srcData->Length;
				av_new_packet(&packet, srcSize);
				//packet.data = reinterpret_cast<uint8_t*>(Packet->data.ToPointer());
				packet.size = srcSize;

				Marshal::Copy(srcData, 0, (IntPtr)packet.data, srcSize);

				//// устанавливаем врямя пакета в отсчетах ffmpeg-а
				//double packet_time_sec = Packet->timestamp / 1000.0;
				//packet.dts = packet.pts = packet_time_sec * AV_TIME_BASE;

				int got_frame;
				res = avcodec_decode_audio4(codec_context, frame, &got_frame, &packet);

				if (res < 0) {
					av_log(codec_context, AV_LOG_ERROR, "Error while decoding audio!");
					//throw gcnew Exception(L"Error while decoding audio");
				}

				if (got_frame > 0) {
					//double pts = frame->pkt_dts;

					//if (pts == AV_NOPTS_VALUE)
					//	pts = frame->pkt_pts;

					//if (pts == AV_NOPTS_VALUE)
					//	pts = 0;

					//pts *= 1 / (double)AV_TIME_BASE;
					//__int64 m_pts = pts * 1000;

					IntPtr frameDataPtr = (IntPtr)frame->data[0];
					int frameDataSize = frame->linesize[0];
					
					destData = gcnew array<Byte>(frameDataSize);

					Marshal::Copy(frameDataPtr,  destData, 0, frameDataSize);

					Result = true;
				}
			}
			finally{
				av_free_packet(&packet);
			}


			return Result;
		}

		void Close() {
			//av_log(NULL, AV_LOG_DEBUG, "FFmpegAudioDecoder::Close()");

			logger->Debug("AudioDecoder::Close()");
			if (codec_context) {
				avcodec_flush_buffers(codec_context);
			}

			CleanUp();

			//av_free(frame);
			//avcodec_close(codec_context);
		}

	private:

		bool isInitilized;

		AVFrame* frame;
		AVCodec* codec;
		AVCodecContext* codec_context;


		volatile bool closing;
		volatile bool CleanedUp;

		void CleanUp() {

			logger->Debug("AudioDecoder::CleanUp()");

			closing = true;

			if (codec_context != NULL) {
				avcodec_close(codec_context);
				codec_context = NULL;
			}

			if (frame != NULL) {
				pin_ptr<AVFrame*> p_Frame = &frame;
				av_frame_free(p_Frame);
				frame = NULL;
			}

			CleanedUp = true;
		}

		static Logger^ logger = LogManager::GetCurrentClassLogger();
	};


}