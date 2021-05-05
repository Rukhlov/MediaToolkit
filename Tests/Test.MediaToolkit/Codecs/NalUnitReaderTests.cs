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
	public class NalUnitReaderTests
	{
		[TestMethod()]
		public void ReadNextTest()
		{
			{
				var srcarray = new byte[] { 0, 0, 0, 1, 3 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 1);

				var nalBytes = nals[0];
				Assert.AreEqual(nalBytes.Length, 1);
				Assert.AreEqual(nalBytes[0], 3);
			}

			{
				var srcarray = new byte[] { 0, 0, 0, 0, 3 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 0);
			}

			{
				 var srcarray = new byte[] { 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 1);

				var nalBytes = nals[0];
				Assert.AreEqual(nalBytes.Length, 4);
				CollectionAssert.AreEqual(new byte[] { 0, 0, 0, 0 }, nalBytes);
			}

			{
				var srcarray = new byte[] { 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 1);

				var nalBytes = nals[0];

				CollectionAssert.AreEqual(new byte[] { 0 }, nalBytes);
			}

			{
				var srcarray = new byte[] { 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 1);

				var nalBytes = nals[0];

				CollectionAssert.AreEqual(new byte[] { 3, 3, 3, 3, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 }, nalBytes);
			}

			{
				var srcarray = new byte[] { 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 4);

				CollectionAssert.AreEqual(new byte[] { 3, 3, 3, 3 }, nals[0]);
				CollectionAssert.AreEqual(new byte[] { 4, 4, 4, 4 }, nals[1]);
				CollectionAssert.AreEqual(new byte[] { 5, 5, 5, 5 }, nals[2]);
				CollectionAssert.AreEqual(new byte[] { 6, 6, 6, 6 }, nals[3]);
			}

			{
				var srcarray = new byte[] { 0, 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 2);

				CollectionAssert.AreEqual(new byte[] { 3, 3, 3, 3, 0}, nals[0]);
				CollectionAssert.AreEqual(new byte[] { 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 }, nals[1]);
			}

			{
				var srcarray = new byte[] { 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };
				MemoryStream stream = new MemoryStream(srcarray);
				NalUnitReader reader = new NalUnitReader(stream);
				List<byte[]> nals = new List<byte[]>();

				bool dataAvailable = false;
				do
				{
					dataAvailable = reader.ReadNext(out var nal);
					if (nal != null && nal.Length > 0)
					{
						nals.Add(nal);
					}

				} while (dataAvailable);

				Assert.AreEqual(nals.Count, 4);

				CollectionAssert.AreEqual(new byte[] { 3, 3, 3, 3 }, nals[0]);
				CollectionAssert.AreEqual(new byte[] { 4, 4, 4, 4 }, nals[1]);
				CollectionAssert.AreEqual(new byte[] { 5, 5, 5, 5 }, nals[2]);
				CollectionAssert.AreEqual(new byte[] { 6, 6, 6, 6 }, nals[3]);

			}


			//var srcarray = new byte[] { 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 };
			//var srcarray = new byte[] { 0, 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 };
			//var srcarray = new byte[] { 0, 0, 0, 0, 3, 3, 3, 3, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 };
			//var srcarray = new byte[] { 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };
			//
			// var srcarray = new byte[] { 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };

			//Assert.Fail();
		}

		[TestMethod()]
		public void HandleH264AnnexbFramesTest()
		{
			{
				var srcarray = new byte[] { 0, 0, 0, 1, 3 };

				var nals = NalUnitReader.HandleH264AnnexbFrames(srcarray);
				Assert.AreEqual(nals.Count, 1);
				var bytes0 = DumpSegemnt(nals[0]);

				CollectionAssert.AreEqual(new byte[] { 3 }, bytes0);
			}

			{
				var srcarray = new byte[] { 0, 0, 0, 0, 3 };

				var nals = NalUnitReader.HandleH264AnnexbFrames(srcarray);
				Assert.AreEqual(nals.Count, 0);
			}

			{
				var srcarray = new byte[] { 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };

				var nals = NalUnitReader.HandleH264AnnexbFrames(srcarray);
				Assert.AreEqual(nals.Count, 4);

				CollectionAssert.AreEqual(new byte[] { 3, 3, 3, 3 }, DumpSegemnt(nals[0]));
				CollectionAssert.AreEqual(new byte[] { 4, 4, 4, 4 }, DumpSegemnt(nals[1]));
				CollectionAssert.AreEqual(new byte[] { 5, 5, 5, 5 }, DumpSegemnt(nals[2]));
				CollectionAssert.AreEqual(new byte[] { 6, 6, 6, 6 }, DumpSegemnt(nals[3]));

			}


			//{// 
			//	var srcarray = new byte[] { 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

			//	var nals = NalUnitReader.HandleH264AnnexbFrames(srcarray);
			//	Assert.AreEqual(nals.Count, 2);
			//	var bytes0 = DumpSegemnt(nals[0]);
			//	var bytes1 = DumpSegemnt(nals[1]);
			//	//CollectionAssert.AreEqual(new byte[] { 0 }, ));

			//}
		}

		private static byte[] DumpSegemnt(ArraySegment<byte> s)
		{
			var bytes = new byte[s.Count];
			Array.Copy(s.Array, s.Offset, bytes, 0, bytes.Length);

			return bytes;

		}
	}
}