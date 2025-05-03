using UnityEngine;

namespace TransferManagerCE.Settings
{
    public class XmlInputKey
    {
        public string Name { get; set; }
        public KeyCode Key { get; set; }
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }

        public XmlInputKey()
        {
            Name = "Unknown";
            Key = KeyCode.A;
            Control = false;
            Shift = false;
            Alt = false;
        }

        public XmlInputKey(string name, KeyCode key, bool bControl, bool bShift, bool bAlt)
        {
            Name = name;
            Key = key;
            Control = bControl;
            Shift = bShift;
            Alt = bAlt;
        }
    }
}
