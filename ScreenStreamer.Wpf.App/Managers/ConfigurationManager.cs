using System;
using System.IO;
using Newtonsoft.Json;
using NLog;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using Unity;

namespace ScreenStreamer.Wpf.Common.Managers
{
    public class ConfigurationManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static string configName = "Configuration.json";

        public static readonly string CommonAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        public static readonly string ConfigPath = Path.Combine(CommonAppDataPath, "Polywall\\ScreenStreamer.Wpf.App");

        
        // private static string configPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

        private static string configFileFullName = System.IO.Path.Combine(ConfigPath, configName);

  
        public static StreamMainModel LoadConfigurations()
        {
            logger.Debug("ConfigurationManager::LoadConfigurations()");

            StreamMainModel item = null;
            try
            {
                if (File.Exists(configFileFullName))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                    };

                    using (StreamReader streamReader = new StreamReader(configFileFullName))
                    {
                        using (JsonReader jsonReader = new JsonTextReader(streamReader))
                        {
                            item = serializer.Deserialize<StreamMainModel>(jsonReader);
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return item ?? StreamMainModel.Default;
        }

        public static void Save()
        {
            logger.Debug("ConfigurationManager::Save()");

            try
            {
                if (!Directory.Exists(ConfigPath))
                {
                    Directory.CreateDirectory(ConfigPath);
                }

                var model = DependencyInjectionHelper.Container.Resolve<StreamMainModel>();

                JsonSerializer serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented,
                };
                
                using (StreamWriter streamWriter = new StreamWriter(configFileFullName))
                {
                    using (JsonWriter jsonWriter = new JsonTextWriter( streamWriter))
                    {
                        serializer.Serialize(jsonWriter, model);
                    }
                       
                }
            
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}