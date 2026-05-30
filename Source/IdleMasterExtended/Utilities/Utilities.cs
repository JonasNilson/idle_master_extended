using System.IO;
using Gameloop.Vdf;
using Microsoft.Win32;
using Gameloop.Vdf.JsonConverter;
using System.Collections.Generic;


namespace IdleMasterExtended.Utilities
{
    public class InstallConfigStore
    {
        public ISoftware Software { get; set; }
        public class ISoftware
        {
            public IValve Valve { get; set; }
        }

        public class IValve
        {
            public ISteam Steam { get; set; }
        }

        public class ISteam
        {
            public Dictionary<string, IAccount> Accounts { get; set; }
        }
        public class IAccount
        {
            public string SteamID { get; set; }
        }
    }

    public static class Steam
    {
        private static string GetPath()
        {
            string steamPath = string.Empty;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                if (key != null)
                {
                    object value = key.GetValue("SteamPath");
                    if (value != null)
                    {
                        steamPath = value.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(steamPath))
            {
                steamPath = Path.GetFullPath(steamPath);
            }
            return steamPath;
        }

        public static List<(string Key, InstallConfigStore.IAccount Value)> GetProfiles()
        {
            var steamPath = GetPath();
            var configFile = Path.Combine(steamPath, "config", "config.vdf");
            
            var fileContent = File.ReadAllText(configFile);

            var desOpts = new VdfSerializerSettings() { MaximumTokenSize = 8192, UsesEscapeSequences = true };
            var desConf = VdfConvert.Deserialize(fileContent, desOpts);

            var configStore = desConf.ToJson().Value.ToObject<InstallConfigStore>();
            if (configStore?.Software?.Valve?.Steam?.Accounts != null)
            {
                var accounts = new List<(string Key, InstallConfigStore.IAccount Value)>();
                foreach (var account in configStore.Software.Valve.Steam.Accounts)
                {
                    accounts.Add((account.Key, account.Value));
                }

                return accounts;
            }
            
            return null;
        }
    }
}
