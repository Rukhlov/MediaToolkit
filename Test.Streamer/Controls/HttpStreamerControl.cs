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

            httpScreenSource = new ScreenSource();
            ScreenCaptureDeviceDescription captureParams = new ScreenCaptureDeviceDescription
            {
                CaptureRegion = srcRect,
                Resolution = destSize,

            };

            captureParams.CaptureType = captureType;//CaptureType.DXGIDeskDupl,

            captureParams.Fps = (int)fps;
            captureParams.CaptureMouse = true;
            captureParams.AspectRatio = true;
            captureParams.UseHardware = false;

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
                //Width = destSize.Width, // options.Width,
                //Height = destSize.Height, // options.Height,
                Resolution = destSize,
                FrameRate = (int)fps,
                EncoderName = "mjpeg",
            };

            httpStreamer.Setup(encodingParams, networkParams);


            httpStreamer.Start();
            httpScreenSource.Start();


            statisticForm.Location = srcRect.Location;
            statisticForm.Start();
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
    }

}
