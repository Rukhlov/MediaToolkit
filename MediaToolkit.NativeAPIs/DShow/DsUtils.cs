using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;

namespace MediaToolkit.NativeAPIs.DShow
{
    public class DsUtils
    {
        public static void ShowVideoDevicePropertyPages(string deviceName, IntPtr hWnd)
        {
            try
            {
                var device = GetDeviceFilterByName(deviceName, FilterCategory.VideoInputDevice);
                if (device != null)
                {
                    ShowDevicePropertyPages(device, hWnd);
                }
                else
                {
                    //not found...
                }
               
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        public static IBaseFilter GetDeviceFilterByName(string deviceName, Guid filterCategory)
		{ //https://docs.microsoft.com/en-us/windows/win32/directshow/selecting-a-capture-device
			int hResult = 0;

            IBaseFilter foundFilter = null;
            IEnumMoniker classEnum = null;
            try
            {
                ICreateDevEnum devEnum = null;
                try
                {
                    devEnum = (ICreateDevEnum)new CreateDevEnum();
                    hResult = devEnum.CreateClassEnumerator(filterCategory, out classEnum, 0);

                    DsError.ThrowExceptionForHR(hResult);
                }
                finally
                {
                    if (devEnum != null)
                    {
                        Marshal.ReleaseComObject(devEnum);
                    }

                }

                if (classEnum != null)
                {
                    IMoniker[] monikerItems = new IMoniker[1];
                    while (classEnum.Next(monikerItems.Length, monikerItems, IntPtr.Zero) == 0)
                    {
                        var moniker = monikerItems[0];
                        try
                        {
                            Guid iid = typeof(IPropertyBag).GUID;

                            moniker.BindToStorage(null, null, ref iid, out var props);

                            IPropertyBag property = (props as IPropertyBag);
                            if (property != null)
                            {
								////A unique string that identifies the device. (Video capture devices only.)
								//property.Read("DevicePath", out var devicePath, null);

								////The identifier for an audio capture device. (Audio capture devices only.)
								//property.Read("WaveInID", out var waveInID, null);

								hResult = property.Read("FriendlyName", out var friendlyName, null);

                                if (deviceName == friendlyName.ToString())
                                {
                                    iid = typeof(IBaseFilter).GUID;
                                    moniker.BindToObject(null, null, ref iid, out var filter);
                                    foundFilter = (IBaseFilter)filter;

                                    break;
                                }
                            }
                        }
                        finally
                        {
                            if (moniker != null)
                            {
                                Marshal.ReleaseComObject(moniker);
                            }
                        }
                    }
                }

            }
            finally
            {
                if (classEnum != null)
                {
                    Marshal.ReleaseComObject(classEnum);
                }

            }

            return foundFilter;
        }

        public static void ShowDevicePropertyPages(IBaseFilter filter, IntPtr hWndOwner)
        {
            int hResult = 0;

            ISpecifyPropertyPages propPages = filter as ISpecifyPropertyPages;
            if (propPages != null)
            {
                try
                {
                    // get the name of the filter from the FilterInfo struct
                    hResult = filter.QueryFilterInfo(out var filterInfo);
                    DsError.ThrowExceptionForHR(hResult);

                    // get the propertypages from the property bag
                    hResult = propPages.GetPages(out var caGUID);
                    DsError.ThrowExceptionForHR(hResult);

                    // create and display the OlePropertyFrame
                    object[] oDevice = new[] { (object)filter };
                    hResult = Ole.OleAut32.OleCreatePropertyFrame(hWndOwner, 0, 0, filterInfo.achName, 1, oDevice, caGUID.cElems, caGUID.ToGuidArray(), 0, 0, 0);
                    DsError.ThrowExceptionForHR(hResult);

                    // release COM objects
                    if (caGUID.pElems != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(caGUID.pElems);
                    }

                    if (filterInfo.pGraph != null)
                    {
                        Marshal.ReleaseComObject(filterInfo.pGraph);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(propPages);
                }
            }
            else
            {
                // if the filter doesn't implement ISpecifyPropertyPages, try displaying IAMVfwCompressDialogs instead
                IAMVfwCompressDialogs compressDialog = filter as IAMVfwCompressDialogs;
                if (compressDialog != null)
                {
                    hResult = compressDialog.ShowDialog(VfwCompressDialogs.Config, IntPtr.Zero);
                    DsError.ThrowExceptionForHR(hResult);
                }
                return;
            }

        }

    }

    static public class DsError
    {
        [DllImport("quartz.dll", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "AMGetErrorTextW"), SuppressUnmanagedCodeSecurity]
        public static extern int AMGetErrorText(int hr, StringBuilder buf, int max);

        /// <summary>
        /// If hr has a "failed" status code (E_*), throw an exception.  Note that status
        /// messages (S_*) are not considered failure codes.  If DirectShow error text
        /// is available, it is used to build the exception, otherwise a generic com error
        /// is thrown.
        /// </summary>
        /// <param name="hr">The HRESULT to check</param>
        public static void ThrowExceptionForHR(int hr)
        {
            // If a severe error has occurred
            if (hr < 0)
            {
                string s = GetErrorText(hr);

                // If a string is returned, build a com error from it
                if (s != null)
                {
                    throw new COMException(s, hr);
                }
                else
                {
                    // No string, just use standard com error
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }

        /// <summary>
        /// Returns a string describing a DS error.  Works for both error codes
        /// (values < 0) and Status codes (values >= 0)
        /// </summary>
        /// <param name="hr">HRESULT for which to get description</param>
        /// <returns>The string, or null if no error text can be found</returns>
        public static string GetErrorText(int hr)
        {
            const int MAX_ERROR_TEXT_LEN = 160;

            // Make a buffer to hold the string
            StringBuilder buf = new StringBuilder(MAX_ERROR_TEXT_LEN, MAX_ERROR_TEXT_LEN);

            // If a string is returned, build a com error from it
            if (AMGetErrorText(hr, buf, MAX_ERROR_TEXT_LEN) > 0)
            {
                return buf.ToString();
            }

            return null;
        }
    }
}
