using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();



        }

        private AutoResetEvent waitEvent = new AutoResetEvent(false);
        private object syncRoot = new object();
        private Socket socket = null;

        public void StartInputSimulator(string address, int port)
        {

            this.userControl11.MouseMove += UserControl11_MouseMove;

            this.userControl11.MouseLeftButtonDown += UserControl11_MouseLeftButtonDown;
            this.userControl11.MouseLeftButtonUp += UserControl11_MouseLeftButtonUp;

            //this.userControl11.MouseRightButtonDown += UserControl11_MouseRightButtonDown;
            //this.userControl11.MouseRightButtonUp += UserControl11_MouseRightButtonUp;

            this.userControl11.MouseDoubleClick += UserControl11_MouseDoubleClick;

            running = true;

            Task.Run(() =>
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var ipaddr = IPAddress.Parse(address);
                socket.Connect(ipaddr, port);

                while (running)
                {

                    try
                    {
                        waitEvent.WaitOne();
                        lock (syncRoot)
                        {

                            byte[] data = Encoding.ASCII.GetBytes(message);
                            socket.Send(data);

                            //client.Send(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }


                }
            });

        }


        private string message = "";
        private bool running = false;
        public void StopInputSimulator()
        {
            running = false;

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

            lock (syncRoot)
            {
                // message = time.ToString("HH:mm:ss.fff") + ">" + _x + " " + _y;
                message = "MouseDown:" + (int)MouseButtons.Left+ " " + 1;
            }

            waitEvent.Set();


        }

        private void UserControl11_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("MouseLeftButtonUp(...) ");
            lock (syncRoot)
            {
                // message = time.ToString("HH:mm:ss.fff") + ">" + _x + " " + _y;
                message = "MouseUp:" + (int)MouseButtons.Left + " " + 1;
            }

            waitEvent.Set();

        }

        private void UserControl11_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("MouseRightButtonUp(...) ");
            lock (syncRoot)
            {
                // message = time.ToString("HH:mm:ss.fff") + ">" + _x + " " + _y;
                message = "MouseUp:" + (int)MouseButtons.Right + " " + 1;
            }

            waitEvent.Set();
        }

        private void UserControl11_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("MouseRightButtonDown(...) ");
            lock (syncRoot)
            { 
                message = "MouseDown:" + (int)MouseButtons.Right + " " + 1;
            }

            waitEvent.Set();
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

            lock (syncRoot)
            {
                // message = time.ToString("HH:mm:ss.fff") + ">" + _x + " " + _y;
                message = "MouseMove:" + _x + " " + _y;
            }

            waitEvent.Set();

            ////client.Send(time.ToString("HH:mm:ss.fff") + ">" + _x + " " + _y);

            ////client.Send("MouseMove: " + DateTime.Now.ToString("HH:mm:ss.fff") + ": " + _x + ":" + _y);

            Console.WriteLine("MouseMove: " + time.ToString("HH:mm:ss.fff") + ">>" + _x + " " + _y);

        }
    }
}
