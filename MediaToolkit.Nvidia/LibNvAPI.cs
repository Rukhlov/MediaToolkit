using MediaToolkit.Nvidia.NvAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{
	public class LibNvApi
	{
		private static bool _isInitialized = false;

		public static bool Initialize()
		{
			if (_isInitialized)
			{
				return false;
			}

			var status = NvApi.Initialize();

			CheckStatus(status, "Initialize");

			_isInitialized = (status == NvApiStatus.Ok);

			return true;
		}

		public static NvDriverSettingSession CreateDriverSettingSession()
		{
			var status = NvApi.DRS.CreateSession(out var pSession);

			CheckStatus(status, "CreateSession");

			return new NvDriverSettingSession(pSession);
		}

		public static bool Shutdown()
		{
			if (!_isInitialized)
			{
				return false;
			}

			var status = NvApi.Unload();

			CheckStatus(status, "Unload");

			return true;
		}

		public static void CheckStatus(NvApiStatus status, string callerName = "")
		{
			if (status != NvApiStatus.Ok)
			{
				string description = "";
				if (_isInitialized)
				{
					NvApi.GetErrorMessage(status, out description);
				}
				
				throw new LibNvApiException(callerName, description, status);
			}
		}
	}



	public class NvDriverSettingSession
	{
		private readonly DRSSessionHandle sessionHandle;
		internal NvDriverSettingSession(DRSSessionHandle handle)
		{
			this.sessionHandle = handle;
		}

		public void Load()
		{
			var status = NvApi.DRS.LoadSettings(sessionHandle);

			LibNvApi.CheckStatus(status, "LoadSettings");
		}

		public void Save()
		{
			var status = NvApi.DRS.SaveSettings(sessionHandle);
			LibNvApi.CheckStatus(status, "SaveSettings");
		}

        public NvDriverSettingProfile CreateProfile(string name)
        {
            DRSProfile p = new DRSProfile
            {
                version = NvApi.MakeVersion<DRSProfile>(1),
                profileName = name,
            };

            return CreateProfile(p);
        }

        public NvDriverSettingProfile CreateProfile(DRSProfile profile)
		{
			var status = NvApi.DRS.CreateProfile(sessionHandle, profile, out var hProfile);
			LibNvApi.CheckStatus(status, "CreateProfile");

			return new NvDriverSettingProfile(hProfile, sessionHandle);
		}

		public NvDriverSettingProfile GetBaseProfile()
		{
			var status = NvApi.DRS.GetBaseProfile(sessionHandle, out var hProfile);
			LibNvApi.CheckStatus(status, "GetBaseProfile");

			return new NvDriverSettingProfile(hProfile, sessionHandle);
		}

		public NvDriverSettingProfile FindProfileByName(string name)
		{
			var status = NvApi.DRS.FindProfileByName(sessionHandle, name, out var hProfile);

            if(status == NvApiStatus.ProfileNotFound)
            {
                return null;
            }

			LibNvApi.CheckStatus(status, "FindProfileByName");

			return new NvDriverSettingProfile(hProfile, sessionHandle, name);
		}


		public void Close()
		{
			var handle = sessionHandle.Handle;
			if (handle != IntPtr.Zero)
			{
				var status = NvApi.DRS.DestroySession(sessionHandle);
				LibNvApi.CheckStatus(status, "DestroySession");
			}
		}
	}

	public class NvDriverSettingProfile
	{
		private readonly DRSProfileHandle profileHandle;
		private readonly DRSSessionHandle sessionHandle;
		internal NvDriverSettingProfile(DRSProfileHandle hProfile, DRSSessionHandle hSession, string name = "")
		{
			this.profileHandle = hProfile;
			this.sessionHandle = hSession;
			this.Name = name;
		}

		public string Name { get; private set; }

		public void SetSetting(DRSSettingV1 setting)
		{
			var status = NvApi.DRS.SetSetting(sessionHandle, profileHandle, ref setting);
			LibNvApi.CheckStatus(status, "SetSetting");
		}

        public DRSSettingV1 GetSetting(uint settingId)
        {
            DRSSettingV1 setting = new DRSSettingV1
            {
                version = NvApi.MakeVersion<DRSSettingV1>(1),
            };

            var status = NvApi.DRS.GetSetting(sessionHandle, profileHandle, settingId, ref setting);
            LibNvApi.CheckStatus(status, "GetSetting");

            return setting;
        }

        public DRSSettingV1[] EnumSettings(int startIndex = 0, int settingsCount = 512)
        {
            DRSSettingV1[] settings = new DRSSettingV1[settingsCount];
            settings[0].version = NvApi.MakeVersion<DRSSettingV1>(1);

            var status = NvApi.DRS.EnumSettings(sessionHandle, profileHandle, (uint)startIndex, ref settings);
            LibNvApi.CheckStatus(status, "EnumSettings");

            return settings;
        }

        public T CreateApplication<T>(string name) where T : DRSApplicationV1, new()
        {
            var _app = new T
            {
                appName = name,
            };

            return CreateApplication(_app);
        }

        public T CreateApplication<T>(T app) where T : DRSApplicationV1, new()
		{
			var status = NvApi.DRS.CreateApplication(sessionHandle, profileHandle, app);
			LibNvApi.CheckStatus(status, "CreateApplication");

			return app;
		}


		public T GetApplicationInfo<T> (string appName) where T: DRSApplicationV1, new()
		{
			T app = new T();

			var status = NvApi.DRS.GetApplicationInfo(sessionHandle, profileHandle, appName, ref app);
            if(status == NvApiStatus.ExecutableNotFound)
            {
                return null;
            }

			LibNvApi.CheckStatus(status, "GetApplicationInfo");

			return app;
		}

	}


}
