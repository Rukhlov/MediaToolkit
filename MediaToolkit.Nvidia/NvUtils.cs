using System;
using System.Collections.Generic;
using System.Text;

namespace MediaToolkit.Nvidia
{
public class LibNvEncException : Exception
    {
        public LibNvEncException(string callerName, string description, NvEncStatus status)
            : base($"{callerName} returned invalid status: {status}, {description}") { }

        public LibNvEncException(string callerName, CuResult result, string errorName, string errorString)
            : base($"{callerName} returned invalid result: {result}. {errorName}: {errorString}") { }
    }

    public static class NvEncRegisterResourceEx
    {
        public static NvEncInputPtr AsInputPointer(this NvEncRegisterResource resource)
        {
            return new NvEncInputPtr
            {
                Handle = resource.RegisteredResource.Handle
            };
        }

        public static NvEncOutputPtr AsOutputPointer(this NvEncRegisterResource resource)
        {
            return new NvEncOutputPtr
            {
                Handle = resource.RegisteredResource.Handle
            };
        }
    }
}
