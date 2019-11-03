using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaToolkit;
using System.Net.NetworkInformation;
using MediaToolkit.Common;
using System.Diagnostics;

namespace TestStreamer.Controls
{
    public partial class ScreenStreamerControl : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ScreenStreamerControl()
        {
            InitializeComponent();

            LoadEncoderProfilesItems();
            LoadScreenItems();
            LoadTransportItems();
            LoadEncoderItems();

            LoadRateModeItems();

            UpdateControls();

            statInfoCheckBox.Checked = showStatistic;

        }

        private readonly MainForm mainForm = null;

        private bool isStreaming = false;
        private ScreenStreamer videoStreamer = null;
        private ScreenSource screenSource = null;


        private StatisticForm statisticForm = new StatisticForm();
        private PreviewForm previewForm = null;

        private RegionForm regionForm = null;

        private void startButton_Click(object sender, EventArgs e)
        {
            logger.Debug("startButton_Click(...)");

            var localAddr = "0.0.0.0";


            //var ipInfo = GetCurrentIpAddrInfo();
            //if (ipInfo != null)
            //{
            //    localAddr = ipInfo.Address?.ToString();
            //}


            int fps = (int)fpsNumeric.Value;
            int bitrate = (int)bitrateNumeric.Value;
            int maxBitrate = (int)MaxBitrateNumeric.Value;

            bool latencyMode = latencyModeCheckBox.Checked;

            H264Profile h264Profile = (H264Profile)encProfileComboBox.SelectedItem;

            BitrateControlMode bitrateMode = (BitrateControlMode)bitrateModeComboBox.SelectedItem;

            bool showMouse = showMouseCheckBox.Checked;

            TransportMode transport = GetTransportMode();


            bool aspectRatio = aspectRatioCheckBox.Checked;
            var top = (int)srcTopNumeric.Value;
            var left = (int)srcLeftNumeric.Value;
            var right = (int)srcRightNumeric.Value;
            var bottom = (int)srcBottomNumeric.Value;

            int width = right - left;
            int height = bottom - top;

            var virtualScreen = SystemInformation.VirtualScreen;
            //var srcRect = new Rectangle(0, 0, virtualScreen.Width, virtualScreen.Height);

            var srcRect = new Rectangle(left, top, width, height); //currentScreen.Bounds;
            //var srcRect = currentScreen.Bounds;

            var _destWidth = (int)destWidthNumeric.Value;
            var _destHeight = (int)destHeightNumeric.Value;

            var destSize = new Size(_destWidth, _destHeight);

            //

            //var destSize = new Size(virtualScreen.Width, virtualScreen.Height);

            //if (aspectRatio)
            //{
            //    var ratio = srcRect.Width / (double)srcRect.Height;
            //    int destWidth = destSize.Width;
            //    int destHeight = (int)(destWidth / ratio);
            //    if (ratio < 1)
            //    {
            //        destHeight = destSize.Height;
            //        destWidth = (int)(destHeight * ratio);
            //    }

            //    destSize = new Size(destWidth, destHeight);

            //    logger.Info("New destionation size: " + destSize);
            //}



            screenSource = new ScreenSource();
            ScreenCaptureParams captureParams = new ScreenCaptureParams
            {
                SrcRect = srcRect,
                DestSize = destSize,
                CaptureType = CaptureType.DXGIDeskDupl,
                //CaptureType = CaptureType.Direct3D,
                //CaptureType = CaptureType.GDI,
                Fps = fps,
                CaptureMouse = showMouse,
                AspectRatio = aspectRatio,
                
            };

            screenSource.Setup(captureParams);

            var cmdOptions = new CommandLineOptions();
            cmdOptions.IpAddr = addressTextBox.Text;
            cmdOptions.Port = (int)portNumeric.Value;

            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {

                LocalAddr = localAddr,
                LocalPort = cmdOptions.Port,

                RemoteAddr = cmdOptions.IpAddr,
                RemotePort = cmdOptions.Port,

                TransportMode = transport,
            };

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height = destSize.Height, // options.Height,
                FrameRate = cmdOptions.FrameRate,
                EncoderName = "libx264", // "h264_nvenc", //
                Bitrate = bitrate,
                LowLatency= latencyMode,
                Profile = h264Profile,
                BitrateMode = bitrateMode,
                MaxBitrate = maxBitrate,
            };

            videoStreamer = new ScreenStreamer(screenSource);

            videoStreamer.Setup(encodingParams, networkParams);

            regionForm = new RegionForm(srcRect);
            regionForm.Visible = true;

            statisticForm.Location = srcRect.Location;//currentScreenRect.Location;
            if (showStatistic)
            {
                statisticForm.Start();
            }

            var captureTask = screenSource.Start();
            var streamerTask = videoStreamer.Start();


            isStreaming = true;

            UpdateControls();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...)");

            if (screenSource != null)
            {
                screenSource.Close();
            }

            if (videoStreamer != null)
            {
                videoStreamer.Close();

            }

            if (statisticForm != null)
            {
                statisticForm.Stop();
                statisticForm.Visible = false;
            }

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();
                previewForm = null;
            }

            regionForm?.Close();
            regionForm = null;

            isStreaming = false;

            UpdateControls();

        }


        private void previewButton_Click(object sender, EventArgs e)
        {
            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Visible = !previewForm.Visible;
            }
            else
            {

                previewForm = new PreviewForm();
                previewForm.Setup(screenSource);
                var pars = screenSource.CaptureParams;

                var title = "Src" + pars.SrcRect + "->Dst" + pars.DestSize + " Fps=" + pars.Fps + " Ratio=" + pars.AspectRatio;

                previewForm.Text = title;

                previewForm.Visible = true;
            }
        }

        private void screensUpdateButton_Click(object sender, EventArgs e)
        {
            LoadScreenItems();
        }

        private BindingList<ComboBoxItem> screenItems = null;
        private void LoadScreenItems()
        {
            var screens = Screen.AllScreens.Select(s => new ComboBoxItem { Name = s.DeviceName, Tag = s }).ToList();

            screens.Add(new ComboBoxItem
            {
                Name = "_AllScreen",
                Tag = null
            });

            screenItems = new BindingList<ComboBoxItem>(screens);
            screensComboBox.DisplayMember = "Name";
            screensComboBox.DataSource = screenItems;
        }

        private Rectangle currentScreenRect = Rectangle.Empty;

        private Screen currentScreen = null;
        private void screensComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            currentScreen = GetCurrentScreen();
            currentScreenRect = currentScreen?.Bounds ?? SystemInformation.VirtualScreen;

            srcTopNumeric.Value = currentScreenRect.Top;
            srcLeftNumeric.Value = currentScreenRect.Left;
            srcRightNumeric.Value = currentScreenRect.Right;
            srcBottomNumeric.Value = currentScreenRect.Bottom;

            //if (currentScreen != null)
            //{
            //    var rect = currentScreen.Bounds;

            //    srcTopNumeric.Value = rect.Top;
            //    srcLeftNumeric.Value = rect.Left;
            //    srcRightNumeric.Value = rect.Right;
            //    srcBottomNumeric.Value = rect.Bottom;
            //}

        }

        private void screensComboBox_SelectedValueChanged_1(object sender, EventArgs e)
        {
            currentScreen = GetCurrentScreen();

            currentScreenRect = currentScreen?.Bounds ?? SystemInformation.VirtualScreen;

            srcTopNumeric.Value = currentScreenRect.Top;
            srcLeftNumeric.Value = currentScreenRect.Left;
            srcRightNumeric.Value = currentScreenRect.Right;
            srcBottomNumeric.Value = currentScreenRect.Bottom;

            //if (currentScreen != null)
            //{
            //    var rect = currentScreen.Bounds;

            //    srcTopNumeric.Value = rect.Top;
            //    srcLeftNumeric.Value = rect.Left;
            //    srcRightNumeric.Value = rect.Right;
            //    srcBottomNumeric.Value = rect.Bottom;
            //}
        }

        private Screen GetCurrentScreen()
        {
            Screen screen = null;
            var obj = screensComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    var tag = item.Tag;
                    if (tag != null)
                    {
                        screen = tag as Screen;
                    }
                }
            }
            return screen;
        }

        private TransportMode GetTransportMode()
        {
            TransportMode transport = TransportMode.Unknown;
            var item = transportComboBox.SelectedItem;
            if (item != null)
            {
                transport = (TransportMode)item;
            }
            return transport;
        }


        private void LoadTransportItems()
        {

            var items = new List<TransportMode>
            {
                TransportMode.Tcp,
                TransportMode.Udp,

            };
            transportComboBox.DataSource = items;
        }

        private void LoadRateModeItems()
        {

            var items = new List<BitrateControlMode>
            {
               BitrateControlMode.CBR,
               BitrateControlMode.VBR,
               BitrateControlMode.Quality,

            };

            bitrateModeComboBox.DataSource = items;
        }

        private void LoadEncoderProfilesItems()
        {

            var items = new List<H264Profile>
            {
               H264Profile.Main,
               H264Profile.Base,
               H264Profile.High,

            };

            encProfileComboBox.DataSource = items;
        }

        private void LoadEncoderItems()
        {
            var items = new List<VideoEncoderMode>
            {
                VideoEncoderMode.H264,
                VideoEncoderMode.JPEG,
            };

            encoderComboBox.DataSource = items;

        }

        private void UpdateControls()
        {
            this.settingPanel.Enabled = !isStreaming;

            this.startButton.Enabled = !isStreaming;
            this.previewButton.Enabled = isStreaming;

            this.stopButton.Enabled = isStreaming;


            //this.fpsNumeric2.Enabled = !ServiceHostOpened;
            //this.inputSimulatorCheckBox2.Enabled = !ServiceHostOpened;
            //this.screensComboBox2.Enabled = !ServiceHostOpened;
            //this.screensUpdateButton2.Enabled = !ServiceHostOpened;

        }

        private SnippingTool snippingTool = new SnippingTool();
        private void snippingToolButton_Click(object sender, EventArgs e)
        {
            if (snippingTool != null)
            {
                snippingTool.Dispose();
            }

            snippingTool = new SnippingTool();
            var screen = GetCurrentScreen();

            var areaSelected = new Action<Rectangle, Rectangle>((a, s) =>
            {
                int left = a.Left + s.Left;
                int top = a.Top + s.Top;
                var rect = new Rectangle(left, top, a.Width, a.Height);


                srcTopNumeric.Value = rect.Top;
                srcLeftNumeric.Value = rect.Left;
                srcRightNumeric.Value = rect.Right;
                srcBottomNumeric.Value = rect.Bottom;

                //regionForm?.Close();

                //regionForm = new RegionForm(rect);
                //regionForm.Visible = true;

                //MessageBox.Show(a.ToString() + " " + s.ToString() + " " + rect.ToString());
            });

            //regionForm?.Close();
            //regionForm = null;
            snippingTool.Snip(screen, areaSelected);
        }


        public void Test(int x, int y, int width, int height)
        {

            int X, X1, Y, Y1;
            int _X, _X1, _Y, _Y1;

            var srcRect = new Rectangle(x, y, width, height);

            Rectangle abcSrcRect = new Rectangle
            {
                X = 0,
                Y = 0,
                Width = srcRect.Width,
                Height = srcRect.Height,
            };

            Debug.WriteLine("srcRect=" + srcRect.ToString() + " abcSrcRect=" + abcSrcRect.ToString());

            foreach (var screen in Screen.AllScreens)
            {
                var screenRect = screen.Bounds;

                Debug.WriteLine("screenRect =" + screenRect.ToString());


                if ((screenRect.X > srcRect.Right) || (screenRect.Right < srcRect.X) ||
                 (screenRect.Y > srcRect.Bottom) || (screenRect.Bottom < srcRect.Y))
                {

                    Debug.WriteLine(screenRect.ToString() + " no common area");
                    continue;
                }
                else
                {
                    if (srcRect.X < screenRect.X)
                    {// за левой границей экрана
                        X = screenRect.X;
                    }
                    else
                    {
                        X = srcRect.X;

                    }

                    if (srcRect.Right > screenRect.Right)
                    { // за правой границей
                        X1 = screenRect.Right;
                    }
                    else
                    {
                        X1 = srcRect.Right;
                    }

                    if (srcRect.Y < screenRect.Y)
                    {// за верхней границей
                        Y = screenRect.Y;
                    }
                    else
                    {
                        Y = srcRect.Y;
                    }

                    if (srcRect.Bottom > screenRect.Bottom)
                    {// за нижней границей
                        Y1 = screenRect.Bottom;
                    }
                    else
                    {
                        Y1 = srcRect.Bottom;
                    }

                    Rectangle drawRect = new Rectangle
                    {
                        X = X - x,
                        Y = Y - y,
                        Width = X1 - X,
                        Height = Y1 - Y,
                    };

                    // в координатах от '0'
                    _X = X - screenRect.X;
                    _X1 = X1 - screenRect.X;
                    _Y = Y - screenRect.Y;
                    _Y1 = Y1 - screenRect.Y;

                    Rectangle desktopRect = new Rectangle
                    {
                        X = _X,
                        Y = _Y,
                        Width = _X1 - _X,
                        Height = _Y1 - _Y,
                    };

                    Debug.WriteLine("desktopRect=" + desktopRect.ToString() + " drawRect=" + drawRect.ToString());

                    // Main API that does memory data transfer
                    //BitBlt(hdcDest, X - x, Y - y, X1 - X, Y1 - Y, hdcSrc, X, Y, 0x40000000 | 0x00CC0020); //SRCCOPY AND CAPTUREBLT
                }
            }
        }

        private void MaxBitrateNumeric_ValueChanged(object sender, EventArgs e)
        {

        }

        private bool showStatistic = true;
        private void statInfoCheckBox_CheckedChanged(object sender, EventArgs e)
        {

            showStatistic = statInfoCheckBox.Checked;

            if (showStatistic)
            {
                statisticForm.Start();
            }
            else
            {
                if (statisticForm != null)
                {
                    statisticForm.Stop();
                    statisticForm.Visible = false;
                }
            }
        }
    }


}
