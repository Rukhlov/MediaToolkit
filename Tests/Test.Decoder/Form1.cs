using MediaToolkit.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

			InitDriverTypes();
			InitVideoFiles();
		}

		private List<ComboBoxItem> videoFiles= new List<ComboBoxItem>();
		private void InitVideoFiles()
		{
			videoFiles.Clear();

			var basePath = AppDomain.CurrentDomain.BaseDirectory;
			var filesPath = Path.Combine(basePath, "Files");

			var di = new DirectoryInfo(filesPath);
			if (di.Exists)
			{
				var files = di.GetFiles("*.h264");

				foreach(var file in files)
				{
					var fileName = file.Name;
					var name = Path.GetFileNameWithoutExtension(fileName);

					videoFiles.Add(new ComboBoxItem
					{
						Name = name,
						Tag = file,
					});
				}
			}


			comboBoxVideoFiles.DataSource = videoFiles;
			comboBoxVideoFiles.DisplayMember = "Name";
			comboBoxVideoFiles.ValueMember = "Tag";

		}

		private FileInfo GetSourceFileInfo()
		{

			FileInfo fileName = null;

			var item = comboBoxVideoFiles.SelectedItem as ComboBoxItem;
			if (item != null)
			{
				fileName = (FileInfo)item.Tag;
			}

			return fileName;
		}

		private void InitDriverTypes()
		{
			driverTypes.Clear();

			driverTypes.Add(new ComboBoxItem
			{
				Name = "DXVA2",
				Tag = MediaToolkit.Core.VideoDriverType.D3D9,
			});

			driverTypes.Add(new ComboBoxItem
			{
				Name = "DX11VA",
				Tag = MediaToolkit.Core.VideoDriverType.D3D11,
			});

			driverTypes.Add(new ComboBoxItem
			{
				Name = "CPU",
				Tag = MediaToolkit.Core.VideoDriverType.CPU,
			});

			driverTypes.Add(new ComboBoxItem
			{
				Name = "NVDec",
				Tag = MediaToolkit.Core.VideoDriverType.Cuda,
			});

			comboBoxDriverType.DataSource = driverTypes;
			comboBoxDriverType.DisplayMember = "Name";
			comboBoxDriverType.ValueMember = "Tag";

		}

		private MediaToolkit.Core.VideoDriverType GetDriverType()
		{
			MediaToolkit.Core.VideoDriverType driverType = MediaToolkit.Core.VideoDriverType.Unknown;

			var item = comboBoxDriverType.SelectedItem as ComboBoxItem;
			if (item != null)
			{
				driverType = (MediaToolkit.Core.VideoDriverType)item.Tag;
			}

			return driverType;
		}

		private List<ComboBoxItem> driverTypes = new List<ComboBoxItem>();

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
				MediaToolkit.Core.VideoDriverType driverType = GetDriverType();

				var fi = GetSourceFileInfo();
				if (fi.Exists)
				{
					//fileName = @"..\..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec_bf0.h264";
					fileName = fi.FullName;
				}
				else
				{
					throw new InvalidOperationException("File not found: " + fi.Name);
				}

			

				width = 1280;
				height = 720;
				fps = 30;

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

			//fileName = @"..\..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec_bf0.h264";

			var fi = GetSourceFileInfo();
			if (fi.Exists)
			{
				//fileName = @"..\..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec_bf0.h264";
				fileName = fi.FullName;
			}
			else
			{
				throw new InvalidOperationException("File not found: " + fi.Name);
			}

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

	class ComboBoxItem
	{
		public string Name { get; set; }
		public object Tag { get; set; }
	}
}
