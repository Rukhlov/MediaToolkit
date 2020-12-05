using MediaToolkit.NativeAPIs.Utils;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Encoder
{
	class DisplayDeviceTest
	{
		public static void PCIDeviceIdTest()
		{
			Console.WriteLine("GetDisplayInfoTest::PCIDeviceIdTest() BEGIN");

			string test1 = @"PCI\VEN_8086 & DEV_1912 & SUBSYS_86941043 & REV_06";
			string test2 = @"PCI\VEN_10DE&DEV_1401&SUBSYS_854D1043&REV_A1";

			Console.WriteLine(test1);
			PciDeviceInfo info1 = PciDeviceInfo.Parse(test1);
			Console.WriteLine(info1.ToString());
			Console.WriteLine("--------------------------");

			Console.WriteLine(test2);
			PciDeviceInfo info2 = PciDeviceInfo.Parse(test2);
			Console.WriteLine(info2.ToString());
			Console.WriteLine("--------------------------");

			Console.WriteLine("GetDisplayInfoTest::PCIDeviceIdTest() END");
		}


		public static void GetDisplayInfoTest()
		{
			Console.WriteLine("GetDisplayInfoTest::Run() BEGIN");

			var displayDevices = DisplayUtil.EnumDisplayDevices();
			foreach (var adapter in displayDevices.Keys)
			{
				Console.WriteLine("-----------------------------");
				Console.WriteLine(adapter);

				var pciInfo = PciDeviceInfo.Parse(adapter.DeviceID);
				Console.WriteLine("VenId " + pciInfo.VendorId + " DevId " + pciInfo.DeviceId);

			}

			Console.WriteLine("-----------------------------");

			Console.WriteLine(MediaToolkit.DirectX.DxTool.LogDxInfo());


			Console.WriteLine("GetDisplayInfoTest::Run() END");
		}

	}
}
