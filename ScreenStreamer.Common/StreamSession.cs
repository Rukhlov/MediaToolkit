﻿using MediaToolkit.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ScreenStreamer.Common
{

	[Serializable]
	public class StreamSession
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		[XmlAttribute("Name")]
		public string StreamName { get; set; } = Environment.MachineName;

		[XmlAttribute("Address")]
		public string NetworkIpAddress { get; set; } = "0.0.0.0";

		[XmlAttribute("Port")]
		public int CommunicationPort { get; set; } = -1;


		[XmlIgnore]
		public string CommunicationAddress
		{
			get
			{
				var address = "net.tcp://" + NetworkIpAddress + "/ScreenCaster/";
				if (CommunicationPort > 0)
				{
					address = "net.tcp://" + NetworkIpAddress + ":" + CommunicationPort + "/ScreenCaster";
				}

				return address;
			}

		}

		[XmlAttribute("Transport")]
		public TransportMode TransportMode { get; set; } = TransportMode.Tcp;

		[XmlAttribute]
		public bool IsMulticast { get; set; } = false;

		[XmlAttribute]
		public string MutlicastAddress { get; set; } = "239.0.0.1";

		[XmlAttribute]
		public int MutlicastPort1 { get; set; } = 1234;

		[XmlAttribute]
		public int MutlicastPort2 { get; set; } = 5555;

		public VideoStreamSettings VideoSettings { get; set; } = new VideoStreamSettings();

		public AudioStreamSettings AudioSettings { get; set; } = new AudioStreamSettings();


		public void Setup()
		{
			logger.Debug("StreamSession::Setup()");

			if (IsMulticast)
			{
				TransportMode = TransportMode.Udp;
			}

			if (!IsMulticast && TransportMode == TransportMode.Udp)
			{
				throw new NotSupportedException("TransportMode.Udp currently not supported...");
			}

			var videoNetworkSettings = VideoSettings.NetworkSettings;
			var audioNetworkSettings = AudioSettings.NetworkSettings;

			videoNetworkSettings.TransportMode = TransportMode;
			audioNetworkSettings.TransportMode = TransportMode;


			if (IsMulticast)
			{
				var multicastAudioPort = MutlicastPort1 + 1;

				videoNetworkSettings.RemoteAddr = MutlicastAddress;
				videoNetworkSettings.RemotePort = MutlicastPort1;

				audioNetworkSettings.RemoteAddr = MutlicastAddress;
				audioNetworkSettings.RemotePort = multicastAudioPort;

			}
			else
			{
				if (TransportMode == TransportMode.Tcp)
				{
					videoNetworkSettings.LocalAddr = NetworkIpAddress;
					videoNetworkSettings.LocalPort = 0;

					audioNetworkSettings.LocalAddr = NetworkIpAddress;
					audioNetworkSettings.LocalPort = 0;
				}
				else
				{
					throw new NotSupportedException("Mode currently not supported: " + TransportMode);
				}
			}


			logger.Info("CommunicationAddress=" + CommunicationAddress +
				" MulticastMode=" + IsMulticast +
				" VideoEnabled=" + VideoSettings.Enabled +
				" AudioEnabled=" + AudioSettings.Enabled);

			if (VideoSettings.Enabled)
			{
				var screenCaptParams = (VideoSettings.CaptureDevice as ScreenCaptureDevice);
				if (screenCaptParams != null)
				{
					if (screenCaptParams.DisplayRegion.IsEmpty)
					{
						logger.Debug("VideoSource DisplayRegion.IsEmpty");
						//videoSettings.Enabled = false;
					}
				}

				var captureDevice = VideoSettings.CaptureDevice;
				var captureResolution = captureDevice.Resolution;

				if (captureResolution.IsEmpty)
				{
					//...
				}

				int width = captureResolution.Width;
				if (width % 2 != 0)
				{// должно быть четным
					width--;
				}

				int height = captureResolution.Height;
				if (height % 2 != 0)
				{
					height--;
				}

				captureDevice.Resolution = new Size(width, height);


                /*
				var encodingSettings = VideoSettings.EncoderSettings;
				if (VideoSettings.UseEncoderResoulutionFromSource)
				{
					encodingSettings.Width = captureDevice.Resolution.Width;
					encodingSettings.Height = captureDevice.Resolution.Height;
				}

				if (!VideoSettings.UseEncoderResoulutionFromSource)
				{
					captureDevice.Resolution = VideoSettings.EncoderSettings.Resolution;
				}
				else
				{
					//captureDevice.Resolution = Size.Empty;
				}
                */
			}

			if (AudioSettings.Enabled)
			{
				var audioCapture = AudioSettings.CaptureDevice;

				if (string.IsNullOrEmpty(audioCapture.DeviceId))
				{
					logger.Debug("Empty MMDeviceId...");
					AudioSettings.Enabled = false;
				}

			}

		}


		public static StreamSession Default()
		{
			int port = -1;

			var freeTcpPorts = MediaToolkit.Utils.NetTools.GetFreePortRange(System.Net.Sockets.ProtocolType.Tcp, 1, 808);
			if (freeTcpPorts != null && freeTcpPorts.Count() > 0)
			{
				port = freeTcpPorts.FirstOrDefault();
			}

			var session = new StreamSession
			{
				StreamName = Environment.MachineName,
				NetworkIpAddress = "0.0.0.0",
				MutlicastAddress = "239.0.0.1",
				CommunicationPort = port,
				IsMulticast = false,
				TransportMode = TransportMode.Tcp,


			};

			var videoEncoderSettings = new VideoEncoderSettings
			{
				Width = 1920,
				Height = 1080,
				Encoder = VideoEncoderMode.H264,
				Profile = H264Profile.Main,
				BitrateMode = BitrateControlMode.CBR,
				Bitrate = 2500,
				MaxBitrate = 5000,
				FrameRate = 30,
				LowLatency = true,

			};

			var videoSettings = new VideoStreamSettings
			{
				Enabled = true,
				//Id = "video_" + Guid.NewGuid().ToString(),
				NetworkSettings = new NetworkSettings(),
				CaptureDevice = null,
				EncoderSettings = videoEncoderSettings,
				StreamFlags = VideoStreamFlags.UseEncoderResoulutionFromSource,

				//ScreenCaptureProperties = captureProperties,

			};

			var audioEncoderSettings = new AudioEncoderSettings
			{
				SampleRate = 8000,
				Channels = 1,
				Encoding = "PCMU",

			};

			var audioSettings = new AudioStreamSettings
			{
				Enabled = false,
				//Id = "audio_" + Guid.NewGuid().ToString(),
				NetworkSettings = new NetworkSettings(),
				CaptureDevice = new AudioCaptureDevice(),
				EncoderSettings = audioEncoderSettings,
			};

			session.AudioSettings = audioSettings;
			session.VideoSettings = videoSettings;

			return session;
		}
	}

	[Serializable]
	public class AudioStreamSettings
	{
		//[XmlAttribute]
		//public string Id { get; set; } = "";

		[XmlAttribute]
		public bool Enabled { get; set; } = false;

        [XmlIgnore]
        public NetworkSettings NetworkSettings { get; set; } = new NetworkSettings();

		public AudioEncoderSettings EncoderSettings { get; set; } = new AudioEncoderSettings();

		public AudioCaptureDevice CaptureDevice { get; set; }
	}

	[Serializable]
	public class VideoStreamSettings
	{
        //[XmlIgnore]
		////[XmlAttribute]
		//public string Id { get; set; } = "";

		[XmlAttribute]
		public bool Enabled { get; set; } = false;

		[XmlIgnore]
		public bool UseEncoderResoulutionFromSource
		{
			get
			{
				return StreamFlags.HasFlag(VideoStreamFlags.UseEncoderResoulutionFromSource);
			}
		}

		[XmlIgnore]
		public VideoStreamFlags StreamFlags { get; set; } = VideoStreamFlags.None;

		[XmlAttribute("Flags")]
		public int flags
		{
			get
			{
				return (int)StreamFlags;
			}
			set
			{
				StreamFlags = (VideoStreamFlags)value;
			}
		}

        [XmlIgnore]
        public NetworkSettings NetworkSettings { get; set; } = new NetworkSettings();

		public VideoCaptureDevice CaptureDevice { get; set; }

		public VideoEncoderSettings EncoderSettings { get; set; } = new VideoEncoderSettings();


        [XmlIgnore]
        public bool IsScreenRegion
        {
            get
            {
                bool isScreenRegion = false;
                if (CaptureDevice != null)
                {
                    var screenCapture = CaptureDevice as ScreenCaptureDevice;
                    if (screenCapture != null)
                    {
                        isScreenRegion = screenCapture.DisplayRegion.IsEmpty;
                    }
                    
                }
                return isScreenRegion;
            }
        }
    }

	public enum VideoStreamFlags : int
	{
		None = 0,
		UseEncoderResoulutionFromSource = 1,
		//...
	}
}
