using DeckLinkAPI;
using NLog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.DeckLink
{
    public class DeckLinkTools
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static bool GetDeviceByIndex(int inputIndex, out IDeckLink deckLink)
        {
            logger.Trace("GetDeviceByIndex(...) " + inputIndex);

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

        public static bool GetDeviceByName(string deviceName, out IDeckLink deckLink)
        {
            logger.Trace("GetDeviceByName(...) " + deviceName);

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
                        deckLink.GetDisplayName( out string name);
                        if(deviceName == name)
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

        public static int GetPixelFormatFourCC(_BMDPixelFormat pixFormat)
        {
            int fourCC = 0;
            if (BMDPixelFormatsToFourCCDict.ContainsKey(pixFormat))
            {
                fourCC = BMDPixelFormatsToFourCCDict[pixFormat];
            }

            return fourCC;
        }

        private static readonly Dictionary<_BMDPixelFormat, int> BMDPixelFormatsToFourCCDict = new Dictionary<_BMDPixelFormat, int>
        {
             //https://docs.microsoft.com/en-us/windows/win32/medfound/video-subtype-guids
             { _BMDPixelFormat.bmdFormat8BitYUV, 0x59565955 /*"UYVY" */},
             { _BMDPixelFormat.bmdFormat8BitBGRA, 0x00000015 /*MFVideoFormat_ARGB32*/},

             //{ _BMDPixelFormat.bmdFormat10BitYUV, 0x30313256 /*"v210"*/ }, //не поддерживается EVR, VideoProcessorMFT конвертит только софтверно!
             // остальные форматы не проверялись....
        };
    }



    class MemoryAllocator : IDeckLinkMemoryAllocator
    {
       // private volatile int count = 0;
        private IntPtr allocatedBuffer = IntPtr.Zero;
        private uint bufferSize = 0;
     
        public void AllocateBuffer(uint size, out IntPtr buffer)
        {// тестовый буффер на один кадр

            if (allocatedBuffer == IntPtr.Zero)
            {
                allocatedBuffer = Marshal.AllocHGlobal((int)size);
                bufferSize = size;
            }

            if (bufferSize < size)
            {
                if (allocatedBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(allocatedBuffer);
                }

                allocatedBuffer = Marshal.AllocHGlobal((int)size);
                bufferSize = size;
            }

            buffer = allocatedBuffer;//Marshal.AllocHGlobal((int)bufferSize);
        }

        public void Commit()
        {
        }

        public void Decommit()
        {
        }

        public void ReleaseBuffer(IntPtr buffer)
        {
            //if (buffer != IntPtr.Zero)
            //{
            //    Marshal.FreeHGlobal(buffer);
            //}

        }

        public void Dispose()
        {
            if (allocatedBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(allocatedBuffer);
            }
        }
    }
}
