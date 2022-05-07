using ICities;
using TransferManagerCE.CustomManager;

namespace TransferManagerCE
{
    public class TransferManagerLoader : LoadingExtensionBase
    {
        private static bool s_loaded = false;

        public override void OnLevelLoaded(LoadMode mode)
        {
            Debug.Log("OnLevelLoaded");
            if (TransferToolMain.IsEnabled && (mode == LoadMode.LoadGame || mode == LoadMode.NewGame))
            {
                s_loaded = true;

                // Create TransferJobPool and initialize
                TransferJobPool.Instance.Initialize();

                // Create TransferDispatcher and initialize
                CustomTransferDispatcher.Instance.Initialize();

                // Create TransferManager background thread and start
                CustomTransferDispatcher._transferThread = new System.Threading.Thread(CustomTransferManager.MatchOffersThread);
                CustomTransferDispatcher._transferThread.IsBackground = true;
                CustomTransferDispatcher._transferThread.Start();
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (s_loaded)
            {
                // Unload
            }
        }
    }
}
