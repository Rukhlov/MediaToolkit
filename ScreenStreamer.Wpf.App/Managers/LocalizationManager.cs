using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenStreamer.Wpf
{
	class LocalizationManager
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly static string localizationsPath = @"pack://application:,,,/Localizations";

		private static ResourceDictionary localizationsDict = new ResourceDictionary();

		public static CultureInfo CultureInfo { get; private set; } = new System.Globalization.CultureInfo("en");

		public static void Init(string cultureName)
		{
            logger.Debug("LocalizationManager::Init(...) " + cultureName);

            if (string.IsNullOrEmpty(cultureName))
            {// 
                var currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
                cultureName = currentCulture.TwoLetterISOLanguageName;

				logger.Debug("Culture not set, apply default UI culture: " + cultureName);
			}

            if (cultureName.Length > 2)
            {
                if(cultureName.Equals("english", StringComparison.OrdinalIgnoreCase))
                {
                    cultureName = "en";
                }
                else if (cultureName.Equals("russian", StringComparison.OrdinalIgnoreCase))
                {
                    cultureName = "ru";
                }
            }

            var defaultLang = new ResourceDictionary
			{
				Source = new Uri(localizationsPath + "/" + "AppStrings.xaml"),
			};
			localizationsDict.MergedDictionaries.Add(defaultLang);

			Application.Current.Resources.MergedDictionaries.Add(localizationsDict);
			try
			{
				if (!string.IsNullOrEmpty(cultureName) && cultureName != "en")
				{
					CultureInfo = new CultureInfo(cultureName);

					var lang = CultureInfo.Name;
					var langFile = "AppStrings." + lang + ".xaml";
					var customLang = new ResourceDictionary
					{
						Source = new Uri(localizationsPath + "/" + langFile),
					};

					localizationsDict.MergedDictionaries.Add(customLang);
					System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message);
			}


		}

		public static string GetString(string key)
		{
			string result = localizationsDict[key] as string;
			
			if(result == null)
			{
				result = "";
			}

			return result;
		}
	}
}
