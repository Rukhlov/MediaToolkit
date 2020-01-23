using DeckLinkAPI;
using MediaToolkit.SharedTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.DeckLink
{
    public class DeckLinkDeviceManager : IDeckLinkDeviceNotificationCallback
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IDeckLinkDiscovery deckLinkDiscovery;

        public bool Started { get; private set; } = false;

        public event Action<DeckLinkDeviceDescription> InputDeviceArrived;
        public event Action<DeckLinkDeviceDescription> InputDeviceRemoved;

        public bool StartUp()
        {
            if (Started)
            {
                return Started;
            }

            logger.Debug("DeckLinkDeviceManager::StartUp()");
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

                Marshal.ReleaseComObject(deckLinkDiscovery);
                deckLinkDiscovery = null;
            }
        }


        void IDeckLinkDeviceNotificationCallback.DeckLinkDeviceArrived(IDeckLink device)
        {
            logger.Trace("IDeckLinkDeviceNotificationCallback::DeckLinkDeviceArrived()");

            try
            {
                var deviceDescr = GetInputDeviceDescription(device);
                if (deviceDescr != null)
                {
                    InputDeviceArrived?.Invoke(deviceDescr);
                }

            }
            finally
            {
                if (device != null)
                {
                    Marshal.ReleaseComObject(device);
                    device = null;
                }
            }
        }



        void IDeckLinkDeviceNotificationCallback.DeckLinkDeviceRemoved(IDeckLink device)
        {
            logger.Trace("IDeckLinkDeviceNotificationCallback::DeckLinkDeviceRemoved()");

            try
            {
                var deviceDescr = GetInputDeviceDescription(device);
                if (deviceDescr != null)
                {
                    InputDeviceRemoved?.Invoke(deviceDescr);
                }

            }
            finally
            {
                if(device != null)
                {
                    Marshal.ReleaseComObject(device);
                    device = null;
                }

            }
        }

        public List<DeckLinkDeviceDescription> GetDeckLinkInputs()
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

                    DeckLinkDeviceDescription deviceDescription = GetInputDeviceDescription(deckLink);
                    if (deviceDescription != null)
                    {
                        deviceDescription.DeviceIndex = index;

                        devices.Add(deviceDescription);
                        index++;
                    }
                }
                while (deckLink != null);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return devices;
        }

        private static DeckLinkDeviceDescription GetInputDeviceDescription(IDeckLink deckLink)
        {

            if (deckLink == null)
            {
                return null;
            }

            DeckLinkDeviceDescription deviceDescription = null;

            try
            {
                deckLink.GetDisplayName(out string deviceName);

                var deckLinkInput = (IDeckLinkInput)deckLink;
                var deckLinkStatus = (IDeckLinkStatus)deckLink;
                var deckLinkAttrs = (IDeckLinkProfileAttributes)deckLink;

                deckLinkAttrs.GetString(_BMDDeckLinkAttributeID.BMDDeckLinkDeviceHandle, out string deviceHandle);

                deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
                bool available = (videoInputSignalLockedFlag != 0);

                deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusBusy, out long deviceBusyStateFlag);
                _BMDDeviceBusyState deviceBusyState = (_BMDDeviceBusyState)deviceBusyStateFlag;
                bool isBusy = (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy);

                deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusCurrentVideoInputMode, out long bmdDeckLinkStatusCurrentVideoInputModeFlag);
                _BMDDisplayMode displayModeId = (_BMDDisplayMode)bmdDeckLinkStatusCurrentVideoInputModeFlag;

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

            }
            catch (InvalidCastException)
            {

            }


            return deviceDescription;
        }


    }

}
