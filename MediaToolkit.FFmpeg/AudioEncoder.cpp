
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

#using <system.dll>
using namespace System;
using namespace System::IO;

using namespace Collections::Generic;

using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace MediaToolkit::Core;
using namespace MediaToolkit::Logging;

using namespace System::Threading;
//using namespace NLog;

namespace FFmpegLib {

	public ref class AudioEncoder {
	public:
		AudioEncoder() { }

		~AudioEncoder() {
			CleanUp();
		}

		void Open(AudioEncoderSettings^ srcParams, AudioEncoderSettings^ dstParams) {

			logger->TraceEvent(TraceEventType::Verbose, 0, "AudioEncoder::Open(...)");
			closing = false;

			//AVCodec* encoder = avcodec_find_encoder_by_name("");
			AVCodec* encoder = avcodec_find_encoder(AV_CODEC_ID_PCM_MULAW);

			enc_ctx = avcodec_alloc_context3(encoder);

			enc_ctx->channels = dstParams->Channels;
			enc_ctx->sample_rate = dstParams->SampleRate; //input_codec_context->sample_rate; //192000; //96000; //44100; //
			enc_ctx->channel_layout = av_get_default_channel_layout(enc_ctx->channels);
			enc_ctx->sample_fmt = encoder->sample_fmts[0];


			if (avcodec_open2(enc_ctx, encoder, NULL) < 0) {
				throw gcnew Exception("Unable to open video codec");
			}


			swr_ctx = swr_alloc();
			if (!swr_ctx) {
				throw gcnew Exception("Could not allocate resampler context");
			}


			src_rate = srcParams->SampleRate;
			src_nb_channels = srcParams->Channels;
			//src_nb_channels = av_get_channel_layout_nb_channels(src_ch_layout);
			src_ch_layout = av_get_default_channel_layout(src_nb_channels);
			src_sample_fmt = AV_SAMPLE_FMT_FLT;


			//av_opt_set_int(swr_ctx, "in_channel_layout", src_ch_layout, 0);
			av_opt_set_int(swr_ctx, "in_channel_layout", src_ch_layout, 0);
			av_opt_set_int(swr_ctx, "in_sample_rate", src_rate, 0);
			av_opt_set_sample_fmt(swr_ctx, "in_sample_fmt", src_sample_fmt, 0);


			av_opt_set_int(swr_ctx, "out_channel_layout", enc_ctx->channel_layout, 0);
			av_opt_set_int(swr_ctx, "out_sample_rate", enc_ctx->sample_rate, 0);
			av_opt_set_sample_fmt(swr_ctx, "out_sample_fmt", enc_ctx->sample_fmt, 0);

			int ret;
			if ((ret = swr_init(swr_ctx)) < 0) {

				throw gcnew Exception("Failed to initialize the resampling context " + ret);

			}

			if (enc_ctx->frame_size) {

			}

			fifo = av_audio_fifo_alloc(enc_ctx->sample_fmt, enc_ctx->channels, 1);
			if (!fifo) {
				throw gcnew Exception("Could not allocate FIFO");
			}


		}

		void Resample2(array<Byte>^ srcData, [Out] array<Byte>^% destData) {

			const int buf_size = enc_ctx->frame_size ? enc_ctx->frame_size : 0;//1024;

			int srcLenght = srcData->Length;
			int sampleSize = av_get_bytes_per_sample(src_sample_fmt);
			int srcSampleNum = srcLenght / (sampleSize * src_nb_channels);


			uint8_t **src_samples = NULL;
			try {
				pin_ptr<uint8_t**>p_src_samples = &src_samples;
				int ret = av_samples_alloc_array_and_samples(p_src_samples, NULL, src_nb_channels, srcSampleNum, src_sample_fmt, 0);
				if (ret < 0) {

					throw gcnew Exception("Could not allocate destination samples " + ret);
				}

				Marshal::Copy(srcData, 0, (IntPtr)src_samples[0], srcData->Length);
				int dst_rate = enc_ctx->sample_rate;
				int dst_nb_samples = av_rescale_rnd(srcSampleNum, enc_ctx->sample_rate, src_rate, AV_ROUND_UP);
				//int dst_nb_samples = av_rescale_rnd(swr_get_delay(swr_ctx, src_rate) + srcSampleNum, dst_rate, src_rate, AV_ROUND_UP);
				//int max_dst_nb_samples = dst_nb_samples;


				uint8_t	**dst_samples = NULL;
				try {

					int dst_linesize;
					pin_ptr<uint8_t**>p_dst_data = &dst_samples;
					pin_ptr<int> p_dst_linesize = &dst_linesize;
					AVSampleFormat dst_sample_fmt = enc_ctx->sample_fmt;
					int dst_nb_channels = enc_ctx->channels;
					ret = av_samples_alloc_array_and_samples(p_dst_data, NULL, dst_nb_channels, dst_nb_samples, dst_sample_fmt, 0);

					if (ret < 0) {
						throw gcnew Exception("Could not allocate destination samples");
					}

					ret = swr_convert(swr_ctx, dst_samples, dst_nb_samples, (const uint8_t**)src_samples, srcSampleNum);// reinterpret_cast<const uint8_t**>(p_srcData), srcSampleNum);

					if (ret < 0) {
						throw gcnew Exception("Error while converting");

					}

					ret = av_audio_fifo_realloc(fifo, av_audio_fifo_size(fifo) + dst_nb_samples);
					if (ret < 0) {
						throw gcnew Exception("Could not reallocate FIFO " + ret);
					}

					ret = av_audio_fifo_write(fifo, (void **)dst_samples, dst_nb_samples);
					if (ret < dst_nb_samples) {
						throw gcnew Exception("Could not write data to FIFO " + ret);
					}

				}
				finally{

					if (dst_samples) {
						av_freep(&dst_samples[0]);
					}
					av_freep(&dst_samples);
				}
			}
			finally{

				if (src_samples) {
					av_freep(&src_samples[0]);
				}
				av_freep(&src_samples);
			}


			if (av_audio_fifo_size(fifo) < buf_size) {
				destData = gcnew array<Byte>(0);
				return;
			}

			List<array<Byte>^>^ destBuffer = gcnew List<array<Byte>^>(1024);

			while ( av_audio_fifo_size(fifo) >= buf_size || (closing && av_audio_fifo_size(fifo) > 0) ) {
				// читаем буфер, кодируем в нужный формат

				int frame_size = av_audio_fifo_size(fifo);
				if (buf_size) {
					frame_size = FFMIN(frame_size, buf_size);
				}
				
				int got_packet = 0;

				AVFrame* frame = NULL;
				AVPacket packet;

				try {

					frame = av_frame_alloc();
					if (!frame) throw gcnew Exception("Could not allocate output frame");

					frame->nb_samples = frame_size; //output_codec_context->frame_size
					frame->channel_layout = enc_ctx->channel_layout;
					frame->format = enc_ctx->sample_fmt;
					frame->sample_rate = enc_ctx->sample_rate;

					int res = av_frame_get_buffer(frame, 0);

					if (res < 0) throw gcnew Exception("Could allocate output frame samples");

					res = av_audio_fifo_read(fifo, (void **)frame->data, frame_size);

					if (res < 0) throw gcnew Exception("Could not read data from FIFO");

					if (frame) {
						//frame->pts = pts;
						//pts += frame->nb_samples;
					}

					packet.data = NULL;
					packet.size = 0;
					av_init_packet(&packet);

					res = avcodec_encode_audio2(enc_ctx, &packet, frame, &got_packet);

					if (res < 0) {
						throw gcnew Exception("Could not encode frame");
					}

					if (got_packet) {


						int dst_bufsize = packet.size; //av_samples_get_buffer_size(p_dst_linesize, dst_nb_channels, ret, dst_sample_fmt, 1);	
						auto buff = gcnew array<Byte>(dst_bufsize);

						Marshal::Copy((IntPtr)packet.data, buff, 0, dst_bufsize);

						destBuffer->Add(buff);
					}
				}
				finally{

					if (frame) {
						pin_ptr<AVFrame*> p_frame = &frame;
						av_frame_free(p_frame);
					}
					av_free_packet(&packet);
				}

				if (!buf_size) {
					break;
				}

			} // read_buf/encode/write_file



			int size = 0;
			auto destArray = destBuffer->ToArray();
			for (int i = 0; i < destArray->Length; i++) {

				array<Byte>^ buff = destArray[i];
				size += buff->Length;
			}

			if (size > 0) {

				destData = gcnew array<Byte>(size);

				int offset = 0;
				for (int i = 0; i < destArray->Length; i++) {

					array<Byte>^ buff = destArray[i];

					Array::Copy(buff, 0, destData, offset, buff->Length);
					offset += buff->Length;

				}

			}



		}

		void Resample(array<Byte>^ srcData, [Out] array<Byte>^% destData) {


			int srcLenght = srcData->Length;
			int sampleSize = av_get_bytes_per_sample(src_sample_fmt);
			int srcSampleNum = srcLenght / (sampleSize * src_nb_channels);

			uint8_t **src_samples = NULL;
			try {
				pin_ptr<uint8_t**>p_src_samples = &src_samples;
				int ret = av_samples_alloc_array_and_samples(p_src_samples, NULL, src_nb_channels, srcSampleNum, src_sample_fmt, 0);
				if (ret < 0) {

					throw gcnew Exception("Could not allocate destination samples " + ret);
				}

				Marshal::Copy(srcData, 0, (IntPtr)src_samples[0], srcData->Length);

				int dst_rate = enc_ctx->sample_rate;
				int dst_nb_samples = av_rescale_rnd(swr_get_delay(swr_ctx, src_rate) + srcSampleNum, dst_rate, src_rate, AV_ROUND_UP);
				int max_dst_nb_samples = dst_nb_samples;


				uint8_t	**dst_samples = NULL;
				try {

					int dst_linesize;
					pin_ptr<uint8_t**>p_dst_data = &dst_samples;
					pin_ptr<int> p_dst_linesize = &dst_linesize;

					AVSampleFormat dst_sample_fmt = enc_ctx->sample_fmt;
					int dst_nb_channels = enc_ctx->channels;

					ret = av_samples_alloc_array_and_samples(p_dst_data, NULL, dst_nb_channels, dst_nb_samples, dst_sample_fmt, 0);

					if (ret < 0) {
						throw gcnew Exception("Could not allocate destination samples");
					}

					ret = swr_convert(swr_ctx, dst_samples, dst_nb_samples, (const uint8_t**)src_samples, srcSampleNum);// reinterpret_cast<const uint8_t**>(p_srcData), srcSampleNum);

					if (ret < 0) {
						throw gcnew Exception("Error while converting");

					}

					int dst_bufsize = av_samples_get_buffer_size(p_dst_linesize, dst_nb_channels, ret, dst_sample_fmt, 1);
					if (dst_bufsize < 0) {

						throw gcnew Exception("Could not get sample buffer size");

					}

					destData = gcnew array<Byte>(dst_bufsize);

					Marshal::Copy((IntPtr)dst_samples[0], destData, 0, dst_bufsize);
				}
				finally{

					if (dst_samples) {
						av_freep(&dst_samples[0]);
					}
					av_freep(&dst_samples);
				}
			}
			finally{

				if (src_samples) {
					av_freep(&src_samples[0]);
				}
				av_freep(&src_samples);
			}
		}


		void Close() {


			logger->TraceEvent(TraceEventType::Verbose, 0, "AudioEncoder::Close(...)");

			if (swr_ctx) {
				pin_ptr<SwrContext*> p_swr_ctx = &swr_ctx;
				swr_free(p_swr_ctx);
				swr_ctx = NULL;
			}

		}


	private:

		void CleanUp() {

			logger->TraceEvent(TraceEventType::Verbose, 0, "AudioEncoder::CleanUp()");

			if (fifo) {
				av_audio_fifo_free(fifo);
				fifo = NULL;
			}

			if (swr_ctx) {
				pin_ptr<SwrContext*> p_swr_ctx = &swr_ctx;
				swr_free(p_swr_ctx);
				swr_ctx = NULL;
			}

			if (enc_ctx) {
				pin_ptr<AVCodecContext*> p_enc_ctx = &enc_ctx;
				avcodec_free_context(p_enc_ctx);
				enc_ctx = NULL;
			}
		}

		AVAudioFifo* fifo;
		struct SwrContext *swr_ctx;
		//AVFrame* frame;

		int src_nb_channels = 0;
		int64_t src_ch_layout = AV_CH_LAYOUT_STEREO;
		int src_rate = 48000;
		enum AVSampleFormat src_sample_fmt = AV_SAMPLE_FMT_FLT;// AV_SAMPLE_FMT_DBL;


		//int dst_nb_channels = 0;
		//int64_t	dst_ch_layout = AV_CH_LAYOUT_MONO;
		//int	dst_rate = 8000; // 44100;
		//enum AVSampleFormat dst_sample_fmt = AV_SAMPLE_FMT_U8;//AV_SAMPLE_FMT_S16;

		AVCodecContext* enc_ctx;

		bool closing;
		//int src_linesize;
		//int src_nb_samples = 1024;

		static TraceSource^ logger = TraceManager::GetTrace("MediaToolkit.FFmpeg");
		//static Logger^ logger = LogManager::GetCurrentClassLogger();

	};


}