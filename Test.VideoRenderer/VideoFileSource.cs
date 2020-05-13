using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using MediaToolkit.Utils;
using NLog;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.VideoRenderer
{
	class VideoFileSource
	{
		private Logger logger = LogManager.GetCurrentClassLogger();

		public VideoFileSource() { }

		private SourceReader sourceReader = null;
		public MediaSource mediaSource = null;

		public MediaType OutputMediaType { get; private set; }

		public void Setup(string fileName, Direct3DDeviceManager devMan = null)
		{
			logger.Debug("VideoFileSource::Setup()");

			//
			using (var sourceResolver = new SourceResolver())
			{
				var unkObj = sourceResolver.CreateObjectFromURL(fileName, SourceResolverFlags.MediaSource);

			    var guid = typeof(MediaSource).GUID;
				unkObj.QueryInterface(ref guid, out var pUnk);

				mediaSource = new MediaSource(pUnk);
			}


			using (var mediaAttributes = new MediaAttributes(IntPtr.Zero))
			{
				MediaFactory.CreateAttributes(mediaAttributes, 5);
				//mediaAttributes.Set(SourceReaderAttributeKeys.EnableVideoProcessing, 1);

				if(devMan != null)
				{
					//mediaAttributes.Set(SourceReaderAttributeKeys.DisableDxva, 0);
					mediaAttributes.Set(SourceReaderAttributeKeys.D3DManager, devMan);
				}

				sourceReader = new SourceReader(mediaSource, mediaAttributes);
			}


			var charact = mediaSource.Characteristics;

			Console.WriteLine(MfTool.LogEnumFlags((MediaSourceCharacteristics)charact));


			Console.WriteLine("------------------CurrentMediaType-------------------");
			int videoStreamIndex = (int)SourceReaderIndex.FirstVideoStream;

			using (var currentMediaType = sourceReader.GetCurrentMediaType(videoStreamIndex))
			{
				Console.WriteLine(MfTool.LogMediaType(currentMediaType));

				var frameSize = currentMediaType.Get(MediaTypeAttributeKeys.FrameSize);
				var frameRate = currentMediaType.Get(MediaTypeAttributeKeys.FrameRate);

				OutputMediaType = new MediaType();
				
				OutputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
				OutputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);// VideoFormatGuids.Yv12);
				OutputMediaType.Set(MediaTypeAttributeKeys.FrameSize, frameSize);
				OutputMediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);

				OutputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
				OutputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

				sourceReader.SetCurrentMediaType(videoStreamIndex, OutputMediaType);

				Console.WriteLine("------------------NEW MediaType-------------------");
				Console.WriteLine(MfTool.LogMediaType(OutputMediaType));
						
			}

		}
		
		public MediaType GetCurrentMediaType(int streamIndex = (int)SourceReaderIndex.FirstVideoStream)
		{
			return sourceReader?.GetCurrentMediaType(streamIndex);
		}

		private AutoResetEvent syncEvent = new AutoResetEvent(false);

		public Action SourceStarted;
		public Action SourceStopped;
		public Action<int, Sample> SampleReady;
		public Action<MediaType> MediaTypeChanged;
		public void Start()
		{
			logger.Debug("VideoCaptureSource::Start()");
			running = true;

			Task.Run(() =>
			{

				int sampleCount = 0;

				try
				{

					while (running)
					{

						SourceStarted?.Invoke();
						Sample sample = null;

						try
						{
							syncEvent.WaitOne();

							var flags = GetSample(out sample);

							//Thread.Sleep(25);
	
							if(flags!= SourceReaderFlags.None)
							{
								logger.Debug("SourceReaderFlags " + flags);
							}
							if (flags == SourceReaderFlags.Endofstream || flags == SourceReaderFlags.Error)
							{
								running = false;
								break;
							}

							//var sampleDuration = MfTool.SecToMfTicks(1);
							//sample.SampleTime = sampleCount * sampleDuration;
							//sample.SampleDuration = sampleDuration;

							//Console.WriteLine(">>>>>>>>>>>>> " + MfTool.MfTicksToSec(sample.SampleTime));
							SampleReady?.Invoke((int)flags, sample);

						}
						finally
						{
							sample?.Dispose();
						}

						sampleCount++;

					}
				}
				catch (Exception ex)
				{
					running = false;
					logger.Error(ex);
				}
				finally
				{
					SourceStopped?.Invoke();
					//Close();
				}

			});

		}


		public void NextSample()
		{
			syncEvent.Set();
		}

		public SourceReaderFlags GetSample(out Sample sample)
		{

			SourceReaderFlags flags = SourceReaderFlags.None;

			sample = sourceReader.ReadSample(SourceReaderIndex.FirstVideoStream, SourceReaderControlFlags.None, out var actualIndex, out flags, out var timestamp);

			//if (sample != null)
			//{
			//	ProcessSample(sample);
			//}

			return flags;
		}

		private static void ProcessSample(Sample sample)
		{
			var sampleTime = sample.SampleTime;
			var sampltDuration = sample.SampleDuration;

			//Console.WriteLine("SampleTime " + sampleTime + " SampleDuration " + sample.SampleDuration + " SampleFlags " + sample.SampleFlags);

			using (var mb = sample.ConvertToContiguousBuffer())
			{
				try
				{
					using (var attrs = sample.QueryInterface<MediaAttributes>())
					{
						var log = MfTool.LogMediaAttributes(attrs);
						Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>> " + log);
					}

					var pData = mb.Lock(out var maxLen, out var currLen);

					//var fileName = "Nv12_1280x720_" + MfTool.MfTicksToSec(sampleTime).ToString("0.00") + ".raw";
					//var path = @"d:\TEMP\Test\";

					//var fileFullName = Path.Combine(path, fileName);
					//TestTools.WriteFile(pData, currLen, fileFullName);
				}
				finally
				{
					mb.Unlock();
				}
			}
		}

		private volatile bool running = false;
		public void Stop()
		{
			running = false;
			syncEvent?.Set();
		}



		public void Close()
		{
			logger.Debug("VideoCaptureSource::Close()");

			if (mediaSource != null)
			{
				//mediaSource?.Shutdown();

				mediaSource.Dispose();
				mediaSource = null;
			}

			if (sourceReader != null)
			{
				sourceReader.Dispose();
				sourceReader = null;
			}

			if (OutputMediaType != null)
			{
				OutputMediaType.Dispose();
				OutputMediaType = null;
			}

		}


	}

}
