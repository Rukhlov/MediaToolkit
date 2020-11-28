using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia.NvAPI
{
    public class NvAPI
    {
		//#define NVAPI_GENERIC_STRING_MAX   4096
		public const uint GentricStringMax = 4096;

		//#define NVAPI_LONG_STRING_MAX   256
		public const uint LongStringMax = 256;

		//#define NVAPI_SHORT_STRING_MAX   64
		public const uint ShortStringMax = 64;

		public const uint UnicodeMaxString = 2048;

		public const uint SettingsMaxValue = 100;

		private const string _path32 = "nvapi.dll";
        private const string _path64 = "nvapi64.dll";


		//#define 	MAKE_NVAPI_VERSION(typeName, ver)   (NvU32)(sizeof(typeName) | ((ver)<<16))
		public static uint MakeVersion<T>(int version)
        {
            return (uint)((Marshal.SizeOf(typeof(T))) | (int)(version << 16));
        }

        [DllImport(_path32, EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        internal static extern IntPtr NvAPI32_QueryInterface(uint interfaceId);

        [DllImport(_path64, EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
		internal static extern IntPtr NvAPI64_QueryInterface(uint interfaceId);

        public static IntPtr QueryInterface(uint interfaceId)
        {
            return Environment.Is64BitProcess ?
                NvAPI64_QueryInterface(interfaceId) :
                NvAPI32_QueryInterface(interfaceId);
        }

        public static NvApiStatus Initialize()
        {
            return DelegateFactory.GetDelegate<NvAPI_Initialize>().Invoke();
        }

        public static NvApiStatus Unload()
        {
            return DelegateFactory.GetDelegate<NvAPI_Unload>().Invoke();
        }

		public static NvApiStatus GetInterfaceVersionString(out string version)
		{
			var sb = new StringBuilder(64);
			var status = DelegateFactory.GetDelegate<NvAPI_GetInterfaceVersionString>().Invoke(sb);
			version =  sb.ToString();
			return status;
		}

		public static NvApiStatus GetErrorMessage(NvApiStatus _status, out string message)
		{
			var sb = new StringBuilder(64);
			var status = DelegateFactory.GetDelegate<NvAPI_GetErrorMessage>().Invoke(_status, sb);
			message = sb.ToString();
			return status;
		}

		public static class SYS
		{
			public static NvApiStatus GetDriverAndBranchVersion(out uint driverVersion, out string buildString)
			{
				var sb = new StringBuilder(64);
				var status = DelegateFactory.GetDelegate<NvAPI_SYS_GetDriverAndBranchVersion>().Invoke(out driverVersion, sb);
				buildString = sb.ToString();
				return status;
			}

			public static NvApiStatus GetChipSetInfo<T>(ref T t) where T : ChipsetInfoV1, new()
			{
				NvApiStatus status = NvApiStatus.Error;
				var size = Marshal.SizeOf(t);
				IntPtr ptr = Marshal.AllocHGlobal(size);
				try
				{
					Marshal.StructureToPtr(t, ptr, false);
					status = DelegateFactory.GetDelegate<NvAPI_SYS_GetChipSetInfo>().Invoke(ptr);
					if (status == NvApiStatus.Ok)
					{
						T newApp = new T();
						Marshal.PtrToStructure(ptr, newApp);
						t = newApp;
					}
				}
				finally
				{
					Marshal.FreeHGlobal(ptr);
				}

				return status;			
			}
		}

        public static class DRS
        {
            public static NvApiStatus CreateSession(out DRSSessionHandle hSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_CreateSession>().Invoke(out hSession);
            }

            public static NvApiStatus DestroySession(DRSSessionHandle hSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_DestroySession>().Invoke(hSession);
            }

            public static NvApiStatus LoadSettings(DRSSessionHandle hSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_LoadSettings>().Invoke(hSession);
            }

            public static NvApiStatus GetBaseProfile(DRSSessionHandle hSession, out DRSProfileHandle hProfile)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_GetBaseProfile>().Invoke(hSession, out hProfile);
            }

            public static NvApiStatus FindProfileByName(DRSSessionHandle hSession, string profileName, out DRSProfileHandle profile)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_FindProfileByName>().Invoke(hSession, new StringBuilder(profileName), out profile);
            }

            public static NvApiStatus CreateProfile(DRSSessionHandle hSession, DRSProfile profile, out DRSProfileHandle hProfile)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_CreateProfile>().Invoke(hSession, ref profile, out hProfile);
            }

			public static NvApiStatus GetApplicationInfo<T>(DRSSessionHandle hSession, DRSProfileHandle hProfile, string appName, ref T t) 
				where T : DRSApplicationV1, new()
			{
				NvApiStatus status = NvApiStatus.Error;
				var size = Marshal.SizeOf(t);
				IntPtr ptr = Marshal.AllocHGlobal(size);
				try
				{
					var buf = new StringBuilder(appName);
					Marshal.StructureToPtr(t, ptr, true);
					var getApplicationInfo = DelegateFactory.GetDelegate<NvAPI_DRS_GetApplicationInfo>();
					status = getApplicationInfo(hSession, hProfile, buf, ptr);

					if(status == NvApiStatus.Ok)
					{
						T newApp = new T();
						Marshal.PtrToStructure(ptr, newApp);
						t = newApp;
					}
				}
				finally
				{
					Marshal.FreeHGlobal(ptr);
				}

				return status;

			}

			public static NvApiStatus CreateApplication<T>(DRSSessionHandle hSession, DRSProfileHandle hProfile, T t)
				where T : DRSApplicationV1
			{
				NvApiStatus status = NvApiStatus.Error;
				var size = Marshal.SizeOf(t);
				IntPtr ptr = Marshal.AllocHGlobal(size);
				try
				{
					Marshal.StructureToPtr(t, ptr, true);
					var createApplication = DelegateFactory.GetDelegate<NvAPI_DRS_CreateApplication>();
					status = createApplication(hSession, hProfile, ptr);
				}
				finally
				{
					Marshal.FreeHGlobal(ptr);
				}

				return status;
			}


            public static NvApiStatus SetSetting(DRSSessionHandle hSession, DRSProfileHandle hProfile, ref DRSSetting pSetting)
            {
                return DelegateFactory.GetDelegate<NvAPI_RS_SetSettingDelegate>().Invoke(hSession, hProfile, ref pSetting);
            }

            public static NvApiStatus SaveSettings(DRSSessionHandle hSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_SaveSettings>().Invoke(hSession);
            }

        }

    }



    public class DRSSession
    {
        private readonly DRSSessionHandle sessionHandle;
        internal DRSSession(DRSSessionHandle handle)
        {
            this.sessionHandle = handle;
        }

        public static DRSSession CreateSession()
        {
            var status = NvAPI.DRS.CreateSession(out var pSession);
            if (status != NvApiStatus.Ok)
            {
                //...
            }

            return new DRSSession(pSession);
        }

        public void Close()
        {
            var handle = sessionHandle.Handle;
            if (handle != IntPtr.Zero)
            {
                var status = NvAPI.DRS.DestroySession(sessionHandle);

                if (status != NvApiStatus.Ok)
                {
                    //...
                }
            }
        }
    }



    internal static class DelegateFactory
    {
        private static readonly Dictionary<KeyValuePair<FunctionId, Type>, object> Delegates = new Dictionary<KeyValuePair<FunctionId, Type>, object>();

        public static T GetDelegate<T>() where T : class
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
            {
                throw new InvalidOperationException($"{typeof(T).Name} is not a delegate type");
            }

            var functionId = typeof(T).GetCustomAttributes(typeof(FunctionIdAttribute), true)
                .Cast<FunctionIdAttribute>()
                .FirstOrDefault();

            if (functionId == null)
            {
                throw new InvalidOperationException($"{typeof(T).Name}'s address is unknown.");
            }

            var delegateKey = new KeyValuePair<FunctionId, Type>(functionId.FunctionId, typeof(T));

            lock (Delegates)
            {
                if (Delegates.ContainsKey(delegateKey))
                {
                    return Delegates[delegateKey] as T;
                }

                var ptr = NvAPI.QueryInterface((uint)functionId.FunctionId);

                if (ptr != IntPtr.Zero)
                {
                    var delegateValue = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
                    Delegates.Add(delegateKey, delegateValue);

                    return delegateValue;
                }
            }

            throw new NotSupportedException(@"Function identification number is invalid or not supported.");
        }

    }

    [AttributeUsage(AttributeTargets.Delegate)]
    internal class FunctionIdAttribute : Attribute
    {
        public FunctionIdAttribute(FunctionId functionId)
        {
            FunctionId = functionId;
        }

        public readonly FunctionId FunctionId;
    }
}
