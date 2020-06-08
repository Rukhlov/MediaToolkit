using MediaToolkit;
//using NAudio.CoreAudioApi;
using ScreenStreamer.Wpf.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Helpers
{
    public static class AudioHelper
    {
        //public static List<MMDevice> GetMultiMediaDevices()
        //{
        //    MMDeviceEnumerator names = new MMDeviceEnumerator();
        //    return names.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
        //                .Cast<MMDevice>()
        //                .ToList();
        //}

        public static List<AudioDeviceViewModel> GetMultiMediaDeviceViewModels()
        {

            var audioDevices = AudioTool.GetAudioCaptureDevices();

            return audioDevices.Select(d => new AudioDeviceViewModel(d)).ToList();

            //return GetMultiMediaDevices()
            //    .Select(mmd => new AudioDeviceViewModel { DeviceId = mmd.ID, DisplayName = mmd.FriendlyName })
            //    .ToList();
        }
    }
}
