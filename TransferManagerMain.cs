using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;

namespace TransferManagerCE
{
    public class TransferToolMain : IUserMod
	{
		public static string ModName => "TransferManager CE " + Version;

		private static string Version = "v1.0.4";
		public static string Title => "TransferManager CE" + " " + Version;

		public static bool IsDebug = false;

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
			Debug.Log(Title);
			HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
		}

		public void OnDisabled()
		{
			IsEnabled = false;

			if (HarmonyHelper.IsHarmonyInstalled)
			{
				Patcher.UnpatchAll();
			}
		}

		// Sets up a settings user interface
		public void OnSettingsUI(UIHelper helper)
		{
			SettingsUI settingsUI = new SettingsUI();
			settingsUI.OnSettingsUI(helper);
		}
    }
}