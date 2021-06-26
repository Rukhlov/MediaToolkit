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

            DecoderPars = new DecoderParams
            {

            };

            InitDriverTypes();
			InitVideoFiles();
		}

		private List<ComboBoxItem> driverTypes = new List<ComboBoxItem>();
		private List<ComboBoxItem> videoFiles = new List<ComboBoxItem>();

		VideoDecoderPresenter decoder = null;

		private INalSourceReader sourceReader = null;

		//bool realTime = true;
		//bool lowLatency = true;

        private DecoderParams DecoderPars = null;

        //private MfVideoArgs inputArgs = null;
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

				decoder = new VideoDecoderPresenter();
				try
				{
					if (sourceReader == null)
					{
						if (DecoderPars.RealTimeMode)
						{
							sourceReader = new NalSourceReaderRealTime();
						}
						else
						{
							sourceReader = new NalSourceReader();
						}
					}


					decoder.sourceReader = sourceReader;

					decoder.hWnd = this.videoPanel.Handle;

					decoder.Start(DecoderPars);
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
            if (decoder != null)
            {
                decoder.Resize(videoPanel.ClientSize);
            }

        }

		private void buttonDecoderStop_Click(object sender, EventArgs e)
		{
            Console.WriteLine("buttonDecoderStop_Click(...)");

			if (decoder != null)
			{
				decoder.Stop();

				decoder.Close();
			}
		}

		private void buttonSourceStart_Click(object sender, EventArgs e)
		{
            Console.WriteLine("buttonSourceStart_Click(...)");
            if (sourceReader == null)
			{
				if (DecoderPars.RealTimeMode)
				{
					sourceReader = new NalSourceReaderRealTime();
				}
				else
				{
					sourceReader = new NalSourceReader();
				}
			}

			var fileName = GetVideoFile();

			var sps = MediaToolkit.Codecs.NalUnitReader.Probe(fileName);

			if(sps == null)
			{
				throw new InvalidOperationException("Not supported h264 stream: " + fileName);
			}

            var maxFps = (int)sps.MaxFps;
            if(maxFps == 0)
            {
                maxFps = 30;
            }

            DecoderPars.Width = sps.Width;
            DecoderPars.Height = sps.Height;
            DecoderPars.FrameRate = new MediaToolkit.Core.MediaRatio((int)maxFps, 1);

            var interval = (double)1.0 / maxFps;
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
				var sps = MediaToolkit.Codecs.NalUnitReader.Probe(fileName);
				if (sps != null)
				{
					this.labelWidth.Text = sps.Width.ToString();
					this.labelHeight.Text = sps.Height.ToString();

                    var fps = sps.MaxFps;


                    var maxFps = (int)sps.MaxFps;
                    if (maxFps == 0)
                    {
                        maxFps = 30;
                    }

                    DecoderPars.Width = sps.Width;
                    DecoderPars.Height = sps.Height;
                    DecoderPars.FrameRate = new MediaToolkit.Core.MediaRatio((int)maxFps, 1);

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
