
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Threading;
using MediaToolkit.Jupiter;
using System.Runtime.InteropServices;

namespace Test.Jupiter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {

            //"+RGBSys NewWindowWithId { " + windowId + " }"


            //if (jupiter == null) 
            //{
            //    return;
            //}

            //var channelCount = jupiter.GetChannelCount();

            //if (channelCount > 0) 
            //{
            //   var winId = jupiter.OpenWindow(1, 0, 0, 100, 100);
            //    jupiter.ChangeWindow(winId, 10, 10, 640, 480);

            //   // jupiter.S
            //}

            ////MessageBox.Show("channelCount " + channelCount);
        }

        //private JupiterConnection jupiterConnection = null;

        // private JupiterApi jupiter = null;

        private CPClient cpClient = null;

        private void buttonInit_Click(object sender, EventArgs e)
        {
            Console.WriteLine("buttonInit_Click(...)");

            //jupiter = new JupiterApi();

            //var channelCount = jupiter.GetChannelCount();
            try
            {
                var host = textBoxHost.Text;
                var port = int.Parse(textBoxPort.Text);

                if(cpClient == null)
                {
                    cpClient = new CPClient();

                    cpClient.Notify.WindowStateEvent += Notify_WindowStateEvent;

                    cpClient.NotificationReceived += CPClient_NotificationReceived;
                    cpClient.StateChanged += CpClient_StateChanged;
                }

                if (!cpClient.IsConnected)
                {
                    cpClient.Connect(host, port);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

		private void Notify_WindowStateEvent(TWindowStateList obj)
		{
			Console.WriteLine("Notify_WindowStateEvent: " + obj);


		}

        private void CpClient_StateChanged()
        {
            Console.WriteLine("CpClient_StateChanged(): " + cpClient.State);

            var state = cpClient.State;
            if (state == ClientState.Connected || state == ClientState.Disconnected)
            {
                MessageBox.Show("CpClient_StateChanged(): " + cpClient.State);
            }
           
        }

        private void CPClient_NotificationReceived(CPNotification notification)
        {
            Console.WriteLine("CPClient_NotificationReceived(...) " + notification.ToString());

            //if(notification.ObjectName == "Notify") 
            //{
            //    if(notification.Method == "WindowsState")
            //    {// +Notify WindowsState { nCount TWindowState pData[ ] } (Id Kind nState nStateChange x y w h ZAfter)

            //        var valueList = notification.ValueList;
            //        var windows = new TWindowStateList(valueList);

            //        Console.WriteLine(windows.ToString());
            //    }
            //}
        }

        private async void button6_Click(object sender, EventArgs e)
        {

            var message = "";
            try
            {
                var user = textBoxUserName.Text;
                var pass = textBoxPassword.Text;

				bool success = await cpClient.Authenticate(user, pass);

                message = "Authenticate " + success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                message = ex.Message;
            }

            MessageBox.Show(message);

        }


        private void button1_Click(object sender, EventArgs e)
        {
            //{
            //    if (jupiter == null)
            //    {
            //        return;
            //    }

            //    var winIds = jupiter.QueryAllWindows();

            //    var result = "NotFound";
            //    if (winIds != null && winIds.Length > 0)
            //    {
            //        result = string.Join("; ", winIds);
            //    }
            //    comboBox1.DataSource = winIds;

            //    MessageBox.Show("GetWindowIds() " + result);
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (jupiter == null)
            //{
            //    return;
            //}

            //jupiter.DeleteAllWindows();
        }

        //private void button3_Click(object sender, EventArgs e)
        //{


        //    //var winIds = jupiter.GetWindowIds();

        //    //var winIds = comboBox1.DataSource as int[];

        //    //if (winIds!=null && winIds.Length > 0) 
        //    {
        //        var item = comboBox1.SelectedItem;
        //        if (item != null)
        //        {
        //            var winid = (int)item;

        //            jupiter.GrabImage(winid, out var file);

        //            file = file.Replace("\"", "");

        //            var fullName = Path.Combine(@"C:\ProgramData\ControlPoint\ServerDataFiles\images\", file);

        //            if (File.Exists(fullName))
        //            {

        //                var _b = pictureBox1.Image;


        //                Bitmap bmp = (Bitmap)Bitmap.FromFile(fullName);

        //                pictureBox1.Image = new Bitmap(bmp);

        //                if (bmp != null)
        //                {
        //                    bmp.Dispose();
        //                    bmp = null;
        //                }

        //                if (_b != null)
        //                {
        //                    _b.Dispose();
        //                    _b = null;
        //                }


        //                File.Delete(fullName);
        //            }

        //        }




        //        //MessageBox.Show(resp);
        //    }

        //}

        private async void button4_Click(object sender, EventArgs e)
        {
            var message = "";
            try
            {
				var serverInfo = await cpClient.ConfigSys.GetServerInfo();
				message = serverInfo.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);




        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                cpClient.Disconnect();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }



        private void buttonClose_Click(object sender, EventArgs e)
        {
            //if (jupiter != null)
            //{
            //    jupiter.Dispose();
            //    jupiter = null;
            //}
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            var message = "";
            try
            {
    			var success = await cpClient.WinServer.RegisterNotifyTarget();
				message = "RegisterNotifyTarget() " + success;
			}
            catch (Exception ex)
            {
                message = ex.Message;
            }
            MessageBox.Show(message);
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            var message = "";
            try
            {
				var success = await cpClient.WinServer.UnregisterNotifyTarget();
				message = "RegisterNotifyTarget() " + success;

			}
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }


        private async void button9_Click(object sender, EventArgs e)
        {
            var message = "";
            try
            {

                var windows = await cpClient.WinServer.QueryAllWindows();

                message = windows.ToString();

                //var request = new CPRequest("WinServer", "QueryAllWindows");
                //var response = await cpClient.SendAsync(request) as CPResponse;

                //if (response.Success)
                //{
                //    var valueList = response.ValueList;

                //    TWindowStateList windows = new TWindowStateList(valueList);

                //    message = windows.ToString();

                //}


                //message = response.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button10_Click(object sender, EventArgs e)
        {
            //GetChannelRange

            var message = "";
            try
            {

				var channelRange = await cpClient.RGBSys.GetChannelRange();

				message = "FirstChannel = " + channelRange.Item1 + "\r\n" + "LastChannel = " + channelRange.Item2;

                //message = response.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);

        }

        private async void button11_Click(object sender, EventArgs e)
        {
            var message = "";
            
            try
            {
                var id = int.Parse(textBox1.Text);

				var winId = new WinId(id);

				var success = await cpClient.RGBSys.NewWindowWithId(winId);
				message = "NewWindowWithId() " + success;

                //var request = new CPRequest("RGBSys", "NewWindowWithId", new WinId(id));

                //var response = await cpClient.SendAsync(request) as CPResponse;

                //if (response.Success)
                //{

                //}
                //message = response.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button12_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

                var winId = new WinId(id);

                await cpClient.WinServer.DeleteWindow(winId);
				message = "OK";

			}
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button13_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var channel = int.Parse(textBox2.Text);

                var id = int.Parse(textBox1.Text);

				var winId = new WinId(id);

				var success = await cpClient.RGBSys.SetChannel(winId, channel);

				message = success ? "OK" : "FALSE";

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button14_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

				var winId = new WinId(id);

				var success = await cpClient.RGBSys.Start(winId);

				message = success ? "OK" : "FALSE";

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button15_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
				var id = int.Parse(textBox1.Text);

				var winId = new WinId(id);

				var success = await cpClient.RGBSys.Stop(winId);

				message = success ? "OK" : "FALSE";

			}
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button16_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

                var state = new TWindowState
                {
                    Id = new WinId(id),

                    Kind = SubSystemKind.RGBCapture,
                    State = (uint)(StateFlag.wsVisible | StateFlag.wsFramed),
                    StateChange = (uint)(StateFlag.wsVisible | StateFlag.wsSize | StateFlag.wsPosition | StateFlag.wsZOrder),

                    x = 10,
                    y = 10,
                    w = 200,
                    h = 200,

                    ZAfter = new WinId(-1),
                };

                var newState = await cpClient.Window.SetState(state);
                message = newState.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button17_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

                var winId = new WinId(id);

                var fileName = await cpClient.Window.GrabImage(winId);
               

                if (!string.IsNullOrEmpty(fileName))
                {

                    fileName = fileName.Replace("\"", "");

                    var fullName = Path.Combine(@"C:\ProgramData\ControlPoint\ServerDataFiles\images\", fileName);

                    if (File.Exists(fullName))
                    {

                        var _b = pictureBox1.Image;


                        Bitmap bmp = (Bitmap)Bitmap.FromFile(fullName);

                        pictureBox1.Image = new Bitmap(bmp);

                        if (bmp != null)
                        {
                            bmp.Dispose();
                            bmp = null;
                        }

                        if (_b != null)
                        {
                            _b.Dispose();
                            _b = null;
                        }


                        File.Delete(fullName);
                    }
                }

                message = fileName;

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button18_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

                var winId = new WinId(id);

                var timing = await cpClient.RGBSys.DetectTiming(winId);
                message = timing.ToString(); 

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }

        private async void button19_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

                var winId = new WinId(id);

                var timing = await cpClient.RGBSys.SetAutoDetectTiming(winId, true);

                message = timing.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }


        CancellationTokenSource ts = new CancellationTokenSource();
        private async void button20_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                ts = new CancellationTokenSource();
                var resp = await cpClient.SendAsync(new CPRequest("1234"), ts.Token, 20000);
                message = resp.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }
        
        private void button21_Click(object sender, EventArgs e)
        {

            ts.Cancel();

        }

		private async void button22_Click(object sender, EventArgs e)
		{
			var message = "";

			try
			{
				var id = int.Parse(textBox1.Text);

				var winId = new WinId(id);

				var result = await cpClient.WinServer.Quit();

				message = result.ToString();

			}
			catch (Exception ex)
			{
				message = ex.Message;
			}

			MessageBox.Show(message);
		}

        private async void button23_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

                var winId = new WinId(id);

                var preview = await cpClient.Window.GetPreview(winId);
                if(preview != null) 
                {
                    var bmp = preview.GetBitmap();

                    var _b = pictureBox1.Image;

                    pictureBox1.Image = new Bitmap(bmp);

                    if (bmp != null)
                    {
                        bmp.Dispose();
                        bmp = null;
                    }

                    if (_b != null)
                    {
                        _b.Dispose();
                        _b = null;
                    }


                    //bmp.Save("test.bmp");

                    message = preview.Width + " " + preview.Height;
                }
                else 
                {
                    message = "FALSE";
                }




            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            Console.WriteLine(message);
            //MessageBox.Show(message);
        }
    }
}
