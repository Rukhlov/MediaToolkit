using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vlc.DotNet.Core;
using System.IO;
using NLog;

namespace TestClient.Controls
{
    public partial class VlcControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public VlcControl()
        {
            InitializeComponent();

            var args = new string[]
            {
                // "--extraintf=logger",
                //"--verbose=0",
                //"--network-caching=300"
            };

            logger.Debug("VlcLibDirectory: " + VlcLibDirectory);

            mediaPlayer = new VlcMediaPlayer(VlcLibDirectory, args);

        }

        public static readonly string CurrentDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        public static readonly System.IO.DirectoryInfo VlcLibDirectory = new System.IO.DirectoryInfo(System.IO.Path.Combine(CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));


        private VideoForm vlcVideoForm = null;
        private VlcMediaPlayer mediaPlayer = null;


        private void playButton_Click(object sender, EventArgs e)
        {
            var address = addressTextBox.Text;
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var port = (int)portNumeric.Value;

            if (vlcVideoForm == null || vlcVideoForm.IsDisposed)
            {
                vlcVideoForm = new VideoForm
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 1280,
                    Height = 720,

                };

                mediaPlayer.VideoHostControlHandle = vlcVideoForm.elementHost1.Handle; //vlcVideoForm.Handle;

                //System.Windows.Forms.Integration.ElementHost host = new System.Windows.Forms.Integration.ElementHost
                //{
                //    Dock = DockStyle.Fill,
                //};
                //testForm.Controls.Add(host);


                //UserControl1 video = new UserControl1();
                //host.Child = video;

                //video.DataContext = imageProvider;


                vlcVideoForm.FormClosed += VlcVideoForm_FormClosed;
            }

            vlcVideoForm.Visible = true;


            var file = Path.Combine(CurrentDirectory, "tmp.sdp");
            string[] sdp =
            {
                "v=0",
                "s=SCREEN_STREAM",
                "c=IN IP4 " + address, // "c=IN IP4 239.0.0.1",
                "m=video " + port  + " RTP/AVP 96", //"m=video 1234 RTP/AVP 96",
                "b=AS:2000",
                "a=rtpmap:96 H264/90000",
                "a=fmtp:96 packetization-mode=1",
                //"a=fmtp:96 packetization-mode=1 sprop-parameter-set=Z2QMH6yyAKALdCAAAAMAIAAAB5HjBkkAAAAB,aOvDyyLA",

                //"m=audio 1236 RTP/AVP 0",
                //"a=rtpmap:0 PCMU/8000",

            };


            File.WriteAllLines(file, sdp);

            var opts = new string[]
            {
                    // "--extraintf=logger",
                    //"--verbose=0",
                    //"--network-caching=100", //не работает
            };

            mediaPlayer?.Play(new FileInfo(file), opts);
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            vlcVideoForm?.Close();
        }

        private void VlcVideoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mediaPlayer?.Stop();
        }

    }
}
