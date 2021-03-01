using MediaToolkit;
using MediaToolkit.Core;

using ScreenStreamer.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;

namespace ScreenStreamer.Wpf.Models
{

	public enum D3DFeatureLevel
	{
		None = 0,
		Level_9_1 = 37120,
		Level_9_2 = 37376,
		Level_9_3 = 37632,
		Level_10_0 = 40960,
		Level_10_1 = 41216,
		Level_11_0 = 45056,
		Level_11_1 = 45312,
		Level_12_0 = 49152,
		Level_12_1 = 49408
	}

	public enum ScreenCaptureFeature
	{
		None = 0,
		Default = 1,
		Win8 = 2,
		NvFBC = 4,
		Datapath = 8,


		All = Default | Win8 | NvFBC | Datapath,
	}

	public enum QualityPreset
    {
        Low,
        Standard,
        High
    }

    public enum ProtocolKind
    {
        TCP = 6,
        UDP = 17
    }

    public class EncoderItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
		public MediaType MediaType => MediaType.Video;
		public VideoCodingFormat Format = VideoCodingFormat.H264;

		public VideoDriverType DriverType { get; set; } = VideoDriverType.CPU;
		public PixFormat PixelFormat { get; set; } = PixFormat.NV12;
		public ColorSpace ColorSpace { get; set; } = ColorSpace.BT709;
		public ColorRange ColorRange { get; set; } = ColorRange.Partial;

        public override bool Equals(object obj)
        {
            if (!(obj is EncoderItem)) return false;
            return (Id == ((EncoderItem)obj).Id);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }


    public class IPAddressInfoItem
    {
        public string DisplayName { get; set; }
        public string InterfaceName { get; set; }

        public IPAddressInformation IPAddressInfo { get; set; }

        public override bool Equals(object obj)
        {
            var ipAddressInfoViewModel = obj as IPAddressInfoItem;
            if (ipAddressInfoViewModel == null)
            {
                return false;
            }

            return this.DisplayName == ipAddressInfoViewModel.DisplayName &&
                   (this.IPAddressInfo == null ?
                        ipAddressInfoViewModel.IPAddressInfo == null :
                        this.IPAddressInfo.IsDeepEquals(ipAddressInfoViewModel.IPAddressInfo));
        }

        public override int GetHashCode()
        {
            return this.DisplayName.GetHashCode() +
                   (this.IPAddressInfo != null ? this.IPAddressInfo.GetDeepHashCode() : 0);
        }
    }

    public class VideoSourceItem
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }


        public Rectangle CaptureRegion { get; set; }

        public bool IsUvcDevice { get; set; } = false;

        public override bool Equals(object obj)
        {
            bool Result = false;
            var item = obj as VideoSourceItem;
            if (item != null)
            {
                Result = this.DeviceId == item.DeviceId;
            }

            return Result;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // public ScreenCaptureDevice Device { get; set; }
    }


    public class AudioSourceItem
    {

        public readonly AudioCaptureDevice Device = null;

        public string DeviceId => Device.DeviceId;
        public string DisplayName => Device.Name;

        public AudioSourceItem(AudioCaptureDevice d)
        {
            this.Device = d;
        }


        public override bool Equals(object obj)
        {
            var item = obj as AudioSourceItem;
            if (item == null)
            {
                return false;
            }
            return this.DeviceId == item.DeviceId;
        }

        public override int GetHashCode()
        {
            return this.DeviceId.GetHashCode();
        }
    }


    public class ScreenCaptureItem
    {
        public VideoCaptureType CaptType { get; internal set; }
        public string Name { get; internal set; }
		public ScreenCaptureFeature CaptFeature { get; internal set; }

		public override bool Equals(object obj)
        {
            if (!(obj is ScreenCaptureItem)) return false;
            return (CaptType == ((ScreenCaptureItem)obj).CaptType);

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        // public ScreenCaptureDevice Device { get; set; }
    }

}
