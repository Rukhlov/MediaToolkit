using MediaToolkit.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TestStreamer
{


    public class CustomComboBox : ComboBox
    {
        private const int WM_PAINT = 0xF;
        private int buttonWidth = SystemInformation.HorizontalScrollBarArrowWidth;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    // Uncomment this if you don't want the "highlight border".
                    /*
                    using (var p = new Pen(this.BorderColor, 1))
                    {
                        g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                    }*/
                    using (var p = new Pen(this.BorderColor, 2))
                    {
                        g.DrawRectangle(p, 2, 2, Width - buttonWidth - 4, Height - 4);
                    }


                    //using (var g = Graphics.FromHwnd(Handle))
                    //{
                    //    using (var p = new Pen(this.BorderColor, 1))
                    //    {
                    //        g.DrawRectangle(p, 0, 0, Width - buttonWidth - 1, Height - 1);
                    //    }

                    //    if (Properties.Settings.Default.Theme == "Dark")
                    //    {
                    //        g.DrawImageUnscaled(Properties.Resources.dropdown, new Point(Width - buttonWidth - 1));
                    //    }
                    //}
                }
            }
        }

        public CustomComboBox()
        {
            BorderColor = Color.DimGray;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color BorderColor { get; set; }
    }

    public class FlatCombo : ComboBox
    {
        private const int WM_PAINT = 0xF;
        private int buttonWidth = SystemInformation.HorizontalScrollBarArrowWidth;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    using (var p = new Pen(this.ForeColor))
                    {
                        g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                        g.DrawLine(p, Width - buttonWidth, 0, Width - buttonWidth, Height);
                    }
                }
            }
        }
    }

    public interface IScreenStreamerServiceControl
    {
        ScreencastChannelInfo[] GetScreencastInfo();
    }

    public class ColoredCombo : ComboBox
    {
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
                e.Graphics.DrawRectangle(Pens.Red, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }
    }

    public class NetUtils
    {
        public static IEnumerable<int> GetFreePortRange(ProtocolType protocolType, int portsCount,
                        int leftBound = 49152, int rightBound = 65535, IEnumerable<int> exceptPorts = null)
        {
            var totalRange = Enumerable.Range(leftBound, rightBound - leftBound + 1);

            IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();

            IEnumerable<System.Net.IPEndPoint> activeListeners = null;
            if (protocolType == ProtocolType.Udp)
            {
                activeListeners = ipProps.GetActiveUdpListeners()
                    .Where(listener => listener.Port >= leftBound && listener.Port <= rightBound);
            }
            else if (protocolType == ProtocolType.Tcp)
            {
                activeListeners = ipProps.GetActiveTcpListeners()
                    .Where(listener => listener.Port >= leftBound && listener.Port <= rightBound);
            }

            //foreach (var listner in activeListeners) 
            //{
            //    Debug.WriteLine(listner);
            //}

            if (activeListeners == null) return null;

            //Список свободных портов  
            var freePorts = totalRange.Except(activeListeners.Select(listener => listener.Port));
            if (exceptPorts != null)
            {
                freePorts = freePorts.Except(exceptPorts);
            }

            return freePorts;
        }
    }


}
