using MediaToolkit.Core;
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
        [XmlAttribute("Name")]
        public string StreamName { get; set; } = Environment.MachineName;

        [XmlAttribute("Address")]
        public string NetworkIpAddress { get; set; } = "0.0.0.0";

        [XmlAttribute("Port")]
        public int CommunicationPort { get; set; } = -1;

        [XmlAttribute("Transport")]
        public TransportMode TransportMode { get; set; } = TransportMode.Tcp;

        [XmlAttribute("IsMulticast")]
        public bool IsMulticast { get; set; } = false;

        [XmlAttribute("MutlicastAddress")]
        public string MutlicastAddress { get; set; } = "239.0.0.1";

        [XmlAttribute("MutlicastPort1")]
        public int MutlicastPort1 { get; set; } = 1234;

        [XmlAttribute("MutlicastPort2")]
        public int MutlicastPort2 { get; set; } = 5555;

        public VideoStreamSettings VideoSettings { get; set; } = new VideoStreamSettings();

        public AudioStreamSettings AudioSettings { get; set; } = new AudioStreamSettings();
    }

    [Serializable]
    public class AudioStreamSettings
    {
        [XmlAttribute("Id")]
        public string Id { get; set; } = "";

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; } = false;

        public NetworkSettings NetworkSettings { get; set; } = new NetworkSettings();

        public AudioEncoderSettings EncoderSettings { get; set; } = new AudioEncoderSettings();

        public AudioCaptureDeviceDescription CaptureDevice { get; set; }
    }

    [Serializable]
    public class VideoStreamSettings
    {

        [XmlAttribute("Id")]
        public string Id { get; set; } = "";

        [XmlAttribute("Enabled")]
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

        public NetworkSettings NetworkSettings  { get; set; } = new NetworkSettings();

        public VideoCaptureDevice CaptureDevice { get; set; }

        public VideoEncoderSettings EncoderSettings { get; set; } = new VideoEncoderSettings();



    }

    public enum VideoStreamFlags :int 
    {
        None = 0,
        UseEncoderResoulutionFromSource = 1,
        //...
    }
}
