using MediaToolkit.MediaFoundation;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Encoder
{
	class MftH264Test
	{
		public static void Run()
		{

			SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();
			var index = 1;

			var adapter = factory1.GetAdapter(index);
			var _flags = DeviceCreationFlags.Debug;

			var device = new SharpDX.Direct3D11.Device(adapter, _flags);

			using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
			{
				multiThread.SetMultithreadProtected(true);
			}



			var transformFlags = TransformEnumFlag.All | // TransformEnumFlag.All |
								 TransformEnumFlag.SortAndFilter;

			var outputType = new TRegisterTypeInformation
			{
				GuidMajorType = MediaTypeGuids.Video,
				GuidSubtype = VideoFormatGuids.H264
			};

			Transform encoder = null;

			var activates = MediaFactory.FindTransform(TransformCategoryGuids.VideoEncoder, transformFlags, null, outputType);


            //foreach (var activate in activates)
            //{
            //	string name = activate.Get(TransformAttributeKeys.MftFriendlyNameAttribute);
            //	Guid clsid = activate.Get(TransformAttributeKeys.MftTransformClsidAttribute);
            //	TransformEnumFlag flags = (TransformEnumFlag)activate.Get(TransformAttributeKeys.TransformFlagsAttribute);

            //	bool isAsync = !(flags.HasFlag(TransformEnumFlag.Syncmft));
            //	isAsync |= (flags.HasFlag(TransformEnumFlag.Asyncmft));
            //	bool isHardware = flags.HasFlag(TransformEnumFlag.Hardware);

            //	if (isHardware)
            //	{


            //                 string venIdStr = activate.Get(TransformAttributeKeys.MftEnumHardwareVendorIdAttribute);
            //                 var adapterVenId = adapter.Description.VendorId;

            //                 if (MfTool.TryGetVendorId(venIdStr, out int activatorVendId))
            //                 {
            //                     if (activatorVendId == adapterVenId)
            //                     {
            //                         encoder = activate.ActivateObject<Transform>();
            //                         break;
            //                     }
            //                 }    
            //	}
            //}

            encoder = activates[0].ActivateObject<Transform>();

            foreach (var activator in activates)
			{
				activator.Dispose();
			}


			if (encoder == null)
			{
				throw new NotSupportedException("Hardware encode acceleration is not available on this platform.");
			}

			using (var attr = encoder.Attributes)
			{
				var log = MfTool.LogMediaAttributes(attr);
				Console.WriteLine(log);

				var transformAsync = (attr.Get(TransformAttributeKeys.TransformAsync) == 1);
				if (transformAsync)
				{
					attr.Set(TransformAttributeKeys.TransformAsyncUnlock, 1);

                    bool d3d11Aware = attr.Get(TransformAttributeKeys.D3D11Aware);
                    if (d3d11Aware)
                    {
                        using (var devMan = new DXGIDeviceManager())
                        {
                            devMan.ResetDevice(device);
                            //attr.Set(SinkWriterAttributeKeys.D3DManager, devMan);

                            encoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
                        }
                    }
                }

				attr.Set(CodecApiPropertyKeys.AVLowLatencyMode, true);
			}

			var inputStreamId = 0;
			var outputStreamId = 0;

			int inputStreamCount = -1;
			int outputStreamsCount = -1;
			encoder.GetStreamCount(out inputStreamCount, out outputStreamsCount);
			int[] inputStreamIDs = new int[inputStreamCount];
			int[] outputStreamIDs = new int[outputStreamsCount];

			bool res = encoder.TryGetStreamIDs(inputStreamIDs, outputStreamIDs);
			if (res)
			{
				inputStreamId = inputStreamIDs[0];
				outputStreamId = outputStreamIDs[0];
			}
			else
			{
				inputStreamId = 0;
				outputStreamId = 0;
			}


			MediaType OutputMediaType = null;
			for (int i = 0; ; i++)
			{
				if (!encoder.TryGetOutputAvailableType(outputStreamId, i, out MediaType mediaType))
				{
					//
					Console.WriteLine("NoMoreOutputTypes");
					break;
				}
                var log = MfTool.LogMediaType(mediaType);
                Console.WriteLine(log);

                mediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
				mediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(640, 480));
				mediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(30, 1));
				mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

				//mediaType.Set(MediaTypeAttributeKeys.Mpeg2Profile, mpegProfile);
				//mediaType.Set(MediaTypeAttributeKeys.AvgBitrate, avgBitrate);
				//mediaType.Set(CodecApiPropertyKeys.AVEncCommonMaxBitRate, maxBitrate);

				//mediaType.Set(CodecApiPropertyKeys.AVEncMPVDefaultBPictureCount, 0);

				encoder.SetOutputType(outputStreamId, mediaType, 0);

				OutputMediaType = mediaType;

				var _mediaLog = MfTool.LogMediaType(mediaType);
				Console.WriteLine("\r\nOutputMediaType:\r\n-----------------\r\n" + _mediaLog);

				//logger.Debug("\r\n" + i + ". AvailableOutputMediaType:\r\n-----------------\r\n" + mediaLog);
				//mediaType.Dispose();
				//mediaType = null;
				break;
			}

			var inputFormat = VideoFormatGuids.Argb32;
			MediaType InputMediaType = null;
			for (int i = 0; ; i++)
			{
				try
				{
					encoder.GetInputAvailableType(0, i, out MediaType availableType);

					var log = MfTool.LogMediaType(availableType);
					Console.WriteLine("\r\n" + i + ". AvalibleInputMediaType:\r\n-----------------\r\n" + log);

					var formatId = availableType.Get(MediaTypeAttributeKeys.Subtype);
					//if (formatId == inputFormat)
					//{
					//	InputMediaType = availableType;
					//	availableType = null;
					//	//logger.Debug("inputFormat " + inputFormat);
					//	break;
					//}

					if (availableType != null)
					{
						availableType.Dispose();
						availableType = null;
					}
				}
				catch (SharpDX.SharpDXException ex)
				{
					if (ex.ResultCode != SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
					{
						throw;
					}
					break;
				}
			}

		}

	}
}
