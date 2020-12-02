using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{
    public class PciDeviceInfo
    {
        //https://docs.microsoft.com/en-us/windows-hardware/drivers/install/identifiers-for-pci-devices
        /*
		 * PCI\\VEN_v(4)&DEV_d(4)&SUBSYS_s(4)n(4)&REV_r(2)
		   PCI\\VEN_v(4)&DEV_d(4)&SUBSYS_s(4)n(4)
		   PCI\\VEN_v(4)&DEV_d(4)&REV_r(2)
		   PCI\\VEN_v(4)&DEV_d(4)
		   PCI\\VEN_v(4)&DEV_d(4)&CC_c(2)s(2)p(2)
		   PCI\\VEN_v(4)&DEV_d(4)&CC_c(2)s(2)
		   Where:

			   v(4) is the four-character PCI SIG-assigned identifier for the vendor of the device, where the term device,
				   following PCI SIG usage, refers to a specific PCI chip.
				   As specified in Publishing restrictions, 0000 and FFFF are invalid codes for this identifier.

			   d(4) is the four-character vendor-defined identifier for the device.

			   s(4) is the four-character vendor-defined subsystem identifier.

			   n(4) is the four-character PCI SIG-assigned identifier for the vendor of the subsystem.
				   As specified in Publishing restrictions, 0000 and FFFF are invalid codes for this identifier.
			   r(2) is the two-character revision number.

			   c(2) is the two-character base class code from the configuration space.

			   s(2) is the two-character subclass code.

			   p(2) is the Programming Interface code.

		 */


        private PciDeviceInfo(string deviceIdString)
        {
            this.deviceIdString = deviceIdString;
        }

        public readonly string deviceIdString = "";
        public int VendorId { get; private set; }
        public string Vendor { get; private set; }

        public int DeviceId { get; private set; }
        public string Device { get; private set; }

        //...
        public string SubSys { get; private set; }
        public string Revision { get; private set; }
        public string ClassCode { get; private set; }

        public static PciDeviceInfo Parse(string deviceIdString)
        {
            PciDeviceInfo info = new PciDeviceInfo(deviceIdString);

            string _deviceIdString = deviceIdString.ToLower().Replace(" ", "");

            if (!_deviceIdString.StartsWith("pci\\"))
            {// invalid string ...

            }

            _deviceIdString = _deviceIdString.Replace("pci\\", "");

            var strings = _deviceIdString.Split('&');

            foreach (var substring in strings)
            {
                if (substring.StartsWith("ven_"))
                {
                    var vendorStr = substring.Substring(4);
                    if (vendorStr.Length != 4)
                    {// invalid string ...
                    }
                    int vendorId = int.Parse(vendorStr, System.Globalization.NumberStyles.HexNumber);
                    info.VendorId = vendorId;
                    info.Vendor = vendorStr;

                }
                else if (substring.StartsWith("dev_"))
                {
                    var deviceStr = substring.Substring(4);
                    if (deviceStr.Length != 4)
                    {// invalid string ...
                    }
                    int deviceId = int.Parse(deviceStr, System.Globalization.NumberStyles.HexNumber);
                    info.DeviceId = deviceId;
                    info.Device = deviceStr;
                }
                else if (substring.StartsWith("subsys_"))
                {
                    info.SubSys = substring.Substring(7);
                }
                else if (substring.StartsWith("rev_"))
                {
                    info.Revision = substring.Substring(4);
                }
                else if (substring.StartsWith("cc_"))
                {
                    info.ClassCode = substring.Substring(3);
                }
            }

            return info;
        }

        public override string ToString()
        {
            var str = "pci\\";
            if (!string.IsNullOrEmpty(Vendor))
            {
                str += ("ven_" + Vendor);
            }
            if (!string.IsNullOrEmpty(Device))
            {
                str += ("&dev_" + Device);
            }
            if (!string.IsNullOrEmpty(SubSys))
            {
                str += ("&subsys_" + SubSys);
            }
            if (!string.IsNullOrEmpty(Revision))
            {
                str += ("&rev_" + Revision);
            }
            if (!string.IsNullOrEmpty(ClassCode))
            {
                str += ("&cc_" + ClassCode);
            }

            return str.ToUpper();
        }
    }


}
