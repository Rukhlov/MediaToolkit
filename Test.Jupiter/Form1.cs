﻿
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

        private CPClient cpClient = new CPClient();

        private void buttonInit_Click(object sender, EventArgs e)
        {
            Console.WriteLine("buttonInit_Click(...)");

            //jupiter = new JupiterApi();

            //var channelCount = jupiter.GetChannelCount();
            try
            {
                var host = textBoxHost.Text;
                var port = int.Parse(textBoxPort.Text);

                cpClient = new CPClient();

                cpClient.NotificationReceived += CPClient_NotificationReceived;
                cpClient.StateChanged += CpClient_StateChanged;

                cpClient.Connect(host, port);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void CpClient_StateChanged()
        {
            Console.WriteLine("CPClient_Connected(): " + cpClient.State);
        }

        private void CPClient_NotificationReceived(CPNotification notification)
        {
            Console.WriteLine("CPClient_NotificationReceived(...) " + notification.ToString());

            if(notification.ObjectName == "Notify") 
            {
                if(notification.Method == "WindowsState")
                {// +Notify WindowsState { nCount TWindowState pData[ ] } (Id Kind nState nStateChange x y w h ZAfter)

                    var valueList = notification.ValueList;
                    var windows = new TWindowStateList(valueList);

                    Console.WriteLine(windows.ToString());
                }
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {

            var message = "";
            try
            {
                var user = textBoxUserName.Text;
                var pass = textBoxPassword.Text;


                var authRequest = new CPRequest($"{user}\r\n{pass}\r\n");
                var resp = await cpClient.SendAsync(authRequest);

                message = resp.ToString();
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

                var request = new CPRequest("WinServer", "GetServerInfo");

                var response = await cpClient.SendAsync(request, 5000);

                message = response.ToString();

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
                var request = new CPRequest("WinServer", "RegisterNotifyTarget");
                var response = await cpClient.SendAsync(request);
                message = response?.ToString();
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
                var request = new CPRequest("WinServer", "UnregisterNotifyTarget");
                var response = await cpClient.SendAsync(request);
                message = response.ToString();

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

                var request = new CPRequest("RGBSys", "GetChannelRange");
                var response = await cpClient.SendAsync(request) as CPResponse;

                if (response.Success)
                {
                    var valueList = response.ValueList;
                    var parts = valueList.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    var firstCh = int.Parse(parts[0]);
                    var lastCh = int.Parse(parts[1]);

                    message = "FirstChannel = " + firstCh + "\r\n" + "LastChannel = " + lastCh;

                }


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

                var request = new CPRequest("RGBSys", "NewWindowWithId", new WinId(id));

                var response = await cpClient.SendAsync(request) as CPResponse;

                if (response.Success)
                {

                }


                message = response.ToString();

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

                //var request = new CPRequest("WinServer", "DeleteWindow", new WinId(id));

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

        private async void button13_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var channel = 1;
                var id = int.Parse(textBox1.Text);
                //+RGBSys SetChannel
                var request = new CPRequest("RGBSys", "SetChannel", new WinId(id), channel);

                var response = await cpClient.SendAsync(request) as CPResponse;

                if (response.Success)
                {

                }

                message = response.ToString();

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

                var request = new CPRequest("RGBSys", "Start", new WinId(id));

                var response = await cpClient.SendAsync(request) as CPResponse;

                if (response.Success)
                {

                }

                message = response.ToString();

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

                var request = new CPRequest("RGBSys", "Stop", new WinId(id));

                var response = await cpClient.SendAsync(request) as CPResponse;

                if (response.Success)
                {

                }

                message = response.ToString();

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
                    w = 640,
                    h = 480,

                    ZAfter = new WinId(-1),
                };

                var newState = await cpClient.Window.SetState(state);
                message = newState.ToString();

                //var request = new CPRequest("Window", "SetState", state);

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

        private async void button17_Click(object sender, EventArgs e)
        {
            var message = "";

            try
            {
                var id = int.Parse(textBox1.Text);

                var winId = new WinId(id);

                var fileName = await cpClient.Window.GrabImage(winId);
                message = fileName;

                //var request = new CPRequest("Window", "GrabImage", new WinId(id));

                //var response = await cpClient.SendAsync(request) as CPResponse;

                //if (response.Success)
                //{
                //    var file = response.ValueList;

                //    file = file.Replace("\"", "");

                //    var fullName = Path.Combine(@"C:\ProgramData\ControlPoint\ServerDataFiles\images\", file);

                //    if (File.Exists(fullName))
                //    {

                //        var _b = pictureBox1.Image;


                //        Bitmap bmp = (Bitmap)Bitmap.FromFile(fullName);

                //        pictureBox1.Image = new Bitmap(bmp);

                //        if (bmp != null)
                //        {
                //            bmp.Dispose();
                //            bmp = null;
                //        }

                //        if (_b != null)
                //        {
                //            _b.Dispose();
                //            _b = null;
                //        }


                //        File.Delete(fullName);
                //    }
                //}

                //message = response.ToString();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            MessageBox.Show(message);
        }
    }
}