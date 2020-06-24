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
    public class MainModel
    {
        public int MaxStreamCount { get; set; } = 3;

        public static MainModel Default => CreateDefault();

        private static MainModel CreateDefault()
        {
            var defaultStream = new MediaStreamModel()
            {
                Name = $"{Environment.MachineName} (Stream 1)"
            };

           // defaultStream.Init();


            var @default = new MainModel();
            @default.StreamList.Add(defaultStream);
            return @default;
        }

        public List<MediaStreamModel> StreamList { get; set; } = new List<MediaStreamModel>();
    }



}