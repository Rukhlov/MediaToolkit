using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.SharedTypes
{
    public class MediaToolkitFactory
    {
        private static IMediaToolkitBootstrapper mediaToolkit = null;
        public static bool IsStarted { get; private set; }

        public static bool Startup(string assemblyPath = "", bool throwExceptions = false)
        {
            if (IsStarted)
            {
                if (!throwExceptions)
                {
                    return false;
                }

                throw new InvalidOperationException("IsStarted  " + IsStarted);
            }

            try
            {
                InstanceFactory.AssemblyPath = assemblyPath;

                InstanceFactory.RegisterType<IMediaToolkitBootstrapper>("MediaToolkit.dll");

                InstanceFactory.RegisterType<IVideoRenderer>("MediaToolkit.dll");
                InstanceFactory.RegisterType<IAudioRenderer>("MediaToolkit.dll");
                InstanceFactory.RegisterType<IMediaRenderSession>("MediaToolkit.dll");

                InstanceFactory.RegisterType<IHttpScreenStreamer>("MediaToolkit.dll");
                InstanceFactory.RegisterType<IScreenCasterControl>("MediaToolkit.UI.dll", "ScreenCastControl");

                //InstanceFactory.RegisterType<IScreenCasterControl>("MediaToolkit.UI.dll", "ScreenCastControlEx");

                InstanceFactory.RegisterType<IDeckLinkInputControl>("MediaToolkit.UI.dll");
                //...

                mediaToolkit = InstanceFactory.CreateInstance<IMediaToolkitBootstrapper>();

                mediaToolkit.Startup();

                IsStarted = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //Debug.Fail(ex.Message);
                if (throwExceptions)
                {
                    throw;
                }
            }

            return IsStarted;
        }

        public static T CreateInstance<T>(object[] args = null, bool throwExceptions = false) where T : class
        {
            if (!IsStarted)
            {
                if (!throwExceptions)
                {
                    return null;
                }

                throw new InvalidOperationException("IsStarted  " + IsStarted);
            }

            return InstanceFactory.CreateInstance<T>(args, throwExceptions);
        }

        public static void Shutdown()
        {
            if (mediaToolkit != null)
            {
                mediaToolkit.Shutdown();
            }
        }


        public const string MediaToolkitPathKey = "MediaToolkitPath";
        public const string SoftwareVisiologyPolywallPathKey = "Software\\Visiology\\Polywall\\Path";

        public static string GetMediaToolkitInstalledPath(string value)
        {
            string path = string.Empty;
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(SoftwareVisiologyPolywallPathKey);
            if (rk != null)
            {
                var names = rk.GetValueNames();
                if (names != null)
                {
                    if (names.Contains(value))
                    {
                        path = rk.GetValue(value)?.ToString();
                    }
                }
            }
            return path;
        }

        public static string GetMediaToolkitDirectory()
        {
            return GetMediaToolkitInstalledPath(MediaToolkitPathKey);
        }

    }

    public class InstanceFactory
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        static InstanceFactory()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //Assembly currentAssem = Assembly.GetExecutingAssembly();
            //Version = currentAssem.GetName().Version;
        }

        public static Version Version { get; private set; }

        public static string AssemblyPath { get; set; } = @".\MediaToolkit";

        public static bool EnableLog { get; set; } = false;
        public static event Action<string, LogLevel> Log;
        private static void OnLog(string log, LogLevel level = LogLevel.Debug)
        {
            if (EnableLog)
            {
                Log?.Invoke(log, level);
            }
        }

        public enum LogLevel
        {
            Debug,
            Error,
        }

        private static Dictionary<Type, Type> Dict = new Dictionary<Type, Type>();

        public static bool RegisterType<T>(string assemblyFileName, string className = "", bool throwExceptions = false) where T : class
        {
            OnLog("RegisterType: " + typeof(T).ToString() + " AssemblyFileName " + assemblyFileName);

            Type targetType = typeof(T);

            string assemblyFileFullName = Path.Combine(AssemblyPath, assemblyFileName);

            bool Result = false;
            try
            {
                if (Dict.ContainsKey(targetType) == false)
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFileFullName);

                    //var externalVersion = assemblyName.Version;
                    //var factoryVersion = InstanceFactory.Version;

                    //if (factoryVersion.Major != externalVersion.Major ||
                    //    factoryVersion.Minor != externalVersion.Minor || 
                    //    factoryVersion.Build != externalVersion.Build)
                    //{
                    //    logger.Error("Different assembly versions " + factoryVersion.ToString() + " " + assemblyName.Version.ToString());
                    //}


                    OnLog("Try to find: " + assemblyName.ToString());

                    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName()))
                        .FirstOrDefault();

                    if (assembly == null)
                    {
                        OnLog("Try to load assembly: " + assemblyName.FullName);

                        assembly = AppDomain.CurrentDomain.Load(assemblyName.FullName);

                    }
                    if (assembly != null)
                    {
                        foreach (Type assemblyType in assembly.GetTypes())
                        {
                            var assemblyInterfaces = assemblyType.GetInterfaces();
                            foreach (var _interface in assemblyInterfaces)
                            {
                                //logger.Trace(_interface.ToString());

                            }

                            if (assemblyInterfaces.Contains(targetType))
                            {

                                if (!string.IsNullOrEmpty(className))
                                {
                                    if(assemblyType.Name == className)
                                    {
                                        if (!Dict.ContainsKey(targetType))
                                        {
                                            Dict.Add(targetType, assemblyType);
                                            Result = true;
                                            break;
                                        }

                                    }
                                }
                                else
                                {
                                    if (!Dict.ContainsKey(targetType))
                                    {
                                        Dict.Add(targetType, assemblyType);
                                        Result = true;
                                        break;
                                    }
                                }

       
                            }
                        }
                    }
                }
                else
                {
                    Result = true;
                }
            }
            catch (Exception ex)
            {
                Result = false;
                // Debug.Fail(ex.Message);
                OnLog(ex.ToString(), LogLevel.Error);

                if (throwExceptions)
                {
                    throw;
                }
            };
            return Result;
        }



        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            OnLog("CurrentDomain_AssemblyResolve(...) " + args.Name + " " + args.RequestingAssembly?.ToString() ?? "");
            var asmName = args.Name;

            if (asmName.Contains(".resources"))
            {
                return null;
            }

            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == asmName);
            if (assembly != null)
            {
                return assembly;
            }

            string filename = asmName.Split(',')[0];
            string asmFileFullName = Path.Combine(AssemblyPath, (filename + ".dll"));
            if (!File.Exists(asmFileFullName))
            {
                asmFileFullName = Path.Combine(AssemblyPath, (filename + ".exe"));
            }

            if (File.Exists(asmFileFullName))
            {
                try
                {
                    return Assembly.LoadFrom(asmFileFullName);
                }
                catch (Exception ex)
                {
                    OnLog(ex.ToString(), LogLevel.Error);
                    return null;
                }
            }
            else
            {
                OnLog("Assembly not found: " + asmFileFullName, LogLevel.Error);
                return null;
            }

        }

        public static T CreateInstance<T>(object[] args = null, bool throwExceptions = false) where T : class
        {
            OnLog("CreateInstance(...) " + typeof(T).ToString());

            Type targetType = typeof(T);
            T instance = null;

            try
            {
                if (Dict.ContainsKey(targetType))
                {
                    Type type = Dict[targetType];
                    instance = (T)Activator.CreateInstance(type, args);
                }
                else
                {
                    throw new Exception("TargetType not registered: " + targetType.ToString());
                   // OnLog("TargetType not registered: " + targetType.ToString());
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.ToString(), LogLevel.Error);
                if (throwExceptions)
                {
                    throw;
                }
            }

            return instance;
        }

    }



}
