using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using MediaToolkit.Core;
using Newtonsoft.Json;
using NLog;
using ScreenStreamer.Common;

using ScreenStreamer.Wpf.Common.Helpers;
using System.Linq;


namespace ScreenStreamer.Wpf
{
    public class AppModel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public readonly static string AppVersion = "1.0.0.0";
        public string ConfigVersion { get; set; } = "1.0.0.0";

        [JsonProperty]
        public readonly static string MinOSVersion = "6.2";

        public int MaxStreamCount { get; set; } = 3;

        public static AppModel Default => CreateDefault();

        private static AppModel CreateDefault()
        {
            var defaultStream = new MediaStreamModel()
            {
                Name = $"{Environment.MachineName} (Stream 1)"
            };

           // defaultStream.Init();


            var @default = new AppModel();
            @default.StreamList.Add(defaultStream);
            return @default;
        }

  
        public List<MediaStreamModel> StreamList { get; set; } = new List<MediaStreamModel>();


        public bool Validate()
        {
            logger.Debug("Validate()");

            if (AppVersion != ConfigVersion)
            {
                logger.Warn("AppVersion is not the same as ConfigVersion: " + AppVersion + " != " + ConfigVersion);
                //...
            }

            if(StreamList.Count == 0)
            {
                //...
            }

            if(StreamList.Count > MaxStreamCount)
            {
               //...
            } 

            foreach(var stream in StreamList)
            {
                stream.Validate();
            }

            return true;
        }
    }



}