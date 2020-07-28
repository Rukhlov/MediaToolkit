using MediaToolkit.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MediaToolkit.Logging;

namespace MediaToolkit.UI
{
    /// <summary>
    /// Логика взаимодействия для D3DImageControl.xaml
    /// </summary>
    public partial class D3DImageControl : UserControl
    {
        private static readonly TraceSource tracer = TraceManager.GetTrace("MediaToolkit.UI");

        public D3DImageControl()
        {
            InitializeComponent();

            d3dRenderer = new D3DImageRenderer();

            DataContext = d3dRenderer;
        }

        private D3D11RendererProvider d3dProvider = null;
        private D3DImageRenderer d3dRenderer = null;

        private IVideoSource videoSource = null;

        public bool Setup(IVideoSource source)
        {
            tracer.Debug("D3DImageControl::Setup(...)");

            bool result = false;
            Debug.Assert(d3dRenderer !=null, "d3dRenderer !=null");

            //Debug.Assert(DataContext != null, "DataContext !=null");

            if(DataContext == null)
            {
                DataContext = d3dRenderer;
            }

            videoSource = source;
            if (videoSource != null)
            {
                d3dProvider = new D3D11RendererProvider();

                //renderer = new D3DImageRenderer();

                var texture = videoSource.SharedTexture;
                if (texture != null)
                {
                    d3dProvider.Init(texture);

                    // //renderer.Setup(texture);
                    // d3DImageControl.DataContext = renderer;

                    //// renderer.Run(d3dProvider);

                    result = true;
                }
            }

            return result;
        }

        public void Start()
        {
            tracer.Debug("D3DImageControl::Start()");

            d3dRenderer.Run(d3dProvider);
            running = true;
        }

        private bool running = false;
        public bool Render()
        {
            bool success = false;

            if (running)
            {

                if (d3dProvider != null)
                {
                    var deviceReady = d3dProvider.TestDevice();
                    if (deviceReady)
                    {
                        d3dProvider.OnNewDataAvailable();
                        success = true;
                    }
                    else
                    {
                        tracer.Debug("TestDevice() == false");

                        d3dProvider.ReInit(videoSource.SharedTexture);

                    }

                }
                else
                {
                    tracer.Debug("d3dProvider == null");
                }

            }
            else
            {
                tracer.Debug("running == false");
            }

            return success;
        }

        public void Stop()
        {
            tracer.Debug("D3DImageControl::Stop()");
            d3dRenderer?.Shutdown();

            d3dProvider?.Close();


            running = false;
        }

        public void Close()
        {
            tracer.Debug("D3DImageControl::Close()");

            Stop();

            DataContext = null;
        }
    }
}
