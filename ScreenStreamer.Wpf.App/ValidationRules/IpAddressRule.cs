using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ScreenStreamer.Wpf.ValidationRules
{
    public class IpAddressRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(IPAddress.TryParse(value.ToString(), out _), "Please provide valid IP address value");
        }
    }
}
