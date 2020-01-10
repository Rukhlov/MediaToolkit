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

        public static string GetPixelFormatFourCC(_BMDPixelFormat pixFormat)
        {
            string fourCC = "";
            if (BMDPixelFormatsToFourCCDict.ContainsKey(pixFormat))
            {
                fourCC = BMDPixelFormatsToFourCCDict[pixFormat];
            }

            return fourCC;
        }

        private static readonly Dictionary<_BMDPixelFormat, string> BMDPixelFormatsToFourCCDict = new Dictionary<_BMDPixelFormat, string>
        {
             { _BMDPixelFormat.bmdFormat8BitYUV, "UYVY" },
             { _BMDPixelFormat.bmdFormat10BitYUV, "v210" }, //не поддерживается EVR, VideoProcessorMFT конвертит только софтверно!
            
            // остальные форматы не проверялись....
            // { _BMDPixelFormat.bmdFormat8BitARGB, "RGBA" },
        };
    }



    class MemoryAllocator : IDeckLinkMemoryAllocator
    {
        public void AllocateBuffer(uint bufferSize, out IntPtr allocatedBuffer)
        {
            allocatedBuffer = Marshal.AllocCoTaskMem((int)bufferSize);
        }

        public void Commit()
        {
        }

        public void Decommit()
        {
        }

        public void ReleaseBuffer(IntPtr buffer)
        {
            Marshal.FreeCoTaskMem(buffer);
        }
    }
}
