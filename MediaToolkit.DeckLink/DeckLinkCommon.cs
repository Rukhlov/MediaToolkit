using DeckLinkAPI;
using MediaToolkit.Core;
using MediaToolkit.Logging;
using MediaToolkit.SharedTypes;


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.DeckLink
{
    public class DeckLinkTools
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DeckLink");

        public static bool GetDeviceByIndex(int inputIndex, out IDeckLink deckLink)
        {
            logger.Verb("GetDeviceByIndex(...) " + inputIndex);

            bool Success = false;

            deckLink = null;
            IDeckLinkIterator deckLinkIterator = null;
            try
            {
                deckLinkIterator = new CDeckLinkIterator();

                int index = 0;
                do
                {
                    if (deckLink != null)
                    {
                        Marshal.ReleaseComObject(deckLink);
                        deckLink = null;
                    }

                    deckLinkIterator.Next(out deckLink);
                    if (index == inputIndex)
                    {
                        Success = true;
                        break;
                    }

                    index++;
                }
                while (deckLink != null);

            }
            catch (Exception ex)
            {
                logger.Error(ex);

            }
            finally
            {
                if (deckLinkIterator != null)
                {
                    Marshal.ReleaseComObject(deckLinkIterator);
                    deckLinkIterator = null;
                }

            }

            return Success;
        }

        public static bool GetDeviceByHandle(string deviceHandle, out IDeckLink deckLink)
        {
            logger.Verb("GetDeviceByName(...) " + deviceHandle);

            bool Success = false;

            deckLink = null;
            IDeckLinkIterator deckLinkIterator = null;
            try
            {
                deckLinkIterator = new CDeckLinkIterator();

                int index = 0;
                do
                {
                    if (deckLink != null)
                    {
                        Marshal.ReleaseComObject(deckLink);
                        deckLink = null;
                    }

                    deckLinkIterator.Next(out deckLink);
                    if (deckLink != null)
                    {
                        ((IDeckLinkProfileAttributes)deckLink).GetString(_BMDDeckLinkAttributeID.BMDDeckLinkDeviceHandle, out string handle);
                        if (deviceHandle == handle)
                        {
                            break;
                        }
                    }
                    index++;
                }
                while (deckLink != null);

            }
            catch (Exception ex)
            {
                logger.Error(ex);

            }
            finally
            {
                if (deckLinkIterator != null)
                {
                    Marshal.ReleaseComObject(deckLinkIterator);
                    deckLinkIterator = null;
                }

            }

            return Success;
        }

        public static List<DeckLinkDeviceDescription> GetDeckLinkInputDevices()
        {
            List<DeckLinkDeviceDescription> devices = new List<DeckLinkDeviceDescription>();
            IDeckLinkIterator deckLinkIterator = null;
            try
            {
                deckLinkIterator = new CDeckLinkIterator();

                int index = 0;
                IDeckLink deckLink = null;
                do
                {
                    if (deckLink != null)
                    {
                        Marshal.ReleaseComObject(deckLink);
                        deckLink = null;
                    }

                    deckLinkIterator.Next(out deckLink);

                    if (deckLink == null)
                    {
                        break;
                    }

                    deckLink.GetDisplayName(out string deviceName);

                    try
                    {
                        var deckLinkInput = (IDeckLinkInput)deckLink;
                        var deckLinkStatus = (IDeckLinkStatus)deckLink;
                        var deckLinkAttrs = (IDeckLinkProfileAttributes)deckLink;

                        deckLinkAttrs.GetString(_BMDDeckLinkAttributeID.BMDDeckLinkDeviceHandle, out string deviceHandle);

                        deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
                        bool available = (videoInputSignalLockedFlag != 0);

                        var pixelFormats = SupportedPixelFormats.Keys.ToList();
                        var displayModeIds = GetDisplayDescriptions(deckLinkInput, pixelFormats);

                        DeckLinkDeviceDescription deviceDescription = new DeckLinkDeviceDescription
                        {
                            DeviceHandle = deviceHandle,
                            DeviceIndex = index,
                            DeviceName = deviceName,
                            Available = available,
                            DisplayModeIds = displayModeIds,
                        };

                        devices.Add(deviceDescription);


                        //Marshal.ReleaseComObject(deckLinkInput);
                        //Marshal.ReleaseComObject(deckLinkStatus);

                    }
                    catch (InvalidCastException)
                    {

                    }

                    index++;

                }
                while (deckLink != null);

            }
            catch (Exception ex)
            {
                if (deckLinkIterator == null)
                {
                    throw new Exception("This application requires the DeckLink drivers installed.\n" +
                        "Please install the Blackmagic DeckLink drivers to use the features of this application");
                }

                throw;
            }

            return devices;
        }


        public static List<DeckLinkDisplayModeDescription> GetDisplayDescriptions(IDeckLinkInput deckLinkInput, List<_BMDPixelFormat> pixelFormats,
            _BMDVideoConnection connection = _BMDVideoConnection.bmdVideoConnectionHDMI,
            _BMDSupportedVideoModeFlags videoModeFlags = _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault)
        {

            List<DeckLinkDisplayModeDescription> displayDescriptions = new List<DeckLinkDisplayModeDescription>();

            IDeckLinkDisplayModeIterator iterator = null;
            try
            {
                deckLinkInput.GetDisplayModeIterator(out iterator);

                while (true)
                {
                    IDeckLinkDisplayMode displayMode = null;
                    try
                    {
                        iterator.Next(out displayMode);
                        if (displayMode == null)
                        {
                            break;
                        }

                        var displayModeId = displayMode.GetDisplayMode();

                        displayMode.GetName(out string displayName);
                        displayMode.GetFrameRate(out long frameDuration, out long timeScale);
                        var fps = (double)timeScale / frameDuration;

                        int width = displayMode.GetWidth();
                        int height = displayMode.GetHeight();
                        var resolution = width + "x" + height;

                        var displayModeFlags = displayMode.GetFlags();
                        var fieldDominance = displayMode.GetFieldDominance();

                        foreach (var pixFmt in pixelFormats)
                        {
                            deckLinkInput.DoesSupportVideoMode(connection, displayModeId, pixFmt, videoModeFlags, out int supported);
                            if (supported != 0)
                            {
                                displayDescriptions.Add(new DeckLinkDisplayModeDescription
                                {
                                    ModeId = (long)displayModeId,
                                    Width = width,
                                    Height = height,
                                    Fps = fps,
                                    PixFmt = (long)pixFmt,
                                    Description = displayName + " (" + resolution + " " + fps.ToString("0.##") + " fps " + GetFourCCStr(pixFmt) +  ")",
                                });
                            }
                            else
                            {
                                //Console.WriteLine("Display mode not supported: "+ displayModeId + " " + pixFmt);
                            }
                        }
                    }
                    finally
                    {
                        if (displayMode != null)
                        {
                            Marshal.ReleaseComObject(displayMode);
                            displayMode = null;
                        }
                    }
                }
            }
            finally
            {
                if (iterator != null)
                {
                    Marshal.ReleaseComObject(iterator);
                    iterator = null;
                }
            }

            return displayDescriptions;

        }

        public static string LogDisplayMode(IDeckLinkDisplayMode deckLinkDisplayMode)
        {
            string log = "";

            if (deckLinkDisplayMode != null)
            {
                var displayMode = deckLinkDisplayMode.GetDisplayMode();
                int width = deckLinkDisplayMode.GetWidth();
                int height = deckLinkDisplayMode.GetHeight();
                deckLinkDisplayMode.GetName(out string name);
                var fieldDominance = deckLinkDisplayMode.GetFieldDominance();

                deckLinkDisplayMode.GetFrameRate(out long frameDuration, out long timeScale);
                var fps = ((double)timeScale / frameDuration);
                log = displayMode + " " + width + "x" + height + " " + fps.ToString("0.00") + "fps " + fieldDominance;

            }

            return log;
        }

        public static VideoFormat GetVideoFormat(_BMDPixelFormat pixFormat)
        {
            VideoFormat format = null;
            if (SupportedPixelFormats.ContainsKey(pixFormat))
            {
                format = SupportedPixelFormats[pixFormat];
            }

            return format;
        }

        public static bool ValidateAndCorrectPixelFormat(ref _BMDPixelFormat fmt)
        {
            logger.Debug("ValidateAndCorrectFormat(...)" + fmt);
            bool supported = true;
            if (IsRgbFormat(fmt))
            {// 
                fmt = _BMDPixelFormat.bmdFormat8BitBGRA;
            }
            else if (fmt == _BMDPixelFormat.bmdFormat10BitYUV ||
                fmt == _BMDPixelFormat.bmdFormat8BitYUV ||
                fmt == _BMDPixelFormat.bmdFormatUnspecified)
            {
                fmt = _BMDPixelFormat.bmdFormat8BitYUV;
            }
            else
            {
                // not supported...
                supported = false;
            }

            return supported;
        }



        private static readonly Dictionary<_BMDPixelFormat, VideoFormat> SupportedPixelFormats = new Dictionary<_BMDPixelFormat, VideoFormat>
        {
             { _BMDPixelFormat.bmdFormat8BitYUV, VideoFormats.VideoFormatUYVY },

             // в WinRgb цвета идут в обратном порядке, поэтому что бы не переставлять пиксели
             // используем формат bmdFormat8BitBGRA
             { _BMDPixelFormat.bmdFormat8BitBGRA,  VideoFormats.VideoFormatARGB32 },

             //не поддерживается EVR, VideoProcessorMFT конвертит только софтверно!
             //{ _BMDPixelFormat.bmdFormat10BitYUV, 0x30313256 /*"v210"*/ }, 

             // остальные форматы не поддерживаются!

        };

        public static bool IsRgbFormat(_BMDPixelFormat fmt)
        {
            return RGBPixelFormats.Contains(fmt);
        }

        private static readonly List<_BMDPixelFormat> RGBPixelFormats = new List<_BMDPixelFormat>
        {
              _BMDPixelFormat.bmdFormat8BitARGB,
              _BMDPixelFormat.bmdFormat8BitBGRA,
              _BMDPixelFormat.bmdFormat8BitRGBA,
              _BMDPixelFormat.bmdFormat10BitRGB,
              _BMDPixelFormat.bmdFormat10BitRGBX,
              _BMDPixelFormat.bmdFormat10BitRGBXLE,
              _BMDPixelFormat.bmdFormat10BitRGBXLE_FULL,
              _BMDPixelFormat.bmdFormat10BitRGBX_FULL,
              _BMDPixelFormat.bmdFormat12BitRGB,
              _BMDPixelFormat.bmdFormat12BitRGBLE,
        };


        public static string GetFourCCStr(_BMDPixelFormat pixFormat)
        {
            string fourCCStr = "";

            if (pixFormat == _BMDPixelFormat.bmdFormat8BitARGB)
            {
                fourCCStr = "ARGB";
            }
            else if (pixFormat == _BMDPixelFormat.bmdFormatUnspecified)
            {
                fourCCStr = "";
            }
            else
            {
                long format = (long)pixFormat;
                byte[] fourBytes = new byte[]
                {
                    (byte)(format >> 24),
                    (byte)(format >> 16),
                    (byte)(format >> 8),
                    (byte)(format)
                };

                fourCCStr = Encoding.ASCII.GetString(fourBytes);
            }

            return fourCCStr;
        }


        private static readonly Dictionary<_BMDFieldDominance, int> videoInterlaceModesMap = new Dictionary<_BMDFieldDominance, int>
        {
            { _BMDFieldDominance.bmdUnknownFieldDominance, 0 }, //MFVideoInterlace_Unknown
            { _BMDFieldDominance.bmdProgressiveFrame, 2 }, //MFVideoInterlace_Progressive
            { _BMDFieldDominance.bmdProgressiveSegmentedFrame, 2 }, //MFVideoInterlace_Progressive
            { _BMDFieldDominance.bmdUpperFieldFirst, 3 }, //MFVideoInterlace_FieldInterleavedUpperFirst
            { _BMDFieldDominance.bmdLowerFieldFirst, 4 }, //MFVideoInterlace_FieldInterleavedLowerFirst
        };

        public static int GetVideoInterlaceMode(_BMDFieldDominance fieldDominance)
        {
            int interlaceMode = 0;

            if (videoInterlaceModesMap.ContainsKey(fieldDominance))
            {
                interlaceMode = videoInterlaceModesMap[fieldDominance];
            }

            return interlaceMode;
        }

        
    }

    class SimpleMemoryAllocator : IDeckLinkMemoryAllocator
    {
        private const int S_OK = 0;
        private const int E_OUTOFMEMORY = 0x000E;

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DeckLink");

        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly int maxAllocatedBuffersCount = 8;
        public SimpleMemoryAllocator(int maxBufferCount = 8)
        {
            this.maxAllocatedBuffersCount = maxBufferCount;
        }

        private volatile int allocatedBuffersCount = 0;

        private object syncRoot = new object();

        public int AllocateBuffer(uint size, out IntPtr buffer)
        {// По умолчанию decklink создает буфер на 64 кадра (>500МБ) 
         // соответственно что бы ограничить потребление памяти,
         // говорим, что места есть только на 8 кадров
            buffer = IntPtr.Zero;
            int hResult = S_OK;
            lock (syncRoot)
            {
                if (allocatedBuffersCount < maxAllocatedBuffersCount)
                {
                    buffer = Marshal.AllocCoTaskMem((int)size);
                    allocatedBuffersCount++;

                    //logger.Trace(">>>>> MemoryAllocator::AllocateBuffer(...) " + size + " " + buffer + " " + allocatedBuffersCount);
                }
                else
                {
                    //logger.Trace("AllocateBuffer E_OUTOFMEMORY");
                    hResult = E_OUTOFMEMORY;
                }
            }

            return hResult;

        }

        public int ReleaseBuffer(IntPtr buffer)
        {
            //logger.Trace("SimpleMemoryAllocator::ReleaseBuffer(...) " + buffer);

            lock (syncRoot)
            {
                allocatedBuffersCount--;
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(buffer);
                    buffer = IntPtr.Zero;
                }
            }

            return S_OK;
        }

        public int Commit()
        {
            logger.Debug("SimpleMemoryAllocator::Commit()");
            return S_OK;
        }

        public int Decommit()
        {
            logger.Debug("SimpleMemoryAllocator::Decommit()");

            if (allocatedBuffersCount != 0)
            {
                logger.Warn("Possible memory leak, allocated buffers count: " + allocatedBuffersCount);
            }

            return S_OK;
        }

    }


    class MemoryAllocator : IDeckLinkMemoryAllocator
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DeckLink");

        private const int S_OK = 0;
        private const int E_OUTOFMEMORY = 0x000E;

        private readonly int maxAllocatedBuffersCount = 8;
        public MemoryAllocator(int maxBufferCount = 8)
        {
            this.maxAllocatedBuffersCount = maxBufferCount;
        }

        private volatile int allocateBufferRequest = 0;

        private IntPtr allocatedBuffer = IntPtr.Zero;
        private uint bufferSize = 0;

        private object syncRoot = new object();
        private Queue<IntPtr> bufferQueue = new Queue<IntPtr>();

        private int allocatedBuffersCount = 0;
        public int AllocateBuffer(uint size, out IntPtr buffer)
        {
            //logger.Trace("MemoryAllocator::AllocateBuffer(...) " + size +" " + Thread.CurrentThread.ManagedThreadId);


            if (bufferSize != size)
            {
                if (bufferSize > 0)
                { //Clear queue ...
                    logger.Warn("bufferSize!= size" + bufferSize + " != " + size);
                    ClearBuffer();
                }

                bufferSize = size;

            }

            buffer = IntPtr.Zero;
            lock (syncRoot)
            {
                if (bufferQueue.Count > 0)
                {
                    buffer = bufferQueue.Dequeue();
                    if (buffer == IntPtr.Zero)
                    {
                        logger.Warn("MemoryAllocator::EmptyBuffer " + buffer);
                    }

                    //logger.Trace(">>>>> BufferFromQueue " + size + " " + buffer + " " + allocateBufferRequest + " " + bufferQueue.Count);
                }
                else
                {
                    if (allocatedBuffersCount < maxAllocatedBuffersCount)
                    {
                        buffer = Marshal.AllocHGlobal((int)size);
                        allocatedBuffersCount++;
                        logger.Verb("MemoryAllocator::AllocateBuffer " + buffer + " " + size + " " + allocateBufferRequest);
                    }
                    //else
                    //{
                    //    Console.WriteLine(allocateBufferRequest + " >= " + maxAllocatedBuffersCount);
                    //}
                }

                if (buffer != IntPtr.Zero)
                {
                    allocateBufferRequest++;
                }
                else
                {
                    // Console.WriteLine("E_OUTOFMEMORY " + allocateBufferRequest);
                    return E_OUTOFMEMORY;
                }

            }

            return S_OK;

        }

        public int ReleaseBuffer(IntPtr buffer)
        {
            //logger.Trace("MemoryAllocator::ReleaseBuffer(...) " + buffer + " " + Thread.CurrentThread.ManagedThreadId);

            lock (syncRoot)
            {
                allocateBufferRequest--;
                if (buffer != IntPtr.Zero)
                {
                    bufferQueue.Enqueue(buffer);

                }
            }

            return S_OK;

        }

        public int Commit()
        {
            logger.Debug("MemoryAllocator::Commit() " + Thread.CurrentThread.ManagedThreadId);
            return S_OK;
        }

        public int Decommit()
        {
            logger.Debug("MemoryAllocator::Decommit() " + Thread.CurrentThread.ManagedThreadId);
            ClearBuffer();

            return S_OK;
        }

        public void ClearBuffer()
        {
            logger.Debug("MemoryAllocator::ClearBuffer() " + Thread.CurrentThread.ManagedThreadId);

            lock (syncRoot)
            {
                int bufferCount = 0;
                while (bufferQueue.Count > 0)
                {
                    var buf = bufferQueue.Dequeue();
                    if (buf != IntPtr.Zero)
                    {
                        logger.Debug("MemoryAllocator::FreeBuffer " + buf + " " + bufferCount);
                        Marshal.FreeHGlobal(buf);
                        buf = IntPtr.Zero;
                    }
                    bufferCount++;
                }

                if (bufferCount != allocatedBuffersCount)
                {
                    logger.Warn("Possible memory leak: " + bufferCount + "!=" + allocatedBuffersCount);
                }
            }

        }
    }

    class MemoryAllocator2 : IDeckLinkMemoryAllocator
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DeckLink");

        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private const int S_OK = 0;
        private const int E_OUTOFMEMORY = 0x000E;

        private readonly int maxAllocatedBuffersCount = 8;
        public MemoryAllocator2(int maxBufferCount = 8)
        {
            this.maxAllocatedBuffersCount = maxBufferCount;
        }

        private volatile int allocateBufferRequest = 0;

        private IntPtr allocatedBuffer = IntPtr.Zero;
        private uint bufferSize = 0;

        private object syncRoot = new object();
        private Queue<IntPtr> frameCache = new Queue<IntPtr>();

        private int allocatedBuffersCount = 0;
        public int AllocateBuffer(uint size, out IntPtr buffer)
        {
            //logger.Trace("MemoryAllocator::AllocateBuffer(...) " + size +" " + Thread.CurrentThread.ManagedThreadId);

            int hResult = S_OK;
            if (bufferSize != size)
            {
                if (bufferSize > 0)
                { //Clear queue ...
                    logger.Warn("bufferSize!= size" + bufferSize + " != " + size);
                    ClearBuffer();
                }

                bufferSize = size;

            }

            buffer = IntPtr.Zero;
            lock (syncRoot)
            {
                if (frameCache.Count == 0)
                {
                    buffer = Marshal.AllocHGlobal((int)size);
                    allocatedBuffersCount++;
                    logger.Debug("MemoryAllocator::AllocateBuffer " + buffer + " " + size + " " + allocateBufferRequest);
                }
                else
                {
                    buffer = frameCache.Dequeue();
                    if (buffer == IntPtr.Zero)
                    {
                        logger.Warn("MemoryAllocator::EmptyBuffer " + buffer);
                    }
                }


            }

            return hResult;

        }

        public int ReleaseBuffer(IntPtr buffer)
        {
            //logger.Trace("MemoryAllocator::ReleaseBuffer(...) " + buffer + " " + Thread.CurrentThread.ManagedThreadId);

            lock (syncRoot)
            {
                if (frameCache.Count < maxAllocatedBuffersCount)
                {
                    frameCache.Enqueue(buffer);
                    logger.Debug("MemoryAllocator::ReleaseBuffer " + frameCache.Count);
                }
                else
                {
                    if (buffer != IntPtr.Zero)
                    {
                        logger.Debug("MemoryAllocator::FreeBuffer " + buffer);
                        Marshal.FreeHGlobal(buffer);
                        buffer = IntPtr.Zero;
                    }
                }
            }

            return S_OK;

        }

        public int Commit()
        {
            logger.Debug("MemoryAllocator::Commit() " + Thread.CurrentThread.ManagedThreadId);
            return S_OK;
        }

        public int Decommit()
        {
            logger.Debug("MemoryAllocator::Decommit() " + Thread.CurrentThread.ManagedThreadId);
            ClearBuffer();

            return S_OK;
        }

        public void ClearBuffer()
        {
            logger.Debug("MemoryAllocator::ClearBuffer() " + Thread.CurrentThread.ManagedThreadId);

            lock (syncRoot)
            {
                int bufferCount = 0;
                while (frameCache.Count > 0)
                {
                    var buf = frameCache.Dequeue();
                    if (buf != IntPtr.Zero)
                    {
                        logger.Debug("MemoryAllocator::FreeBuffer " + buf + " " + bufferCount);
                        Marshal.FreeHGlobal(buf);
                        buf = IntPtr.Zero;
                    }
                    bufferCount++;
                }

                if (bufferCount != allocatedBuffersCount)
                {
                    logger.Warn("Possible memory leak: " + bufferCount + "!=" + allocatedBuffersCount);
                }
            }

        }
    }



}
