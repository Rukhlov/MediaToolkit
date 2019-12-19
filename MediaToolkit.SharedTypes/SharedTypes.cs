using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.SharedTypes
{
    public interface IMediaToolkitBootstrapper
    {
        void Startup();
        void Shutdown();
       
    }

    public interface IHttpScreenStreamer
    {
        bool Setup(string addr, int port, Rectangle srcRect, Size destSize) ;
        void Start();
        void Stop();
        void Close();
    }


    public interface IScreenCasterControl
    {
        void Connect(string addr, int port);

        void Disconnect();
        //...
    }
}
