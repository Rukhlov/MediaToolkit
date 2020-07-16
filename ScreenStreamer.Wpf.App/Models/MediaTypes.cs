using MediaToolkit;
using MediaToolkit.Core;
using Newtonsoft.Json;
using ScreenStreamer.Wpf.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf
{

    public enum QualityPreset
    {
        Low,
        Standard,
        High
    }


    public enum ProtocolKind
    {
        TCP = 6,
        UDP = 17
    }


}
