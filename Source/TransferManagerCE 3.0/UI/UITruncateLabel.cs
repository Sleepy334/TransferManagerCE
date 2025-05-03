using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE
{
    public class UITruncateLabel : UILabel
    {
        public override string text 
        {
            get
            {
                return base.text;
            }
            set 
            { 
                base.text = value;
                TruncateLabel();
            }
        }

        public void TruncateLabel()
        {
            if (text.Length >= 4)
            {
                float actualWidth = width;
                float actualHeight = height;
                bool bAutoSize = autoSize;

                // Make sure label is autosizeable and up-to-date.
                autoSize = true;
                PerformLayout();

                // Iterativly remove the last remaining letter through text scales until acceptible width is reached.
                if (width > actualWidth)
                {
                    base.text = base.text + "...";
                    PerformLayout();

                    while (width > actualWidth)
                    {
                        base.text = base.text.Substring(0, base.text.Length - 4) + "...";
                        PerformLayout();
                    }
                }
                
                // Now set settings back again
                if (!bAutoSize)
                {
                    autoSize = false;
                    width = actualWidth;
                    height = actualHeight;
                }
            }
        }
    }
}
