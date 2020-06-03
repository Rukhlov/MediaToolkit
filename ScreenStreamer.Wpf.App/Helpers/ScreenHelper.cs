using MediaToolkit.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
//using Polywall.Share.Exceptions;

namespace ScreenStreamer.Wpf.Common.Helpers
{
    public class EncoderItem
    {
        public string Name { get; set; }
        public string Id { get; set; }

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


    public class EncoderHelper
    {
        public static List<EncoderItem> GetVideoEncoderItems()
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

   
    public class VideoSourceItem
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public Rectangle CaptureRegion { get; set; }

        public bool IsUvcDevice { get; set; } = false;

        public override bool Equals(object obj)
        {
            if (!(obj is VideoSourceItem)) return false;
            return (DeviceId == ((VideoSourceItem)obj).DeviceId);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // public ScreenCaptureDevice Device { get; set; }
    }

    public class ScreenHelper
    {

        public static List<VideoSourceItem> GetDisplayItems()
        {

            List<VideoSourceItem> items = new List<VideoSourceItem>();

            //var captureProperties = Config.Data.ScreenCaptureProperties;

            int monitorIndex = 1;
            foreach (var screen in Screen.AllScreens)
            {
                var friendlyName = MediaToolkit.Utils.DisplayHelper.GetFriendlyScreenName(screen);

                var bounds = screen.Bounds;

                ScreenCaptureDevice device = new ScreenCaptureDevice
                {
                    CaptureRegion = bounds,
                    DisplayRegion = bounds,
                    Name = screen.DeviceName,

                    Resolution = bounds.Size,
                    //Properties = captureProperties,
                    DeviceId = screen.DeviceName,

                };

                var monitorDescr = bounds.Width + "x" + bounds.Height;
                if (!string.IsNullOrEmpty(friendlyName))
                {
                    monitorDescr = friendlyName + " " + monitorDescr;
                }

                var monitorName = "Display " + monitorIndex + " (" + monitorDescr + ")";

                //var name = screen.DeviceName;
                //if (!string.IsNullOrEmpty(friendlyName))
                //{
                //    name += " (" + friendlyName + " " + bounds.Width  + "x" + bounds.Height + ") ";
                //}
                device.Name = monitorName;

                items.Add(new VideoSourceItem
                {
                    Name = monitorName,//screen.DeviceName,//+ "" + s.Bounds.ToString(),
                    DeviceId = device.DeviceId,
                    CaptureRegion = device.CaptureRegion,
                });

                monitorIndex++;
            }

            var customRegion = new Rectangle(10, 10, 640, 480);
            ScreenCaptureDevice customRegionDescr = new ScreenCaptureDevice
            {
                CaptureRegion = customRegion,
                DisplayRegion = Rectangle.Empty,

                Resolution = customRegion.Size,

                Name = "Screen Region",
                DeviceId = "ScreenRegion",

            };

            items.Add(new VideoSourceItem
            {
                Name = customRegionDescr.Name,//+ "" + s.Bounds.ToString(),
                DeviceId = customRegionDescr.DeviceId,
                CaptureRegion = customRegionDescr.CaptureRegion,
            });


            if (items.Count > 1)
            {
                var allScreenRect = SystemInformation.VirtualScreen;

                ScreenCaptureDevice device = new ScreenCaptureDevice
                {
                    DisplayRegion = allScreenRect,
                    CaptureRegion = allScreenRect,
                    Resolution = allScreenRect.Size,
                    //Properties = captureProperties,
                    Name = "All Displays (" + allScreenRect.Width + "x" + allScreenRect.Height + ")",
                    DeviceId = "AllScreens",
                };

                items.Add(new VideoSourceItem
                {
                    Name = device.Name,//+ "" + s.Bounds.ToString(),
                    DeviceId = device.DeviceId,
                    CaptureRegion = device.CaptureRegion,
                });

            }

            var captDevices = MediaToolkit.MediaFoundation.MfTool.FindUvcDevices();
            if (captDevices.Count > 0)
            {
                var captItems = captDevices.Select(d => new VideoSourceItem
                {
                    Name = d.Name,
                    DeviceId = d.DeviceId,
                    CaptureRegion = new Rectangle(new Point(0, 0), d.Resolution),
                    IsUvcDevice = true,
                });

                items.AddRange(captItems);
            }

            return items;
        }

        public const string ALL_DISPLAYS = "All";
        public static List<string> GetScreens()
        {

            var screens = Screen.AllScreens.Select(s => s.DeviceName)
                .Select(display => display.TrimStart(new[] { '\\', '.' })).ToList();

            screens.Add(ALL_DISPLAYS);

            return screens;
        }

        public static Rectangle? GetScreenBounds(string name)
        {

            var bounds = Screen.AllScreens.FirstOrDefault(s => s.DeviceName.EndsWith(name))?.Bounds;
            if (bounds.HasValue)
            {
                return bounds.Value;
            }
            else if (name == ALL_DISPLAYS)
            {
                return SystemInformation.VirtualScreen;
            }

            return null;
        }

    }
}