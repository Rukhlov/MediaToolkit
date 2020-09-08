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
            Notify = new Notify(this);
            RGBSys = new RGBSys(this);
            ConfigSys = new ConfigSys(this);
        }

        public string Host { get; private set; } = "127.0.0.1";
        public int Port { get; private set; } = 25456;

        public NetworkCredential Credential { get; private set; } = new NetworkCredential("admin", "");

        public ConfigSys ConfigSys { get; } = null;
        public Window Window { get; } = null;
        public WinServer WinServer { get; } = null;
        public Notify Notify { get; } = null;
        public RGBSys RGBSys { get; } = null;

        public event Action<CPNotification> NotificationReceived;
        public event Action StateChanged;

        private volatile ClientState state = ClientState.Disconnected;
        public ClientState State => state;

        private Socket socket = null;

        public bool IsConnected => socket?.Connected ?? false;

        private volatile bool isAuthenticated = false;
        public bool IsAuthenticated => isAuthenticated;


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
                socket.ReceiveTimeout = 10000;
                socket.SendTimeout = 10000;

                logger.Verb($"socket.ConnectAsync(...) {Host} {Port}");

                var connectionTask = socket.ConnectAsync(Host, Port);

                Task.WaitAny(new[] { connectionTask }, 10000);

                if (!IsSocketConnected())
                {
                    throw new Exception("Connection error");
                }

                if (state == ClientState.Connecting)
                {
                    state = ClientState.Connected;
                }

                StateChanged?.Invoke();

                while (true)
                {
                    bool socketConnected = IsSocketConnected();
                    if (!(socketConnected && state == ClientState.Connected))
                    {
                        logger.Debug("Connection break " + socketConnected + " " + state);
                        break;
                    }

                    int available = socket.Available;
                    if (available > 0)
                    {
                        IEnumerable<byte[]> dataLines = null;

                        CPSocketHandler socketHanlder = new CPSocketHandler();

                        Exception exception = null;
                        try
                        {                           
                            bool bufferProcessed = false;
                            do
                            {
                                byte[] recBuffer = new byte[1024];

                                int bytesRec = socket.Receive(recBuffer);

                                if (bytesRec > 0)
                                { //The ControlPoint protocol is text-based. Each command line terminates
                                    //with CR/LF.Tokens are space separated. All identifiers are case-sensitive.

                                    dataLines = socketHanlder.ReadLines(recBuffer, bytesRec);

                                    bufferProcessed = (dataLines != null);
                                }
                                else
                                {// socket closed...

                                    socketConnected = false;
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

                        if (dataLines != null)
                        {
                            foreach (var bytes in dataLines)
                            {
                                var cpResponse = CPResponseBase.Create(bytes);
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

                            dataLines = null;
                        }

                    }

                    socketConnected = IsSocketConnected();
                    if (!(socketConnected && state == ClientState.Connected))
                    {
                        logger.Debug("Connection break " + socketConnected + " " + state);
                        break;
                    }

                    { // Response must be received before the next Request is sent!
                        if (sendCommand == null)// обнуляется при получении ответа
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

                isAuthenticated = false;
                state = ClientState.Disconnected;
                StateChanged?.Invoke();
            }

            logger.Verb("CommunicationLoop END");
        }

        private bool IsSocketConnected()
        {
            return !((socket.Poll(1, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
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

        public async Task<bool> Authenticate(string user, string password, int timeout = 5000)
        {
            if (isAuthenticated)
            {
                return false;
            }

            Credential = new NetworkCredential(user, password);

            var authRequest = new CPRequest($"{Credential.UserName}\r\n{Credential.Password}\r\n");

            var resp = await SendAsync(authRequest, timeout);
            resp.ThrowIfError();

            isAuthenticated = resp?.Success ?? false;
            return isAuthenticated;
        }

        public Task<CPResponseBase> SendAsync(CPRequest request, CancellationToken ct, int timeout = 10000)
        {
            return Task.Run(() => SendInternal(request, ct, timeout));
        }

        public Task<CPResponseBase> SendAsync(CPRequest request, int timeout = 10000)
        {
            return SendAsync(request, CancellationToken.None, timeout);
        }

        public CPResponseBase Send(CPRequest request, int timeout = -1)
        {
            return SendInternal(request, CancellationToken.None, timeout);
        }

        private CPResponseBase SendInternal(CPRequest request, CancellationToken ct, int timeout)
        {
            logger.Debug(request.ToString());

            if (!IsConnected)
            {
                throw new Exception("Not connected");
            }

            if (state != ClientState.Connected)
            {
                throw new Exception("Invalid state: " + state);
            }

            CPResponseBase response = null;

            InvokeCommand command = null;
            try
            {
                command = new InvokeCommand(request, (cSeq++));

                StartCommand(command);

                bool cancel = false;
                ct.Register(() =>
                {
                    cancel = true;
                });
                
                Func<bool> cancelled = () => (state != ClientState.Connected || cancel);

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

            isAuthenticated = false;

            StateChanged?.Invoke();
        }


        class CPSocketHandler
        {
            private byte[] tempBuffer = null;
            private bool carriageReturn = false;

            private List<byte[]> dataLines = new List<byte[]>();

            public IEnumerable<byte[]> ReadLines(byte[] buffer, int size)
            {
                List<byte[]> resultBuffer = null;  
                int dataRemaining = size;
                int startIndex = 0;
                for (int i = 0; i < size; i++)
                {
                    var ch = (char)buffer[i];
                    if (ch == '\r')
                    {
                        carriageReturn = true;
                        continue;
                    }
                    else if (ch == '\n' && carriageReturn)
                    {
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

                        Array.Copy(buffer, startIndex, dataBuffer, offset, dataSize);

                        startIndex = endIndex + 1;// next char...

                        //оставшиеся в буфере данные
                        dataRemaining = (size - startIndex);

                        dataLines.Add(dataBuffer);

                        if (dataRemaining == 0)
                        {// весь буфер прочитан

							resultBuffer = new List<byte[]>(dataLines);
							dataLines.Clear();

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
                    }
                }

                Debug.Assert(dataRemaining >= 0, "dataRemaining >= 0");

                if (dataRemaining > 0)
                {// остались не обработаные данные сохраняем их в буффер...

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

                    Array.Copy(buffer, startIndex, tempBuffer, offset, dataRemaining);
                }

                //if(dataLines.Count> 0)
                //{
                //    resultBuffer = new List<byte[]>(dataLines);
                //    dataLines.Clear();

                //}

                return resultBuffer;
            }

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
                logger.Verb($"Request #{seq} started");
                bool result = false;
                do
                {
                    result = waitEvent.WaitOne(300);
                    if (result)
                    {
                        logger.Verb($"Request #{seq} completed " + sw.ElapsedMilliseconds);
                        break;
                    }

                    if (canceled?.Invoke() ?? false)
                    {
                        ex = new OperationCanceledException();

                        logger.Verb($"Request #{seq} canceled");
                        break;
                    }

                    if (timeout > 0)
                    {
                        msec = sw.ElapsedMilliseconds;
                        if (msec > timeout)
                        {
                            ex = new TimeoutException();

                            logger.Verb($"Request #{seq} timed out: {msec} {timeout}");
                            break;
                        }
                    }

                    logger.Verb($"#{seq} waiting for result...");

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
                    var message = "Request dropped";
                    if (request != null)
                    {
                        message = "Request dropped: " + "\"" + request.ToString() + "\"";
                    }

                    throw new Exception(message);
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


                logger.Verb($"<< #{seq} {resp}");

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
            this.ValueList = string.Join(" ", values); // "{ " + string.Join(" ", values) + " }";

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
        public virtual bool Success { get; } = true;

        public abstract CPResponseType ResponseType { get; }

        public byte[] RawData { get; set; }


        public static CPResponseBase Create(byte[] bytes)
        {

            const byte DataLinkEscape = 0x10;
            const byte CarriageReturn = (byte)'\r';
            const byte LineFeed = (byte)'\n';

            var endIndex = bytes.Length - 2;
            if (bytes[endIndex] != CarriageReturn || bytes[endIndex + 1] != LineFeed)
            {
                //invalid format...
            }

            for (int i = 0; i< bytes.Length; i++)
            {
                var b = bytes[i];
                if(b == DataLinkEscape)
                {// после DLE начинаются bin данные
                 //=00000000 128 96DLE...rgb555...\r\n

                    int offset = 0;
                    var startIndex = i;
                    byte[] headerBytes = new byte[startIndex];
                    Array.Copy(bytes, offset, headerBytes, 0, headerBytes.Length);
                    offset += headerBytes.Length + 1;

                    var dataSize= endIndex - startIndex - 1;

                    var rawData = new byte[dataSize];

                    Array.Copy(bytes, offset, rawData, 0, rawData.Length);

                    var respStr = Encoding.UTF8.GetString(headerBytes, 0, headerBytes.Length) + "\r\n";

                    return Create(respStr , rawData);
                    
                }
            }

            return Create(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
        }

        public virtual void ThrowIfError()
        {
            if (!Success)
            {
                throw new Exception();
            }
        }

        public static CPResponseBase Create(string respStr, byte[] rawData = null)
        {

            if (string.IsNullOrEmpty(respStr))
            {
                throw new ArgumentException("_resp == null");
            }

            if (!respStr.EndsWith("\r\n"))
            {
                throw new FormatException("Invalid response format: " + respStr);
            }


            CPResponseBase response = null;

            if (respStr.StartsWith("="))
            {// ответ на запрос от сервера
                response = new CPResponse(respStr);
            }
            else if (respStr.StartsWith(":"))
            {// события сервера
                response = new CPNotification(respStr);
            }
            else
            {
                if (respStr.StartsWith("OK") || respStr.StartsWith("ER"))
                {// ответ от Galileo Connect
                    response = new GCResponse(respStr);

                }
                else
                {
                    throw new FormatException("Invalid response format: " + respStr);
                }
            }

            response.RawData = rawData;

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

        public int ResultCode { get; private set; } = 0;

        public string ValueList { get; private set; } = "";

        public override bool Success => (ResultCode == (uint)ResultCodes.S_OK); // || ResultCode == (uint)ResultCodes.S_FALSE);//(ResultCode == 0);

        public override void ThrowIfError()
        {
            if (ResultCode != (int)ResultCodes.S_OK && ResultCode != (int)ResultCodes.S_FALSE)
            {
                throw new CPException((ResultCodes)ResultCode);
            }
        }

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

    public class GCResponse : CPResponseBase
    {
        public GCResponse(string resp) : base(resp)
        {
            if (resp.StartsWith("OK"))
            {
                success = true;

				Data = resp.Substring(2);
				Data = Data.TrimStart(' ');
				Data = Data.TrimEnd('\r', '\n');
            }
            else if (resp.StartsWith("ER"))
            {// error...
                success = false;
				Data = resp.Substring(2);
				Data = Data.TrimStart(' ');
				Data = Data.TrimEnd('\r', '\n');
			}
            else
            {
				throw new ArgumentException(resp);
            }
        }

		public string Data { get; private set; } = "";
        private bool success = false;

        public override bool Success => success;
        public override CPResponseType ResponseType => CPResponseType.Galileo;
    }
}
