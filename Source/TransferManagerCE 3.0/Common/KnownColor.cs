using UnityEngine;

namespace TransferManagerCE
{
    public class KnownColor
    {
        // ========================================================================================
        public static KnownColor darkGrey => new KnownColor("Dark Grey", new Color(0.25f, 0.25f, 0.25f, 1f));
        public static KnownColor grey => new KnownColor("Grey", Color.grey);
        public static KnownColor lightGrey => new KnownColor("Light Grey", new Color(0.75f, 0.75f, 0.75f, 1f));
        public static KnownColor red => new KnownColor("Red", Color.red);
        public static KnownColor magenta => new KnownColor("Magenta", Color.magenta);
        public static KnownColor blue => new KnownColor("Blue", Color.blue);
        public static KnownColor navy => new KnownColor("Navy", new Color(0f, 0.5f, 0f, 1f));
        public static KnownColor lightBlue => new KnownColor("Light Blue", new Color(0f, 0.8f, 1f, 1f));
        public static KnownColor skyBlue => new KnownColor("Sky Blue", new Color32(0, 172, 234, 255));
        public static KnownColor cyan => new KnownColor("Cyan", Color.cyan);
        public static KnownColor green => new KnownColor("Green", Color.green);
        public static KnownColor darkGreen => new KnownColor("Dark Green", new Color(0f, 0.5f, 0f, 1f));
        public static KnownColor brown => new KnownColor("Brown", new Color(0.72f, 0.5f, 0.34f, 1f));
        public static KnownColor yellow => new KnownColor("Yellow", Color.yellow);
        public static KnownColor white => new KnownColor("White", Color.white);
        public static KnownColor black => new KnownColor("Black", Color.black);
        public static KnownColor orange => new KnownColor("Orange", new Color(1f, 0.5f, 0.15f, 1f));
        public static KnownColor gold => new KnownColor("Gold", new Color(1f, 0.78f, 0.05f, 1f));        
        public static KnownColor purple => new KnownColor("Purple", new Color32(128, 0, 128, 255));
        public static KnownColor maroon => new KnownColor("Maroon", new Color32(128, 0, 0, 255));

        // ========================================================================================
        private Color m_color;
        private string m_name;

        public KnownColor(string sName, Color color)
        {
            m_name = sName;
            m_color = color;
        }

        public static implicit operator Color(KnownColor known)
        {
            return known.m_color;
        }

        public static implicit operator Color32(KnownColor known)
        {
            return known.m_color;

        }

        public string name
        {
            get { return m_name; }
        }

        public UnityEngine.Color color
        {
            get { return m_color; }
        }
    }
}