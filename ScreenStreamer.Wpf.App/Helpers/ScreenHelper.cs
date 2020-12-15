using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.Utils;
using ScreenStreamer.Wpf.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;


//using Polywall.Share.Exceptions;

namespace ScreenStreamer.Wpf.Helpers
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

    public class ScreenHelper
    {
        public static D3DFeatureLevel GetDefaultAdapterFeatureLevel()
        {    
           return (D3DFeatureLevel)MediaToolkit.DirectX.DxTool.GetDefaultAdapterFeatureLevel();        
        }

        public static readonly List<ScreenCaptureItem> SupportedCaptures = new List<ScreenCaptureItem>
        {
            new ScreenCaptureItem
            {
                CaptType = VideoCaptureType.DXGIDeskDupl,
                Name = "Desktop Duplication API"
            },

            new ScreenCaptureItem
            {
                CaptType = VideoCaptureType.GDI,
                Name = "GDI"
            },

            new ScreenCaptureItem
            {
                CaptType = VideoCaptureType.GDILayered,
                Name = "GDI Layered"
            },

            new ScreenCaptureItem
            {
                CaptType = VideoCaptureType.Datapath,
                Name = "Datapath"
            },
        };


        public enum LengthDirection
        {
            Vertical,
            Horizontal
        }

        public static double PointsToPixels(double wpfPoints, LengthDirection direction)
        {
            if (direction == LengthDirection.Horizontal)
            {
                return wpfPoints * Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.WorkArea.Width;
            }
            else
            {
                return wpfPoints * Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.WorkArea.Height;
            }
        }

        public static double PixelsToPoints(int pixels, LengthDirection direction)
        {
            if (direction == LengthDirection.Horizontal)
            {
                return pixels * SystemParameters.WorkArea.Width / Screen.PrimaryScreen.WorkingArea.Width;
            }
            else
            {
                return pixels * SystemParameters.WorkArea.Height / Screen.PrimaryScreen.WorkingArea.Height;
            }
        }


        private static List<VideoSourceItem> videoSourceItems = null;

        public static List<VideoSourceItem> GetVideoSources(bool ForceUpdate = false)
        {

           // List<VideoSourceItem> items = new List<VideoSourceItem>();

            //var captureProperties = Config.Data.ScreenCaptureProperties;

            if(videoSourceItems == null || ForceUpdate)
            {
                videoSourceItems = new List<VideoSourceItem>();

                var displayDeviceInfos = DisplayUtil.GetDisplayDeviceInfos();

                var displayConfigInfos = DisplayUtil.GetDisplayConfigInfos().ToDictionary(d => d.GdiDeviceName);

				int monitorIndex = 1;
                foreach (var screen in Screen.AllScreens)
                {
					var deviceName = screen.DeviceName;
					DisplayConfigInfo gdiDisplayInfo = null;
					if (displayConfigInfos.ContainsKey(deviceName))
					{
						gdiDisplayInfo = displayConfigInfos[deviceName];
					}

                    MediaToolkit.NativeAPIs.DisplayDevice displayDevice = default(MediaToolkit.NativeAPIs.DisplayDevice);
                    if (displayDeviceInfos.ContainsKey(deviceName))
                    {
                        displayDevice = displayDeviceInfos[deviceName];
                    }

                    var adapterName = displayDevice.DeviceString;
					var friendlyName = gdiDisplayInfo?.FriendlyName ?? "";

                    string refreshRateStr = "";
                    string formatStr = "";
                    if (gdiDisplayInfo != null)
                    {
                        var refreshRate = gdiDisplayInfo.RefreshRate;
                        double _refreshRate = ((double)refreshRate.Num / refreshRate.Den);
                        refreshRateStr = System.Math.Round(_refreshRate).ToString() + "Hz";

                        formatStr = (gdiDisplayInfo.PixelFormat * 8).ToString() + "bit";
                    }

                    var bounds = screen.Bounds;

                    ScreenCaptureDevice device = new ScreenCaptureDevice
                    {
                        CaptureRegion = bounds,
                        DisplayRegion = bounds,
                        Name = deviceName,

                        Resolution = bounds.Size,
                        //Properties = captureProperties,
                        DeviceId = deviceName,

                    };
                    
                    var monitorDescr = bounds.Width + "x" + bounds.Height;
                    if (!string.IsNullOrEmpty(friendlyName))
                    {
                        monitorDescr = friendlyName + " " + monitorDescr;
                    }

                    if (!string.IsNullOrEmpty(formatStr))
                    {
                        monitorDescr += " " + formatStr;
                    }

                    if (!string.IsNullOrEmpty(refreshRateStr))
                    {
                        monitorDescr += " " + refreshRateStr;
                    }

                    var monitorName = "Display " + monitorIndex + " (" + monitorDescr + ")";

                    //var name = screen.DeviceName;
                    //if (!string.IsNullOrEmpty(friendlyName))
                    //{
                    //    name += " (" + friendlyName + " " + bounds.Width  + "x" + bounds.Height + ") ";
                    //}
                    device.Name = monitorName;

                    videoSourceItems.Add(new VideoSourceItem
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

                videoSourceItems.Add(new VideoSourceItem
                {
                    Name = customRegionDescr.Name,//+ "" + s.Bounds.ToString(),
                    DeviceId = customRegionDescr.DeviceId,
                    CaptureRegion = customRegionDescr.CaptureRegion,
                });


                if (videoSourceItems.Count > 1)
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

                    videoSourceItems.Add(new VideoSourceItem
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
                        CaptureRegion = new Rectangle(new System.Drawing.Point(0, 0), d.Resolution),
                        IsUvcDevice = true,
                    });

                    videoSourceItems.AddRange(captItems);
                }

            }


            return new List<VideoSourceItem>(videoSourceItems);
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