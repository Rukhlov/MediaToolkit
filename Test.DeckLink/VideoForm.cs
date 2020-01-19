using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.DeckLink
{
    public partial class VideoForm : Form
    {
        public VideoForm()
        {
            InitializeComponent();

            this.videoPanel.BackColor = Color.Transparent;
            this.BackColor = Color.Black;
            statusLabel.Text = "";

        }

        public IntPtr VideoHandle { get => this.videoPanel.Handle; }
        public Rectangle VideoRectangle => videoPanel.ClientRectangle;

        public void UpdateWindow(bool fitToVideo, Size videoSize)
        {
            if (!fitToVideo)
            {
                videoPanel.Dock = DockStyle.Fill;
                this.MaximumSize = new Size(0, 0);
            }
            else
            {

                videoPanel.Dock = DockStyle.None;
                videoPanel.Size = videoSize;
                this.ClientSize = videoPanel.ClientSize;

                int maxWidth = (videoPanel.ClientRectangle.Width + (this.Width - this.ClientSize.Width));
                int maxHeight = (videoPanel.ClientRectangle.Height + (this.Height - this.ClientSize.Height));

                this.MaximumSize = new Size(maxWidth, maxHeight); 

            }
        }

        public void UpdateStatusText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                statusLabel.Text = text;
            }
            else
            {
                statusLabel.Text = "";
            }

        }

    }
}
