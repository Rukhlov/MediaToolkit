using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaToolkit.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MediaToolkit.Codecs.Tests
{
    [TestClass()]
    public class SequenceParameterSetTests
    {
        [TestMethod()]
        public void SequenceParameterSetTest()
        {
			{
				byte[] nal = { 00, 00, 00, 01, 0x67, 0x64, 00, 0x28, 0xAD, 0x84, 0x3F, 0xFF, 0xC2, 0x1F, 0xFF, 0xE1, 0x0F, 0xFF, 0xF0, 0x87, 0xFF, 0xF8, 0x43, 0xFF, 0xFC, 0x21, 0xFF, 0xFE, 0x10, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x08, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x2C, 0xC5, 0x01, 0xE0, 0x11, 0x3F, 0x78, 0x0A, 0x10, 0x10, 0x10, 0x1F, 0x00, 0x00, 0x03, 0x03, 0xE8, 0x00, 0x00, 0xC3, 0x50, 0x94 };


				var rbsp = NalUnitReader.NalToRbsp(nal, 5);
				bool success = SequenceParameterSet.TryParse(rbsp, out var sps);
				Assert.AreEqual(success, true);

				Assert.AreEqual(sps.Profile, 100);
				Assert.AreEqual(sps.Level, 40);
				Assert.AreEqual(sps.Width, 1920);
				Assert.AreEqual(sps.Height, 1084);
				Assert.AreEqual(sps.FixedFrameRate, true);
				Assert.AreEqual(sps.TickRate.Item1, 1000);
				Assert.AreEqual(sps.TickRate.Item2, 50000);
			}

			{
				byte[] nal = { 0x67, 0x64, 0x00, 0x1e, 0xac, 0xd9, 0x40, 0xa0, 0x2f, 0xf9, 0x61, 0x00, 0x00, 0x03, 0x00, 0x7d, 0x00, 0x00, 0x17, 0x6a, 0x0f, 0x16, 0x2d, 0x96 };

				var rbsp = NalUnitReader.NalToRbsp(nal, 1);
				bool success = SequenceParameterSet.TryParse(rbsp, out var sps);
				Assert.AreEqual(success, true);

				Assert.AreEqual(sps.Profile, 100);
				Assert.AreEqual(sps.Level, 30);

				Assert.AreEqual(sps.Width, 640);
				Assert.AreEqual(sps.Height, 360);
				Assert.AreEqual(sps.FixedFrameRate, false);
				Assert.AreEqual(sps.TickRate.Item1, 125);
				Assert.AreEqual(sps.TickRate.Item2, 5994);
			}

			{
			    byte[] nal = { 00, 00, 00, 01, 0x67, 0x42, 0x80, 0x1E, 0x95, 0xA0, 0x50, 0x7C, 0x84, 0x00, 0x00, 0x0F };

				var rbsp = NalUnitReader.NalToRbsp(nal, 5);
				bool success = SequenceParameterSet.TryParse(rbsp, out var sps);

				Assert.AreEqual(success, false);
				Assert.AreEqual(sps.Profile, 66);
				Assert.AreEqual(sps.Level, 30);
				Assert.AreEqual(sps.Width, 320);
				Assert.AreEqual(sps.Height, 240);
				Assert.AreEqual(sps.FixedFrameRate, false);
				Assert.AreEqual(sps.TickRate.Item1, 0);
				Assert.AreEqual(sps.TickRate.Item2, 0);
			}

			{
				byte[] nal = { 00, 00, 00, 01, 0x67, 0x42, 0x00, 0x1E, 0x9A, 0x74, 0x05, 0x81, 0xEC, 0x80 };

				var rbsp = NalUnitReader.NalToRbsp(nal, 5);
				bool success = SequenceParameterSet.TryParse(rbsp, out var sps);

				Assert.AreEqual(success, true);
				Assert.AreEqual(sps.Level, 30);
				Assert.AreEqual(sps.Profile, 66);
				Assert.AreEqual(sps.Width, 704);
				Assert.AreEqual(sps.Height, 480);

				Assert.AreEqual(sps.FixedFrameRate, false);
				Assert.AreEqual(sps.TickRate.Item1, 0);
				Assert.AreEqual(sps.TickRate.Item2, 0);
			}





		}
	}
}