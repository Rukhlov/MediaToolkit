using MediaToolkit.Core;
using ScreenStreamer.Wpf.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ScreenStreamer.Wpf.Helpers
{
    public class EncoderHelper
    {
        public static List<EncoderItem> GetVideoEncoders()
        {
            List<EncoderItem> items = new List<EncoderItem>();

            var encoders = MediaToolkit.MediaFoundation.MfTool.FindVideoEncoders();

            foreach (var enc in encoders)
            {
                if (enc.Activatable && enc.Format == VideoCodingFormat.H264)
                {
                    var item = new EncoderItem
                    {
                        Name = enc.Name,
                        Id = enc.Id,
                    };

                    items.Add(item);
                }

            }

            VideoEncoderDescription libx264Description = new VideoEncoderDescription
            {
                Id = "libx264",
                Name = "libx264",
                Format = VideoCodingFormat.H264,
                IsHardware = false,
                Activatable = true,

            };

            items.Add(new EncoderItem
            {
                Name = libx264Description.Name,
                Id = libx264Description.Id,
            });

            return items;
        }
    }
}
