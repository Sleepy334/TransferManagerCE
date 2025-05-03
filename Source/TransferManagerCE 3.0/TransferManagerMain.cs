using ICities;
using System;
using System.Diagnostics;
using System.Reflection;

namespace TransferManagerCE
{
    public class TransferManagerMain : IUserMod
	{
		public static string Version
		{
			get
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"v{version.Major}.{version.Minor}.{version.Build}";
            }
		}
		 
#if TEST_RELEASE || TEST_DEBUG
        private static string Edition => " TEST";
#else
		private static string Edition => "";
#endif

#if DEBUG
        private static string Config => $" [DEBUG]";
#else
		private static string Config => "";
#endif
        public static string ModName => $"TransferManager CE {Version}{Edition}{Config}"; 
		public static string Title => $"Transfer Manager CE {Version}{Edition}{Config}";

		public static bool IsEnabled = false;

		public string Name
		{
			get { return ModName; }
		}

		public string Description
		{
			get { return "More realistic response to service requests."; }
		}

		public void OnEnabled()
		{
			IsEnabled = true;
		}

		public void OnDisabled()
		{
			IsEnabled = false;
		}

		// Sets up a settings user interface
		public void OnSettingsUI(UIHelper helper)
		{
			SettingsUI settingsUI = new SettingsUI();
			settingsUI.OnSettingsUI(helper);
		}
    }
}