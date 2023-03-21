using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ColossalFramework.Globalization;
using ColossalFramework.Plugins;
using TransferManagerCE.Settings;

namespace TransferManagerCE.Common
{
    public class Localization
    {
        const string SYSTEM_DEFAULT = "System Default";

        private static readonly Dictionary<string, Locale> localeStore = new Dictionary<string, Locale>();

        public static void LoadAllLanguageFiles()
        {
            string sPath = LocalePath();
            if (Directory.Exists(sPath))
            {
                // Load each file in directory and attempt to deserialise as a translation file.
                string[] localFiles = Directory.GetFiles(sPath);
                foreach (string file in localFiles)
                {
                    Debug.Log("Locales file: " + file);
                    if (file.EndsWith(".csv"))
                    {
                        Locale locale = LocaleFromFile(file);

                        string sLanguage = locale.Get(new Locale.Key { m_Identifier = "CODE" });
                        if (!string.IsNullOrEmpty(sLanguage) && !localeStore.ContainsKey(sLanguage))
                        {
                            localeStore[sLanguage] = locale;
                        }
                    }
                }
                Debug.Log("Locales loaded: " + localeStore.Count);
            }
            else
            {
                Debug.Log("Locales directory not found: " + LocalePath());
            }

            // Load en from resources so we have at least 1 language
            if (localeStore.Count == 0)
            {
                localeStore.Add("en", LocaleFromResource("TransferManagerCE.Locales.en.csv"));
            }
        }

        public static string[] GetLoadedLanguages()
        {
            List<string> languages = new List<string>();

            languages.Add(SYSTEM_DEFAULT);
            foreach (KeyValuePair<string, Locale> kvp in localeStore)
            {
                Locale locale = kvp.Value;
                string sCode = locale.Get(new Locale.Key { m_Identifier = "CODE" });
                string sName = locale.Get(new Locale.Key { m_Identifier = "NAME" });
                languages.Add(sName + " (" + sCode + ")");
            }

            return languages.ToArray();
        }

        public static List<string> GetLoadedCodes()
        {
            List<string> languages = new List<string>();

            languages.Add(SYSTEM_DEFAULT);
            foreach (KeyValuePair<string, Locale> kvp in localeStore)
            {
                Locale locale = kvp.Value;
                string sCode = locale.Get(new Locale.Key { m_Identifier = "CODE" });
                languages.Add(sCode);
            }

            return languages;
        }

        public static int GetLanguageIndexFromCode(string sCode)
        {
            List<string> languages = GetLoadedCodes();
            return Math.Max(0, languages.IndexOf(sCode));
        }

        private static Locale LocaleFromFile(string file)
        {
            var locale = new Locale();
            if (File.Exists(file))
            {
                Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                try
                {
                    using (var reader = new StreamReader(File.OpenRead(file)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) is not null)
                        {
                            string[] fields = CSVParser.Split(line);
                            if (fields.Length == 2)
                            {
                                locale.AddLocalizedString(new Locale.Key { m_Identifier = fields[0].Trim('"') }, fields[1].Trim('"'));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Unable to load localization file: " + file + " Error: " + e.ToString());
                }
            }

            return locale;
        }

        private static Locale LocaleFromResource(string resource)
        {
            var locale = new Locale();
            var assembly = Assembly.GetExecutingAssembly();
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) is not null)
                    {
                        string[] fields = CSVParser.Split(line);
                        if (fields.Length == 2)
                        {
                            locale.AddLocalizedString(new Locale.Key { m_Identifier = fields[0].Trim('"') }, fields[1].Trim('"'));
                        }
                    }
                }
            }

            return locale;
        }

        private static string LocalePath()
        {
            var modPath = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly()).modPath;
            return Path.Combine(modPath, "Locales\\");
        }

        private static string GetValue(string id)
        {
            string sLanguage = ModSettings.GetSettings().PreferredLanguage;
            if (string.IsNullOrEmpty(sLanguage) || sLanguage == SYSTEM_DEFAULT)
            {
                sLanguage = LocaleManager.instance.language ?? "en";
            }
            if (localeStore.ContainsKey(sLanguage))
            {
                return localeStore[sLanguage].Get(new Locale.Key { m_Identifier = id });
            }
            else
            {
                // We should at least have english due to file in resources.
                return localeStore["en"].Get(new Locale.Key { m_Identifier = id });
            }
        }

        public static string Get(string sKey)
        {
            string sText = GetValue(sKey);
            if (string.IsNullOrEmpty(sText) || sText.Contains(sKey))
            {
                Debug.Log("LOCALIZATION Couldn't find: " + sKey);
            }
            sText = sText.Replace("\\r\\n", "\r\n");
            return sText;
        }
    }
}