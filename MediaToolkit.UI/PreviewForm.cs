using MediaToolkit;
using MediaToolkit.MediaFoundation;
using MediaToolkit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaToolkit.UI
{
    public partial class PreviewForm : Form
    {
        public PreviewForm()
        {
            InitializeComponent();
        }

        private D3D11RendererProvider d3dProvider = null;
        private D3DImageRenderer renderer = null;

        public bool Setup(IVideoSource videoSource)
        {
            bool result = false;
            if(renderer != null)
            {
                return result;
            }

            if (videoSource != null)
            {
                d3dProvider = new D3D11RendererProvider();

                renderer = new D3DImageRenderer();

                var texture = videoSource.SharedTexture;
                if (texture != null)
                {
                    d3dProvider.Init(texture);

                    //renderer.Setup(texture);
                    d3DImageControl1.DataContext = renderer;

                    renderer.Run(d3dProvider);

                    result = true;
                }
            }

            return result;

        }


        public bool Render()
        {
            bool success = false;

            if (Visible)
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
                        Console.WriteLine("TestDevice() == false");
                    }

                }

                //if (renderer != null)
                //{
                //    renderer.Update();
                //    success = true;
                //}
            }

            return success;
        }

        public void UpdateWindow(bool fitToVideo, Size videoSize)
        {
            if (!fitToVideo)
            {

                elementHost1.Dock = DockStyle.Fill;
                this.MaximumSize = new Size(0, 0);
            }
            else
            {

                elementHost1.Dock = DockStyle.None;
                elementHost1.Size = videoSize;
                //this.ClientSize = elementHost1.ClientSize;

                int maxWidth = (elementHost1.ClientRectangle.Width + (this.Width - this.ClientSize.Width));
                int maxHeight = (elementHost1.ClientRectangle.Height + (this.Height - this.ClientSize.Height));

                this.MaximumSize = new Size(maxWidth, maxHeight);

            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (renderer != null)
            {
                if (this.Visible)
                {
                    renderer.Run(d3dProvider);
                    //renderer.Start();
                }
                else
                {
                    renderer.Shutdown();

                    d3dProvider?.Close();
                   // renderer.Stop();
                }
            }


            base.OnVisibleChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {

            if (d3dProvider != null)
            {
                d3dProvider.Close();
            }

            if (renderer != null)
            {
                renderer.Shutdown();
                // renderer.Close(true);
                renderer = null;
            }

            d3DImageControl1.DataContext = null;
            base.OnClosed(e);
        }
    }
}
