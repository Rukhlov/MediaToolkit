using MediaToolkit.Logging;
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
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation.MfTool");

        public static readonly Dictionary<Guid, string> AttrsDict = new Dictionary<Guid, string>();
        public static readonly Dictionary<Guid, string> TypesDict = new Dictionary<Guid, string>();

        public static readonly Dictionary<Guid, SharpDX.DXGI.Format> DxgiFormatsDict = new Dictionary<Guid, SharpDX.DXGI.Format>
        {
            { VideoFormatGuids.NV12, SharpDX.DXGI.Format.NV12 },
            { VideoFormatGuids.Argb32, SharpDX.DXGI.Format.B8G8R8A8_UNorm },
            //...
        };

        public static readonly Dictionary<Guid, Core.VideoCodingFormat> MediaCodingFormatDict = new Dictionary<Guid, Core.VideoCodingFormat>
        {
            { VideoFormatGuids.H264, Core.VideoCodingFormat.H264},
            { VideoFormatGuids.Hevc, Core.VideoCodingFormat.HEVC },
            { VideoFormatGuids.Mjpg, Core.VideoCodingFormat.JPEG },
            //...
        };

        static MfTool()
        {
            FillTypeDict(typeof(ExtendedTypeGuids));

            FillTypeDict(typeof(AudioFormatGuids));
            FillTypeDict(typeof(VideoFormatGuids));
            FillTypeDict(typeof(VideoFormatGuidsEx));
            FillTypeDict(typeof(MediaTypeGuids));
            FillTypeDict(typeof(TransformCategoryGuids));

            FillAttrDict(typeof(CodecApiPropertyKeys));
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

        public static Guid GetVideoFormatGuidFromDXGIFormat(Format format)
        {
            Guid videoGuid = Guid.Empty;

            foreach (var guid in DxgiFormatsDict.Keys)
            {
                var _format = DxgiFormatsDict[guid];
                if (format == _format)
                {
                    videoGuid = guid;
                    break;
                }
            }

            return videoGuid;
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

        public static List<Core.VideoEncoderDescription> FindVideoEncoders()
        {
            List<Core.VideoEncoderDescription> encoderDescriptions = new List<Core.VideoEncoderDescription>();


            Activate[] activates = null;
            try
            {
                var category = TransformCategoryGuids.VideoEncoder;
                var transformFlags = TransformEnumFlag.All | TransformEnumFlag.SortAndFilter;

                //var outputType = new TRegisterTypeInformation
                //{
                //    GuidMajorType = MediaTypeGuids.Video,
                //    GuidSubtype = VideoFormatGuids.H264
                //    // GuidSubtype = VideoFormatGuids.Hevc
                //};

                activates = MediaFactory.FindTransform(category, transformFlags, null, null);
                foreach (var activate in activates)
                {
                    //var actLog = LogMediaAttributes(activate);
                    //Console.WriteLine("\r\nActivator:\r\n-----------------\r\n" + actLog);

                    Core.VideoEncoderDescription encoderDescription = new Core.VideoEncoderDescription();

                    for (int i = 0; i < activate.Count; i++)
                    {
                        Guid guid = Guid.Empty;
                        object obj = null;

                        obj = activate.GetByIndex(i, out guid);

                        if (obj == null || guid == Guid.Empty)
                        {
                            continue;
                        }

                        if (guid == TransformAttributeKeys.MftOutputTypesAttributes.Guid)
                        {
                            var data = obj as byte[];
                            if (GetRegTypeInfo(data, out var ti))
                            {
                                if (ti.GuidMajorType == MediaTypeGuids.Video)
                                {
                                    var subtype = ti.GuidSubtype;
                                    if (MediaCodingFormatDict.ContainsKey(subtype))
                                    {
                                        encoderDescription.Format = MediaCodingFormatDict[subtype];
                                    }
                                }
                            }
                        }
                        else if (guid == TransformAttributeKeys.MftFriendlyNameAttribute.Guid)
                        {
                            encoderDescription.Name = obj.ToString();
                        }
                        else if (guid == TransformAttributeKeys.MftEnumHardwareVendorIdAttribute.Guid)
                        {
                            string venIdStr = obj.ToString();
                            if (!string.IsNullOrEmpty(venIdStr))
                            {
                                if (TryGetVendorId(venIdStr, out int vendorId))
                                {
                                    encoderDescription.VendorId = vendorId;
                                }
                            }
                        }
                        else if (guid == TransformAttributeKeys.TransformFlagsAttribute.Guid)
                        {// TODO: видеокарт может быть несколько, 
                            //не понятно как связать полученый IMFTransform c Direct3D если установлено несколько видокарт или гибридный режим
                           
                            TransformEnumFlag enumFlags = (TransformEnumFlag)obj;
                            encoderDescription.IsHardware = enumFlags.HasFlag(TransformEnumFlag.Hardware);
                        }
                        else if (guid == TransformAttributeKeys.MftTransformClsidAttribute.Guid)
                        {
                            encoderDescription.ClsId = (Guid)obj;
                        }
                        else if (guid == TransformAttributeKeys.MftEnumHardwareUrlAttribute.Guid)
                        {
                            encoderDescription.HardwareUrl = obj.ToString();
                        }
                    }

                    if (encoderDescription.Format != Core.VideoCodingFormat.Unknown)
                    {
                        Transform transform = null;
                        try
                        {// x.з как по другому понять будет энкодер работать или нет!!
                            // т.к могут быть энкодеры связанные с оключенными или удаленными видеокартами
                            transform = activate.ActivateObject<Transform>();
                            encoderDescription.Activatable = true;
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex.Message);

                        }
                        finally
                        {
                            if (transform != null)
                            {
                                transform.Dispose();
                                transform = null;
                            }
                        }

                        encoderDescriptions.Add(encoderDescription);
                    }

                }

            }
            finally
            {
                if (activates != null)
                {
                    foreach(var a in activates)
                    {
                        if (a != null)
                        {
                            a.Dispose();
                        }
                    }
                }
            }

            return encoderDescriptions;
        }

        private static bool GetRegTypeInfo(Activate activate, out TRegisterTypeInformation typeInfo)
        {
            var result = false;

            typeInfo = default(TRegisterTypeInformation);
            var data = activate.Get(TransformAttributeKeys.MftOutputTypesAttributes);
            if (data != null)
            {
                result = GetRegTypeInfo(data, out typeInfo);
            }

            return result;
        }

        private unsafe static bool GetRegTypeInfo(byte[] data, out TRegisterTypeInformation typeInfo)
        {
            bool result = false;
            typeInfo = default(TRegisterTypeInformation);
            try
            {
                fixed (byte* ptr = data)
                {
                    typeInfo = (TRegisterTypeInformation)Marshal.PtrToStructure((IntPtr)ptr, typeof(TRegisterTypeInformation));
                    result = true;
                }
            }
            catch (Exception ex)
            {

                Debug.Fail(ex.Message);
            }

            return result;
        }

        public static List<Core.UvcDevice> FindUvcDevices()
        {
            List<Core.UvcDevice> deviceDescriptions = new List<Core.UvcDevice>();

            Activate[] activates = null;
            try
            {
                using (var attributes = new MediaAttributes())
                {
                    MediaFactory.CreateAttributes(attributes, 1);
                    attributes.Set(CaptureDeviceAttributeKeys.SourceType, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);

                    activates = MediaFactory.EnumDeviceSources(attributes);

                    foreach (var activate in activates)
                    {
                        try
                        {
                            var friendlyName = activate.Get(CaptureDeviceAttributeKeys.FriendlyName);
                            var symbolicLink = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink);
                            var deviceDescription = new Core.UvcDevice
                            {
                                Name = friendlyName,
                                DeviceId = symbolicLink,
                            };

                            try
                            {
                                using (var mediaSource = activate.ActivateObject<MediaSource>())
                                {
                                    using (var mediaType = GetCurrentMediaType(mediaSource))
                                    {

                                        var frameSize = GetFrameSize(mediaType);
                                        var frameRate = GetFrameRate(mediaType);

                                        var subtype = mediaType.Get(MediaTypeAttributeKeys.Subtype);
                                        var subtypeName = GetMediaTypeName(subtype);

                                        var profile = new Core.UvcProfile
                                        {
                                            FrameSize = frameSize,
                                            FrameRate = frameRate,
                                            Format = subtypeName,
                                        };

                                        deviceDescription.Resolution = frameSize;
                                        deviceDescription.CurrentProfile = profile;


                                    }
                                }

                                deviceDescriptions.Add(deviceDescription);

                            }
                            catch (Exception ex)
                            {
                                logger.Info("Device not supported: " + friendlyName + " " + symbolicLink);
                            }

                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                //logger.Error(ex);
            }
            finally
            {
                if (activates != null)
                {
                    foreach (var act in activates)
                    {
                        act.Dispose();
                    }
                }
            }

            return deviceDescriptions;

        }

        public static string GetActiveObjectsReport()
        {
            return SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects();
        }

    }

    public class DxTool
    {

        public static string LogDxAdapters(Adapter1[] adapters)
        {
            StringBuilder log = new StringBuilder();
            log.AppendLine("");
            foreach (var _adapter in adapters)
            {
                var adaptDescr = _adapter.Description1;
                log.AppendLine("-------------------------------------");
                log.AppendLine(string.Join("|", adaptDescr.Description, adaptDescr.DeviceId, adaptDescr.VendorId));

                foreach (var _output in _adapter.Outputs)
                {
                    var outputDescr = _output.Description;
                    var bound = outputDescr.DesktopBounds;
                    var rect = new GDI.Rectangle
                    {
                        X = bound.Left,
                        Y = bound.Top,
                        Width = (bound.Right - bound.Left),
                        Height = (bound.Bottom - bound.Top),
                    };

                    log.AppendLine(string.Join("| ", outputDescr.DeviceName, rect.ToString()));

                    _output.Dispose();
                }

                _adapter.Dispose();
            }

            return log.ToString();
        }

        public static void TextureToBitmap(Texture2D texture, ref GDI.Bitmap bmp)
        {

            var descr = texture.Description;
            if (descr.Format != Format.B8G8R8A8_UNorm)
            {
                throw new Exception("Invalid texture format " + descr.Format);
            }

            if (descr.Width <= 0 || descr.Height <= 0)
            {
                throw new Exception("Invalid texture size: " + descr.Width + " " + descr.Height);
            }

            if (bmp == null)
            {
                bmp = new GDI.Bitmap(descr.Width, descr.Height, GDI.Imaging.PixelFormat.Format32bppArgb);
            }

            if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                throw new Exception("Invalid bitmap format " + bmp.PixelFormat);

            }

            if (bmp.Width != descr.Width || bmp.Height != descr.Height)
            {
                throw new Exception("Invalid params");

            }

            using (Surface surface = texture.QueryInterface<Surface>())
            {
                try
                {
                    var srcData = surface.Map(SharpDX.DXGI.MapFlags.Read);

                    int width = bmp.Width;
                    int height = bmp.Height;
                    var rect = new GDI.Rectangle(0, 0, width, height);
                    var destData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                    try
                    {
                        int bytesPerPixel = GDI.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                        IntPtr srcPtr = srcData.DataPointer;
                        int srcOffset = rect.Top * srcData.Pitch + rect.Left * bytesPerPixel;

                        srcPtr = IntPtr.Add(srcPtr, srcOffset);

                        var destPtr = destData.Scan0;
                        for (int row = rect.Top; row < rect.Bottom; row++)
                        {
                            Utilities.CopyMemory(destPtr, srcPtr, width * bytesPerPixel);
                            srcPtr = IntPtr.Add(srcPtr, srcData.Pitch);
                            destPtr = IntPtr.Add(destPtr, destData.Stride);

                        }
                    }
                    finally
                    {
                        bmp.UnlockBits(destData);
                    }
                }
                finally
                {
                    surface.Unmap();
                }
            }
        }

        public static byte[] GetTextureBytes(Texture2D texture)
        {
            byte[] textureBytes = null;
            using (Surface surface = texture.QueryInterface<Surface>())
            {
                try
                {
                    surface.Map(SharpDX.DXGI.MapFlags.Read, out DataStream dataStream);
                    textureBytes = dataStream.ReadRange<byte>((int)dataStream.Length);
                }
                finally
                {
                    surface.Unmap();
                }
            }

            return textureBytes;
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
                //Windows 7 
                var descr = new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,

                };

                /*
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
				*/

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

    public static class CodecApiPropertyKeys
    {
        /// <summary>
        /// Sets the number of worker threads used by a video encoder.
        /// </summary> //CODECAPI_AVEncNumWorkerThreads
        public static readonly MediaAttributeKey<int> AVEncNumWorkerThreads = new MediaAttributeKey<int>(new Guid(0xb0c8bf60, 0x16f7, 0x4951, 0xa3, 0xb, 0x1d, 0xb1, 0x60, 0x92, 0x93, 0xd6));

        //#define STATIC_CODECAPI_AVLowLatencyMode  0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee
        public static readonly MediaAttributeKey<bool> AVLowLatencyMode = new MediaAttributeKey<bool>(new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee));

        /// <summary>
        /// Applications can set this property to specify the rate control mode. Encoders can also return this property as a capability.
        /// </summary> //CODECAPI_AVEncCommonRateControlMode
        public static readonly MediaAttributeKey<RateControlMode> AVEncCommonRateControlMode = new MediaAttributeKey<RateControlMode>("1c0608e9-370c-4710-8a58-cb6181c42423");

        public static readonly MediaAttributeKey<int> AVEncCommonQuality = new MediaAttributeKey<int>("fcbf57a3-7ea5-4b0c-9644-69b40c39c391");

        public static readonly MediaAttributeKey<int> AVEncCommonMaxBitRate = new MediaAttributeKey<int>("fcbf57a3-7ea5-4b0c-9644-69b40c39c391");


    }

    /// <summary>
    /// https://github.com/tpn/winsdk-10/blob/master/Include/10.0.10240.0/um/codecapi.h
    /// https://docs.microsoft.com/en-us/windows/win32/medfound/h-264-video-encoder
    /// </summary>
    public static class MFAttributeKeys
    {


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

    public static class ExtendedTypeGuids
    {
        /// <summary>
        /// Approximate processing latency introduced by the component, in 100-nanosecond units.
        /// Processing latency is the amount of latency that a component introduces into the pipeline by processing a sample.In some cases,
        /// the latency cannot be derived simply by looking at the calls to IMFQualityManager::NotifyProcessInput and IMFQualityManager::NotifyProcessOutput.
        /// For example, there may not be a one-to-one correspondence between input samples and output samples.In this case,
        /// the component might send an MEQualityNotify event with the processing latency.If the processing latency changes,
        /// the component can send a new event at any time during streaming.
        /// </summary> //MF_QUALITY_NOTIFY_PROCESSING_LATENCY
        public static readonly Guid QualityNotifyProcessingLatency = new Guid("f6b44af8-604d-46fe-a95d-45479b10c9bc");

        /// <summary>
        /// Lag time for the sample, in 100-nanosecond units. 
        /// If the value is positive, the sample was late. If the value is negative, the sample was early.
        /// </summary> // MF_QUALITY_NOTIFY_SAMPLE_LAG
        public static readonly Guid QualityNotifySampleLag = new Guid("30d15206-ed2a-4760-be17-eb4a9f12295c");
    }

    public static class MediaServiceKeysEx
    {
        //MR_VIDEO_RENDER_SERVICE
        public static readonly Guid RenderService = new Guid("1092a86c-ab1a-459a-a336-831fbc4d11ff");

        //MR_VIDEO_MIXER_SERVICE
        public static readonly Guid MixerService = new Guid("073cd2fc-6cf4-40b7-8859-e89552c841f8");

    }

    public static class VideoFormatGuidsEx
    {
        public static readonly Guid Abgr32 = new Guid("00000020-0000-0010-8000-00aa00389b71");
        public static readonly Guid P208 = new Guid("38303250-0000-0010-8000-00aa00389b71");

        //30313456-0000-0010-8000-00aa00389b71
        public static readonly Guid V410 = new Guid("30313476-0000-0010-8000-00aa00389b71");

        public static readonly Guid V216 = new Guid("36313256-0000-0010-8000-00aa00389b71");


    }


    public class ClsId
    {
        //CLSID_VideoProcessorMFT
        public static readonly Guid VideoProcessorMFT = new Guid("88753B26-5B24-49BD-B2E7-0C445C78C982");

        //CLSID_CColorConvertDMO
        public static readonly Guid CColorConvertDMO = new Guid("98230571-0087-4204-b020-3282538e57d3");

        public static readonly Guid MJPEGDecoderMFT = new Guid("CB17E772-E1CC-4633-8450-5617AF577905");

        //CLSID_CResamplerMediaObject
        public static readonly Guid CResamplerMediaObject = new Guid("f447b69e-1884-4a7e-8055-346f74d6edb3");

        //NVIDIA H.264 Encoder MFT
        Guid NVidiaH264EncoderMFT = new Guid("60F44560-5A20-4857-BFEF-D29773CB8040");

        //Intel® Quick Sync Video H.264 Encoder MFT
        Guid IntelQSVH264EncoderMFT = new Guid("4BE8D3C0-0515-4A37-AD55-E4BAE19AF471");

        //CLSID_MSH264EncoderMFT
        public static readonly Guid MSH264EncoderMFT = new Guid("6CA50344-051A-4DED-9779-A43305165E35");
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
        Unknown = 0,
        Simple = 66,
        Base = 66,
        Main = 77,
        High = 100,
        High422 = 122,
        High10 = 110,
        High444 = 144,
        Extended = 88,
        ScalableBase = 83,
        ScalableHigh = 86,
        MultiviewHigh = 118,
        StereoHigh = 128,
        ConstrainedBase = 256,
        UCConstrainedHigh = 257,
        UCScalableConstrainedBase = 258,
        UCScalableConstrainedHigh = 259
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

        public eAVEncH264VProfile Profile { get; set; } = eAVEncH264VProfile.Main;

        public int InterlaceMode { get; set; } = 2; //Progressive

    }

}
