using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit.NativeAPIs.Utils;
using MediaToolkit.Nvidia;
using MediaToolkit.Nvidia.NvAPI;

namespace Test.Encoder
{
    class NvApiTest
    {
        public static void Run()
        {
            Console.WriteLine("NvApiTest::Run() BEGIN");

            var res = NvApi.Initialize();

            Console.WriteLine("NvAPI_Initialize() " + res);

			res = NvApi.GetInterfaceVersionString(out var version);
			Console.WriteLine("GetInterfaceVersionString() " + res + " " + version);

			res = NvApi.SYS.GetDriverAndBranchVersion(out var driverVersion, out var buildString);
			Console.WriteLine("GetDriverAndBranchVersion() " + res + " " + driverVersion + " " + buildString);


			ChipsetInfoV4 info = new ChipsetInfoV4();

			res = NvApi.SYS.GetChipSetInfo(ref info);
			Console.WriteLine("GetChipSetInfo() " + res + " " + info);

			res = NvApi.GetErrorMessage(res, out var message);
			Console.WriteLine("GetErrorMessage() " + res + " " + message);


			var status = NvApi.DRS.CreateSession(out var phSession);

            Console.WriteLine("NvAPI_DRS_CreateSession() " + status + " " + phSession);

            status = NvApi.DRS.LoadSettings(phSession);
            Console.WriteLine("DRS_LoadSettings() " + status + " " + phSession);


            //DRSApplicationV2 app = new DRSApplicationV2
            //{
            //    appName = "TEST6.exe",

            //};

            status = NvApi.DRS.FindApplicationByName<DRSApplicationV2>(phSession, "TEST.exe", out var profileHanle, out var _app);


            var profileName = "TEST5";
            status = NvApi.DRS.FindProfileByName(phSession, profileName, out var hProfile);
            Console.WriteLine("DRS_FindProfileByName() " + status + " " + phSession);

            //status = nvapi.DRS_GetBaseProfile(phSession, out var hProfile);
            //Console.WriteLine("DRS_GetBaseProfile() " + status + " " + phSession);

            if (status == NvApiStatus.ProfileNotFound)
            {
                //uint version = 1;
                //var _version =  (uint)(Marshal.SizeOf(typeof(DRSProfile)) | (version << 16));
                DRSProfile prof = new DRSProfile
                {
                    version = NvApi.MakeVersion<DRSProfile>(1),
                    profileName = profileName,
                };

                status = NvApi.DRS.CreateProfile(phSession, prof, out hProfile);
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
			status = NvApi.DRS.GetApplicationInfo(phSession, hProfile, app.appName, ref app);

			//status = NvAPI.DRS._CreateApplication(phSession, hProfile, app);

			if (status == NvApiStatus.ExecutableNotFound)
            {
                status = NvApi.DRS.CreateApplication(phSession, hProfile, app);
            }
            else if(status != NvApiStatus.Ok)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!! DRS_GetApplicationInfo() " + status);
            }
            

            Console.WriteLine("DRS_CreateApplication() " + status + " " + phSession);

			var settingVersion = NvApi.MakeVersion<DRSSettingV1>(1);
			var setting1 = new DRSSettingV1
            {
                version = settingVersion,
                settingId = (uint)ESettingId.ShimMCCOMPAT,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)ShimMCCOMPAT.Integrated,
                    //dwordValue = (uint)ShimMCCOMPAT.Integrated,
                }
            };

            status = NvApi.DRS.SetSetting(phSession, hProfile, ref setting1);
            Console.WriteLine("DRS_SetSetting() 1" + status + " " + phSession);


            var setting2 = new DRSSettingV1
            {
                version = settingVersion,
                settingId = (uint)ESettingId.ShimMCCOMPAT,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)ShimMCCOMPAT.Integrated,
                    //dwordValue = (uint)ShimRenderingMode.Integrated,
                },

            };

            status = NvApi.DRS.SetSetting(phSession, hProfile, ref setting2);
            Console.WriteLine("DRS_SetSetting() 2" + status + " " + phSession);


            var setting3 = new DRSSettingV1
            {
                version = settingVersion,
                settingId = (uint)ESettingId.ShimRenderingOptions,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)(ShimRenderingOptions.DefaultRenderingMode |
                    ShimRenderingOptions.IGPUTranscoding)

                    //dwordValue = (uint)(ShimRenderingOptions.DefaultRenderingMode |
                    //ShimRenderingOptions.IGPUTranscoding),
                },

            };

            status = NvApi.DRS.SetSetting(phSession, hProfile, ref setting3);
            Console.WriteLine("DRS_SetSetting() 3" + status + " " + phSession);

            status = NvApi.DRS.SaveSettings(phSession);
            Console.WriteLine("DRS_SaveSettings() 3" + status);


            status = NvApi.DRS.DestroySession(phSession);

            Console.WriteLine("NvAPI_DRS_DestroySession() " + status + " " + phSession);

            res = NvApi.Unload();

            Console.WriteLine("NvAPI_Unload() " + res);

            Console.WriteLine("NvApiTest::Run() END");
        }

        public static void Run3(string profileName)
        {
            Console.WriteLine("NvApiTest::Run3() BEGIN");
            LibNvApi.Initialize();
            var session = LibNvApi.CreateDriverSettingSession();

            session.LoadSettings();

            var profiles = session.GetProfiles();
            StringBuilder sb = new StringBuilder();
            int num = 0;
            foreach(var profile in profiles)
            {
                var info = profile.GetProfileInfo();
               // if (info.isPredefined == 0)
                {
                    var profStr = (++num) + " " + info.profileName + " (" + info.isPredefined + " " + info.gpuSupport + " " + info.numOfApps + " " + info.numOfSettings + ")";
                    //Console.WriteLine(profStr);
                    sb.AppendLine(profStr);
                    //var profile = session.FindProfileByName(profileName);
                    var apps = profile.GetApplications<DRSApplicationV4>();
                    foreach (var a in apps)
                    {
                       
                        //if (!string.IsNullOrEmpty(a.launcher))
                        {
                            var appStr = string.Join(" ", a.appName, a.userFriendlyName, a.launcher, a.fileInFolder, a.isPredefined);
                            //Console.WriteLine(appStr);
                            sb.AppendLine(appStr);
                        }
                    }

                    sb.AppendLine();

                    //if(info.isPredefined == 0)
                    //{
                    //    var settings = profile.GetSettings();
                    //    foreach (var s in settings)
                    //    {
                    //        if (s.isCurrentPredefined == 1)
                    //        {
                    //            var setStr = string.Join(" ", s.settingId, s.settingName, s.currentValue.stringValue);
                    //            sb.AppendLine(setStr);
                    //        }

                    //    }


                    //}

                    //Console.WriteLine("--------------------------");
                    sb.AppendLine("-----------------------");
                }

               // Console.WriteLine("");
            }

            File.WriteAllText("log.txt", sb.ToString());

            Console.WriteLine("NvApiTest::Run3() END");
        }

        public static void SetupNvOptimusProfile(string profileName, string appName, bool forceIntegrated = true)
        {
            Console.WriteLine("NvApiTest::SetupNvOptimusProfile() BEGIN");

            Console.WriteLine(profileName + " " + appName + " " + forceIntegrated);

            LibNvApi.Initialize();
            try
            {
                var session = LibNvApi.CreateDriverSettingSession();
                try
                {
                    session.LoadSettings();

                    //var profile = session.FindProfileByName(profileName);

                    var profile = session.FindProfileByApplicationName(appName, out DRSApplicationV4 app);
                    if (profile != null)
                    {
                        var apps = profile.GetApplications<DRSApplicationV1>();
                    }

                    if (profile == null)
                    {//ExecutableNotFound

                        profile = session.CreateProfile(profileName);
                        if(profile == null)
                        {// ProfileNameInUse
                            profile = session.FindProfileByName(profileName);
                        }

                        var friendlyName = System.IO.Path.GetFileName(appName);
                        var appSettings = new DRSApplicationV4
                        {
                            appName = appName,
                            userFriendlyName = friendlyName,                          
                        };

                        app = profile.CreateApplication(appSettings);

                        if (app == null)
                        {// ExecutableAlreadyInUse
                           /// ???
                        }
  
                    }
                    
                    //profile.Delete();
                    //session.SaveSettings();
                    //return;

                    var settingVersion = NvApi.MakeVersion<DRSSettingV1>(1);
                    var setting1 = new DRSSettingV1
                    {
                        version = settingVersion,
                        settingId = (uint)ESettingId.ShimMCCOMPAT,
                        settingType = DRSSettingType.DWORD,
                    };

                    var setting2 = new DRSSettingV1
                    {
                        version = settingVersion,
                        settingId = (uint)ESettingId.ShimRenderingMode,
                        settingType = DRSSettingType.DWORD,
                    };

                    var setting3 = new DRSSettingV1
                    {
                        version = settingVersion,
                        settingId = (uint)ESettingId.ShimRenderingOptions,
                        settingType = DRSSettingType.DWORD,
                    };

                    if (forceIntegrated)
                    {
                        setting1.currentValue = new DRSSettingUnion
                        {
                            dwordValue = (uint)ShimMCCOMPAT.Integrated,
                        };
                        setting2.currentValue = new DRSSettingUnion
                        {
                            dwordValue = (uint)ShimRenderingMode.Integrated,
                        };
                        setting3.currentValue = new DRSSettingUnion
                        {
                            dwordValue = (uint)(ShimRenderingOptions.DefaultRenderingMode |
                            ShimRenderingOptions.IGPUTranscoding)
                        };
                    }
                    else
                    {
                        setting1.currentValue = new DRSSettingUnion
                        {
                            dwordValue = (uint)ShimMCCOMPAT.Enable,
                        };
                        setting2.currentValue = new DRSSettingUnion
                        {
                            dwordValue = (uint)ShimRenderingMode.Enable,
                        };
                        setting3.currentValue = new DRSSettingUnion
                        {
                            dwordValue = (uint)ShimRenderingOptions.DefaultRenderingMode,
                        };
                    }

                    profile.SetSetting(setting1);
                    profile.SetSetting(setting2);
                    profile.SetSetting(setting3);

                    session.SaveSettings();

                    var settings = profile.GetSettings();
                    foreach(var s in settings)
                    {
                        Console.WriteLine(s.settingId + " " + s.settingName + " " + s.currentValue.dwordValue);
                    }


                }
                finally
                {
                    if (session != null)
                    {
                        session.Close();
                        session = null;
                             
                    }
                    
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //throw;
            }
            finally
            {
                LibNvApi.Shutdown();
            }

            Console.WriteLine("NvApiTest::SetupNvOptimusProfile() BEGIN");
        }

        public static void Run2()
        {
            Console.WriteLine("NvApiTest::Run2() BEGIN");

            LibNvApi.Initialize();


            var session = LibNvApi.CreateDriverSettingSession();
            session.LoadSettings();


            //var _profile = session.FindProfileByApplicationName("TEST.exe");

            //var profileInfo = _profile.GetProfileInfo();



            var profileName = "TEST7";
            var profile = session.FindProfileByName(profileName);

            if(profile == null)
            {
                profile = session.CreateProfile(profileName);
            }

            string appName = "TEST7.exe";
            DRSApplicationV1 app = profile.GetApplicationInfo<DRSApplicationV1>(appName);

            if(app == null)
            {
                app = profile.CreateApplication<DRSApplicationV1>(appName);
            }

            var settings = profile.GetSettings();


            var setting = profile.GetSetting((uint)ESettingId.ShimMCCOMPAT);

            //setting = profile.GetSetting((uint)ESettingId.ShimRenderingOptions);

            var settingVersion = NvApi.MakeVersion<DRSSettingV1>(1);
            var setting1 = new DRSSettingV1
            {
                version = settingVersion,
                settingId = (uint)ESettingId.ShimMCCOMPAT,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)ShimMCCOMPAT.Integrated,
                }
            };

            profile.SetSetting(setting1);

            var setting2 = new DRSSettingV1
            {
                version = settingVersion,
                settingId = (uint)ESettingId.ShimRenderingMode,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)ShimRenderingMode.Integrated,
                },
            };

            profile.SetSetting(setting2);

            var setting3 = new DRSSettingV1
            {
                version = settingVersion,
                settingId = (uint)ESettingId.ShimRenderingOptions,
                settingType = DRSSettingType.DWORD,
                currentValue = new DRSSettingUnion
                {
                    dwordValue = (uint)(ShimRenderingOptions.DefaultRenderingMode |
                    ShimRenderingOptions.IGPUTranscoding)
                },

            };

            profile.SetSetting(setting3);

            session.SaveSettings();

            session.Close();

            LibNvApi.Shutdown();


            Console.WriteLine("NvApiTest::Run() END");
        }


    }
}
