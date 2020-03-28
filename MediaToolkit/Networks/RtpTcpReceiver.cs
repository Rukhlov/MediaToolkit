
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.SharedTypes;
using System.Diagnostics;
using MediaToolkit.Logging;

namespace MediaToolkit.Networks
{
    public class RtpTcpReceiver : IRtpReceiver
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Networks");

        private RtpSession session = null;

        private Socket socket = null;
        public IPEndPoint RemoteEndpoint { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }


        private volatile ErrorCode errorCode = ErrorCode.Ok;
        public ErrorCode ErrorCode => errorCode;

        private volatile ReceiverState state = ReceiverState.Closed;
        public ReceiverState State => state;

        public RtpTcpReceiver(RtpSession session)
        {
            this.session = session;
        }

        public void Open(NetworkSettings settings)//string address, int port, int ttl = 10)
        {
            if (state != ReceiverState.Closed)
            {
                throw new InvalidOperationException("Invalid receiver state " + state);
            }

            var address = settings.LocalAddr;
            var port = settings.LocalPort;
            var ttl = 10;

            logger.Debug("RtpTcpReceiver::Open(...) " + address + " " + port + " " + ttl);
            try
            {
                IPAddress addr = IPAddress.Parse(address);

                RemoteEndpoint = new IPEndPoint(addr, port);
                LocalEndpoint = new IPEndPoint(IPAddress.Any, 0);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


               // socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.ReceiveBufferSize = 100 * 1024;//int.MaxValue;//32 * 1024 * 1024;

                socket.Connect(RemoteEndpoint);

                state = ReceiverState.Initialized;

                logger.Info("Client started: " + LocalEndpoint.ToString() + " " + RemoteEndpoint.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Close();
                throw;
            }
        }



        public Task Start()
        {

            logger.Debug("TcpReceiver::Start()");

            if (state != ReceiverState.Initialized)
            {
                throw new InvalidOperationException("Invalid state " + state);
            }

            return Task.Run(() =>
            {
                state = ReceiverState.Running;

                try
                {
                    logger.Debug("TcpReceiver thread started...");

                    byte[] buf = new byte[16256];

                    var tcpHanlder = new TcpHandler();

                    while (state == ReceiverState.Running)
                    {

                        int bytesReceived = socket.Receive(buf, SocketFlags.None);

                        if (bytesReceived > 0)
                        {
                            var frames = tcpHanlder.ProcessData(buf, bytesReceived, session);

                            if(frames!=null && frames.Count > 0)
                            {
                                foreach(var frame in frames)
                                {
                                    var rtpData = frame.Data;
                                    RtpPacket rtpPacket = RtpPacket.Create(rtpData, rtpData.Length, session);
                                    if (rtpPacket != null)
                                    {
                                        OnRtpPacketReceived(rtpPacket);
                                    }
                                }
                            }
                        }

                    }
                }
                catch (ObjectDisposedException)
                {
                    logger.Warn("RtpTcpReceiver::ObjectDisposedException");
                }
                catch (Exception ex)
                {
                    bool socketAborted = false;
                    var socketException = ex as SocketException;
                    if (socketException != null)
                    {
                        var code = socketException.SocketErrorCode;
                        if (code == SocketError.ConnectionAborted ||
                            code == SocketError.ConnectionReset ||
                            code == SocketError.Interrupted)
                        {
                            logger.Warn("Socket closed: " + code);
                            socketAborted = true;
                        }
                    }

                    if (!socketAborted)
                    {
                        logger.Error(ex);
                    }
                    

                }
                finally
                {
                    Close();
                    logger.Debug("TcpReceiver thread stopped...");
                }
 
            });

        }

        class TcpHandler
        {
            enum RtspStreamState
            {
                WaitDollar,

                ReadId,
                ReadLen,
                ReadPkt,

                ReadHeader,
                ReadContent,
            }

            RtspStreamState streamState = RtspStreamState.WaitDollar;

            byte[] tcpBuffer = new byte[30 * 1024 * 1024];
            int tcpBufferOffset = 0;
            RtspFrame rtspFrame = null;

            public List<RtspFrame> ProcessData(byte[] buf, int length, RtpSession session)
            {

                List<RtspFrame> frames = new List<RtspFrame>();
                int offset = 0;
                int pos = 0;

                while (true)
                {
                    if (offset >= length)
                    {
                        logger.Debug("!!!!!!!!!!!!!!!!!!! " + offset + " >= " + length);

                        break;
                        //return;
                    }

                    if (tcpBufferOffset > 0)
                    {// если остались не обработанные данные
                        int len = length - offset;
                        int newSize = tcpBufferOffset + len;

                        byte[] newBuf = new byte[newSize];

                        Array.Copy(tcpBuffer, 0, newBuf, 0, tcpBufferOffset);

                        Array.Copy(buf, offset, newBuf, tcpBufferOffset, len);

                        tcpBufferOffset = 0;

                        // Debug.WriteLine("ProcessTcpBuffer newSize " + newSize);

                        buf = newBuf;
                        length = newSize;
                    }

                    if (streamState == RtspStreamState.WaitDollar ||
                        streamState == RtspStreamState.ReadHeader ||
                        streamState == RtspStreamState.ReadContent)
                    {
                        for (int i = offset; i < length; i++)
                        {
                            char ch = (char)buf[i];
                            if (ch == '$')
                            {
                                //Console.WriteLine("Find dollar at pos: " + i);

                                streamState = RtspStreamState.ReadId;
                                pos = i + 1; //header
                                rtspFrame = new RtspFrame();

                                break;
                            }
                        }
                    }

                    if (streamState == RtspStreamState.ReadId)
                    {// идентификатор 1 байт
                        int len = length - pos;
                        if (len > 0)
                        {
                            rtspFrame.Id = buf[pos];
                            //Console.WriteLine("ID: " + rtspFrame.Id + " pos: " + pos);

                            pos++;

                            streamState = RtspStreamState.ReadLen;
                        }
                        else
                        { // буфер закончился выходим 
                            break;
                        }
                    }

                    if (streamState == RtspStreamState.ReadLen)
                    {// длина пакета с данными 2 байта

                        int len = length - pos;

                        if (len > 1)
                        {
                            //int frameLength = BitConverter.ToInt16(buf, pos);

                            int frameLength = BigEndian.ReadInt16(buf, pos);

                            if(frameLength <= 0)
                            {
                                logger.Warn("frameLength " + frameLength);
                                //TODO:
                                //...
                            }

                            rtspFrame.Init(frameLength);

                            //Console.WriteLine("frameLength: " + frameLength + " pos: " + pos);

                            pos += 2;

                            streamState = RtspStreamState.ReadPkt;

                        }
                        else
                        {// сохраняем в буффер то что есть и выходим

                            // Debug.WriteLine(len + " < " + 2);

                            Array.Copy(buf, pos, tcpBuffer, tcpBufferOffset, len);

                            tcpBufferOffset += len;
                            //...
                            break;
                        }

                    }

                    if (streamState == RtspStreamState.ReadPkt)
                    {
                        if (rtspFrame != null && rtspFrame.Length > 0)
                        {
                            // сколькоданных в буфере
                            int bufToRead = length - pos;

                            int bytesAdded = rtspFrame.AddData(buf, pos, bufToRead);
                            pos += bytesAdded;

                            if (rtspFrame.BytesToRead == 0)
                            {// все данные прочитаны

                                // client.ProcessRtspFrame(rtspFrame);
                                //Console.WriteLine(rtspFrame.ToString());

                                var rtpData = rtspFrame.Data;
                                RtpPacket rtpPacket = RtpPacket.Create(rtpData, rtpData.Length, session);

                                // OnRtpPacketReceived(rtpPacket);

                                frames.Add(rtspFrame);

                                // переходим в режим ожидания доллара
                                streamState = RtspStreamState.WaitDollar;

                                // читаем следующий rtsp frame
                                rtspFrame = null;
                            }

                            if (pos < length)
                            {
                                //ProcessTcpBuffer(buf, pos, length);
                                offset = pos;
                                continue;

                            }

                            break;
                            // return;
                        }
                    }

                    {  // понять что это за данные пока не удалось копируем в буфер
                        int bufToRead = length - offset;

                        Array.Copy(buf, offset, tcpBuffer, tcpBufferOffset, bufToRead);

                        tcpBufferOffset += bufToRead;

                        logger.Debug("Array.Copy tcpBufferOffset " + tcpBufferOffset);

                        break;
                    }

                }

                return frames;
            }

        }

        class RtspFrame
        {
            // RFC 2326 10.12 Embedded (Interleaved) Binary Data

            /*
            *   S->C: $\000{2 byte length}{"length" bytes data, w/RTP header}
                S->C: $\000{2 byte length}{"length" bytes data, w/RTP header}
                S->C: $\001{2 byte length}{"length" bytes  RTCP packet}
            */

            public int Id;
            public int Length;
            public byte[] Data;

            private int dataOffset = 0;

            public void Init(int len)
            {
                if (len > 0)
                {
                    Length = len;
                    Data = new byte[len];
                    dataOffset = 0;
                }
                else
                {
                    throw new ArgumentException("len " + len);
                }
            }

            public int AddData(byte[] buf, int offset, int len)
            {
                int dataToRead = 0;

                if (BytesToRead > 0)
                {
                    dataToRead = (len >= BytesToRead) ? BytesToRead : len;

                    Array.Copy(buf, offset, Data, dataOffset, dataToRead);

                    dataOffset += dataToRead;
                }

                return dataToRead;
            }

            public int BytesToRead
            {
                get
                {

                    return this.Length - this.dataOffset;
                }
            }


            public override string ToString()
            {
                return "Frame Id=" + Id + " Length=" + Length + " DataOffset=" + dataOffset;
            }
        }


        public event Action<RtpPacket> RtpPacketReceived;
        private void OnRtpPacketReceived(RtpPacket pkt)
        {
            RtpPacketReceived?.Invoke(pkt);
        }

        public event Action<byte[], int> DataReceived;
        private void OnDataReceived(byte[] buf, int len)
        {
            DataReceived?.Invoke(buf, len);
        }


        public void Stop()
        {
            logger.Debug("RtpTcpReceiver::Stop()");

            state = ReceiverState.Closing;
        }


        private void Close()
        {
            logger.Debug("RtpTcpReceiver::Close()");

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            state = ReceiverState.Closed;
        }

    }

}
