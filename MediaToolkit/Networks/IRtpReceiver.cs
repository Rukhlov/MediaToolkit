using MediaToolkit.Core;

using MediaToolkit.SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Networks
{
    public interface IRtpReceiver
    {
        void Open(NetworkSettings settings);

        //void Open(string address, int port, int ttl = 10);
        IPEndPoint RemoteEndpoint { get; }
        IPEndPoint LocalEndpoint { get; }

        ErrorCode ErrorCode { get; }
        ReceiverState State { get; }

        Task Start();
        void Stop();

        event Action<RtpPacket> RtpPacketReceived;
    }

    public enum ReceiverState
    {
        Initialized,
        Running,
        Closing,
        Closed,
    }
}
