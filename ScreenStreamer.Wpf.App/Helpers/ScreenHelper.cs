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

    public class DisplayItem
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public Rectangle CaptureRegion { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is DisplayItem)) return false;
            return (DeviceId == ((DisplayItem)obj).DeviceId);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // public ScreenCaptureDevice Device { get; set; }
    }

    public class ScreenHelper
    {

        public static List<DisplayItem> GetDisplayItems()
        {

            List<DisplayItem> items = new List<DisplayItem>();

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

                items.Add(new DisplayItem
                {
                    Name = monitorName,//screen.DeviceName,//+ "" + s.Bounds.ToString(),
                    DeviceId = device.DeviceId,
                    CaptureRegion = device.CaptureRegion,
                });

                monitorIndex++;
            }

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

                items.Add(new DisplayItem
                {
                    Name = device.Name,//+ "" + s.Bounds.ToString(),
                    DeviceId = device.DeviceId,
                    CaptureRegion = device.CaptureRegion,
                });

            }

            //var captDevices = MediaToolkit.MediaFoundation.MfTool.FindUvcDevices();
            //if (captDevices.Count > 0)
            //{
            //    var captItems = captDevices.Select(d => new DisplayItem
            //    {
            //        Name = d.Name,
            //        DeviceId = d.DeviceId,
            //        CaptureRegion = new Rectangle(new Point(0, 0), d.Resolution),
            //    });

            //    items.AddRange(captItems);
            //}

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