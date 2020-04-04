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

        public bool Setup(IVideoSource videoSource)
        {
            bool result = false;
            if(renderer != null)
            {
                return result;
            }

            if (videoSource != null)
            {
                renderer = new D3DImageRenderer();

                var texture = videoSource.SharedTexture;
                if (texture != null)
                {
                    renderer.Setup(texture);
                    d3DImageControl1.DataContext = renderer;
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
                if (renderer != null)
                {
                    renderer.Update();
                    success = true;
                }
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
                this.ClientSize = elementHost1.ClientSize;

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
                renderer.Close(true);
                renderer = null;
            }

            d3DImageControl1.DataContext = null;
            base.OnClosed(e);
        }
    }
}
