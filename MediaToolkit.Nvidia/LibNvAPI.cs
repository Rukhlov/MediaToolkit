using MediaToolkit.Nvidia.NvAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{
	class LibNvApi
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

		public void SetSetting(DRSSetting setting)
		{
			var status = NvApi.DRS.SetSetting(sessionHandle, profileHandle, ref setting);
			LibNvApi.CheckStatus(status, "SetSetting");
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
			LibNvApi.CheckStatus(status, "GetApplicationInfo");

			return app;
		}

	}


}
