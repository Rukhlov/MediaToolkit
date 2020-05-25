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
        private static string configPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

        private static string configFileFullName = System.IO.Path.Combine(configPath, configName);

  
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