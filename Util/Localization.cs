using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ColossalFramework.Globalization;
using ColossalFramework.Plugins;

namespace TransferManagerCE.Util
{
    public class Localization
    {
        private static readonly Dictionary<string, Locale> localeStore = new Dictionary<string, Locale>();

        private static Locale LocaleFromFile(string file)
        {
            var locale = new Locale();
            using (var reader = new StreamReader(File.OpenRead(file)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var rows = line.Split('\t');
                    if (rows.Length < 2)
                    {
                        //Debug.LogErrorFormat("Not enough tabs in locale string from {0}:\n'{1}'", file, line);
                        continue;
                    }
                    locale.AddLocalizedString(new Locale.Key { m_Identifier = rows[0] }, rows[1]);
                }
            }
            return locale;
        }

        private static string LocalePath(string lang)
        {
            var modPath = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly()).modPath;
            return Path.Combine(modPath, $"Locales/{lang}.txt");
        }

        public static string Get(string id)
        {
            var lang = LocaleManager.instance.language ?? "en";
            if (!localeStore.ContainsKey(lang))
            {
                localeStore.Add(lang, LocaleFromFile(LocalePath(File.Exists(LocalePath(lang)) ? lang : "en")));
            }
            return localeStore[lang].Get(new Locale.Key { m_Identifier = id });
        }
    }
}