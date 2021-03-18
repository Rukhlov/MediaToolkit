using DeckLinkAPI;
using MediaToolkit.Logging;
using MediaToolkit.SharedTypes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.BMDDeckLink
{
    public class DeckLinkDeviceManager : IDeckLinkDeviceNotificationCallback
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DeckLink");

        private IDeckLinkDiscovery deckLinkDiscovery;

        public bool Started { get; private set; } = false;

        public event Action<DeckLinkDeviceDescription> InputDeviceArrived;
        public event Action<DeckLinkDeviceDescription> InputDeviceRemoved;

        private List<DeckLinkInput> inputDevices = new List<DeckLinkInput>();

        public bool Startup()
        {
            logger.Debug("DeckLinkDeviceManager::Startup()");

            if (Started)
            {
                Debug.Assert(deckLinkDiscovery != null, "deckLinkDiscovery!=null");

                return Started;
            }

            try
            {
                deckLinkDiscovery = new CDeckLinkDiscovery();
                deckLinkDiscovery.InstallDeviceNotifications(this);

                Started = true;

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Started = false;
            }

            return Started;
        }


        public void Shutdown()
        {
            logger.Debug("DeckLinkDeviceManager::Shutdown()");

            Started = false;

            if (deckLinkDiscovery != null)
            {
                deckLinkDiscovery.UninstallDeviceNotifications();

                var refCount = Marshal.ReleaseComObject(deckLinkDiscovery);

                Debug.Assert(refCount == 0, "refCount == 0");
                deckLinkDiscovery = null;
            }

            if (inputDevices.Count > 0)
            {
                for (int i = 0; i < inputDevices.Count; i++)
                {
                    var inputDevice = inputDevices[i];
                    if (inputDevice != null)
                    {
                        inputDevice.Dispose();
                        inputDevice = null;
                    }
                }
            }
        }


        void IDeckLinkDeviceNotificationCallback.DeckLinkDeviceArrived(IDeckLink device)
        {
            logger.Verb("IDeckLinkDeviceNotificationCallback::DeckLinkDeviceArrived()");

            DeckLinkDeviceDescription inputDescr = null;
            var inputDevice = new DeckLinkInput(device);
            if (inputDevice.Init())
            {
                inputDescr = inputDevice.GetDeviceDescription();
                inputDevices.Add(inputDevice);

                InputDeviceArrived?.Invoke(inputDescr);
            }

            if(inputDescr == null)
            {//  нас интересуют только decklink input
                if (inputDevice != null)
                {
                    inputDevice.Dispose();
                    inputDevice = null;
                }

                if (device != null)
                {
                    Marshal.ReleaseComObject(device);
                    device = null;
                }
            }

        }



        void IDeckLinkDeviceNotificationCallback.DeckLinkDeviceRemoved(IDeckLink device)
        {// Удаление не проверялось т.к не было в наличии устройств поддерживающий динамическое отключение
            logger.Verb("IDeckLinkDeviceNotificationCallback::DeckLinkDeviceRemoved()");

            try
            { 
                var inputDevice =  inputDevices.FirstOrDefault(d => d.deckLink == device);

                DeckLinkDeviceDescription inputDescr = inputDevice?.GetDeviceDescription();
                if (inputDescr != null)
                {
                    bool removed = inputDevices.Remove(inputDevice);
                    Debug.Assert(removed, "removed");

                    InputDeviceRemoved?.Invoke(inputDescr);
                }

                if (inputDevice != null)
                {
                    inputDevice.Dispose();
                    inputDevice = null;
                }
            }
            finally
            {
                if (device != null)
                {
                    var refCount = Marshal.ReleaseComObject(device);
                    Debug.Assert(refCount == 0, "refCount == 0");

                    device = null;
                }

            }
        }

        public List<DeckLinkDeviceDescription> GetInputsFromMTA()
        {
            logger.Debug("DeckLinkDeviceManager::GetInputsFromMTA()");

            //IDeckLink требует MTA поэтому вызываем его из потока
            return GetInputsAsync().Result;

        }

        public async Task<List<DeckLinkDeviceDescription>> GetInputsAsync()
        {
            logger.Debug("DeckLinkDeviceManager::GetDeckLinkInputsAsync()");

            return await Task.Factory.StartNew(() =>
            {
                List<DeckLinkDeviceDescription> deviceDescriptions = GetInputs();

                return deviceDescriptions;

            }).ConfigureAwait(false);

        }

        public List<DeckLinkDeviceDescription> GetInputs()
        {
            List<DeckLinkDeviceDescription> deviceDescriptions = new List<DeckLinkDeviceDescription>();

            for (int index = 0; index < inputDevices.Count; index++)
            {
                var inputDevice = inputDevices[index];

                DeckLinkDeviceDescription deviceDescription = inputDevice.GetDeviceDescription();
                if (deviceDescription != null)
                {
                    deviceDescription.DeviceIndex = index;

                    deviceDescriptions.Add(deviceDescription);
                }
            }

            return deviceDescriptions;
        }

        public List<DeckLinkDeviceDescription> FindInputs()
        {
            logger.Debug("DeckLinkDeviceManager::GetDeckLinkInputs()");

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

                    var inputDevice = new DeckLinkInput(deckLink);
                    if (inputDevice.Init())
                    {
                        DeckLinkDeviceDescription deviceDescription = inputDevice.GetDeviceDescription();
                        if (deviceDescription != null)
                        {
                            deviceDescription.DeviceIndex = index;

                            devices.Add(deviceDescription);
                            index++;
                        }
                    }

                    if (inputDevice != null)
                    {
                        inputDevice.Dispose();
                        inputDevice = null;
                    }

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

            return devices;
        }

        class DeckLinkInput
        {
            internal IDeckLink deckLink = null;
            internal IDeckLinkInput deckLinkInput = null;
            internal IDeckLinkStatus deckLinkStatus = null;
            internal IDeckLinkProfileAttributes deckLinkAttrs = null;

            internal DeckLinkInput(IDeckLink _deckLink)
            {
                deckLink = _deckLink;
            }
            private bool initialized = false;
            internal bool Init()
            {

                try
                {
                    deckLinkInput = (IDeckLinkInput)deckLink;
                    deckLinkStatus = (IDeckLinkStatus)deckLink;
                    deckLinkAttrs = (IDeckLinkProfileAttributes)deckLink;
                    initialized = true;
                }
                catch (InvalidCastException)
                {
                    //logger.Warn("");
                }

                return initialized;
            }

            internal DeckLinkDeviceDescription GetDeviceDescription()
            {
                if (!initialized)
                {
                    return null;
                }

                DeckLinkDeviceDescription deviceDescription = null;

                deckLink.GetDisplayName(out string deviceName);

                deckLinkAttrs.GetString(_BMDDeckLinkAttributeID.BMDDeckLinkDeviceHandle, out string deviceHandle);

                deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
                bool available = (videoInputSignalLockedFlag != 0);

                deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusBusy, out long deviceBusyStateFlag);
                _BMDDeviceBusyState deviceBusyState = (_BMDDeviceBusyState)deviceBusyStateFlag;
                bool isBusy = (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy);

                deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusCurrentVideoInputMode, out long bmdDeckLinkStatusCurrentVideoInputModeFlag);
                _BMDDisplayMode displayModeId = (_BMDDisplayMode)bmdDeckLinkStatusCurrentVideoInputModeFlag;

                deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusCurrentVideoInputPixelFormat, out long currentVideoInputPixelFormatFlag);
                _BMDPixelFormat pixelFormat = (_BMDPixelFormat)currentVideoInputPixelFormatFlag;

                DeckLinkDisplayModeDescription displayDescription = null;
                IDeckLinkDisplayMode displayMode = null;
                try
                {
                    deckLinkInput.GetDisplayMode(displayModeId, out displayMode);

                    int width = displayMode.GetWidth();
                    int height = displayMode.GetHeight();
                    displayMode.GetFrameRate(out long duration, out long scale);
                    displayMode.GetName(out string displayName);
                   
                    displayDescription = new DeckLinkDisplayModeDescription
                    {
                        Width = width,
                        Height = height,
                        PixFmt = (long)pixelFormat,
                        Fps = ((double)scale / duration),
                        Description = displayName,

                    };
                }
                finally
                {
                    if (displayMode != null)
                    {
                        Marshal.ReleaseComObject(displayMode);
                    }

                }

                deviceDescription = new DeckLinkDeviceDescription
                {
                    DeviceHandle = deviceHandle,
                    DeviceName = deviceName,
                    Available = available,
                    IsBusy = isBusy,
                    DisplayModeIds = new List<DeckLinkDisplayModeDescription> { displayDescription },
                };

                return deviceDescription;
            }

            internal void Dispose()
            {
                if (deckLink != null)
                {
                    var refCount = Marshal.ReleaseComObject(deckLink);
                    Debug.Assert(refCount == 0, "refCount == 0");

                    deckLink = null;

                    deckLinkStatus = null;
                    deckLinkInput = null;
                    deckLinkAttrs = null;

                    initialized = false;

                }
            }
        }

    }

}
