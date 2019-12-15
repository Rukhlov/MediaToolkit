using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit
{
    public class MediaToolkitManager
    {
        public static void Startup()
        {
            var winVersion = Environment.OSVersion.Version;
            bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

            if (!isCompatibleOSVersion)
            {
                throw new Exception("Windows versions earlier than 8 are not supported.");
            }

            // TODO:
            // Validate directx, medaiafoundations... 

            SharpDX.MediaFoundation.MediaManager.Startup();

            SharpDX.Configuration.EnableObjectTracking = true;
            SharpDX.Diagnostics.ObjectTracker.StackTraceProvider = null;

            //SharpDX.Configuration.EnableReleaseOnFinalizer = true;
            //SharpDX.Configuration.EnableTrackingReleaseOnFinalizer = false;

            NativeAPIs.WinMM.timeBeginPeriod(1);
        }


        public static void Shutdown()
        {

            NativeAPIs.WinMM.timeEndPeriod(1);

            SharpDX.MediaFoundation.MediaManager.Shutdown();
        }
    }

    public class MediaToolkitBootstrapper : SharedTypes.IMediaToolkitBootstrapper
    {
        public void Startup()
        {
            MediaToolkitManager.Startup();
        }


        public void Shutdown()
        {

            MediaToolkitManager.Shutdown();
        }
    }
}
