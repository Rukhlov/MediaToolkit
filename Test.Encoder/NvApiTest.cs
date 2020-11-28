using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit.Nvidia.NvAPI;

namespace Test.Encoder
{
    class NvApiTest
    {
        public static void Run()
        {
            Console.WriteLine("NvApiTest::Run() BEGIN");

            var res = NvAPI.Initialize();

            Console.WriteLine("NvAPI_Initialize() " + res);

			res = NvAPI.GetInterfaceVersionString(out var version);
			Console.WriteLine("GetInterfaceVersionString() " + res + " " + version);

			res = NvAPI.SYS.GetDriverAndBranchVersion(out var driverVersion, out var buildString);
			Console.WriteLine("GetDriverAndBranchVersion() " + res + " " + driverVersion + " " + buildString);


			ChipsetInfoV4 info = new ChipsetInfoV4();

			res = NvAPI.SYS.GetChipSetInfo(ref info);
			Console.WriteLine("GetChipSetInfo() " + res + " " + info);

			res = NvAPI.GetErrorMessage(res, out var message);
			Console.WriteLine("GetErrorMessage() " + res + " " + message);


			var status = NvAPI.DRS.CreateSession(out var phSession);

            Console.WriteLine("NvAPI_DRS_CreateSession() " + status + " " + phSession);

            status = NvAPI.DRS.LoadSettings(phSession);
            Console.WriteLine("DRS_LoadSettings() " + status + " " + phSession);

            var profileName = "TEST5";
            status = NvAPI.DRS.FindProfileByName(phSession, profileName, out var hProfile);
            Console.WriteLine("DRS_FindProfileByName() " + status + " " + phSession);

            //status = nvapi.DRS_GetBaseProfile(phSession, out var hProfile);
            //Console.WriteLine("DRS_GetBaseProfile() " + status + " " + phSession);

            if (status == NvApiStatus.ProfileNotFound)
            {
                //uint version = 1;
                //var _version =  (uint)(Marshal.SizeOf(typeof(DRSProfile)) | (version << 16));
                DRSProfile prof = new DRSProfile
                {
                    version = NvAPI.MakeVersion<DRSProfile>(1),
                    profileName = profileName,
                };

                status = NvAPI.DRS.CreateProfile(phSession, prof, out hProfile);
                Console.WriteLine("DRS_CreateProfile() " + status + " " + phSession);
            }
            else if( status != NvApiStatus.Ok)
            {

            }



            if (status != NvApiStatus.Ok)
            {
                Console.WriteLine("!!!!!!!!!!! DRS_CreateProfile() " + status );
            }


			DRSApplicationV2 app = new DRSApplicationV2
			{
                appName = "TEST6.exe",
				
            };

			//status = NvAPI.DRS.CreateApplication(phSession, hProfile, ref _app);
			status = NvAPI.DRS.GetApplicationInfo(phSession, hProfile, app.appName, ref app);

			//status = NvAPI.DRS._CreateApplication(phSession, hProfile, app);

			if (status == NvApiStatus.ExecutableNotFound)
            {
                status = NvAPI.DRS.CreateApplication(phSession, hProfile, app);
            }
            else if(status != NvApiStatus.Ok)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!! DRS_GetApplicationInfo() " + status);
            }
            

            Console.WriteLine("DRS_CreateApplication() " + status + " " + phSession);

			var settingVersion = NvAPI.MakeVersion<DRSSetting>(1);
			var setting1 = new DRSSetting
            {
                version = settingVersion,
                settingId = (uint)ESetting.SHIM_MCCOMPAT_ID,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)ShimMCCOMPAT.Integrated,
                }
            };

            status = NvAPI.DRS.SetSetting(phSession, hProfile, ref setting1);
            Console.WriteLine("DRS_SetSetting() 1" + status + " " + phSession);


            var setting2 = new DRSSetting
            {
                version = settingVersion,
                settingId = (uint)ESetting.SHIM_MCCOMPAT_ID,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)ShimRenderingMode.Integrated,
                },

            };

            status = NvAPI.DRS.SetSetting(phSession, hProfile, ref setting2);
            Console.WriteLine("DRS_SetSetting() 2" + status + " " + phSession);


            var setting3 = new DRSSetting
            {
                version = settingVersion,
                settingId = (uint)ESetting.SHIM_RENDERING_OPTIONS_ID,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)(ShimRenderingOptions.DefaultRenderingMode |
                    ShimRenderingOptions.IGPUTranscoding),
                },

            };

            status = NvAPI.DRS.SetSetting(phSession, hProfile, ref setting3);
            Console.WriteLine("DRS_SetSetting() 3" + status + " " + phSession);

            status = NvAPI.DRS.SaveSettings(phSession);
            Console.WriteLine("DRS_SaveSettings() 3" + status);






            status = NvAPI.DRS.DestroySession(phSession);

            Console.WriteLine("NvAPI_DRS_DestroySession() " + status + " " + phSession);

            res = NvAPI.Unload();

            Console.WriteLine("NvAPI_Unload() " + res);

            Console.WriteLine("NvApiTest::Run() END");
        }
    }
}
