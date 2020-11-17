using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Encoder.DDATest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            deskDuplCapture = new DDACapture(null);
        }

        private Renderer renderer = null;
        private DDACapture deskDuplCapture = null;
        private void buttonInit_Click(object sender, EventArgs e)
        {

            var width = 1920;
            var height = 1080;

            var fps = 60;

            try
            {

                var screen = Screen.PrimaryScreen;
                //var screen = Screen.AllScreens[1];

                 var screenRect = screen.Bounds;
                //var screenRect = SystemInformation.VirtualScreen;

                Console.WriteLine(SystemInformation.VirtualScreen);

                // videoSource.Init(width, height);

                deskDuplCapture.adapterIndex = 0;

                deskDuplCapture.AspectRatio = true;
                deskDuplCapture.CaptureMouse = true;
                deskDuplCapture.Init(screenRect, screenRect.Size);


                renderer = new Renderer();
                //videoPanel.Size = new Size(1280, 720);

                var srcSize = screenRect.Size;
                var destSize = new Size(1280, 720);
                renderer.Init(videoPanel.Handle, srcSize, destSize);


                //// Stopwatch sw = Stopwatch.StartNew();
                //var screens = Screen.AllScreens;
                //foreach(var s in screens) 
                //{
                //    var rect = s.Bounds;
                //    DesktopDuplicator dupl = new DesktopDuplicator();

                //    dupl.Init(rect);
                //    dupl.FrameAcquired += () =>
                //    {

                //        //Console.WriteLine("FrameAcquired ");

                //        //Console.WriteLine("FrameAcquired " + sw.ElapsedMilliseconds);

                //        //sw.Restart();
                //    };

                //    dupl.StartCapture();
                //}



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private bool running = false;
        private void buttonPresent_Click(object sender, EventArgs e)
        {
            running = true;

            deskDuplCapture.Start();
            
            var sharedTexture = deskDuplCapture.SharedTexture;



            renderer.Resize(videoPanel.Size);
            

            renderer.Start();

            Task.Run(() =>
            {

                var texture = deskDuplCapture.SharedTexture;
                renderer.UpdateTexture(texture);

                //videoRenderer.Setup(texture);

                while (running)
                {
                    

                    //videoRenderer.UpdataBuffer();

                    //if (!useSharedTexture) 
                    //{
                    //    var texture = deskDuplCapture.SharedTexture;
                    //    videoRenderer.UpdataBuffer(texture);
                    //}
                    //else 
                    //{
                    //    var d = videoRenderer.d3device1;
                    //    using (var texture = d.OpenSharedResource<Texture2D>(sharedTexturePointer)) 
                    //    {
                    //        videoRenderer.UpdataBuffer(texture);
                    //    }

                    //}




                    //var texture = deskDuplCapture.SharedTexture;
                    //foreach (var r in renderers)
                    //{
                    //    r.UpdataBuffer(texture);
                    //}

                    ////var res = deskDuplCapture.UpdateBuffer(10);
                    ////if(res == ErrorCode.Ok) 
                    ////{
                    ////   var texture = deskDuplCapture.SharedTexture;
                    ////    videoRenderer.UpdataBuffer(texture);
                    ////}

                    //var texture = deskDuplCapture.SharedTexture;
                    //videoRenderer.UpdataBuffer(texture);

                    ////var tex = videoSource.GetTexture();
                    ////videoRenderer.UpdataBuffer(tex);


                    Thread.Sleep(16);

                }

                sharedTexture?.Dispose();

            });

        }

        protected override void OnResize(EventArgs e)
        {

            if (renderer != null)
            {
                var size = videoPanel.Size;
                renderer.Resize(size);
            }

            base.OnResize(e);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {

        }
    }

   
}
