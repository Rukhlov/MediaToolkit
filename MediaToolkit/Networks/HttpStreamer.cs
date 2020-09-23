
using MediaToolkit.Utils;
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
using MediaToolkit.Core;
using MediaToolkit.Logging;

namespace MediaToolkit.Networks
{

    public class HttpStreamer
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Networks");

        private const string BOUNDARY = "7b3cc56e5f51db803f790dad720ed50a";

        private List<MJpegClientHandler> clients = new List<MJpegClientHandler>();

        private HttpStats statCounter = new HttpStats();
        public StatCounter Stats => statCounter;

        private TcpListener listener = null;

        public Task Start(NetworkSettings networkParams)
        {
           
            logger.Debug("HttpStreamer::Start()");

            return Task.Run(() =>
            {
                running = true;

                string ipStr = networkParams.RemoteAddr;
                var addr = System.Net.IPAddress.Any;
                if (!System.Net.IPAddress.TryParse(ipStr, out addr))
                {
                    logger.Warn("Unsupprted ip format " + ipStr);
                }

                if (addr == null)
                {
                    addr = System.Net.IPAddress.Any;
                }

                var port = networkParams.RemotePort;

  
                try
                {
                    statCounter.Reset();

                    //Statistic.RegisterCounter(statCounter);

                    listener = new TcpListener(addr, port);
                    //listener.AllowNatTraversal(true);
                    listener.Start();

                   // List<Task> clientTasks = new List<Task>();
                    while (running)
                    {
                        try
                        {
                            logger.Info(" ========== Waiting for connection on " + listener.Server.LocalEndPoint.ToString() + " ================");

                            // System.Net.Sockets.Socket socket = listener.AcceptSocket();
                            var tcpClient = listener.AcceptTcpClient();

                            if (!running)
                            {
                                break;
                            }

                            var mjpegClient = new MJpegClientHandler(this);
                            lock (syncRoot)
                            {
                                clients.Add(mjpegClient);
                            }
      

                            logger.Debug("Clients count: " + clients.Count);

                            // Start response task...
                            var clientTask = mjpegClient.StartClientTask(tcpClient);
                            //clientTasks.Add(clientTask);

                            clientTask.ContinueWith((t) =>
                            {
                                lock (syncRoot)
                                {
                                    clients.Remove(mjpegClient);
                                }

                                var ex = t.Exception;
                                if (ex != null)
                                {
                                    logger.Error(ex);
                                    running = false;
                                }

                                logger.Debug("Clients count: " + clients.Count);
                            });

                        }
                        catch (Exception ex)
                        {
                            var socketException = ex as SocketException;
                            if (socketException != null)
                            {
                                var code = socketException.SocketErrorCode;
                                if (code == SocketError.ConnectionAborted ||
                                code == SocketError.ConnectionReset || 
                                code == SocketError.Interrupted)
                                {
                                    logger.Warn("Socket closed: " + code);
                                    break;
                                }
                            }

                            logger.Error(ex);
                            Thread.Sleep(1000);
                            //throw;
                        }
                    }
                    //Task.WaitAll(clientTasks.ToArray());
                }
                finally
                {
                    logger.Debug("listener?.Stop()");

                    listener?.Stop();
                    
                    //foreach (var c in clients)
                    //{
                    //    c.Dispose();
                    //}

                    //Statistic.UnregisterCounter(statCounter);
                }
            });

           
        }


        class MJpegClientHandler
        {
            private HttpStreamer streamer = null;
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

                            logger.Debug("Client request:\r\n" + request);
                        }

                        const int MaxClientCount = 100500;
                        if (streamer.clients.Count > MaxClientCount)
                        {
                            string resp = "HTTP/1.0 503 Service Unavailable\r\n" + "Connectin: close\r\n";
                            byte[] bytes = Encoding.ASCII.GetBytes(resp);
                            stream.Write(bytes, 0, bytes.Length);

                            return;
                        }

                        // Send response...
                        string response = "HTTP/1.0 200 OK\r\n" +
                                "Content-type: multipart/x-mixed-replace; boundary=" + BOUNDARY + "\r\n" +
                                "Cache-Control: no-cache\r\n\r\n";

                        byte[] respBytes = Encoding.ASCII.GetBytes(response);

                        logger.Debug("Serv response: " + response);
                        stream.Write(respBytes, 0, respBytes.Length);

                        logger.Verb("Response loop BEGIN");
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

                                this.streamer.statCounter.Update(MediaTimer.GetRelativeTime(), bytesToSend.Length);

                                // Statistic.RtpStats.Update(MediaTimer.GetRelativeTime(), bytesToSend.Length);

                                //logger.Debug("stream.Write(...) " + remoteAddr.ToString() + " "+ frameCount);

                            }

                            if (stream.CanRead && stream.DataAvailable)
                            { // Получаем от клиента запрос

                                string _request = "";
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

                                _request = sb.ToString();

                                if (_request != null)
                                {
                                    logger.Debug("Client request:\r\n" + _request);
                                }

                            }

                        }// next frame...

                        logger.Verb("Response loop END");
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

                        throw;
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
                logger.Debug("MJpegClientHandler::Stop()");

                running = false;
                syncEvent?.Set();


            }

            public void Dispose()
            {
                streamer = null;

                if (syncEvent != null)
                {
                    syncEvent.Dispose();
                    syncEvent = null;
                }
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
                    lock (syncRoot)
                    {
                        for (int i = 0; i < clients.Count; i++)
                        {
                            var c = clients[i];
                            c?.SignalToResponse();
                        }

                        //foreach (var client in clients)
                        //{
                        //    client.SignalToResponse();
                        //}
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

        private object syncRoot = new object();
        private bool running = false;
        public void Stop()
        {
            logger.Debug("HttpStreamer::Stop()");

            running = false;

            lock (syncRoot)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    var c = clients[i];
                    c?.Stop();
                }
            }


            listener?.Stop();

        }

    }

    public class HttpStats : StatCounter
    {
        public uint packetsCount = 0;

        public long totalBytesSend = 0;
        public double sendBytesPerSec = 0;

        public double lastTimestamp = 0;

        public HttpStats() { }

        public void Update(double timestamp, int bytesSend)
        {
            if (lastTimestamp > 0)
            {
                var time = timestamp - lastTimestamp;
                if (time > 0)
                {
                    var bytesPerSec = bytesSend / time;

                    sendBytesPerSec = (bytesPerSec * 0.05 + sendBytesPerSec * (1 - 0.05));
                }
            }

            totalBytesSend += bytesSend;

            lastTimestamp = timestamp;
            packetsCount++;
        }

        public override string GetReport()
        {
            StringBuilder sb = new StringBuilder();

            //var mbytesPerSec = sendBytesPerSec / (1024.0 * 1024);
            //var mbytes = totalBytesSend / (1024.0 * 1024);

            sb.AppendLine(packetsCount + " Frames");

            sb.AppendLine(StringHelper.SizeSuffix((long)sendBytesPerSec) + "/s");
            sb.AppendLine(StringHelper.SizeSuffix(totalBytesSend));


            //sb.AppendLine(mbytes.ToString("0.0") + " MBytes");
            //sb.AppendLine(mbytesPerSec.ToString("0.000") + " MByte/s");

            return sb.ToString();
        }

        public override void Reset()
        {
            packetsCount = 0;

            totalBytesSend = 0;
            sendBytesPerSec = 0;

            lastTimestamp = 0;
        }
    }

}
