using MediaToolkit.Core;
using ScreenStreamer.Wpf.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ScreenStreamer.Wpf.Helpers
{
    public class EncoderHelper
    {
		private static List<EncoderItem> encoderItems = null;
		public static List<EncoderItem> GetVideoEncoders(bool ForceUpdate = false)
        {
           // List<EncoderItem> items = new List<EncoderItem>();

			if(encoderItems == null || ForceUpdate)
			{
				encoderItems = new List<EncoderItem>();

				var encoders = MediaToolkit.MediaFoundation.MfTool.FindVideoEncoders();

				foreach (var enc in encoders)
				{
					if (enc.Activatable && enc.Format == VideoCodingFormat.H264 && enc.IsHardware)
					{
						var item = new EncoderItem
						{
							Name = enc.Name,
							Id = enc.Id,
						};

						encoderItems.Add(item);
					}

				}

				VideoEncoderDescription libx264Description = new VideoEncoderDescription
				{
					Id = "libx264",
					Name = "libx264 (CPU)",
					Format = VideoCodingFormat.H264,
					IsHardware = false,
					Activatable = true,

				};

				encoderItems.Add(new EncoderItem
				{
					Name = libx264Description.Name,
					Id = libx264Description.Id,
				});

			}

            return new List<EncoderItem>(encoderItems);
        }
    }
}
