using ICities;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public class TransferManagerMain : IUserMod
	{
		private static string Version = "v1.9.28";
#if TEST_RELEASE || TEST_DEBUG
		public static string ModName => "TransferManager CE " + Version + " TEST";
		public static string Title => ModName;
#else
		public static string ModName => "TransferManager CE " + Version; 
		public static string Title => "Transfer Manager CE " + " " + Version;
#endif
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

			Localization.LoadAllLanguageFiles();
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