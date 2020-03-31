using System;
using System.IO;
using Newtonsoft.Json;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using Unity;

namespace ScreenStreamer.Wpf.Common.Managers
{
    public class ConfigurationManager
    {
        private static string _configPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Configuration.json");
        private static JsonSerializerSettings _deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static StreamMainModel LoadConfigurations()
        {
            StreamMainModel item = null;
            try
            {
                if (!File.Exists(_configPath))
                {
                    item = StreamMainModel.Default;
                }
                else
                {
                    item = JsonConvert.DeserializeObject<StreamMainModel>(File.ReadAllText(_configPath), _deserializeSettings);
                }
            }
            catch (Exception ex)
            {
                logger.FatalException("Не удалось загрузить конфигурацию.", ex);
            }
            return item ?? StreamMainModel.Default;
        }

        public static void Save()
        {
            var model = DependencyInjectionHelper.Container.Resolve<StreamMainModel>();
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(model, Formatting.Indented));
        }
    }
}