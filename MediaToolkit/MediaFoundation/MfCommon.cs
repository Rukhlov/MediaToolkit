using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using GDI = System.Drawing;

namespace MediaToolkit.MediaFoundation
{

    public class MfTool
    {
        public static readonly Dictionary<Guid, string> AttrsDict = new Dictionary<Guid, string>();
        public static readonly Dictionary<Guid, string> TypesDict = new Dictionary<Guid, string>();

        public static readonly Dictionary<Guid, SharpDX.DXGI.Format> DxgiFormatsDict = new Dictionary<Guid, SharpDX.DXGI.Format>
        {
            { VideoFormatGuids.NV12, SharpDX.DXGI.Format.NV12},
            { VideoFormatGuids.Argb32, SharpDX.DXGI.Format.B8G8R8A8_UNorm},
            //...
        };

        static MfTool()
        {
            FillTypeDict(typeof(MediaEventExtendedTypes));

            FillTypeDict(typeof(AudioFormatGuids));
            FillTypeDict(typeof(VideoFormatGuids));
            FillTypeDict(typeof(VideoFormatGuidsEx));
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

        public static string GetMediaSubtypeName(MediaType mediaType, bool GetFullName = false)
        {
            string typeName = "UnknownType";
            var guid = mediaType.Get(MediaTypeAttributeKeys.Subtype);
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
            for (int i = 0; i < mediaType.Count; i++)
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
            else if (guid == TransformAttributeKeys.MftInputTypesAttributes.Guid ||
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
                        catch (Exception ex)
                        {
                            valStr = "error";
                            Debug.Fail(ex.Message);
                        }
                    }
                }
            }
            else if (guid == TransformAttributeKeys.TransformFlagsAttribute.Guid)
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

        public static string LogEnumFlags(Enum flags)
        {
            string log = "";

            Type type = flags.GetType();

            var values = Enum.GetValues(type).Cast<Enum>().Where(f => flags.HasFlag(f));
            log = string.Join(" | ", values);

            return log;
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

        public const long TicksPerSecond = 10_000_000;
        public static double MfTicksToSec(long tick)
        {
            return tick / (double)TicksPerSecond;
        }

        public static long SecToMfTicks(double sec)
        {
            return (long)(sec * TicksPerSecond);
        }


        public static long SizeToLong(GDI.Size size)
        {
            return PackToLong(size.Width, size.Height);
        }
        public static GDI.Size LongToSize(long val)
        {
            var pars = UnPackLongToInts(val);
            return new GDI.Size(pars[0], pars[1]);
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

        public static Format GetDXGIFormatFromVideoFormatGuid(Guid guid)
        {
            Format format = Format.Unknown;
            if (DxgiFormatsDict.ContainsKey(guid))
            {
                format = DxgiFormatsDict[guid];
            }
            return format;
        }


        public static double GetFrameRate(MediaType mediaType)
        {
            double frameRate = 0;
            if (mediaType != null)
            {
                var sizeLong = mediaType.Get(MediaTypeAttributeKeys.FrameRate);
                var sizeInts = MfTool.UnPackLongToInts(sizeLong);
                if (sizeInts != null && sizeInts.Length == 2)
                {
                    double num = sizeInts[0];
                    double den = sizeInts[1];

                    frameRate = num / den;
                }
                else
                {
                    //...
                }
            }

            return frameRate;
        }

        public static GDI.Size GetFrameSize(MediaType mediaType)
        {
            GDI.Size frameSize = GDI.Size.Empty;
            if (mediaType != null)
            {
                var sizeLong = mediaType.Get(MediaTypeAttributeKeys.FrameSize);
                var sizeInts = MfTool.UnPackLongToInts(sizeLong);
                if (sizeInts != null && sizeInts.Length == 2)
                {
                    frameSize = new GDI.Size
                    {
                        Width = sizeInts[0],
                        Height = sizeInts[1],
                    };
                }
                else
                {
                    //...
                }
            }

            return frameSize;
        }


        public static string LogMediaSource(MediaSource mediaSource)
        {
            StringBuilder log = new StringBuilder();
            PresentationDescriptor presentationDescriptor = null;
            try
            {
                mediaSource.CreatePresentationDescriptor(out presentationDescriptor);

                for (int streamIndex = 0; streamIndex < presentationDescriptor.StreamDescriptorCount; streamIndex++)
                {

                    log.AppendLine("StreamIndex " + streamIndex + "---------------------------------------");

                    using (var steamDescriptor = presentationDescriptor.GetStreamDescriptorByIndex(streamIndex, out SharpDX.Mathematics.Interop.RawBool selected))
                    {

                        using (var mediaHandler = steamDescriptor.MediaTypeHandler)
                        {
                            for (int mediaIndex = 0; mediaIndex < mediaHandler.MediaTypeCount; mediaIndex++)
                            {
                                using (var mediaType = mediaHandler.GetMediaTypeByIndex(mediaIndex))
                                {
                                    var mediaTypeLog = LogMediaType(mediaType);

                                    log.AppendLine(mediaTypeLog);
                                }

                            }
                        }
                    }

                }
            }
            finally
            {
                presentationDescriptor?.Dispose();
            }

            return log.ToString();
        }

        public static MediaType GetCurrentMediaType(MediaSource mediaSource)
        {
            MediaType mediaType = null;
            PresentationDescriptor presentationDescriptor = null;
            try
            {
                mediaSource.CreatePresentationDescriptor(out presentationDescriptor);

                for (int streamIndex = 0; streamIndex < presentationDescriptor.StreamDescriptorCount; streamIndex++)
                {
                    using (var steamDescriptor = presentationDescriptor.GetStreamDescriptorByIndex(streamIndex, out SharpDX.Mathematics.Interop.RawBool selected))
                    {
                        if (selected)
                        {
                            using (var mediaHandler = steamDescriptor.MediaTypeHandler)
                            {
                                mediaType = mediaHandler.CurrentMediaType;

                            }
                        }
                    }
                }
            }
            finally
            {
                presentationDescriptor?.Dispose();
            }

            return mediaType;
        }

        public static string LogSourceReaderTypes(SourceReader sourceReader)
        {
            StringBuilder log = new StringBuilder();

            int streamIndex = 0;
            while (true)
            {
                bool invalidStreamNumber = false;

                int _streamIndex = -1;

                for (int mediaIndex = 0; ; mediaIndex++)
                {
                    try
                    {
                        var nativeMediaType = sourceReader.GetNativeMediaType(streamIndex, mediaIndex);

                        if (_streamIndex != streamIndex)
                        {
                            _streamIndex = streamIndex;
                            log.AppendLine("====================== StreamIndex#" + streamIndex + "=====================");
                        }

                        log.AppendLine(MfTool.LogMediaType(nativeMediaType));
                        nativeMediaType?.Dispose();

                    }
                    catch (SharpDX.SharpDXException ex)
                    {
                        if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
                        {
                            //Console.WriteLine("");
                            break;
                        }
                        else if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.InvalidStreamNumber)
                        {
                            invalidStreamNumber = true;
                            break;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (invalidStreamNumber)
                {
                    break;
                }

                streamIndex++;
            }

            return log.ToString();
        }

        public static bool TryGetVendorId(string venIdStr, out int vendorId)
        {
            bool result = false;
            vendorId = -1;

            const string VEN = "VEN_";

            var index = venIdStr.IndexOf(VEN);
            if (index >= 0)
            {
                var startIndex = VEN.Length - index;
                var venid = venIdStr.Substring(startIndex, venIdStr.Length - startIndex);

                if (!string.IsNullOrEmpty(venid))
                {
                    result = int.TryParse(venid, System.Globalization.NumberStyles.HexNumber, null, out vendorId);
                }
            }
            return result;
        }



        public static MediaType CreateMediaTypeFromWaveFormat(NAudio.Wave.WaveFormat mixWaveFormat)
        {
            MediaType mediaType = null;
            object comObj = null;
            try
            {
                comObj = NAudio.MediaFoundation.MediaFoundationApi.CreateMediaTypeFromWaveFormat(mixWaveFormat);
                var pUnk = Marshal.GetIUnknownForObject(comObj);
                mediaType = new MediaType(pUnk);
            }
            finally
            {
                if (comObj != null)
                {
                    Marshal.ReleaseComObject(comObj);
                }
            }

            return mediaType;
        }

        private static MediaType CreateAudioType(Guid format, int sampleRate, int channelsNum, int bitsPerSample)
        {
            var inputMediaType = new MediaType();
            {

                int bytesPerSample = bitsPerSample / 8;

                //This attribute corresponds to the nAvgBytesPerSec member of the WAVEFORMATEX structure. 
                var avgBytesPerSecond = sampleRate * channelsNum * bytesPerSample; // х.з зачем это нужно, но без этого не работает!!!

                var blockAlignment = 8;
                if (format == AudioFormatGuids.Pcm || format == AudioFormatGuids.Float)
                {
                    // If wFormatTag = WAVE_FORMAT_PCM or wFormatTag = WAVE_FORMAT_IEEE_FLOAT, 
                    //set nBlockAlign to (nChannels*wBitsPerSample)/8, 
                    //which is the size of a single audio frame. 
                    blockAlignment = channelsNum * bytesPerSample;
                }
                else
                {
                    //not supported...
                }

                inputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio);
                inputMediaType.Set(MediaTypeAttributeKeys.Subtype, format);
                inputMediaType.Set(MediaTypeAttributeKeys.AudioSamplesPerSecond, sampleRate);
                inputMediaType.Set(MediaTypeAttributeKeys.AudioBitsPerSample, bitsPerSample);
                inputMediaType.Set(MediaTypeAttributeKeys.AudioNumChannels, channelsNum);
                inputMediaType.Set(MediaTypeAttributeKeys.AudioAvgBytesPerSecond, avgBytesPerSecond);
                inputMediaType.Set(MediaTypeAttributeKeys.AudioBlockAlignment, blockAlignment);
                inputMediaType.Set(MediaTypeAttributeKeys.AudioChannelMask, 3);


            }

            return inputMediaType;
        }


    }

    public class DxTool
    {

        public static bool TextureToBitmap(Texture2D texture, GDI.Bitmap bmp)
        {

            bool success = false;
            var descr = texture.Description;
            if (descr.Format != Format.B8G8R8A8_UNorm)
            {
                throw new InvalidOperationException("Invalid texture format " + descr.Format);
            }

            if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                throw new InvalidOperationException("Invalid bitmap format " + bmp.PixelFormat);
            }

            if (bmp.Width != descr.Width || bmp.Height != descr.Height)
            {
                throw new InvalidOperationException("Invalid size " + bmp.PixelFormat);

                //...
                //logger.Warn(bmp.Width != descr.Width || bmp.Height != descr.Height);
                //return false;
            }

            var device = texture.Device;
            try
            {
                var srcData = device.ImmediateContext.MapSubresource(texture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                int width = bmp.Width;
                int height = bmp.Height;
                var rect = new GDI.Rectangle(0, 0, width, height);
                var destData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                try
                {
                    IntPtr srcPtr = srcData.DataPointer;
                    int srcOffset = rect.Top * srcData.RowPitch + rect.Left * 4;

                    srcPtr = IntPtr.Add(srcPtr, srcOffset);

                    var destPtr = destData.Scan0;
                    for (int row = rect.Top; row < rect.Bottom; row++)
                    {
                        Utilities.CopyMemory(destPtr, srcPtr, width * 4);
                        srcPtr = IntPtr.Add(srcPtr, srcData.RowPitch);
                        destPtr = IntPtr.Add(destPtr, destData.Stride);

                    }

                    success = true;
                }
                finally
                {
                    bmp.UnlockBits(destData);
                }

            }
            finally
            {
                device.ImmediateContext.UnmapSubresource(texture, 0);
            }

            return success;
        }

        public static Texture2D GetTexture(GDI.Bitmap bitmap, SharpDX.Direct3D11.Device device)
        {
            Texture2D texture = null;

            var rect = new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            if (bitmap.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                var _bitmap = bitmap.Clone(rect, GDI.Imaging.PixelFormat.Format32bppArgb);

                bitmap.Dispose();
                bitmap = _bitmap;
            }


            var data = bitmap.LockBits(rect, GDI.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                var descr = new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    ArraySize = 1,
                    //BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                    //Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                    //CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    MipLevels = 1,
                    //OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                };

                var dataRect = new SharpDX.DataRectangle(data.Scan0, data.Stride);

                texture = new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return texture;
        }
    }


    class MediaEventHandler : CallbackBase, IAsyncCallback
    {

        private readonly MediaEventGenerator eventGenerator = null;
        public event Action<MediaEvent> EventReceived;
        public MediaEventHandler(MediaEventGenerator eventGen)
        {
            this.eventGenerator = eventGen;
            this.eventGenerator.BeginGetEvent(this, null);
        }

        public bool IsShutdown
        {
            get
            {
                return (eventGenerator == null || eventGenerator.IsDisposed) || this.IsDisposed;
            }
        }

        public void Invoke(AsyncResult asyncResultRef)
        {
            if (IsShutdown)
            {
                return;
            }

            try
            {
                var mediaEvent = eventGenerator?.EndGetEvent(asyncResultRef);
                try
                {
                    EventReceived?.Invoke(mediaEvent);
                }
                finally
                {
                    if (mediaEvent != null)
                    {
                        mediaEvent.Dispose();
                        mediaEvent = null;
                    }
                }

                if (IsShutdown)
                {
                    return;
                }

                eventGenerator?.BeginGetEvent(this, null);
            }
            catch (Exception ex)
            { //может привести к неопределенному состоянию...
              // т.е события больше не генерятся, подписчики ни чего об этом не знают...
                Console.WriteLine(ex);

                throw;
            }
        }

        public IDisposable Shadow { get; set; }
        public AsyncCallbackFlags Flags { get; private set; }
        public WorkQueueId WorkQueueId { get; private set; }

    }

    public static class ServiceProviderExt
    {
        public static T GetNativeMfService<T>(this ServiceProvider service, Guid guid) where T : class
        {
            T comObj = null;
            IntPtr pUnk = IntPtr.Zero;
            try
            {
                pUnk = service.GetService(guid, typeof(T).GUID);
                comObj = (T)Marshal.GetObjectForIUnknown(pUnk);

            }
            finally
            {
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                    pUnk = IntPtr.Zero;
                }
            }

            return comObj;
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

        public static readonly MediaAttributeKey<int> CODECAPI_AVEncCommonQuality = new MediaAttributeKey<int>("fcbf57a3-7ea5-4b0c-9644-69b40c39c391");

        public static readonly MediaAttributeKey<int> CODECAPI_AVEncCommonMaxBitRate = new MediaAttributeKey<int>("fcbf57a3-7ea5-4b0c-9644-69b40c39c391");


        // MF_VIDEO_MAX_MB_PER_SEC e3f2e203-d445-4b8c-9211ba017-ae390d3
        public static readonly MediaAttributeKey<int> MF_VIDEO_MAX_MB_PER_SEC = new MediaAttributeKey<int>(new Guid(0xe3f2e203, 0xd445, 0x4b8c, 0x92, 0x11, 0xae, 0x39, 0xd, 0x3b, 0xa0, 0x17));

        /// <summary>
        /// For hardware MFTs, this attribute allows the HMFT to report the graphics driver version.
        /// MFT_GFX_DRIVER_VERSION_ID_Attribute f34b9093-05e0-4b16-993d-3e2a2cde6ad3
        /// </summary>
        public static readonly MediaAttributeKey<int> MFT_GFX_DRIVER_VERSION_ID_Attribute = new MediaAttributeKey<int>(new Guid(0xf34b9093, 0x05e0, 0x4b16, 0x99, 0x3d, 0x3e, 0x2a, 0x2c, 0xde, 0x6a, 0xd3));

        //MFT_ENCODER_SUPPORTS_CONFIG_EVENT 86a355ae-3a77-4ec4-9f31-01149a4e92de
        public static readonly MediaAttributeKey<int> MFT_ENCODER_SUPPORTS_CONFIG_EVENT = new MediaAttributeKey<int>(new Guid(0x86a355ae, 0x3a77, 0x4ec4, 0x9f, 0x31, 0x1, 0x14, 0x9a, 0x4e, 0x92, 0xde));


        public static readonly MediaAttributeKey<int> MF_SA_REQUIRED_SAMPLE_COUNT = new MediaAttributeKey<int>("18802c61-324b-4952-abd0-176ff5c696ff");

        public static readonly MediaAttributeKey<int> EVRConfig_ForceBob = new MediaAttributeKey<int>("e447df01-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_AllowDropToBob = new MediaAttributeKey<int>("e447df02-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_ForceHalfInterlace = new MediaAttributeKey<int>("e447df05-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_AllowDropToHalfInterlace = new MediaAttributeKey<int>("e447df06-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_ForceThrottle = new MediaAttributeKey<int>("e447df03-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_AllowDropToThrottle = new MediaAttributeKey<int>("e447df04-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_ForceScaling = new MediaAttributeKey<int>("e447df07-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_AllowScaling = new MediaAttributeKey<int>("e447df08-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_ForceBatching = new MediaAttributeKey<int>("e447df09-10ca-4d17-b17e-6a840f8a3a4c");
        public static readonly MediaAttributeKey<int> EVRConfig_AllowBatching = new MediaAttributeKey<int>("e447df0a-10ca-4d17-b17e-6a840f8a3a4c");

        public static readonly MediaAttributeKey<byte[]> VIDEO_ZOOM_RECT = new MediaAttributeKey<byte[]>("7aaa1638-1b7f-4c93-bd89-5b9c9fb6fcf0");


    }
    public static class MediaServiceKeysEx
    {
        public static readonly Guid RenderService = new Guid(0x1092a86c, 0xab1a, 0x459a, 0xa3, 0x36, 0x83, 0x1f, 0xbc, 0x4d, 0x11, 0xff);

        //MR_VIDEO_MIXER_SERVICE
        public static readonly Guid MixerService = new Guid("073cd2fc-6cf4-40b7-8859-e89552c841f8");


        


    }

    public static class MediaEventExtendedTypes
    {
        /// <summary>
        /// Approximate processing latency introduced by the component, in 100-nanosecond units.
        /// Processing latency is the amount of latency that a component introduces into the pipeline by processing a sample.In some cases,
        /// the latency cannot be derived simply by looking at the calls to IMFQualityManager::NotifyProcessInput and IMFQualityManager::NotifyProcessOutput.
        /// For example, there may not be a one-to-one correspondence between input samples and output samples.In this case,
        /// the component might send an MEQualityNotify event with the processing latency.If the processing latency changes,
        /// the component can send a new event at any time during streaming.
        /// </summary>
        public static readonly Guid QualityNotifyProcessingLatency = new Guid("f6b44af8-604d-46fe-a95d-45479b10c9bc");

        /// <summary>
        /// Lag time for the sample, in 100-nanosecond units. 
        /// If the value is positive, the sample was late. If the value is negative, the sample was early.
        /// </summary>
        public static readonly Guid QualityNotifySampleLag = new Guid("30d15206-ed2a-4760-be17-eb4a9f12295c");
    }

    public static class VideoFormatGuidsEx
    {
        public static readonly Guid Abgr32 = new Guid("00000020-0000-0010-8000-00aa00389b71");
        public static readonly Guid P208 = new Guid("38303250-0000-0010-8000-00aa00389b71");
        public static readonly Guid V410 = new Guid("30313476-0000-0010-8000-00aa00389b71");
    }


    public class CLSID
    {
        public static readonly Guid VideoProcessorMFT = new Guid("88753B26-5B24-49BD-B2E7-0C445C78C982");

        public static readonly Guid CColorConvertDMO = new Guid("98230571-0087-4204-b020-3282538e57d3");

        public static readonly Guid MJPEGDecoderMFT = new Guid("CB17E772-E1CC-4633-8450-5617AF577905");

        public static readonly Guid CResamplerMediaObject = new Guid("f447b69e-1884-4a7e-8055-346f74d6edb3");



    }

    public enum RateControlMode
    {
        CBR,
        PeakConstrainedVBR,
        UnconstrainedVBR,
        Quality,
        LowDelayVBR,
        GlobalVBR,
        GlobalLowDelayVBR
    };

    public enum eAVEncH264VProfile
    {
        eAVEncH264VProfile_unknown = 0,
        eAVEncH264VProfile_Simple = 66,
        eAVEncH264VProfile_Base = 66,
        eAVEncH264VProfile_Main = 77,
        eAVEncH264VProfile_High = 100,
        eAVEncH264VProfile_422 = 122,
        eAVEncH264VProfile_High10 = 110,
        eAVEncH264VProfile_444 = 144,
        eAVEncH264VProfile_Extended = 88,
        eAVEncH264VProfile_ScalableBase = 83,
        eAVEncH264VProfile_ScalableHigh = 86,
        eAVEncH264VProfile_MultiviewHigh = 118,
        eAVEncH264VProfile_StereoHigh = 128,
        eAVEncH264VProfile_ConstrainedBase = 256,
        eAVEncH264VProfile_UCConstrainedHigh = 257,
        eAVEncH264VProfile_UCScalableConstrainedBase = 258,
        eAVEncH264VProfile_UCScalableConstrainedHigh = 259

    }

    //https://docs.microsoft.com/en-us/windows/win32/api/mfidl/nf-mfidl-imfmediasink-getcharacteristics
    public enum MediaSinkCharacteristics
    {
        FIXED_STREAMS = 0x00000001,
        CANNOT_MATCH_CLOCK = 0x00000002,
        RATELESS = 0x00000004,
        CLOCK_REQUIRED = 0x00000008,
        CAN_PREROLL = 0x00000010,
        REQUIRE_REFERENCE_MEDIATYPE = 0x00000020,

    }

    public enum MFVideoRenderPrefs
    {
        DoNotRenderBorder = 0x00000001,
        DoNotClipToDevice = 0x00000002,
        AllowOutputThrottling = 0x00000004,
        ForceOutputThrottling = 0x00000008,
        ForceBatching = 0x00000010,
        AllowBatching = 0x00000020,
        ForceScaling = 0x00000040,
        AllowScaling = 0x00000080,
        DoNotRepaintOnStop = 0x00000100,
        Mask = 0x000000ff
    }


    public class MfVideoArgs
    {

        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;

        public int FrameRate { get; set; } = 15;
        public int Quality { get; set; } = 70;

        public Guid Format { get; set; } = VideoFormatGuids.NV12;

        public long AdapterId { get; set; } = -1;

        public int AvgBitrate { get; set; } = 2500;

        public int MaxBitrate { get; set; } = 5000;

        public RateControlMode BitrateMode { get; set; } = RateControlMode.CBR;

        public bool LowLatency { get; set; } = true;

        public eAVEncH264VProfile Profile { get; set; } = eAVEncH264VProfile.eAVEncH264VProfile_Main;

        public int InterlaceMode { get; set; } = 2; //Progressive

    }

}
