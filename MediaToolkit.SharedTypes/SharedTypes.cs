using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.SharedTypes
{
    public interface IMediaToolkit
    {
        void Startup();
        void Shutdown();
        T CreateInstance<T>(object[] args = null) where T : class;
    }

    public interface IScreenCasterControl
    {
        void Connect(string addr);
        void Disconnect();
        //...
    }
}
