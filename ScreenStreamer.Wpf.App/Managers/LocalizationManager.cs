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

		private readonly static string localizationsPath = @"pack://application:,,,/ScreenStreamer.Wpf.App;component/Localizations";

		private static ResourceDictionary localizationsDict = new ResourceDictionary();

		public static CultureInfo Culture { get; private set; } = new System.Globalization.CultureInfo("en");

		public static void Init(string cultureName)
		{
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
					Culture = new CultureInfo(cultureName);

					var lang = Culture.Name;
					var langFile = "AppStrings." + lang + ".xaml";
					var customLang = new ResourceDictionary
					{
						Source = new Uri(localizationsPath + "/" + langFile),
					};

					localizationsDict.MergedDictionaries.Add(customLang);
					System.Threading.Thread.CurrentThread.CurrentUICulture = Culture;
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
