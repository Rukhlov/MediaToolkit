using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using MediaToolkit.Core;
using Newtonsoft.Json;
using NLog;
using ScreenStreamer.Common;

using ScreenStreamer.Wpf.Helpers;
using System.Linq;


namespace ScreenStreamer.Wpf.Models
{
    public class AppModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

		public readonly static string AppVersion = AppConsts.AssemblyVersion;//AppConsts.AppVersion;
        public string ConfigVersion { get; set; } = "1.0.0.0";

        public string Culture { get; set; } = "en";
        public int Dx11FeatureLevel { get; set; } = (int)D3DFeatureLevel.Level_11_0;

        [JsonProperty]
        public readonly static string MinOSVersion = "6.2";

        //[JsonProperty]
        public readonly static bool AllowMutipleInstance = false;

        // [JsonProperty]
        // public readonly static GlobalVideoConfig VideoConfig = new GlobalVideoConfig();


        public int MaxStreamCount { get; set; } = 4;

        public static AppModel Default => CreateDefault();

        private static AppModel CreateDefault()
        {
            var @default = new AppModel();

            var defaultStream = new MediaStreamModel()
            {
                Name = $"{Environment.MachineName} (Stream 1)"
            };


            @default.StreamList.Add(defaultStream);
            return @default;
        }

        public List<MediaStreamModel> StreamList { get; set; } = new List<MediaStreamModel>();

        public bool Init()
        {
            logger.Debug("Init()");

			var winVersion = Environment.OSVersion.Version;
			Version minOsVersion = new Version(MinOSVersion);

			if (winVersion < minOsVersion)
			{
				//...

				//logger.Warn($"OS version {winVersion} currently not supported!");
				throw new NotSupportedException($"This version of the operating system currently is not supported.");
			}

            var featueLevel = (int)ScreenHelper.GetDefaultAdapterFeatureLevel();
            if(featueLevel < this.Dx11FeatureLevel)
            {              
                //logger.Warn("DX11 feature level 11.0 is required");
                throw new NotSupportedException($"DirectX feature level 11.0 is required");
            }

            var appVersion = new Version(AppVersion);
			var configVersion = new Version(ConfigVersion);

            logger.Info("Application Version: " + appVersion);
            logger.Info("Configuration Version: " + configVersion);

            if (appVersion != configVersion)
            {
				if(appVersion.Major != configVersion.Major)
				{
					logger.Warn("AppVersion is not the same as ConfigVersion: " + AppVersion + " != " + ConfigVersion);
					//Проверяем совместимость версий...
				}

				//...
			}

            if(StreamList.Count == 0)
            {
				logger.Warn("StreamList.Count == 0");

				StreamList.Add(new MediaStreamModel()
				{
					Name = $"{Environment.MachineName} (Stream 1)"
				});
			}

			if (MaxStreamCount < 1)
			{
				MaxStreamCount = 4;
			}

            if(StreamList.Count > MaxStreamCount)
            {
				logger.Warn("MaxStreamCountLimit: " + StreamList.Count +  " > "  + MaxStreamCount );

				StreamList.RemoveAt(MaxStreamCount-1);
			}

            var videoEncoders = EncoderHelper.GetVideoEncoders();

            if(videoEncoders.Count == 0)
            {
                logger.Warn("videoEncoders.Count == 0");
                //...
            }

            var audioSources = AudioHelper.GetAudioSources();

            if (audioSources.Count == 0)
            {
                logger.Warn("audioSources.Count == 0");
                //...
            }

            var videoSources = ScreenHelper.GetVideoSources();
            if (videoSources.Count == 0)
            {
                logger.Warn("videoSources.Count == 0");
                //...
            }


            foreach (var stream in StreamList)
            {
                stream.Init(videoEncoders, videoSources, audioSources);
            }

            return true;
        }
    }

    public class GlobalVideoConfig
    {
        public int MaxEncoderWidth = 4096;
        public int MaxEncoderHeight = 4096;

        public int MinEncoderWidth = 64;
        public int MinEncoderHeight = 64;

        public int MaxFps  = 60;
        public int MinFps  = 1;
    }


}