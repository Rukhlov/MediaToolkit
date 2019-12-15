using System;
using System.Collections.Generic;
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

    public interface IScreenCasterControl
    {
        void Connect(string addr);
        void Disconnect();
        //...
    }
}
