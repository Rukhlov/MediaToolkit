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

            
        }

        private Renderer renderer = null;
        private DDACapture deskDuplCapture = null;


        private Renderer renderer1 = null;
        private DDACapture deskDuplCapture1 = null;

        private DDAOutputManager outputManager = new DDAOutputManager();

        private void buttonInit_Click(object sender, EventArgs e)
        {

            var width = 1920;
            var height = 1080;

            var fps = 60;

            try
            {

                //var screen = Screen.PrimaryScreen;
                var screen = Screen.AllScreens[0];

                var screenRect = screen.Bounds;
                //var screenRect = new Rectangle(0, 0, 640, 480);
                
                //var screenRect = SystemInformation.VirtualScreen;

                Console.WriteLine(SystemInformation.VirtualScreen);

                // videoSource.Init(width, height);

                deskDuplCapture = new DDACapture(null);
                deskDuplCapture.OutputManager = outputManager;

                deskDuplCapture.PrimaryAdapterIndex = 0;

                deskDuplCapture.AspectRatio = true;
                deskDuplCapture.CaptureMouse = true;
                deskDuplCapture.Init(screenRect, screenRect.Size);




                //deskDuplCapture1 = new DDACapture(null);
                //deskDuplCapture1.OutputManager = outputManager;

                //deskDuplCapture1.adapterIndex = 0;

                //deskDuplCapture1.AspectRatio = true;
                //deskDuplCapture1.CaptureMouse = true;
                //deskDuplCapture1.Init(screenRect, screenRect.Size);




                renderer = new Renderer();
                //renderer1 = new Renderer();

                //videoPanel.Size = new Size(1280, 720);

                var srcSize = screenRect.Size;
                //var destSize = new Size(1280, 720);

                var destSize = new Size(1920, 1080);
                renderer.adapterIndex = deskDuplCapture.PrimaryAdapterIndex;

                renderer.Init(videoPanel.Handle, srcSize, destSize);

                //renderer1.Init(this.Handle, srcSize, destSize);

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
            Console.WriteLine("buttonPresent_Click(...)");

            if (running)
            {
                Console.WriteLine("running == true");
                return;
            }

            Task.Run(() =>
            {

                Console.WriteLine("Presenter task BEGIN");

                running = true;

                deskDuplCapture.Start();
                //deskDuplCapture1.Start();

                renderer.Resize(videoPanel.Size);

                renderer.Start();
                var texture = deskDuplCapture.SharedTexture;
                renderer.UpdateTexture(texture);




                //renderer1.Resize(this.Size);
                //renderer1.Start();
                //renderer1.UpdateTexture(texture);

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

                //renderer1.Stop();
                //renderer1.Close();


                renderer.Stop();
                renderer.Close();

                deskDuplCapture.Stop();
               // deskDuplCapture1.Stop();

                //deskDuplCapture.Close();


                texture?.Dispose();

                Console.WriteLine("Presenter task END");
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
            running = false;

        }
    }

   
}
