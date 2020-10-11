using MediaToolkit.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit
{
    public class MediaToolkitManager
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit");

        public static void Startup()
        {
            logger.Debug("MediaToolkitManager::Startup()");

            var winVersion = Environment.OSVersion.Version;
            bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

            if (!isCompatibleOSVersion)
            {
               // throw new Exception("Windows versions earlier than 8 are not supported.");
            }


            //NativeAPIs.Kernel32.SetDllDirectory(".\\libFFmpeg");

            // TODO:
            // Validate directx, medaiafoundations... 

            SharpDX.MediaFoundation.MediaManager.Startup();

#if DEBUG
            // Debug
            SharpDX.Configuration.EnableObjectTracking = true;
            SharpDX.Diagnostics.ObjectTracker.StackTraceProvider = null;

            SharpDX.Configuration.EnableTrackingReleaseOnFinalizer = true;
#else

			
			//Release
			SharpDX.Configuration.EnableObjectTracking = false;
			
#endif

            NativeAPIs.WinMM.timeBeginPeriod(1);
        }


        public static void Shutdown()
        {
            logger.Debug("MediaToolkitManager::Shutdown()");

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
