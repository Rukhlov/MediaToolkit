using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MediaToolkit.Logging
{

    public class TraceManager
    {
        private static Dictionary<string, TraceSource> traceSources = new Dictionary<string, TraceSource>();

        private static object syncRoot = new object();

        public static void Init() { }

        public static TraceSource GetTrace(string name)
        {
            TraceSource ts = null;

            lock (syncRoot)
            {
                if (traceSources.ContainsKey(name))
                {
                    ts = traceSources[name];
                }
                else
                {
                    ts = new TraceSource(name);
                    traceSources.Add(name, ts);
                }
            }

            return ts;
        }


        public static void Shutdown()
        {
            lock (syncRoot)
            {
                foreach (var k in traceSources.Keys)
                {
                    var ts = traceSources[k];
                    ts.Close();
                    ts = null;
                }

                traceSources.Clear();
            }

        }
    }

    internal static class TraceSourceExtension
    {
        public static void Fatal<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Critical, 0, t);
        }

        public static void Fatal(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Critical, 0, message);
        }

        public static void Fatal(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Critical, 0, format, args);
        }

        public static void Error<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Error, 0, t);
        }

        public static void Error(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Error, 0, message);
        }


        public static void Error(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Error, 0, format, args);
        }

        public static void Warn<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Warning, 0, t);
        }

        public static void Warn(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Warning, 0, message);
        }


        public static void Warn(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        public static void Info<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Information, 0, t);
        }

        public static void Info(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Information, 0, format, args);

        }

        public static void Info(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Information, 0, message);
        }

        public static void Verb<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Verbose, 0, t);
        }


        public static void Verb(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, message);
        }


        public static void Verb(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }



        public static void Debug<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Verbose, 0, t);
        }


        public static void Debug(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, message);
        }


        public static void Debug(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }

    }
}
