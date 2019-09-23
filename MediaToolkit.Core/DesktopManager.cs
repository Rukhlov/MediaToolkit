using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;

namespace MediaToolkit.Core
{
    public class DesktopManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Socket socket = null;

        private InputSimulator inputSimulator = null;


        public Task Start(string addr = "0.0.0.0", int port = 8888)
        {
            logger.Debug("DesktopManager::Start(...) " + addr + " " + port);
     
            return Task.Run(() =>
            {

                running = true;

                logger.Info("Input simulator START");

                try
                {

                    var ipaddr = IPAddress.Parse(addr);
  
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                    socket.Bind(new IPEndPoint(ipaddr, port));

                    logger.Debug("Socket binded " + ipaddr.ToString() + " " + port);

                    inputSimulator = new InputSimulator();

                    byte[] buffer = new byte[1024];
                    while (running)
                    {
                        try
                        {
                            int bytesReceived = 0;
                            try
                            {
                                bytesReceived = socket.Receive(buffer, SocketFlags.None);
                            }
                            catch(SocketException ex)
                            {
                                if(ex.SocketErrorCode == SocketError.Interrupted || ex.SocketErrorCode == SocketError.TimedOut)
                                {
                                    break;
                                }
                                else
                                {
                                    throw;
                                }

                            }
                            catch (ObjectDisposedException)
                            {
                                break;
                            }

                            if (bytesReceived > 0)
                            {
                                var message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

                                var pars = message.Split(':');
                                if (pars != null && pars.Length > 1)
                                {
                                    //logger.Debug(string.Join(" ", pars));

                                    var command = pars[0];
                                    if (command == "MouseMove")
                                    {

                                        var position = pars[1];
                                        var pos = position.Split(' ');

                                        if (pos != null && pos.Length == 2)
                                        {
                                            var posX = pos[0];
                                            var posY = pos[1];

                                            if (posX != null && posY != null)
                                            {
                                                if (double.TryParse(posX.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double _x) &&
                                                   double.TryParse(posY.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double _y))
                                                {
                                                    inputSimulator.Mouse.MoveMouseTo(_x, _y);

                                                    //logger.Debug("MoveMouseTo: " + _x + " " + _y);
                                                }
                                            }

                                        }
                                    }
                                    else if (command == "MouseUp")
                                    {
                                        var par = pars[1];
                                        var buttonParams = par.Split(' ');

                                        if (buttonParams != null && buttonParams.Length == 2)
                                        {
                                            logger.Debug(command + " " + string.Join(" ", buttonParams));

                                            var button = (MouseButtons)int.Parse(buttonParams[0]);
                                            var click = int.Parse(buttonParams[1]);

                                            if (button == MouseButtons.Left)
                                            {
                                                inputSimulator.Mouse.LeftButtonUp();
                                            }
                                            else if (button == MouseButtons.Right)
                                            {
                                                inputSimulator.Mouse.RightButtonUp();
                                            }
                                        }
                                    }
                                    else if (command == "MouseDown")
                                    {
                                        var par = pars[1];
                                        var buttonParams = par.Split(' ');

                                        if (buttonParams != null && buttonParams.Length == 2)
                                        {
                                            logger.Debug(command + " " + string.Join(" ", buttonParams));

                                            var button = (MouseButtons)int.Parse(buttonParams[0]);
                                            var click = int.Parse(buttonParams[1]);

                                            if (button == MouseButtons.Left)
                                            {
                                                inputSimulator.Mouse.LeftButtonDown();
                                            }
                                            else if (button == MouseButtons.Right)
                                            {
                                                inputSimulator.Mouse.RightButtonDown();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                        }

                    }


                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                    running = false;


                }

                logger.Info("Input simulator STOP");

            });
        }


        private bool running = false;
        public void Stop()
        {
            logger.Debug("DesktopManager::Stop()");

            running = false;
            socket?.Close();
            //socket?.Shutdown(SocketShutdown.Both);
        }

    }


}
