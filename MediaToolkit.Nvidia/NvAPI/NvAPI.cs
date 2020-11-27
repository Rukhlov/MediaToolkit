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
        public const uint NVAPI_UNICODE_STRING_MAX = 2048;

        private const string _path32 = "nvapi.dll";
        private const string _path64 = "nvapi64.dll";

        public static uint NVDRS_PROFILE_VER = MakeVersion<DRSProfile>(1);
        public const uint NVAPI_SETTING_MAX_VALUES = 100;

        public static readonly uint NVDRS_SETTING_VALUES_VER = MakeVersion<DRSSettingValues>(1);
        public static readonly uint NVDRS_SETTING_VER = MakeVersion<DRSSetting>(1);

        public static readonly uint NVDRS_APPLICATION_VER_V1 = MakeVersion<DRSApplicationV1>(1);
        public static readonly uint NVDRS_APPLICATION_VER_V2 = MakeVersion<DRSApplicationV2>(2);
        public static readonly uint NVDRS_APPLICATION_VER_V3 = MakeVersion<DRSApplicationV3>(3);
        public static readonly uint NVDRS_APPLICATION_VER = NVDRS_APPLICATION_VER_V3;

        private static uint MakeVersion<T>(int version)
        {
            return (uint)((Marshal.SizeOf(typeof(T))) | (int)(version << 16));
        }

        [DllImport(_path32, EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        private static extern IntPtr NvAPI32_QueryInterface(uint interfaceId);

        [DllImport(_path64, EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        private static extern IntPtr NvAPI64_QueryInterface(uint interfaceId);

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


        public static class DRS
        {
            public static NvApiStatus CreateSession(out DRSSessionHandle phSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_CreateSession>().Invoke(out phSession);
            }

            public static NvApiStatus DestroySession(DRSSessionHandle phSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_DestroySession>().Invoke(phSession);
            }

            public static NvApiStatus LoadSettings(DRSSessionHandle phSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_LoadSettings>().Invoke(phSession);
            }

            public static NvApiStatus GetBaseProfile(DRSSessionHandle hSession, out DRSProfileHandle hProfile)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_GetBaseProfile>().Invoke(hSession, out hProfile);
            }

            public static NvApiStatus FindProfileByName(DRSSessionHandle phSession, string profileName, out DRSProfileHandle profile)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_FindProfileByName>().Invoke(phSession, new StringBuilder(profileName), out profile);
            }

            public static NvApiStatus CreateProfile(DRSSessionHandle phSession, DRSProfile profile, out DRSProfileHandle hProfile)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_CreateProfile>().Invoke(phSession, ref profile, out hProfile);
            }

            public static NvApiStatus GetApplicationInfo(DRSSessionHandle hSession, DRSProfileHandle hProfile, string appName, ref DRSApplicationV1 app)// where T : NVDRS_APPLICATION_V1
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_GetApplicationInfo>().Invoke(hSession, hProfile, new StringBuilder(appName), ref app);
            }

            public static NvApiStatus CreateApplication(DRSSessionHandle phSession, DRSProfileHandle hProfile, ref DRSApplicationV1 app)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_CreateApplication>().Invoke(phSession, hProfile, ref app);
            }

            public static NvApiStatus SetSetting(DRSSessionHandle phSession, DRSProfileHandle hProfile, ref DRSSetting pSetting)
            {
                return DelegateFactory.GetDelegate<NvAPI_RS_SetSettingDelegate>().Invoke(phSession, hProfile, ref pSetting);
            }

            public static NvApiStatus SaveSettings(DRSSessionHandle phSession)
            {
                return DelegateFactory.GetDelegate<NvAPI_DRS_SaveSettings>().Invoke(phSession);
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
