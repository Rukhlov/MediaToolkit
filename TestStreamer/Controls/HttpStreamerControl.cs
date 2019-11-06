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
using MediaToolkit.Common;

namespace TestStreamer.Controls
{
    public partial class HttpStreamerControl : UserControl
    {
        public HttpStreamerControl()
        {
            InitializeComponent();

            HttpUpdateScreens();

            HttpUpdateCaptures();
        }

        private StatisticForm statisticForm = new StatisticForm();

        private HttpScreenStreamer httpStreamer = null;
        private ScreenSource httpScreenSource = null;
        private Screen currentScreen = null;


        private void httpStartButton_Click(object sender, EventArgs e)
        {
            currentScreen = HttpGetCurrentScreen();

            var srcRect = currentScreen.Bounds;
            //var srcRect = currentScreen.Bounds;

            var _destWidth = (int)httpDestWidthNumeric.Value;
            var _destHeight = (int)httpDestHeightNumeric.Value;

            var destSize = new Size(_destWidth, _destHeight);

            var fps = httpFpsNumeric.Value;

            var addr = httpAddrTextBox.Text;
            var port = (int)httpPortNumeric.Value;

            CaptureType captureType = (CaptureType)captureTypesComboBox.SelectedItem;

            httpScreenSource = new ScreenSource();
            ScreenCaptureParams captureParams = new ScreenCaptureParams
            {
                SrcRect = srcRect,
                DestSize = destSize,
                CaptureType = captureType,//CaptureType.DXGIDeskDupl,
                //CaptureType = CaptureType.Direct3D,
                //CaptureType = CaptureType.GDI,
                Fps = (int)fps,
                CaptureMouse = true,
                AspectRatio = true,
                UseHardware = false,
            };

            if(captureType == CaptureType.GDI || captureType == CaptureType.GDIPlus)
            {// масштабируем на энкодере
                captureParams.DestSize = new Size(srcRect.Width, srcRect.Height);
            }

            httpScreenSource.Setup(captureParams);


            httpStreamer = new HttpScreenStreamer(httpScreenSource);

            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                RemoteAddr = addr,
                RemotePort = port,
            };


            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height = destSize.Height, // options.Height,
                FrameRate = (int)fps,
                EncoderName = "mjpeg",
            };

            httpStreamer.Start(encodingParams, networkParams);
            httpScreenSource.Start();


            statisticForm.Location = currentScreen.Bounds.Location;
            statisticForm.Start();
        }

        private void httpStopButton_Click(object sender, EventArgs e)
        {
            httpStreamer?.Close();
            httpScreenSource?.Stop();

            //statisticForm.Close();
        }

        private void httpUpdateButton_Click(object sender, EventArgs e)
        {
            HttpUpdateScreens();
        }



        private BindingList<ComboBoxItem> screenItems2 = null;
        private void HttpUpdateScreens()
        {
            var screens = Screen.AllScreens.Select(s => new ComboBoxItem { Name = s.DeviceName, Tag = s }).ToList();

            screenItems2 = new BindingList<ComboBoxItem>(screens);

            httpDisplayComboBox.DisplayMember = "Name";
            httpDisplayComboBox.DataSource = screenItems2;
        }


        private void HttpUpdateCaptures()
        {

            List<CaptureType> captureTypes = new List<CaptureType>();
            captureTypes.Add(CaptureType.DXGIDeskDupl);
            captureTypes.Add(CaptureType.GDI);
            //captureTypes.Add(CaptureType.GDIPlus);
            captureTypes.Add(CaptureType.Direct3D);
            captureTypes.Add(CaptureType.Datapath);

            captureTypesComboBox.DataSource = captureTypes;
        }

        private Screen HttpGetCurrentScreen()
        {
            Screen screen = null;
            var obj = httpDisplayComboBox.SelectedItem;
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
    }
}
