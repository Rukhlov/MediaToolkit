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

            var status = NvAPI.DRS.CreateSession(out var phSession);

            Console.WriteLine("NvAPI_DRS_CreateSession() " + status + " " + phSession);

            status = NvAPI.DRS.LoadSettings(phSession);
            Console.WriteLine("DRS_LoadSettings() " + status + " " + phSession);

            var profileName = "TEST4";
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
                    version = NvAPI.NVDRS_PROFILE_VER,
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


            DRSApplicationV1 app = new DRSApplicationV1
            {
                version = NvAPI.NVDRS_APPLICATION_VER_V1,
                appName = "TEST4.exe",
            };

            status = NvAPI.DRS.GetApplicationInfo(phSession, hProfile, app.appName, ref app);

            if(status == NvApiStatus.ExecutableNotFound)
            {
                status = NvAPI.DRS.CreateApplication(phSession, hProfile, ref app);
            }
            else if(status != NvApiStatus.Ok)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!! DRS_GetApplicationInfo() " + status);
            }
            

            Console.WriteLine("DRS_CreateApplication() " + status + " " + phSession);


            var setting1 = new DRSSetting
            {
                version = NvAPI.NVDRS_SETTING_VER,
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
                version = NvAPI.NVDRS_SETTING_VER,
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
                version = NvAPI.NVDRS_SETTING_VER,
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
