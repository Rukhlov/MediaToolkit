using CommonData;
using FFmpegWrapper;
using NLog;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer
{
    class MJpegOverHttpStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ScreenSource screenSource = null;

        public MJpegOverHttpStreamer(ScreenSource source)
        {
            this.screenSource = source;
        }

        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        public void Start(VideoEncodingParams encPars)
        {
            Task.Run(() =>
            {
                running = true;

                screenSource.BufferUpdated += () => syncEvent.Set();

                encPars.EncoderName = "mjpeg";

                FFmpegVideoEncoder encoder = new FFmpegVideoEncoder();

                encoder.Open(encPars);

                List<AutoResetEvent> syncEvents = new List<AutoResetEvent>();

                HttpStreamer httpStreamer = new HttpStreamer();

                double sec = 0;

                encoder.DataEncoded += (ptr, len) =>
                {// получили данные от энкодера 


                    httpStreamer.TryToPush(ptr, len, sec);

                    // File.WriteAllBytes("d:\\test_3.jpg", frame);

                };


                Task.Run(() =>
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    while (running)
                    {
                        sw.Restart();

                        try
                        {
                            syncEvent.WaitOne();

                            if (!running)
                            {
                                break;
                            }

                            var buffer = screenSource.Buffer;
                            sec = buffer.time;
                            encoder.Encode(buffer, sec);

                            //sec += 0.2;//(sw.ElapsedMilliseconds / 1000.0);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            Thread.Sleep(1000);
                        }

                        //rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                        var mSec = sw.ElapsedMilliseconds;

                    }

                    httpStreamer?.Stop();

                });

                httpStreamer.Start();
            });
        }


        private bool running = false;

        public void Close()
        {
            running = false;

            syncEvent.Set();
        }

        class HttpStreamer
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();

            private const string BOUNDARY = "7b3cc56e5f51db803f790dad720ed50a";

            private List<MJpegClientHandler> clients = new List<MJpegClientHandler>();

            public readonly OutputStats Stats = new OutputStats();
            public void Start()
            {
                Statistic.Stats.Add(Stats);

                logger.Debug("Start()");
                running = true;

                var addr = System.Net.IPAddress.Any;
                //var addr = System.Net.IPAddress.Parse("192.168.1.135");
                var port = 8086;
                var listener = new TcpListener(addr, port);
                //listener.AllowNatTraversal(true);

                try
                {

                    listener.Start();

                    while (running)
                    {
                        try
                        {
                            logger.Info(" ========== Waiting for connection on " + listener.Server.LocalEndPoint.ToString() + " ================");

                            // System.Net.Sockets.Socket socket = listener.AcceptSocket();
                            var tcpClient = listener.AcceptTcpClient();

                            var mjpegClient = new MJpegClientHandler(this);
                            clients.Add(mjpegClient);

                            logger.Debug("Clients count: " + clients.Count);

                            // Start response task...
                            var clientTask = mjpegClient.StartClientTask(tcpClient);

                            clientTask.ContinueWith((_) =>
                            {
                                clients.Remove(mjpegClient);

                                logger.Debug("Clients count: " + clients.Count);
                            });


                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);

                            Thread.Sleep(1000);
                        }
                    }
                }
                finally
                {
                    logger.Debug("listener?.Stop()");

                    listener?.Stop();
                }
            }

            class MJpegClientHandler
            {
                private readonly HttpStreamer streamer = null;
                internal MJpegClientHandler(HttpStreamer streamer)
                {
                    this.streamer = streamer;
                }

                //private TcpClient tcpClient = null;
                private AutoResetEvent syncEvent = new AutoResetEvent(false);

                private bool running = false;

                internal Task StartClientTask(TcpClient tcpClient)
                {
                    logger.Debug("StartClientTask(...)");

                    return Task.Run(() =>
                    {
                        running = true;

                        var socket = tcpClient.Client;

                        var remoteAddr = socket.RemoteEndPoint;
                        logger.Info("Client connected: " + remoteAddr.ToString());

                        try
                        {

                            var stream = tcpClient.GetStream();

                            string request = "";
                            if (stream.CanRead && stream.DataAvailable)
                            {
                                byte[] buf = new byte[1024];
                                StringBuilder sb = new StringBuilder();
                                int bytesRead = 0;
                                do
                                {
                                    bytesRead = stream.Read(buf, 0, buf.Length);
                                    var str = Encoding.ASCII.GetString(buf, 0, bytesRead);
                                    sb.Append(str);
                                }
                                while (stream.DataAvailable);

                                request = sb.ToString();

                            }

                            //TODO: process request...
                            if (request != null)
                            {

                                logger.Debug("Client request: " + request);
                            }

                            // Send response...
                            string response = "HTTP/1.0 200 OK\r\n" +
                                    "Content-type: multipart/x-mixed-replace; boundary=" + BOUNDARY + "\r\n" +
                                    "Cache-Control: no-cache\r\n\r\n";

                            byte[] respBytes = Encoding.ASCII.GetBytes(response);

                            logger.Debug("Serv response: " + response);
                            stream.Write(respBytes, 0, respBytes.Length);

                            logger.Trace("Response loop BEGIN");
                            while (running)
                            {
                                if (!syncEvent.WaitOne(1000))
                                {
                                    continue;
                                }

                                byte[] bytesToSend = streamer.TryToPullResponse();

                                if (!running)
                                {
                                    break;
                                }

                                if (bytesToSend != null)
                                {

                                    stream.Write(bytesToSend, 0, bytesToSend.Length);

                                    this.streamer.Stats.Update(MediaTimer.GetRelativeTime(), bytesToSend.Length);

                                   // Statistic.RtpStats.Update(MediaTimer.GetRelativeTime(), bytesToSend.Length);
                                    

                                    //logger.Debug("stream.Write(...) " + remoteAddr.ToString() + " "+ frameCount);

                                }

                                if (stream.CanRead && stream.DataAvailable)
                                {
                                    //...
                                    logger.Warn("stream.CanRead && stream.DataAvailable");
                                }

                            }// next frame...

                            logger.Trace("Response loop END");
                        }
                        catch (IOException ex)
                        {
                            var iex = ex.InnerException;
                            if (iex != null)
                            {
                                var socketException = iex as SocketException;
                                if (socketException != null)
                                {
                                    var code = socketException.SocketErrorCode;
                                    if (code == SocketError.ConnectionAborted || code == SocketError.ConnectionReset)
                                    {
                                        logger.Warn(code + " " + remoteAddr.ToString());
                                        return;
                                    }
                                }
                            }

                            logger.Error(ex);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);

                        }
                        finally
                        {
                            running = false;
                            tcpClient?.Close();

                            logger.Info("Client disconnected: " + remoteAddr.ToString());

                        }

                    });

                }

                internal void SignalToResponse()
                {
                    syncEvent?.Set();
                }


                internal void Stop()
                {
                    logger.Debug("Stop()");

                    running = false;
                    syncEvent?.Set();
                }
            }


            private byte[] responseBuffer = new byte[1024];

            private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

            internal byte[] TryToPullResponse(int timeout = 10)
            {
                byte[] respByte = null;

                bool locked = lockSlim.TryEnterReadLock(timeout);
                try
                {
                    if (locked)
                    {
                        if (responseBuffer != null)
                        {
                            respByte = new byte[responseBuffer.Length];
                            Buffer.BlockCopy(responseBuffer, 0, respByte, 0, responseBuffer.Length);
                        }
                    }
                    else
                    {
                        logger.Warn("lockSlim.TryEnterReadLock(10)== false");
                    }
                }
                finally
                {
                    if (locked)
                    {
                        lockSlim.ExitReadLock();
                    }
                }


                return respByte;
            }

            public void TryToPush(IntPtr ptr, int len, double sec)
            {
                bool locked = lockSlim.TryEnterWriteLock(40);
                try
                {
                    if (locked)
                    {

                        //var contentBuffer = new byte[len];
                        //Marshal.Copy(ptr, contentBuffer, 0, len);

                        responseBuffer = CreateMJpegResponse(ptr, len, sec);

                        foreach (var client in clients)
                        {
                            client.SignalToResponse();
                        }
                    }
                    else
                    {
                        logger.Warn("lockSlim.TryEnterWriteLock(40) == false");
                    }
                }
                finally
                {
                    if (locked)
                    {
                        lockSlim.ExitWriteLock();
                    }
                }

            }

            private byte[] CreateMJpegResponse(IntPtr contentData, int contentLength, double timestamp)
            //private byte[] CreateMJpegResponse(byte[] contentBytes, double timestamp)
            {
                string respHeader = "\r\n" +
                                "--" + BOUNDARY + "\r\n" +
                                "Content-Type: image/jpeg\r\n" +
                                "Content-Length: " + contentLength + "\r\n" +
                                "X-Timestamp: " + timestamp.ToString("0.00000") + "\r\n\r\n";

                byte[] headerBytes = Encoding.ASCII.GetBytes(respHeader);
                byte[] footerBytes = Encoding.ASCII.GetBytes("\r\n");

                var respLength = headerBytes.Length + contentLength + footerBytes.Length;
                var responseByte = new byte[respLength];

                int offset = 0;
                Array.Copy(headerBytes, 0, responseByte, offset, respHeader.Length);
                offset += respHeader.Length;

                Marshal.Copy(contentData, responseByte, offset, contentLength);
                //Array.Copy(contentBytes, 0, responseByte, offset, contentBytes.Length);
                offset += contentLength;

                Array.Copy(footerBytes, 0, responseByte, offset, footerBytes.Length);
                offset += footerBytes.Length;
                return responseByte;
            }


            private bool running = false;
            public void Stop()
            {
                logger.Debug("Stop()");

                running = false;

                foreach (var client in clients)
                {
                    client.Stop();
                }
            }

        }
    }
}
