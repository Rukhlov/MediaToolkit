﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;

using System.IO;
using MediaToolkit.Logging;

namespace MediaToolkit.MediaFoundation
{


	/// <summary>
	/// https://docs.microsoft.com/en-us/windows/win32/medfound/h-264-video-decoder
	/// </summary>
	public class MfH264DecoderEx
	{
		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

		// private Device device = null;

		private Transform decoder = null;

		private int inputStreamId = -1;
		private int outputStreamId = -1;

		private long frameNumber = -1;
		private long frameDuration;

		public MfH264DecoderEx()
		{ }

		public MediaToolkit.Core.VideoDriverType DriverType { get; private set; } = MediaToolkit.Core.VideoDriverType.CPU;
		public MediaType InputMediaType { get; private set; }
		public MediaType OutputMediaType { get; private set; }

		public System.Drawing.Size MinSize => new System.Drawing.Size(48, 48);
		public System.Drawing.Size MaxSize => new System.Drawing.Size(maxCodedWidth, maxCodedHeight);

		private int maxCodedWidth = 4096;
		private int maxCodedHeight = 2304;

		public void Setup(MfVideoArgs inputArgs)
		{
			logger.Debug("MfH264Decoder::Setup(...)");

			var frameRate = inputArgs.FrameRate;
			var width = inputArgs.Width;
			var height = inputArgs.Height;

			this.DriverType = inputArgs.DriverType;
			var inputFormat = VideoFormatGuids.H264;

			try
			{
				decoder = new Transform(ClsId.CMSH264DecoderMFT);
				StringBuilder log = new StringBuilder();

				using (var attr = decoder.Attributes)
				{
					for (int i = 0; i < attr.Count; i++)
					{
						var obj = attr.GetByIndex(i, out Guid guid);
						if (guid == CodecApiPropertyKeys.AVDecVideoMaxCodedWidth.Guid)
						{
							maxCodedWidth = (int)obj;
						}
						else if (guid == CodecApiPropertyKeys.AVDecVideoMaxCodedHeight.Guid)
						{
							maxCodedHeight = (int)obj;
						}

						string logStr = MfTool.LogAttribute(guid, obj);
						log.AppendLine(logStr);
					}

					logger.Debug("MFTransformInfo:\r\n" + log);

					if (inputArgs.DriverType == MediaToolkit.Core.VideoDriverType.D3D11)
					{

						bool d3d11Aware = attr.Get(TransformAttributeKeys.D3D11Aware);
						if (d3d11Aware)
						{
							using (DXGIDeviceManager devMan = new DXGIDeviceManager())
							{
								using (var device = new SharpDX.Direct3D11.Device(inputArgs.D3DPointer))
								{
									((SharpDX.IUnknown)device).AddReference();
									devMan.ResetDevice(device);
									decoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
								}
							}
						}

						//attr.Set(TransformAttributeKeys.D3D11Bindflags, (int)(BindFlags.ShaderResource | BindFlags.Decoder));
					}
					else if (inputArgs.DriverType == MediaToolkit.Core.VideoDriverType.D3D9)
					{
						bool d3dAware = attr.Get(TransformAttributeKeys.D3DAware);
						if (d3dAware)
						{
							using (SharpDX.MediaFoundation.DirectX.Direct3DDeviceManager devMan = new SharpDX.MediaFoundation.DirectX.Direct3DDeviceManager())
							{

								using (var device = new SharpDX.Direct3D9.DeviceEx(inputArgs.D3DPointer))
								{
									((SharpDX.IUnknown)device).AddReference();
									devMan.ResetDevice(device, devMan.CreationToken);
									decoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
								}
							}
						}
					}


					attr.Set(SinkWriterAttributeKeys.LowLatency, inputArgs.LowLatency);

					//attr.Set(MFAttributeKeys.MF_SA_MINIMUM_OUTPUT_SAMPLE_COUNT, 1);
				}


				int inputStreamCount = -1;
				int outputStreamsCount = -1;
				decoder.GetStreamCount(out inputStreamCount, out outputStreamsCount);

				int[] inputStreamIDs = new int[inputStreamCount];
				int[] outputStreamIDs = new int[outputStreamsCount];

				bool res = decoder.TryGetStreamIDs(inputStreamIDs, outputStreamIDs);
				if (res)
				{
					inputStreamId = inputStreamIDs[0];
					outputStreamId = outputStreamIDs[0];
				}
				else
				{
					inputStreamId = 0;
					outputStreamId = 0;
				}

				InputMediaType = MfTool.GetTransformInputType(decoder, inputStreamId, inputFormat);

				if (InputMediaType == null)
				{
					logger.Warn("Unsuported format: " + MfTool.GetMediaTypeName(inputFormat));
					return;
				}

				InputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
				InputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.H264);
				InputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
				InputMediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);

				InputMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, MfTool.PackToLong(1, 1));

				InputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
				InputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
				decoder.SetInputType(inputStreamId, InputMediaType, 0);

				logger.Info("============== INPUT TYPE==================\r\n" + MfTool.LogMediaType(InputMediaType));

				// Console.WriteLine(MfTool.LogTransformOutputs(decoder, outputStreamId));

				OutputMediaType = new MediaType();

				//OutputMediaType = MfTool.GetTransformOutputType(decoder, outputStreamId, VideoFormatGuids.NV12);

				OutputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
				OutputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
				//OutputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 30000000);
				OutputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
				OutputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
				OutputMediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);
				OutputMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, MfTool.PackToLong(1, 1));
				OutputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

				decoder.SetOutputType(outputStreamId, OutputMediaType, 0);

				//var guid = new Guid("901db4c7-31ce-41a2-85dc-8fa0bf41b8da");
				//decoder.QueryInterface(guid, out var pUnk);
				//var codecAPI = (MediaToolkit.NativeAPIs.DShow.ICodecAPI)Marshal.GetObjectForIUnknown(pUnk);

				logger.Info("============== OUTPUT TYPE==================\r\n" + MfTool.LogMediaType(OutputMediaType));


			}
			catch (Exception ex)
			{
				logger.Error(ex);

				Close();
				throw;
			}
		}


		public void Start()
		{
			logger.Debug("MfH264Decoder::Start()");

			decoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
			decoder.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
			decoder.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);


			//decoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInformation);

			//logger.Debug(streamInformation.CbSize);


		}


		public DecodeResult ProcessSample(Sample inputSample, Action<Sample> OnSampleDecoded)
		{

			DecodeResult Result = DecodeResult.Error;

			if (inputSample != null)
			{
				//FIXME:
				frameNumber++;
				//inputSample.SampleTime = frameNumber * frameDuration; //!!!!!!!!!1
				//inputSample.SampleDuration = frameDuration;

				decoder.ProcessInput(0, inputSample, 0);
			}

			//if (decoder.OutputStatus == (int)MftOutputStatusFlags.MftOutputStatusSampleReady)
			{

				decoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

				MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
				bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);

				do
				{
					Sample pSample = null;
					// Create output sample
					if (createSample)
					{
						pSample = MediaFactory.CreateSample();
						if (inputSample != null)
						{
							pSample.SampleTime = inputSample.SampleTime;
							pSample.SampleDuration = inputSample.SampleDuration;
							pSample.SampleFlags = inputSample.SampleFlags;
						}

						//logger.Debug("CreateSample: " + inputSample.SampleTime);

						using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(streamInfo.CbSize))
						{
							pSample.AddBuffer(mediaBuffer);
						}
					}

					TOutputDataBuffer[] outputBuffers = new TOutputDataBuffer[1];

					var outputBuffer = new TOutputDataBuffer
					{
						DwStatus = 0,
						DwStreamID = 0,
						PSample = pSample,
						PEvents = null,
					};
					outputBuffers[0] = outputBuffer;


					var res = decoder.TryProcessOutput(TransformProcessOutputFlags.None, outputBuffers, out TransformProcessOutputStatus status);

					//logger.Info("TryProcessOutput(...) " + res + " " + outputDataBuffer[0].DwStatus);

					if (res == SharpDX.Result.Ok)
					{
						var buf = outputBuffers[0];

						var outputSample = buf.PSample;

						var pEvents = buf.PEvents;
						if (pEvents != null)
						{
							var eventsCount = pEvents.ElementCount;

							Debug.Assert(eventsCount == 0, "eventsCount == 0");

							if (eventsCount > 0)
							{
								for (int i = 0; i < eventsCount; i++)
								{
									var e = pEvents.GetElement(i);
									if (e != null)
									{
										e.Dispose();
										e = null;
									}

								}

							}
						}


						Debug.Assert(outputSample != null, "res.Success && outputSample != null");
						Result = DecodeResult.Ok;

						OnSampleDecoded?.Invoke(outputSample);

						//continue;
					}
					else if (res == SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput)
					{
						//logger.Info("-------------------------");
						Result = DecodeResult.NeedMoreData;

						if (pSample != null)
						{
							pSample.Dispose();
							pSample = null;
						}

						break;
					}
					else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
					{
						logger.Warn(res.ToString() + " TransformStreamChange");

						MediaType newOutputType = null;
						try
						{
							decoder.TryGetOutputAvailableType(outputStreamId, 0, out newOutputType);
							decoder.SetOutputType(outputStreamId, newOutputType, 0);

							if (OutputMediaType != null)
							{
								OutputMediaType.Dispose();
								OutputMediaType = null;
							}
							OutputMediaType = newOutputType;
							Result = DecodeResult.Changed;
							logger.Info("============== NEW OUTPUT TYPE==================\r\n" + MfTool.LogMediaType(OutputMediaType));

						}
						finally
						{
							//newOutputType?.Dispose();
							//newOutputType = null;
						}
					}
					else
					{
						res.CheckError();

						Result = DecodeResult.Error;
						break;
					}

					//if (outputSample != null)
					//{
					//   // logger.Debug("DisposeSample: " + outputSample?.SampleTime);
					//    outputSample?.Dispose();
					//    outputSample = null;
					//}


				}
				while (true);

			}

			return Result;
		}

		public void Drain()
		{
			logger.Debug("MfH264Decoder::Drain()");
			if (decoder != null)
			{
				decoder.ProcessMessage(TMessageType.CommandDrain, IntPtr.Zero);

			}
		}

		public void Stop()
		{
			logger.Debug("MfH264Decoder::Stop()");

			if (decoder != null)
			{


				decoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
				//decoder.ProcessMessage(TMessageType.CommandDrain, IntPtr.Zero);
				decoder.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
				decoder.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
				//decoder.ProcessMessage(TMessageType.SetD3DManager, IntPtr.Zero);


			}

		}

		public void Close()
		{
			logger.Debug("MfH264Decoder::Close()");

			if (InputMediaType != null)
			{
				InputMediaType.Dispose();
				InputMediaType = null;
			}

			if (OutputMediaType != null)
			{
				OutputMediaType.Dispose();
				OutputMediaType = null;
			}

			if (decoder != null)
			{
				decoder.Dispose();
				decoder = null;
			}

		}

	}

	public enum DecodeResult
	{
		Ok,
		Changed,
		NeedMoreData,
		Error,
	}
}
