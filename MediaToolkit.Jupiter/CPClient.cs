using MediaToolkit.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Jupiter
{

    public enum ClientState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Closed,
    }


    public class CPClient
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Jupiter");

        public CPClient()
        {
            Window = new Window(this);
            WinServer = new WinServer(this);
        }

        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 25456;

        public NetworkCredential Credential { get; private set; } = new NetworkCredential("admin", "");

        public event Action<CPNotification> NotificationReceived;
        public event Action StateChanged;

        private volatile ClientState state = ClientState.Disconnected;
        public ClientState State => state;

        private Socket socket = null;
        //private volatile bool running = false;

        public bool IsConnected => socket?.Connected ?? false;

        public Window Window { get; } = null;
        public WinServer WinServer { get; } = null;

        private Queue<InvokeCommand> commandQueue = new Queue<InvokeCommand>();

        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private object syncRoot = new object();

        private ushort cSeq = 0;

        private volatile InvokeCommand sendCommand = null;

        private void StartCommand(InvokeCommand command)
        {
            if (state != ClientState.Connected)
            {
                logger.Warn("StartCommand(...) invalid state: " + state);
                return;
            }

            lock (syncRoot)
            {
                commandQueue.Enqueue(command);
            }

            syncEvent.Set();
        }

        private bool TryGetCommand(out InvokeCommand command)
        {
            bool result = false;
            command = null;

            //if (state != ClientState.Connected)
            //{
            //    logger.Warn("TryGetCommand(...) invalid state: " + state);
            //    return result;
            //}

            lock (syncRoot)
            {
                if (commandQueue.Count > 0)
                {
                    command = commandQueue.Dequeue();
                    result = (command != null);
                }
            }

            return result;
        }

        private void DrainCommands()
        {
            logger.Debug("DrainCommands()");

            if (sendCommand != null)
            {
                sendCommand.SetResult(null, null);
                sendCommand = null;

                //sendCommand.Dispose();
                //sendCommand = null;
            }

            while (commandQueue.Count > 0)
            {
                TryGetCommand(out var command);

                if (command != null)
                {
                    command.SetResult(null, null);

                    //command.Dispose();
                    //command = null;
                }
            }
        }

        public bool Connect(string host, int port = 25456)
        {
            logger.Debug("Connect() " + host + " " + port);

            if (state != ClientState.Disconnected)
            {
                logger.Warn("Connect(...) invalid state: " + state);
                return false;
            }

            state = ClientState.Connecting;
            StateChanged?.Invoke();

            this.Host = host;
            this.Port = port;

            Task.Run(() =>
            {
                CommunicationLoop();
            });

            return true;

        }

        private void CommunicationLoop()
        {
            logger.Verb("CommunicationLoop BEGIN");

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = 5000;
                socket.SendTimeout = 5000;

                logger.Verb($"socket.ConnectAsync(...) {Host} {Port}");

                var connectionTask = socket.ConnectAsync(Host, Port);

                Task.WaitAny(new[] { connectionTask }, 5000);

                if (!socket.Connected)
                {
                    throw new TimeoutException("Connection error");
                }

                if (state == ClientState.Connecting)
                {
                    state = ClientState.Connected;
                }

                StateChanged?.Invoke();

                byte[] tempBuffer = null;


                bool socketClosed = false;
                while (state == ClientState.Connected || socketClosed)
                {
                    int available = socket.Available;
                    if (available > 0)
                    {
                        List<byte[]> responseBuffer = new List<byte[]>();

                        CPResponseBase cpResponse = null;
                        Exception exception = null;
                        try
                        {

                            byte[] receiveBuffer = new byte[8196];
                            bool bufferProcessed = false;
                            bool carriageReturn = false;
                            do
                            {
                                int bytesRec = socket.Receive(receiveBuffer);

                                //logger.Trace("==================  socket.Receive ====================== " + bytesRec);

                                if (bytesRec > 0)
                                {
                                    int dataRemaining = bytesRec;

                                    int startIndex = 0;

                                    for (int i = 0; i < bytesRec; i++)
                                    {
                                        var ch = (char)receiveBuffer[i];
                                        if (ch == '\r')
                                        {
                                            //logger.Trace("------------ CARRIAGE RETURN " + i);
                                            carriageReturn = true;
                                            continue;
                                        }
                                        else if (ch == '\n' && carriageReturn)
                                        {
                                            //logger.Trace("------------ LINE FEED " + i);

                                            var endIndex = i;

                                            var dataSize = (endIndex - startIndex) + 1;

                                            int offset = 0;
                                            byte[] dataBuffer = null;

                                            if (tempBuffer == null)
                                            {
                                                dataBuffer = new byte[dataSize];
                                            }
                                            else
                                            {
                                                var totalSize = dataSize + tempBuffer.Length;
                                                dataBuffer = new byte[totalSize];

                                                Array.Copy(tempBuffer, 0, dataBuffer, offset, tempBuffer.Length);
                                                offset += tempBuffer.Length;

                                                tempBuffer = null;
                                            }

                                            Array.Copy(receiveBuffer, startIndex, dataBuffer, offset, dataSize);

                                            startIndex = endIndex + 1;// next char...

                                            //оставшиеся в буфере данные
                                            dataRemaining = (bytesRec - startIndex);

                                            responseBuffer.Add(dataBuffer);

                                            if (dataRemaining == 0)
                                            {// весь буфер прочитан

                                                bufferProcessed = true;

                                                break;
                                            }
                                            else
                                            {// остались данные нужно дочитать...

                                            }

                                            carriageReturn = false;

                                        }
                                        else
                                        {
                                            if (carriageReturn)
                                            {
                                                carriageReturn = false;
                                            }

                                            //if (ch == '=' || (ch == ':'))
                                            //{// ControlPoint response or notification

                                            //    //var log = (ch == '=') ? "CPRESPONSE" : "CPNOTIFY";
                                            //    //logger.Trace(log + " " + i);
                                            //}

                                        }

                                    }

                                    //logger.Trace("==========================================================");

                                    Debug.Assert(dataRemaining >= 0, "dataRemaining >= 0");

                                    if (dataRemaining > 0)
                                    {// остались не обработаные данные сохраняем их в буффер

                                        // logger.Trace("dataRemaining == " + dataRemaining);

                                        int offset = 0;
                                        if (tempBuffer != null)
                                        {
                                            var tempSize = tempBuffer.Length + dataRemaining;
                                            var newTempBuffer = new byte[tempSize];

                                            Array.Copy(tempBuffer, 0, newTempBuffer, offset, tempBuffer.Length);
                                            offset += tempBuffer.Length;

                                            tempBuffer = newTempBuffer;
                                        }
                                        else
                                        {
                                            tempBuffer = new byte[dataRemaining];
                                        }

                                        Array.Copy(receiveBuffer, startIndex, tempBuffer, offset, dataRemaining);
                                    }
                                }
                                else
                                {// socket closed...

                                    socketClosed = true;
                                    break;
                                }

                            }
                            while (!bufferProcessed);

                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            exception = ex;
                        }

                        foreach (var bytes in responseBuffer)
                        {
                            cpResponse = CPResponseBase.Create(bytes);
                            var responseType = cpResponse?.ResponseType ?? CPResponseType.Unknown;

                            if (responseType == CPResponseType.Response || cpResponse.ResponseType == CPResponseType.Galileo)
                            {
                                ProcessResponse(cpResponse, exception);
                            }
                            else if (cpResponse.ResponseType == CPResponseType.Notify)
                            {
                                ProcessNotification(cpResponse);
                            }
                            else
                            {
                                ProcessResponse(null, exception);
                            }
                        }

                        responseBuffer.Clear();

                    }

                    if (state != ClientState.Connected || socketClosed)
                    {
                        break;
                    }

                    if (sendCommand == null)
                    {
                        if (!socket.Poll(1, SelectMode.SelectRead))
                        {
                            if (TryGetCommand(out var command))
                            {
                                try
                                {
                                    var seq = command.seq;
                                    var request = (CPRequest)command.request;

                                    byte[] sendBuffer = request.GetBytes();
                                    int bytesSent = socket.Send(sendBuffer);

                                    sendCommand = command;

                                    logger.Debug(">> " + sendCommand.ToString() + "bytesSent: " + bytesSent);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex);

                                    ProcessResponse(null, ex);
                                }
                            }
                            //continue;
                        }
                    }

                    syncEvent.WaitOne(10);

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
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }

                    socket.Close();
                    socket = null;
                }

                DrainCommands();

                state = ClientState.Disconnected;
                StateChanged?.Invoke();
            }

            logger.Verb("CommunicationLoop END");
        }



        private void ProcessResponse(CPResponseBase cpResponse, Exception exception)
        {
            if (sendCommand != null)
            {
                sendCommand.SetResult(cpResponse, exception);

                sendCommand = null;
            }
            else
            {// что то пошло не так...
                logger.Warn("!!!!!!!!!!!!!! WARN: " + cpResponse.ToString());
            }
        }

        private void ProcessNotification(CPResponseBase cpResponse)
        {
            NotificationReceived?.Invoke((CPNotification)cpResponse);
        }


        public CPResponseBase Send(CPRequest request, int timeout = -1)
        {
            logger.Debug(request.ToString());

            if (state != ClientState.Connected)
            {
                throw new Exception("CPClient is not connected!");

                //logger.Warn("Invalid state: " + state);
                //return null;
            }

            CPResponseBase response = null;

            InvokeCommand command = null;
            try
            {
                command = new InvokeCommand(request, (cSeq++));

                StartCommand(command);

                Func<bool> cancelled = () => (state != ClientState.Connected);

                response = command.WaitResult(timeout, cancelled) as CPResponseBase;
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                    command = null;
                }

            }

            return response;
        }



        public Task<CPResponseBase> SendAsync(CPRequest request, int timeout = -1)
        {
            return Task.Run(() => Send(request, timeout));
        }


        public void Disconnect()
        {
            logger.Debug("Disconnect()");

            if (state != ClientState.Connected)
            {//...

            }

            state = ClientState.Disconnecting;
            StateChanged?.Invoke();

            syncEvent?.Set();
        }


        public void Close()
        {
            logger.Debug("Close()");

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            if (syncEvent != null)
            {
                syncEvent.Close();
                syncEvent = null;
            }

            state = ClientState.Closed;

            StateChanged?.Invoke();
        }

        class InvokeCommand
        {
            public InvokeCommand(object _request, ushort _seq)
            {
                this.request = _request;
                this.seq = _seq;
            }

            public readonly ushort seq = 0;
            public readonly object request = null;

            private object response = null;
            private Exception exception = null;

            private AutoResetEvent waitEvent = new AutoResetEvent(false);

            public object WaitResult(int timeout = -1, Func<bool> canceled = null)
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("InvokeCommand");
                }

                Exception ex = null;

                long msec = 0;
                Stopwatch sw = Stopwatch.StartNew();

                bool result = false;
                do
                {
                    result = waitEvent.WaitOne(300);
                    if (result)
                    {
                        break;
                    }

                    if (canceled?.Invoke() ?? false)
                    {
                        ex = new OperationCanceledException();

                        logger.Debug("canceled");
                        break;
                    }

                    logger.Debug("!waitevent.WaitOne(300) && running");

                    if (timeout > 0)
                    {
                        msec += sw.ElapsedMilliseconds;
                        if (msec > timeout)
                        {
                            ex = new TimeoutException();

                            logger.Debug("msec > timeout");
                            break;
                        }
                    }

                } while (!result);

                if (exception == null)
                {
                    exception = ex;
                }

                if (exception != null)
                {
                    throw exception;
                }

                if (response == null)
                {
                    throw new InvalidOperationException();
                }

                return response;

            }

            public void SetResult(object resp, Exception ex)
            {
                if (disposed)
                {
                    return;
                }

                this.response = resp;
                this.exception = ex;


                logger.Debug($"<< #{seq} {resp}");

                waitEvent?.Set();
            }

            public override string ToString()
            {
                return $"#{seq} {request}";
            }

            private bool disposed = false;
            public void Dispose()
            {
                if (waitEvent != null)
                {
                    waitEvent.Dispose();
                    waitEvent = null;
                }

                disposed = true;

            }
        }

    }



    public class CPRequest
    {
        public CPRequest(string req)
        {
            this.RequestString = req;
        }

        public CPRequest(string objName, string method, params object[] values)
        {
            this.ObjectName = objName;
            this.Method = method;
            this.ValueList = string.Join(" ", values); ;// "{ " + string.Join(" ", values) + " }";

            this.RequestString = $"+{ObjectName} {Method} {ValueList}\r\n";
        }


        public CPRequest(string objName, string method, string valueList = "")
        {
            this.ObjectName = objName;
            this.Method = method;
            this.ValueList = valueList;

            this.RequestString = $"+{ObjectName} {Method} {ValueList}\r\n";
        }

        public readonly string ObjectName = "";
        public readonly string Method = "";
        public readonly string ValueList = "";

        public string RequestString { get; private set; }

        public byte[] GetBytes()
        {

            if (string.IsNullOrEmpty(RequestString))
            {
                throw new InvalidOperationException("Invalid request: " + RequestString);
            }

            return Encoding.UTF8.GetBytes(RequestString);
        }
        public override string ToString()
        {
            return RequestString;
        }
    }

    public enum CPResponseType
    {
        Unknown,
        Response,
        Notify,
        Galileo,
    }

    public abstract class CPResponseBase
    {
        protected CPResponseBase(string resp)
        {
            this.ResponseString = resp;
        }

        public readonly string ResponseString = "";
        public virtual bool Success { get; } = false;

        public abstract CPResponseType ResponseType { get; }


        public static CPResponseBase Create(byte[] bytes)
        {
            return Parse(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
        }


        public static bool TryParse(string _resp, out CPResponseBase response)
        {
            response = null;
            bool result = false;
            try
            {
                response = Parse(_resp);
                result = true;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                Debug.WriteLine(ex);
            }

            return result;
        }



        public static CPResponseBase Parse(string _resp)
        {

            if (string.IsNullOrEmpty(_resp))
            {
                throw new ArgumentException("_resp == null");
            }

            if (!_resp.EndsWith("\r\n"))
            {
                throw new FormatException("Invalid response format: " + _resp);
            }


            CPResponseBase response = null;

            if (_resp.StartsWith("="))
            {// ответ на запрос от сервера
                response = new CPResponse(_resp);
            }
            else if (_resp.StartsWith(":"))
            {// события сервера
                response = new CPNotification(_resp);
            }
            else
            {
                if (_resp.StartsWith("OK") || _resp.StartsWith("ER"))
                {// ответ от Galileo Connect
                    response = new GalileoResponse(_resp);

                }
                else
                {
                    throw new FormatException("Invalid response format: " + _resp);
                }
            }


            return response;

        }


        public override string ToString()
        {
            return ResponseString;
        }
    }


    public class CPResponse : CPResponseBase
    {
        private const int ResultCodeLength = 8;
        private const int MinResponseLength = ResultCodeLength + 3; //"=00000000\r\n";


        public CPResponse(string resp) : base(resp)
        {
            if (!resp.StartsWith("=") || !resp.EndsWith("\r\n") || resp.Length < MinResponseLength)
            {
                throw new FormatException("Invalid response format: " + resp);
            }


            //"="
            int offset = 1;

            var codeStr = resp.Substring(offset, ResultCodeLength);
            offset += ResultCodeLength;

            ResultCode = int.Parse(codeStr, System.Globalization.NumberStyles.HexNumber);

            var valueList = "";
            if (resp.Length > offset)
            {
                valueList = resp.Substring(offset);

                valueList = valueList.TrimEnd('\r', '\n');
                valueList = valueList.TrimStart(' ');
                valueList = valueList.TrimEnd(' ');
            }

            ValueList = valueList;
        }

        public override CPResponseType ResponseType => CPResponseType.Response;

        public int ResultCode { get; private set; } = -1;

        public string ValueList { get; private set; } = "";

        public override bool Success => (ResultCode == 0);
    }


    public class CPNotification : CPResponseBase
    {
        //":Notify WindowsState { 1 { { 10002 } 2 1 5120 254 244 684 507 { -1 } } }\r\n"
        public CPNotification(string resp) : base(resp)
        {
            if (!(resp.StartsWith(":") && resp.EndsWith("\r\n")))
            {
                throw new FormatException("Invalid notification format: " + resp);
            }

            resp = resp.TrimStart(':', ' ');
            resp = resp.TrimEnd('\r', '\n');

            // Parse object, method...
            int offset = 0;
            var index = resp.IndexOf(" ", offset);
            if (index > offset)
            {
                ObjectName = resp.Substring(offset, index);
                offset = index + 1;

                index = resp.IndexOf(" ", offset);
                if (index > offset)
                {
                    var len = (index - offset);
                    Method = resp.Substring(offset, len);
                    offset = index + 1;

                    ValueList = resp.Substring(offset);
                }
            }
        }

        public readonly string ObjectName = "";
        public readonly string Method = "";
        public readonly string ValueList = "";

        public override CPResponseType ResponseType => CPResponseType.Notify;

        public override string ToString()
        {
            return ":" + ObjectName + " " + Method + " " + string.Join(" ", ValueList) + "\r\n";
        }
    }

    public class GalileoResponse : CPResponseBase
    {
        public GalileoResponse(string resp) : base(resp)
        {
            if (resp.StartsWith("OK"))
            {
                success = true;
            }
            else if (resp.StartsWith("ER"))
            {// error...

            }
            else
            {
                //invalid...
            }
        }

        private bool success = false;

        public override bool Success => success;
        public override CPResponseType ResponseType => CPResponseType.Galileo;
    }
}
