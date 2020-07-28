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
using MediaToolkit.Logging;
using System.Diagnostics;
using NLog;

namespace MediaToolkit.UI
{
    public partial class VideoPreviewForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public VideoPreviewForm()
        {
            InitializeComponent();

            //renderer = new D3DImageRenderer();

            //d3DImageControl.DataContext = renderer;
        }

        //private D3D11RendererProvider d3dProvider = null;
        //private D3DImageRenderer renderer = null;

        //private IVideoSource videoSource = null;
        public bool Setup(IVideoSource source)
        {
            logger.Debug("PreviewForm::Setup(...)");

            return d3DImageControl.Setup(source);

        }


        public bool Render()
        {
            return d3DImageControl.Render();
        }

        public void UpdateWindow(bool fitToVideo, Size videoSize)
        {
            logger.Debug("PreviewForm::UpdateWindow(...) " + fitToVideo + " " + videoSize.ToString());

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
            logger.Debug("PreviewForm::OnVisibleChanged(...)");

            if (this.Visible)
            {
                d3DImageControl.Start();
            }
            else
            {
                d3DImageControl.Stop();
            }
            


            base.OnVisibleChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            logger.Debug("PreviewForm::UpdaOnClosedteWindow(...)");

            d3DImageControl.Close();

            base.OnClosed(e);
        }
    }
}
