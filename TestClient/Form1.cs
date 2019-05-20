using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vlc.DotNet.Core;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public static readonly string CurrentDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        public static readonly System.IO.DirectoryInfo VlcLibDirectory = new System.IO.DirectoryInfo(System.IO.Path.Combine(CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

        public Form1()
        {
            InitializeComponent();

            var args = new string[]
            {
             // "--extraintf=logger",
             //"--verbose=0",
               "--network-caching=300"
             };

            mediaPlayer = new VlcMediaPlayer(VlcLibDirectory, args);

            mediaPlayer.VideoHostControlHandle = this.panel1.Handle;

        }

        private VlcMediaPlayer mediaPlayer = null;
        private void button1_Click(object sender, EventArgs e)
        {
            

            //OpenFileDialog dlg = new OpenFileDialog
            //{
            //    InitialDirectory = CurrentDirectory,
            //};

            //if (dlg.ShowDialog() == DialogResult.OK)
            //{
            //    var file = dlg.FileName;              
            //    var opts = new string[]
            //    {
            //        // "--extraintf=logger",
            //        //"--verbose=0",
            //        "--network-caching=100",
            //    };

            //    mediaPlayer?.Play(new FileInfo(sdpFileName), opts);
            //}


            var file = Path.Combine(CurrentDirectory, "tmp.sdp");
            string[] sdp = 
            {
                "v=0",
                "s=SCREEN_STREAM",
                "c=IN IP4 239.0.0.1",
                "m=video 1234 RTP/AVP 96",
                "b=AS:2000",
                "a=rtpmap:96 H264/90000",
                "a=fmtp:96 packetization-mode=1",
                //"a=fmtp:96 packetization-mode=1 sprop-parameter-set=Z2QMH6yyAKALdCAAAAMAIAAAB5HjBkkAAAAB,aOvDyyLA",
            };


            File.WriteAllLines(file, sdp);

            var opts = new string[]
            {
                    // "--extraintf=logger",
                    //"--verbose=0",
                    "--network-caching=100",
            };

            mediaPlayer?.Play(new FileInfo(file), opts);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            mediaPlayer?.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(CurrentDirectory, "ScreenStreamer.exe"));
        }
    }
}
