using MediaToolkit.DirectX;
using MediaToolkit.MediaFoundation;
using NLog;
using SharpDX.DXGI;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

		public Form1()
		{
			InitializeComponent();
			videoPanel.Resize += VideoPanel_Resize;

			DecoderPars = new DecoderParams();

			InitDecoderTypes();
			InitVideoFiles();
            InitVideoAdapters();

            UpdateControls();
		}

		private List<ComboBoxItem> decoderTypes = new List<ComboBoxItem>();
		private List<ComboBoxItem> videoFiles = new List<ComboBoxItem>();

		VideoDecoderPresenter decoder = null;

		private INalSourceReader sourceReader = null;

		//bool realTime = true;
		//bool lowLatency = true;

		private DecoderParams DecoderPars = null;

		//private MfVideoArgs inputArgs = null;
		private void InitDecoderTypes()
		{
			decoderTypes.Clear();

			decoderTypes.Add(new ComboBoxItem
			{
				Name = "DX11VA",
				Tag = VideoDecoderType.D3D11VA,
            });

            decoderTypes.Add(new ComboBoxItem
            {
                Name = "DXVA2",
                Tag = VideoDecoderType.Dxva2,
            });

            decoderTypes.Add(new ComboBoxItem
			{
				Name = "DXVA2(CPU)",
				Tag = VideoDecoderType.Dxva2Software,
			});

            decoderTypes.Add(new ComboBoxItem
            {
                Name = "FFmpeg",
                Tag = VideoDecoderType.FFmpeg,
            });

            decoderTypes.Add(new ComboBoxItem
			{
				Name = "NVDEC",
				Tag = VideoDecoderType.NvDec,
            });

			comboBoxDecoderTypes.DataSource = decoderTypes;
			comboBoxDecoderTypes.DisplayMember = "Name";
			comboBoxDecoderTypes.ValueMember = "Tag";

		}

        private List<ComboBoxItem> videoAdapters = new List<ComboBoxItem>();
        private void InitVideoAdapters()
        {
            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {
                var adapters = dxgiFactory.Adapters1;
                for(int adapterIndex= 0; adapterIndex < adapters.Length; adapterIndex++)
                {
                    var adapter = adapters[adapterIndex];
                    var descr = adapter.Description1;
                    if (!descr.Flags.HasFlag(AdapterFlags.Software))
                    {
                        videoAdapters.Add(new ComboBoxItem
                        {
                            Name = descr.Description,
                            Tag = adapterIndex,
                        });
                    }
                }

                foreach (var a in adapters)
                {
                    DxTool.SafeDispose(a);
                }
            }


            comboBoxVideoAdapters.DataSource = videoAdapters;
            comboBoxVideoAdapters.DisplayMember = "Name";
            comboBoxVideoAdapters.ValueMember = "Tag";
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

				foreach (var file in files)
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


        private int GetVideoAdapterIndex()
        {
            int index = 0;

            var item = comboBoxVideoAdapters.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                index = (int)item.Tag;
            }

            return index;
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

		private VideoDecoderType GetDecoderType()
		{
            VideoDecoderType decoderType = VideoDecoderType.Dxva2;

            var item = comboBoxDecoderTypes.SelectedItem as ComboBoxItem;
			if (item != null)
			{
				decoderType = (VideoDecoderType)item.Tag;
			}

			return decoderType;
		}

		private bool decoderStarted = false;

		private void buttonDecoderStart_Click(object sender, EventArgs e)
		{
            logger.Debug("buttonDecoderStart_Click(...)");
			try
			{
				if (!decoderStarted)
				{
					StartDecoder();
				}
				else
				{
					StopDecoder();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				MessageBox.Show(ex.Message);
			}

		}

		private void StartDecoder()
		{

			var fileName = GetVideoFile();

			decoder = new VideoDecoderPresenter();
			decoder.StateChanged += Decoder_StateChanged;
			decoder.PresenterStateChanged += Decoder_PresenterStateChanged;

			DecoderPars.DecoderType = GetDecoderType(); 
            DecoderPars.VideoAdapterIndex = GetVideoAdapterIndex();

			DecoderPars.hWnd = this.videoPanel.Handle;
			var sourceReader = GetSourceReader();

			decoder.Start(DecoderPars, sourceReader);

		}


		private void StopDecoder()
		{
			if (decoder != null)
			{
				decoder.Stop();

				decoder.Close();
			}
		}

		private void Decoder_PresenterStateChanged(bool started)
		{
			if (started)
			{
				decoder.Resize(videoPanel.ClientSize);
			}
			else
			{
				videoPanel.Invalidate();

				decoder.PresenterStateChanged -= Decoder_PresenterStateChanged;
			}

		}

		private void Decoder_StateChanged(bool started)
		{
			this.decoderStarted = started;

			if (!started)
			{
				decoder.Close();
				decoder.StateChanged -= Decoder_StateChanged;
			}
			else
			{

			}

			UpdateControls();
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
            logger.Debug("buttonDecoderStop_Click(...)");
			StopDecoder();
		}



		private void buttonSourceStart_Click(object sender, EventArgs e)
		{
            logger.Debug("buttonSourceStart_Click(...)");
			try
			{
				if (!isSourceStarted)
				{
					StartSource();

				}
				else
				{
					StopSource();
				}
			}
			catch(Exception ex)
			{
                logger.Error(ex);
				MessageBox.Show(ex.Message);
			}

		}

		private void buttonSourceStop_Click(object sender, EventArgs e)
		{
            logger.Debug("buttonSourceStop_Click(...)");

			StopSource();

		}

		private void StartSource()
		{
			var sourceReader = GetSourceReader();

			var fileName = GetVideoFile();

			var sps = MediaToolkit.Codecs.NalUnitReader.Probe(fileName);

			if (sps == null)
			{
				throw new InvalidOperationException("Not supported h264 stream: " + fileName);
			}

			var maxFps = (int)sps.MaxFps;
			if (maxFps == 0)
			{
				maxFps = 30;
			}

			DecoderPars.Width = sps.Width;
			DecoderPars.Height = sps.Height;
			DecoderPars.FrameRate = new MediaToolkit.Core.MediaRatio((int)maxFps, 1);
			var bufferSize = (int)numericUpDownBufferSize.Value;

			var interval = (double)1.0 / maxFps;
			var readerTask = sourceReader.Start(fileName, interval, bufferSize);
		}

		private void StopSource()
		{
			if (sourceReader != null)
			{
				sourceReader.Stop();
			}
		}

		private void comboBoxVideoFiles_SelectedValueChanged(object sender, EventArgs e)
		{
            logger.Debug("comboBoxVideoFiles_SelectedValueChanged(...)");

			try
			{
				var fileName = GetVideoFile();
				var sps = MediaToolkit.Codecs.NalUnitReader.Probe(fileName);
				if (sps != null)
				{
                    this.Text = fileName;
					this.labelWidth.Text = sps.Width.ToString();
					this.labelHeight.Text = sps.Height.ToString();

					this.labelLevel.Text = (sps.Level / 10.0).ToString("0.0");
					this.labelProfile.Text = sps.ProfileName;

					var fps = sps.MaxFps;

					var maxFps = (int)sps.MaxFps;
					if (maxFps == 0)
					{
						maxFps = 30;
					}

					DecoderPars.Width = sps.Width;
					DecoderPars.Height = sps.Height;
					DecoderPars.FrameRate = new MediaToolkit.Core.MediaRatio((int)maxFps, 1);

					this.labelFps.Text = !double.IsNaN(fps) ? fps.ToString("0.0") : "-";
				}
				else
				{
                    this.Text = "";
					this.labelWidth.Text = "-";
					this.labelHeight.Text = "-";
					this.labelFps.Text = "-";
					this.labelLevel.Text = "-";
					this.labelProfile.Text = "-";
				}

                UpdateControls();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

		}


		private INalSourceReader GetSourceReader()
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

				sourceReader.StateChanged += SourceReader_StateChanged;
			}


			return sourceReader;
		}

		private bool isSourceStarted = false;
		private void SourceReader_StateChanged(bool started)
		{
            logger.Debug("SourceReader_StateChanged(...) " + started);
			this.isSourceStarted = started;

			UpdateControls();
		}


		private void UpdateControls()
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() =>
                {
                    _updateControls();

                }));
			}
			else
			{
                _updateControls();
            }

		}

        private void _updateControls()
        {
            //buttonSourceStart.Enabled = !isSourceStarted;
            //buttonSourceStop.Enabled = isSourceStarted;
            buttonSourceStart.Text = isSourceStarted ? "Stop Source" : "Start Source";
            comboBoxVideoFiles.Enabled = !isSourceStarted;
			numericUpDownBufferSize.Enabled = !isSourceStarted;

			buttonDecoderStart.Text = decoderStarted ? "Stop Decoder" : "Start Decoder";
            comboBoxDecoderTypes.Enabled = !decoderStarted;

            checkBoxDebugInfo.Enabled = decoderStarted;
            checkBoxAspectRatio.Enabled = decoderStarted;

            checkBoxAspectRatio.Checked = decoder?.AspectRatio ?? false;
            checkBoxDebugInfo.Checked = decoder?.ShowLabel ?? false;

            comboBoxVideoAdapters.Enabled = !decoderStarted;
			

			numericFps.Enabled = isSourceStarted;
			
            var fps = 30;
            if (sourceReader != null)
            {
                fps = (int)(1.0 / sourceReader.PacketInterval);
            }

            if (fps > numericFps.Maximum)
            {
                fps = (int)numericFps.Maximum;
            }
            if (fps < numericFps.Minimum)
            {
                fps = (int)numericFps.Minimum;
            }
            if (numericFps.Value != fps)
            {
                numericFps.Value = fps;
            }

            //buttonDecoderStart.Enabled = !decoderStarted;
            //buttonDecoderStop.Enabled = decoderStarted;
        }

        private void checkBoxDebugInfo_CheckedChanged(object sender, EventArgs e)
		{
            logger.Debug("checkBoxDebugInfo_CheckedChanged(...)");

            if (decoder != null)
			{
				decoder.ShowLabel = checkBoxDebugInfo.Checked;
			}
		}

		private void checkBoxAspectRatio_CheckedChanged(object sender, EventArgs e)
		{
            logger.Debug("checkBoxAspectRatio_CheckedChanged(...)");

            if (decoder != null)
			{
				decoder.AspectRatio = checkBoxAspectRatio.Checked;
			}
		}

        private void numericFps_ValueChanged(object sender, EventArgs e)
        {
            //logger.Debug("numericFps_ValueChanged(...)");

            var fps = (int)numericFps.Value;

            var interval = (double)1.0 / fps;
            if (sourceReader != null)
            {
                sourceReader.PacketInterval = interval;
            }
        }


		protected override void OnClosed(EventArgs e)
		{
			logger.Debug("OnClosed(...)");
			base.OnClosed(e);

			StopDecoder();
			StopSource();
		}
	}

    class ComboBoxItem
	{
		public string Name { get; set; }
		public object Tag { get; set; }
	}
}
