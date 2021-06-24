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

		private List<ComboBoxItem> driverTypes = new List<ComboBoxItem>();
		private List<ComboBoxItem> videoFiles = new List<ComboBoxItem>();

		VideoDecoderPresenter test = null;

		private INalSourceReader sourceReader = null;

		bool realTime = true;
		bool lowLatency = true;

		private MfVideoArgs inputArgs = null;
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

		private string GetVideoFile()
		{
			string fileName;
			var fi = GetSourceFileInfo();
			if (fi.Exists)
			{
				fileName = fi.FullName;
			}
			else
			{
				throw new InvalidOperationException("File not found: " + fi.Name);
			}

			return fileName;
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

		private void buttonDecoderStart_Click(object sender, EventArgs e)
		{
			Console.WriteLine("buttonDecoderStart_Click()");
			try
			{
				MediaToolkit.Core.VideoDriverType driverType = GetDriverType();

				var fileName = GetVideoFile();

				test = new VideoDecoderPresenter();
				try
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


					test.sourceReader = sourceReader;

					test.hWnd = this.videoPanel.Handle;


					inputArgs.LowLatency = lowLatency;

					test.Start(inputArgs, driverType);
					//test.Resize(videoPanel.ClientSize);

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

		private void buttonDecoderStop_Click(object sender, EventArgs e)
		{
            Console.WriteLine("buttonDecoderStop_Click(...)");

			if (test != null)
			{
				test.Stop();

				test.Close();
			}
		}

		private void buttonSourceStart_Click(object sender, EventArgs e)
		{
            Console.WriteLine("buttonSourceStart_Click(...)");
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

			var fileName = GetVideoFile();

			inputArgs = MediaToolkit.Codecs.NalUnitReader.Probe(fileName);

			if(inputArgs == null)
			{

				throw new InvalidOperationException("Not supported h264 stream: " + fileName);
			}

			if(inputArgs.FrameRate == 0)
			{
				inputArgs.FrameRate = MfTool.PackToLong(30, 1);
			}

			var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);
			var interval = (double)frameRate[1] / frameRate[0];

			sourceReader.Start(fileName, interval);
		}

		private void buttonSourceStop_Click(object sender, EventArgs e)
		{
            Console.WriteLine("buttonSourceStop_Click(...)");

            if (sourceReader != null)
			{
				sourceReader.Stop();
			}
			
		}

		private void comboBoxVideoFiles_SelectedValueChanged(object sender, EventArgs e)
		{
            Console.WriteLine("comboBoxVideoFiles_SelectedValueChanged(...)");

            try
			{
				var fileName = GetVideoFile();
				inputArgs = MediaToolkit.Codecs.NalUnitReader.Probe(fileName);
				if (inputArgs != null)
				{
					this.labelWidth.Text = inputArgs.Width.ToString();
					this.labelHeight.Text = inputArgs.Height.ToString();

					var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);
					var fps = (double)frameRate[0] / frameRate[1];

					this.labelFps.Text = !double.IsNaN(fps) ? fps.ToString("0.0"): "-";
				}
				else
				{
					this.labelWidth.Text = "-";
					this.labelHeight.Text = "-";
					this.labelFps.Text = "-";
				}

			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

		}
	}

	class ComboBoxItem
	{
		public string Name { get; set; }
		public object Tag { get; set; }
	}
}
