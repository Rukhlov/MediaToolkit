using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.Utils;
using ScreenStreamer.Wpf.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;


//using Polywall.Share.Exceptions;

namespace ScreenStreamer.Wpf.Helpers
{

	public class VideoHelper
	{
		public static D3DFeatureLevel GetDefaultAdapterFeatureLevel()
		{
			return (D3DFeatureLevel)MediaToolkit.DirectX.DxTool.GetDefaultAdapterFeatureLevel();
		}

        public static ScreenCaptureFeature GetCaptureFeatures()
        {
            ScreenCaptureFeature features = ScreenCaptureFeature.None;

            var winVersion = Environment.OSVersion.Version;
            features = ScreenCaptureFeature.Default;
            if (winVersion.Major >= 6 && winVersion.Minor >= 2)
            {
                features |= ScreenCaptureFeature.Win8;
            }

            //try
            //{
            //    if (MediaToolkit.ScreenCaptures.DatapathDesktopCapture.Load())
            //    {
            //        features |= ScreenCaptureFeature.Datapath;
            //    }
            //}
            //finally
            //{// 
            // // MediaToolkit.ScreenCaptures.DatapathDesktopCapture.Unload();
            //}

            //TODO:
            // Nvidia capture...

            return features;

        }

        public static readonly List<ScreenCaptureItem> AllSupportedCaptures = new List<ScreenCaptureItem>
		{
			new ScreenCaptureItem
			{
				CaptFeature = ScreenCaptureFeature.Win8,
				CaptType = VideoCaptureType.DXGIDeskDupl,
				Name = "Desktop Duplication API"
			},

			new ScreenCaptureItem
			{
				CaptFeature = ScreenCaptureFeature.Default,
				CaptType = VideoCaptureType.GDI,
				Name = "GDI"
			},

			new ScreenCaptureItem
			{
				CaptFeature = ScreenCaptureFeature.Default,
				CaptType = VideoCaptureType.GDILayered,
				Name = "GDI Layered"
			},

			new ScreenCaptureItem
			{
				CaptFeature = ScreenCaptureFeature.Datapath,
				CaptType = VideoCaptureType.Datapath,
				Name = "Datapath"
			},

            new ScreenCaptureItem
            {
                CaptFeature = ScreenCaptureFeature.Default,
                CaptType = VideoCaptureType.DummyRGB32,
                Name = "DummyRGB"
            },
        };

        public static List<ScreenCaptureItem> GetScreenCaptures()
        {
           var features = GetCaptureFeatures();
            var allCaptures = AllSupportedCaptures;

            return allCaptures.Where(c =>
            {
                if (!Program.TestMode)
                {
                    if (c.CaptType == VideoCaptureType.DummyRGB32)
                    {
                        return false;
                    }
                }

                return features.HasFlag(c.CaptFeature);

            }).ToList();
        }

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


		public static List<VideoSourceItem> GetVideoSources()
		{

			var videoSources = new List<VideoSourceItem>();

			var deviceInfos = DisplayUtil.GetDisplayDeviceInfos();

            var configInfos = new Dictionary<string, DisplayConfigInfo>();
            try
            {
                configInfos = DisplayUtil.GetDisplayConfigInfos().ToDictionary(d => d.GdiDeviceName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            int monitorIndex = 1;
			foreach (var screen in Screen.AllScreens)
			{
				var deviceName = screen.DeviceName;
				DisplayConfigInfo gdiDisplayInfo = null;
				if (configInfos.ContainsKey(deviceName))
				{
					gdiDisplayInfo = configInfos[deviceName];
				}

				MediaToolkit.NativeAPIs.DisplayDevice displayDevice = default(MediaToolkit.NativeAPIs.DisplayDevice);
				if (deviceInfos.ContainsKey(deviceName))
				{
					displayDevice = deviceInfos[deviceName];
				}

				var adapterName = displayDevice.DeviceString;
				var friendlyName = gdiDisplayInfo?.FriendlyName ?? "";

				string refreshRateStr = "";
				string formatStr = "";
				if (gdiDisplayInfo != null)
				{
					var refreshRate = gdiDisplayInfo.RefreshRate;
					double _refreshRate = ((double)refreshRate.Num / refreshRate.Den);
					refreshRateStr = System.Math.Round(_refreshRate).ToString() + LocalizationManager.GetString("CommonStringsHz");

					formatStr = (gdiDisplayInfo.PixelFormat * 8).ToString() + LocalizationManager.GetString("CommonStringsBit");
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

				var monitorName = LocalizationManager.GetString("CommonStringsDisplay") + " " + monitorIndex + " (" + monitorDescr + ")";

				//var name = screen.DeviceName;
				//if (!string.IsNullOrEmpty(friendlyName))
				//{
				//    name += " (" + friendlyName + " " + bounds.Width  + "x" + bounds.Height + ") ";
				//}
				device.Name = monitorName;

				videoSources.Add(new VideoSourceItem
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

				Name = LocalizationManager.GetString("CommonStringsScreenRegion"),//"Screen Region",
				DeviceId = "ScreenRegion",

			};

			videoSources.Add(new VideoSourceItem
			{
				Name = customRegionDescr.Name,//+ "" + s.Bounds.ToString(),
				DeviceId = customRegionDescr.DeviceId,
				CaptureRegion = customRegionDescr.CaptureRegion,
			});


			if (videoSources.Count > 1)
			{
				var allScreenRect = SystemInformation.VirtualScreen;
				var name = LocalizationManager.GetString("CommonStringsAllDisplays") + " (" + allScreenRect.Width + "x" + allScreenRect.Height + ")";
				ScreenCaptureDevice device = new ScreenCaptureDevice
				{
					DisplayRegion = allScreenRect,
					CaptureRegion = allScreenRect,
					Resolution = allScreenRect.Size,
					//Properties = captureProperties,
					Name = name, //"All Displays (" + allScreenRect.Width + "x" + allScreenRect.Height + ")",
					DeviceId = "AllScreens",
				};

				videoSources.Add(new VideoSourceItem
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

				videoSources.AddRange(captItems);
			}

			return videoSources;
		}


		public static List<EncoderItem> GetVideoEncoders()
		{

			var videoEncoders = new List<EncoderItem>();

			var encoders = MediaToolkit.MediaFoundation.MfTool.FindVideoEncoders();
			if (encoders.Count > 0)
			{// отфильтровываем одинаковые энкодеры (могут быть одинаковые Intel H264 MFT)
				encoders = encoders.GroupBy(e => e.Id).Select(y => y.First()).ToList();

				foreach (var enc in encoders)
				{
					if (enc.Activatable && enc.Format == VideoCodingFormat.H264 && enc.IsHardware)
					{
						var item = new EncoderItem
						{
							Name = enc.Name,
							Id = enc.Id,
							DriverType = VideoDriverType.D3D11,
							PixelFormat = PixFormat.NV12,
							Format = VideoCodingFormat.H264,
						};

						videoEncoders.Add(item);
					}
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

			videoEncoders.Add(new EncoderItem
			{
				Name = libx264Description.Name,
				Id = libx264Description.Id,
				DriverType = VideoDriverType.CPU,
				PixelFormat = PixFormat.NV12,
				Format = VideoCodingFormat.H264,
			});

            if (Program.TestMode)
            {
                videoEncoders.Add(new EncoderItem
                {
                    Name = "H264EncCpuNull",
                    Id = "H264EncCpuNull",
                    DriverType = VideoDriverType.CPU,
                    PixelFormat = PixFormat.NV12,
                    Format = VideoCodingFormat.H264,
                });

                videoEncoders.Add(new EncoderItem
                {
                    Name = "H264EncGpuNull",
                    Id = "H264EncGpuNull",
                    DriverType = VideoDriverType.D3D11,
                    PixelFormat = PixFormat.NV12,
                    Format = VideoCodingFormat.H264,
                });
            }

            return videoEncoders;
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