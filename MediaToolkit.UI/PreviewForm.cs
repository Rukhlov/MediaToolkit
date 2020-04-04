using MediaToolkit;
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

        private D3DImageRenderer renderer = null;

        private IVideoSource videoSource = null;

        public void Setup(IVideoSource videoSource)
        {
            if (videoSource != null)
            {
                this.videoSource = videoSource;

                videoSource.BufferUpdated += VideoSource_BufferUpdated;

                renderer = new D3DImageRenderer();

                var texture = videoSource.SharedTexture;
                if (texture != null)
                {
                    renderer.Setup(texture);
                    d3DImageControl1.DataContext = renderer;
                }
            }

        }

        private void VideoSource_BufferUpdated()
        {
            if (renderer != null)
            {
                renderer.Update();
            }

        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (renderer != null)
            {
                if (this.Visible)
                {
                    renderer.Start();
                }
                else
                {
                    renderer.Stop();
                }
            }


            base.OnVisibleChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {

            if (renderer != null)
            {
                videoSource.BufferUpdated -= VideoSource_BufferUpdated;
                renderer.Close(true);
                renderer = null;
            }

            d3DImageControl1.DataContext = null;
            base.OnClosed(e);
        }
    }
}
