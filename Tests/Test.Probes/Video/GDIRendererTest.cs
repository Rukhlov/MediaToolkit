using MediaToolkit.NativeAPIs;
using MediaToolkit.Renderers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Probe.Video
{
    internal class GDIRendererTest
    {
        public static void Run()
        {
            //var fileName = @"Files\rgba_640x480.bmp";
            var fileName = @"Files\1920x1080.bmp";

            Bitmap bmp = (Bitmap)Bitmap.FromFile(fileName);
			var bmpSize = new Size(bmp.Width, bmp.Height);
			GdiRenderer renderer = new GdiRenderer();


            Form f = new Form
            {
                Text = fileName,
               // BackColor = Color.Red,
                BackColor = Color.Black,
                ClientSize = bmpSize,
				AutoScroll = false,
            };
			Panel p = new Panel
			{
				BackColor = Color.Black,
			};
			f.Controls.Add(p);


			bool aspectRatio = false;
			Action formResize = new Action(() =>
			{
				var formRect = f.ClientRectangle;
				if (aspectRatio)
				{
					float formRatio = (float)formRect.Width / formRect.Height;
					float bmpRatio = (float)bmpSize.Width / bmpSize.Height;
					if (formRatio < bmpRatio)
					{
						p.Width = formRect.Width;
						p.Height = (int)(p.Width / bmpRatio);
						p.Top = (formRect.Height - p.Height) / 2;
						p.Left = 0;
					}
					else
					{
						p.Height = formRect.Height;
						p.Width = (int)(p.Height * bmpRatio);
						p.Top = 0;
						p.Left = (formRect.Width - p.Width) / 2;
					}
				}
				else
				{
					p.Top = 0;
					p.Left = 0;
					p.Width = formRect.Width;
					p.Height = formRect.Height;
				}

			});

			formResize.Invoke();
			f.Resize += (o, a) =>
            {
				formResize();
			};


            f.KeyDown += (o, a) =>
            {
                if(a.KeyCode == Keys.A)
                {
                    aspectRatio = !aspectRatio;
					formResize();
                }

			};

            bool running = true;
            f.Shown += (o, a) =>
            {
				var hWnd = p.Handle;
                var width = bmp.Width;
                var height = bmp.Height;
                var format = bmp.PixelFormat;

                Task.Run(() =>
                {
                    renderer.Init(hWnd, width, height, format);

                    while (running)
                    {
                        renderer.Update(bmp);
                        renderer.Draw();

                        // GdiRenderer.Draw(hWnd, hBitmap, bmpRect, false);
                        System.Threading.Thread.Sleep(33);
                    }

                    renderer.Close();
                });
            };


            Application.Run(f);

            running = false;
        }


    }
}
