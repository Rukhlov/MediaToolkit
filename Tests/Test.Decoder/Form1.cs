using MediaToolkit.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Decoder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            videoPanel.Resize += VideoPanel_Resize;
        }

 
        VideoDecoderPresenter test = null;

        private INalSourceReader sourceReader = null;
		string fileName = "";
		int width = 1280;
		int height = 720;
		int fps = 30;
		bool realTime = true;
		bool lowLatency = true;


		private void buttonRun_Click(object sender, EventArgs e)
		{
			Console.WriteLine("VideoDecoderTest::Run()");
			try
			{
				MediaToolkit.Core.VideoDriverType driverType = MediaToolkit.Core.VideoDriverType.Cuda;


				//// string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_30fps_1sec_bf0.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_1sec.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_30fps_60sec.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_Iframe.h264";
				//var width = 320;
				//var height = 240;
				//var fps = 30;

				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_640x480_yuv420p_4frame.h264";
				//var width = 640;
				//var height = 480;\


				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv444p_30fps_30sec_bf0.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\smptebars_1280x720_nv12_30fps_30sec_bf0.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec.h264";
				fileName = @"..\..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec_bf0.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_nv12_30fps_30sec_bf0.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_Iframe.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_1fps_30sec_bf0.h264";
				width = 1280;
				height = 720;
				fps = 30;


                //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\test_mov_annexb_1920x1080_5sec.h264";
                ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1920x1080_yuv420p_30fps_30sec_bf0.h264";
                //var width = 1920;
                //var height = 1080;
                //var fps = 30;

                //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_2560x1440_yuv420p_Iframe.h264";
                //var width = 2560;
                //var height = 1440;
                //var fps = 30;

                //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\test_mov_annexb_3840x2160_5sec.h264";
                ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_3840x2160_yuv420p_30fps_10sec_bf0.h264";
                ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_3840x2160_yuv420p_Iframe.h264";
                //var width = 3840;
                //var height = 2160;
                //var fps = 30;



                //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\test_mov_annexb_4096x2304_5sec.h264";
                ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_3840x2160_yuv420p_Iframe.h264";
                //var width = 4096;
                //var height = 2304;
                //var fps = 30;



                test = new VideoDecoderPresenter();
				try
				{
					if(sourceReader == null)
					{
						if (realTime)
						{
							sourceReader = new NalSourceReaderRealTime();
						}
						else
						{
							sourceReader = new NalSourceReader();
						}
					}


                    test.sourceReader = sourceReader;

                    test.hWnd = this.videoPanel.Handle;

                    var inputArgs = new MfVideoArgs
                    {
                        Width = width,
                        Height = height,

                        //Width = 320,
                        //Height = 240,
                        FrameRate = MfTool.PackToLong(fps, 1),
                        LowLatency = lowLatency,
                    };

                    test.Start(inputArgs, driverType);
                    test.Resize(videoPanel.ClientSize);



                  

                }
				finally
				{

					//test.Close();
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

		}

        private void VideoPanel_Resize(object sender, EventArgs e)
        {
            if (test != null)
            {
                test.Resize(videoPanel.ClientSize);
            }

        }

		private void button1_Click(object sender, EventArgs e)
		{
			if (test != null)
			{
				test.Stop();
			}
		}

		private void buttonSourceStart_Click(object sender, EventArgs e)
		{

			if (sourceReader == null)
			{
				if (realTime)
				{
					sourceReader = new NalSourceReaderRealTime();
				}
				else
				{
					sourceReader = new NalSourceReader();
				}
			}

			fileName = @"..\..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec_bf0.h264";

			var inputArgs = MediaToolkit.Codecs.NalUnitReader.Probe(fileName);

			if(inputArgs == null)
			{

				throw new InvalidOperationException("Not supported h264 stream: " + fileName);
			}

			if(inputArgs.FrameRate == 0)
			{
				inputArgs.FrameRate = MfTool.PackToLong(30, 1);
			}

			//width = 1280;
			//height = 720;
			//fps = 30;

			//var inputArgs = new MfVideoArgs
			//{
			//	Width = width,
			//	Height = height,
			//	FrameRate = MfTool.PackToLong(fps, 1),

			//};

			sourceReader.Start(fileName, inputArgs);
		}

		private void buttonSourceStop_Click(object sender, EventArgs e)
		{
			if (sourceReader != null)
			{
				sourceReader.Stop();
			}
			
		}
	}
}
