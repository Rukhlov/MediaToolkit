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

        [JsonProperty]
        public readonly static string MinOSVersion = "6.2";

        //[JsonProperty]
        public readonly static bool AllowMutipleInstance = false;

       // [JsonProperty]
       // public readonly static GlobalVideoConfig VideoConfig = new GlobalVideoConfig();


        public int MaxStreamCount { get; set; } = 3;

        public static AppModel Default => CreateDefault();

        private static AppModel CreateDefault()
        {
            var defaultStream = new MediaStreamModel()
            {
                Name = $"{Environment.MachineName} (Stream 1)"
            };

            var @default = new AppModel();
            @default.StreamList.Add(defaultStream);
            return @default;
        }

        public List<MediaStreamModel> StreamList { get; set; } = new List<MediaStreamModel>();

        public bool Validate()
        {
            logger.Debug("Validate()");

			var winVersion = Environment.OSVersion.Version;
			Version minOsVersion = new Version(MinOSVersion);

			if (winVersion < minOsVersion)
			{
				//...

				logger.Warn($"OS version {winVersion} currently not supported!");
				//throw new NotSupportedException($"This version of the operating system currently is not supported.");
			}

			var appVersion = new Version(AppVersion);
			var configVersion = new Version(ConfigVersion);

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
				MaxStreamCount = 3;
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
                stream.Validate(videoEncoders, videoSources, audioSources);
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