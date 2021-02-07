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
using MediaToolkit.Core;
using System.Diagnostics;
using NLog;
using MediaToolkit.SharedTypes;
using MediaToolkit.MediaStreamers;
using System.Threading;
using MediaToolkit.UI;

namespace TestStreamer.Controls
{
    public partial class HttpStreamerControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public HttpStreamerControl()
        {
            InitializeComponent();

            HttpUpdateScreens();

            HttpUpdateCaptures();

            syncContext = SynchronizationContext.Current;
        }

        private StatisticForm statisticForm = new StatisticForm();

        private VideoHttpStreamer httpStreamer = null;
        private IVideoSource httpScreenSource = null;
       // private Screen currentScreen = null;


        private void httpStartButton_Click(object sender, EventArgs e)
        {
            //currentScreen = HttpGetCurrentScreen();

            var srcRect = HttpGetCurrentScreen(); //currentScreen.Bounds;
            //var srcRect = currentScreen.Bounds;

            var _destWidth = (int)httpDestWidthNumeric.Value;
            var _destHeight = (int)httpDestHeightNumeric.Value;

            var destSize = new Size(_destWidth, _destHeight);

            var ratio = srcRect.Width / (double)srcRect.Height;
            int destWidth = destSize.Width;
            int destHeight = (int)(destWidth / ratio);
            if (ratio < 1)
            {
                destHeight = destSize.Height;
                destWidth = (int)(destHeight * ratio);
            }

            destSize = new Size(destWidth, destHeight);


            var fps = httpFpsNumeric.Value;

            var addr = httpAddrTextBox.Text;
            var port = (int)httpPortNumeric.Value;

            VideoCaptureType captureType = (VideoCaptureType)captureTypesComboBox.SelectedItem;

            httpScreenSource = new ScreenCaptureSource();
            ScreenCaptureDevice captureParams = new ScreenCaptureDevice
            {
                CaptureRegion = srcRect,
                Resolution = destSize,

            };

            captureParams.Properties.CaptureType = captureType;//CaptureType.DXGIDeskDupl,

            captureParams.Properties.Fps = (int)fps;
            captureParams.Properties.CaptureMouse = true;
            captureParams.Properties.AspectRatio = true;
            captureParams.Properties.UseHardware = false;

            if (captureType == VideoCaptureType.GDI || captureType == VideoCaptureType.GDIPlus)
            {// масштабируем на энкодере
                captureParams.Resolution = new Size(srcRect.Width, srcRect.Height);
            }

            httpScreenSource.Setup(captureParams);


            httpStreamer = new VideoHttpStreamer(httpScreenSource);

            NetworkSettings networkParams = new NetworkSettings
            {
                RemoteAddr = addr,
                RemotePort = port,
            };


            VideoEncoderSettings encodingParams = new VideoEncoderSettings
            {
                Width = destSize.Width, // options.Width,
                Height = destSize.Height, // options.Height,
                //Resolution = destSize,
                FrameRate = new MediaRatio((int)fps, 1),
                EncoderId = "mjpeg",
            };

            httpStreamer.Setup(encodingParams, networkParams);


            httpStreamer.Start();
            httpScreenSource.Start();


            statisticForm.Location = srcRect.Location;
           // statisticForm.Start();
        }

        private void httpStopButton_Click(object sender, EventArgs e)
        {
            httpStreamer?.Close();
            httpScreenSource?.Close();

            //statisticForm.Close();
        }

        private void httpUpdateButton_Click(object sender, EventArgs e)
        {
            HttpUpdateScreens();
        }



        private BindingList<ComboBoxItem> screenItems2 = null;
        private void HttpUpdateScreens()
        {

            var screens = Screen.AllScreens.Select(s => new ComboBoxItem { Name = s.DeviceName, Tag = s.Bounds }).ToList();

            screens.Add(new ComboBoxItem
            {
                Name = "_AllScreen",
                Tag = SystemInformation.VirtualScreen
            });


            screenItems2 = new BindingList<ComboBoxItem>(screens);

            httpDisplayComboBox.DisplayMember = "Name";
            httpDisplayComboBox.DataSource = screenItems2;
        }


        private void HttpUpdateCaptures()
        {

            List<VideoCaptureType> captureTypes = new List<VideoCaptureType>();
            captureTypes.Add(VideoCaptureType.DXGIDeskDupl);
            captureTypes.Add(VideoCaptureType.GDI);
            //captureTypes.Add(CaptureType.GDIPlus);
            captureTypes.Add(VideoCaptureType.Direct3D9);
            captureTypes.Add(VideoCaptureType.Datapath);

            captureTypesComboBox.DataSource = captureTypes;
        }

        private Rectangle HttpGetCurrentScreen()
        {
            Rectangle rect = Rectangle.Empty;
            var obj = httpDisplayComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    var tag = item.Tag;
                    if (tag != null && tag is Rectangle)
                    {
                        rect = (Rectangle)tag;
                    }
                }
            }

            return rect;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var port = (int)httpPortNumeric.Value;
                string url = @"http://127.0.0.1" + ":" + port;

                Process.Start(url);
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        IHttpScreenStreamer httpScreenStreamer = null;

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (httpScreenStreamer == null)
                {
                    httpScreenStreamer = new HttpScreenStreamer();

                    httpScreenStreamer.StreamerStarted += HttpScreenStreamer_StreamerStarted;
                    httpScreenStreamer.StreamerStopped += HttpScreenStreamer_StreamerStopped;

                }

                var srcRect = HttpGetCurrentScreen(); //currentScreen.Bounds;
                var _destWidth = (int)httpDestWidthNumeric.Value;
                var _destHeight = (int)httpDestHeightNumeric.Value;

                var destSize = new Size(_destWidth, _destHeight);
                VideoCaptureType captureType = (VideoCaptureType)captureTypesComboBox.SelectedItem;


                HttpScreenStreamerArgs args = new HttpScreenStreamerArgs
                {
                    Addres = httpAddrTextBox.Text,
                    Port = (int)httpPortNumeric.Value,
                    CaptureRegion = srcRect,
                    Resolution = destSize,

                    Fps = (int)httpFpsNumeric.Value,
                    CaptureTypes = captureType,
                    CaptureMouse = true,

                };

                //args.Attributes["ResizeOnCapture"] = checkBoxResizeOnCapture.Checked;
                //args.Attributes["GdiStretchingMode"] = (int)numericGdiStretchingMode.Value;

                httpScreenStreamer.Setup(args);

                httpScreenStreamer.Start();

                this.Cursor = Cursors.WaitCursor;

                this.Enabled = false;
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                MessageBox.Show(ex.ToString());
            }

        }

        private SynchronizationContext syncContext = null;

        private void HttpScreenStreamer_StreamerStopped(object obj)
        {
            logger.Debug("HttpScreenStreamer_StreamerStopped(...)");

            if (obj != null)
            {
                var error = obj.ToString();
                MessageBox.Show(error);
            }

            if (httpScreenStreamer != null)
            {
                httpScreenStreamer.Close(true);
            }

            //logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

            this.Cursor = Cursors.Default;
            this.Enabled = true;
        }

        private void HttpScreenStreamer_StreamerStarted()
        {
            logger.Debug("HttpScreenStreamer_StreamerStarted(...)");

            this.Cursor = Cursors.Default;
            this.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (httpScreenStreamer.State == MediaState.Started)
                {
                    if (httpScreenStreamer != null)
                    {
                        httpScreenStreamer.Stop();
                    }
                    this.Cursor = Cursors.WaitCursor;
                    this.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                MessageBox.Show(ex.ToString());
            }

        }

    }

}
