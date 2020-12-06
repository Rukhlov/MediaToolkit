using MediaToolkit.Core;
using MediaToolkit.Logging;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{

	public class AudioUtils
	{
		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit");

		public static List<AudioCaptureDevice> GetAudioCaptureDevices()
		{
			List<AudioCaptureDevice> captureDevices = new List<AudioCaptureDevice>();

			var mmdevices = GetMMDevices();

			foreach (var d in mmdevices)
			{
				AudioCaptureDevice captureDevice = null;
				var client = d.AudioClient;
				if (client != null)
				{
					var mixFormat = client.MixFormat;
					if (mixFormat != null)
					{

						captureDevice = new AudioCaptureDevice
						{
							DeviceId = d.ID,
							Name = d.FriendlyName,

							BitsPerSample = mixFormat.BitsPerSample,
							SampleRate = mixFormat.SampleRate,
							Channels = mixFormat.Channels,
							Description = $"{mixFormat.BitsPerSample} bit PCM: {mixFormat.SampleRate / 1000}kHz {mixFormat.Channels} channels",

							//Properties = prop,
						};

						captureDevices.Add(captureDevice);

					}
				}

				d?.Dispose();
			}
			mmdevices.Clear();


			return captureDevices;
		}

		private static List<MMDevice> GetMMDevices()
		{
			List<MMDevice> mmdevices = new List<MMDevice>();

			try
			{
				using (var deviceEnum = new MMDeviceEnumerator())
				{

					var defaultCaptureId = "";
					try
					{
						if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Capture, Role.Console))
						{
							var captureDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
							if (captureDevice != null)
							{

								defaultCaptureId = captureDevice.ID;
								mmdevices.Add(captureDevice);
							}
						}
					}
					catch (Exception ex)
					{
						logger.Warn(ex);
					}

					var defaultRenderId = "";
					try
					{
						if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
						{
							var renderDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
							if (renderDevice != null)
							{
								defaultRenderId = renderDevice.ID;
								mmdevices.Add(renderDevice);
							}
						}
					}
					catch (Exception ex)
					{
						logger.Warn(ex);
					}

					try
					{

						var allDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
						foreach (var d in allDevices)
						{
							if (d.ID == defaultRenderId || d.ID == defaultCaptureId)
							{
								continue;
							}
							mmdevices.Add(d);
						}
					}
					catch (Exception ex)
					{
						logger.Warn(ex);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return mmdevices;
		}
	}
}
