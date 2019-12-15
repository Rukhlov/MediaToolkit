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
                InstanceFactory.RegisterType<IScreenCasterControl>("MediaToolkit.UI.dll");
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

    }

    public class InstanceFactory
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        static InstanceFactory()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public static Version Version { get; private set; }

        public static string AssemblyPath { get; set; } = @".\Plugins";

        public static bool EnableLog { get; set; } = false;
        public static event Action<string> Log;
        private static void OnLog(string log)
        {
            if (EnableLog)
            {
                Log?.Invoke(log);
            }
        }

        private static Dictionary<Type, Type> Dict = new Dictionary<Type, Type>();

        public static bool RegisterType<T>(string assemblyFileName, bool throwExceptions = false) where T : class
        {
            OnLog("RegisterType: " + typeof(T).ToString() + " AssemblyFileName " + assemblyFileName);

            Type TargetType = typeof(T);

            string assemblyFileFullName = Path.Combine(AssemblyPath, assemblyFileName);

            bool Result = false;
            try
            {
                if (Dict.ContainsKey(TargetType) == false)
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
                        foreach (Type AssemblyType in assembly.GetTypes())
                        {
                            var AssemblyInterfaces = AssemblyType.GetInterfaces();
                            foreach (var _interface in AssemblyInterfaces)
                            {
                                //logger.Trace(_interface.ToString());

                            }

                            if (AssemblyInterfaces.Contains(TargetType))
                            {
                                Dict.Add(TargetType, AssemblyType);
                                Result = true;
                                break;
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
                OnLog(ex.ToString());

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
                    OnLog(ex.ToString());
                    return null;
                }
            }
            else
            {
                OnLog("Assembly not found: " + asmFileFullName);
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
                    OnLog("TargetType not registered: " + targetType.ToString());
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.ToString());
                if (throwExceptions)
                {
                    throw;
                }
            }

            return instance;
        }

    }
}
