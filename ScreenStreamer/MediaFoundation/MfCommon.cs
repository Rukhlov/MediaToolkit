using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.IO;

using System.Threading;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
//using SharpDX.Direct2D1;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;
using ScreenStreamer.Utils;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using System.Reflection;

namespace ScreenStreamer.MediaFoundation
{

    class MfTool
    {
        public static readonly Dictionary<Guid, string> AttrsDict= new Dictionary<Guid, string>();
        public static readonly Dictionary<Guid, string> TypesDict = new Dictionary<Guid, string>();

        static MfTool()
        {
              
            FillTypeDict(typeof(AudioFormatGuids));
            FillTypeDict(typeof(VideoFormatGuids));
            FillTypeDict(typeof(MediaTypeGuids));
            FillTypeDict(typeof(TransformCategoryGuids));

            FillAttrDict(typeof(MediaTypeAttributeKeys));
            FillAttrDict(typeof(TransformAttributeKeys));
            FillAttrDict(typeof(SinkWriterAttributeKeys));

            FillAttrDict(typeof(MFAttributeKeys));

        }

        public static string GetMediaTypeName(Guid guid, bool GetFullName = false)
        {
            string typeName = "UnknownType";

            if (TypesDict.ContainsKey(guid))
            {
                typeName = TypesDict[guid];
                if (GetFullName)
                {
                    typeName += " {" + guid + "}";
                }
            }
            else
            {
                typeName = "{" + guid + "}";
            }

            return typeName;
        }

        public static string LogMediaType(MediaType mediaType)
        {

            StringBuilder log = new StringBuilder();
            for(int i= 0; i < mediaType.Count; i++)
            {
                var obj = mediaType.GetByIndex(i, out Guid guid);
                {
                    string result = LogAttribute(guid, obj);

                    log.AppendLine(result);
                }
            }

            return log.ToString();
        }

        public static string LogMediaAttributes(MediaAttributes mediaAttributes)
        {
            StringBuilder log = new StringBuilder();
            for (int i = 0; i < mediaAttributes.Count; i++)
            {
                var obj = mediaAttributes.GetByIndex(i, out Guid guid);
                {
                    string result = LogAttribute(guid, obj);

                    log.AppendLine(result);
                }
            }

            return log.ToString();
        }

        public unsafe static string LogAttribute(Guid guid, object obj)
        {
            var attrName = guid.ToString();

            if (AttrsDict.ContainsKey(guid))
            {
                attrName = AttrsDict[guid];

            }

            var valStr = "";
            if (obj != null)
            {
                valStr = obj.ToString();

                if (obj is Guid)
                {
                    valStr = GetMediaTypeName((Guid)obj, true);
                }
            }

            if (guid == MediaTypeAttributeKeys.FrameRate.Guid ||
                guid == MediaTypeAttributeKeys.FrameRateRangeMax.Guid ||
                guid == MediaTypeAttributeKeys.FrameRateRangeMin.Guid ||
                guid == MediaTypeAttributeKeys.FrameSize.Guid ||
                guid == MediaTypeAttributeKeys.PixelAspectRatio.Guid)
            {
                // Attributes that contain two packed 32-bit values.
                long val = (long)obj;

                valStr = string.Join(" ", UnPackLongToInts(val));

            }
            else if (guid == MediaTypeAttributeKeys.GeometricAperture.Guid ||
                  guid == MediaTypeAttributeKeys.MinimumDisplayAperture.Guid ||
                  guid == MediaTypeAttributeKeys.PanScanAperture.Guid)
            {
                // Attributes that an MFVideoArea structure.
                //...
            }
            else if(guid == TransformAttributeKeys.MftInputTypesAttributes.Guid || 
                guid == TransformAttributeKeys.MftOutputTypesAttributes.Guid)
            {
                if (obj != null)
                {
                    var data = obj as byte[];
                    if (data != null)
                    {
                        try
                        {
                            TRegisterTypeInformation typeInfo;
                            fixed (byte* ptr = data)
                            {
                                typeInfo = (TRegisterTypeInformation)Marshal.PtrToStructure((IntPtr)ptr, typeof(TRegisterTypeInformation));
                            }
                            valStr = GetMediaTypeName(typeInfo.GuidMajorType) + " " + GetMediaTypeName(typeInfo.GuidSubtype);
                        } 
                        catch(Exception ex)
                        {
                            valStr = "error";
                            Debug.Fail(ex.Message);
                        }
                    }
                }
            }
            else if(guid == TransformAttributeKeys.TransformFlagsAttribute.Guid)
            {
                if (obj != null)
                {
                    var flag = (TransformEnumFlag)obj;
                    var flags = Enum.GetValues(typeof(TransformEnumFlag))
                         .Cast<TransformEnumFlag>()
                         .Where(m => (m != TransformEnumFlag.None && flag.HasFlag(m)));

                    valStr = string.Join("|", flags);
                }
            }

            return attrName + " " + valStr;
        }


        private static void FillTypeDict(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(Guid))
                {
                    Guid guid = (Guid)field.GetValue(null);
                    var name = field.Name;
                    if (!TypesDict.ContainsKey(guid))
                    {
                        TypesDict.Add(guid, name);
                    }
                }
            }
        }

        private static void FillAttrDict(Type type)
        {
            var attrFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in attrFields)
            {
                // if (field.DeclaringType == typeof(MediaAttributeKey))
                {
                    MediaAttributeKey attr = (MediaAttributeKey)field.GetValue(null);
                    var name = attr.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = field.Name;
                    }

                    var guid = attr.Guid;

                    if (!AttrsDict.ContainsKey(guid))
                    {
                        AttrsDict.Add(guid, name);
                    }
                }
            }
        }

        public static long PackToLong(int left, int right)
        {
            return ((long)left << 32 | (uint)right);
        }

        public static int[] UnPackLongToInts(long val)
        {
            return new int[]
            {
                 (int)(val >> 32),
                 (int)(val & uint.MaxValue),
            };

        }
    }

    /// <summary>
    /// https://github.com/tpn/winsdk-10/blob/master/Include/10.0.10240.0/um/codecapi.h
    /// https://docs.microsoft.com/en-us/windows/win32/medfound/h-264-video-encoder
    /// </summary>
    class MFAttributeKeys
    {
        
        /// <summary>
        /// Sets the number of worker threads used by a video encoder.
        /// </summary>
        public static readonly MediaAttributeKey<int> CODECAPI_AVEncNumWorkerThreads = new MediaAttributeKey<int>(new Guid(0xb0c8bf60, 0x16f7, 0x4951, 0xa3, 0xb, 0x1d, 0xb1, 0x60, 0x92, 0x93, 0xd6));

        //#define STATIC_CODECAPI_AVLowLatencyMode  0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee
        public static readonly MediaAttributeKey<bool> CODECAPI_AVLowLatencyMode = new MediaAttributeKey<bool>(new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee));

        /// <summary>
        /// Applications can set this property to specify the rate control mode. Encoders can also return this property as a capability.
        /// </summary>
        public static readonly MediaAttributeKey<RateControlMode> CODECAPI_AVEncCommonRateControlMode = new MediaAttributeKey<RateControlMode>("1c0608e9-370c-4710-8a58-cb6181c42423");


        // MF_VIDEO_MAX_MB_PER_SEC e3f2e203-d445-4b8c-9211ba017-ae390d3
        public static readonly MediaAttributeKey<int> MF_VIDEO_MAX_MB_PER_SEC = new MediaAttributeKey<int>(new Guid(0xe3f2e203, 0xd445, 0x4b8c, 0x92, 0x11, 0xae, 0x39, 0xd, 0x3b, 0xa0, 0x17));

        /// <summary>
        /// For hardware MFTs, this attribute allows the HMFT to report the graphics driver version.
        /// MFT_GFX_DRIVER_VERSION_ID_Attribute f34b9093-05e0-4b16-993d-3e2a2cde6ad3
        /// </summary>
        public static readonly MediaAttributeKey<int> MFT_GFX_DRIVER_VERSION_ID_Attribute = new MediaAttributeKey<int>(new Guid(0xf34b9093, 0x05e0, 0x4b16, 0x99, 0x3d, 0x3e, 0x2a, 0x2c, 0xde, 0x6a, 0xd3));

        //MFT_ENCODER_SUPPORTS_CONFIG_EVENT 86a355ae-3a77-4ec4-9f31-01149a4e92de
        public static readonly MediaAttributeKey<int> MFT_ENCODER_SUPPORTS_CONFIG_EVENT = new MediaAttributeKey<int>(new Guid(0x86a355ae, 0x3a77, 0x4ec4, 0x9f, 0x31, 0x1, 0x14, 0x9a, 0x4e, 0x92, 0xde));

 
    }

    enum RateControlMode
    {
        CBR,
        PeakConstrainedVBR,
        UnconstrainedVBR,
        Quality,
        LowDelayVBR,
        GlobalVBR,
        GlobalLowDelayVBR
    };

    public class VideoWriterArgs
    {
        public string FileName { get; set; }
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;

        //public IImageProvider ImageProvider { get; set; }
        public int FrameRate { get; set; } = 15;
        public int VideoQuality { get; set; } = 70;
        //public int AudioQuality { get; set; } = 50;
        //public IAudioProvider AudioProvider { get; set; }
    }

   


    public class MfContext
    {
        public readonly object syncRoot = new object();

        public Texture2D StagingTexture { get; set; }
        public DXGIDeviceManager DeviceManager { get; private set; } = new DXGIDeviceManager();

        private SharpDX.DXGI.Factory1 dxgiFactory = null;

        private Device device = null;
        public Adapter1 adapter = null;

        public void Init()
        {

            dxgiFactory = new SharpDX.DXGI.Factory1();
            //adapter = dxgiFactory.Adapters1.FirstOrDefault();

            SharpDX.Direct3D.DriverType driverType = SharpDX.Direct3D.DriverType.Hardware;

            DeviceCreationFlags creationFlags = DeviceCreationFlags.VideoSupport |
                                        DeviceCreationFlags.BgraSupport |
                                        DeviceCreationFlags.Debug;

            SharpDX.Direct3D.FeatureLevel[] features =
            {
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                    SharpDX.Direct3D.FeatureLevel.Level_9_1,
             };

            // device3d11 = new Device(adapter, DeviceCreationFlags.BgraSupport);
            device = new Device(driverType, creationFlags, features);
            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            DeviceManager.ResetDevice(device);

        }

        public void Close()
        {
            if (StagingTexture != null)
            {
                StagingTexture.Dispose();
                StagingTexture = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }

            if (adapter != null)
            {
                adapter.Dispose();
                adapter = null;
            }

            if (dxgiFactory != null)
            {
                dxgiFactory.Dispose();
                dxgiFactory = null;
            }

            if (DeviceManager != null)
            {
                DeviceManager.Dispose();
                DeviceManager = null;
            }
        }
    }

}
