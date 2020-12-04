using MediaToolkit.NativeAPIs;
using MediaToolkit.NativeAPIs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{
	public class ShortcutUtil
	{

		public static void CreateShortcut(string shortcutPath, string targetPath,
			string arguments = null, string workingDirectory = null, string description = null)
		{
			CShellLink cShellLink = new CShellLink();
			try
			{
				IShellLink iShellLink = (IShellLink)cShellLink;

				iShellLink.SetPath(targetPath);
				if (!string.IsNullOrEmpty(arguments))
				{
					iShellLink.SetArguments(arguments);
				}

				if (!string.IsNullOrEmpty(description))
				{
					iShellLink.SetDescription(description);
				}

				if (!string.IsNullOrEmpty(workingDirectory))
				{
					iShellLink.SetWorkingDirectory(workingDirectory);
				}

				iShellLink.SetShowCmd(SW.SHOWNORMAL);

				IPersistFile iPersistFile = (IPersistFile)iShellLink;
				iPersistFile.Save(shortcutPath, false);

			}
			finally
			{
				ComBase.SafeRelease(cShellLink);
			}
		}


		public static void DeleteShortcut(string shortcutFile, string targetFile = null)
		{
			if (!string.IsNullOrEmpty(targetFile))
			{
				CShellLink cShellLink = new CShellLink();
				try
				{
					IShellLink iShellLink = (IShellLink)cShellLink;
					IPersistFile iPersistFile = (IPersistFile)iShellLink;
					iPersistFile.Load(shortcutFile, 0);

					StringBuilder sb = new StringBuilder(260);
					WIN32_FIND_DATA data = new WIN32_FIND_DATA();
					iShellLink.GetPath(sb, 260, ref data, 0);

					var shortcutFileName = System.IO.Path.GetFullPath(sb.ToString());
					if (!string.Equals(shortcutFileName, targetFile, StringComparison.OrdinalIgnoreCase))
					{
						return;
					}
				}
				finally
				{
					ComBase.SafeRelease(cShellLink);
				}
			}

			if (System.IO.File.Exists(shortcutFile))
			{
				System.IO.File.Delete(shortcutFile);
			}
		}

	}

}
