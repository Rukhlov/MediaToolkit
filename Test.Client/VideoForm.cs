using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    public partial class VideoForm : Form
    {
        public VideoForm()
        {
            InitializeComponent();
        }

        private InputManager inputMan = null;


        public void LinkInputManager(InputManager manager)
        {
            this.userControl11.MouseMove += UserControl11_MouseMove;

            this.userControl11.MouseLeftButtonDown += UserControl11_MouseLeftButtonDown;
            this.userControl11.MouseLeftButtonUp += UserControl11_MouseLeftButtonUp;

            this.userControl11.MouseRightButtonDown += UserControl11_MouseRightButtonDown;
            this.userControl11.MouseRightButtonUp += UserControl11_MouseRightButtonUp;

            this.userControl11.MouseDoubleClick += UserControl11_MouseDoubleClick;

            this.inputMan = manager;

        }

        public void UnlinkInputManager()
        {
            this.inputMan = null;

            this.userControl11.MouseMove -= UserControl11_MouseMove;
            this.userControl11.MouseLeftButtonDown -= UserControl11_MouseLeftButtonDown;
            this.userControl11.MouseLeftButtonUp -= UserControl11_MouseLeftButtonUp;


            this.userControl11.MouseRightButtonDown -= UserControl11_MouseRightButtonDown;
            this.userControl11.MouseRightButtonUp -= UserControl11_MouseRightButtonUp;


            this.userControl11.MouseDoubleClick -= UserControl11_MouseDoubleClick;
        }

        private void UserControl11_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
        }

        private void UserControl11_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("MouseLeftButtonDown(...) ");
            if (inputMan != null)
            {
                var message = "MouseDown:" + (int)MouseButtons.Left + " " + 1;

                inputMan.ProcessMessage(message);
            }

        }

        private void UserControl11_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("MouseLeftButtonUp(...) ");

            if (inputMan != null)
            {
                var message = "MouseUp:" + (int)MouseButtons.Left + " " + 1;

                inputMan.ProcessMessage(message);
            }

        }

        private void UserControl11_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("MouseRightButtonUp(...) ");

            if (inputMan != null)
            {
                var message = "MouseUp:" + (int)MouseButtons.Right + " " + 1;

                inputMan.ProcessMessage(message);
            }

        }

        private void UserControl11_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("MouseRightButtonDown(...) ");

            if (inputMan != null)
            {
                var message = "MouseDown:" + (int)MouseButtons.Right + " " + 1;

                inputMan.ProcessMessage(message);
            }
        }


        private void UserControl11_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Console.WriteLine("MouseMove(...)");

           var pos = e.GetPosition(userControl11);

            double x = pos.X;
            double y = pos.Y;

            double w = userControl11.RenderSize.Width;
            double h = userControl11.RenderSize.Height;

            double _x = (x * 65536.0) / w;
            double _y = (y * 65536.0) / h;

            var time = DateTime.Now;

            if (inputMan != null)
            {
                var message = "MouseMove:" + _x.ToString(CultureInfo.InvariantCulture) + " " + _y.ToString(CultureInfo.InvariantCulture);

                inputMan.ProcessMessage(message);
            }


        }
    }


}
