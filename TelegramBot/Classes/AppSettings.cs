using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;

namespace TelegramBot
{

    public static class ApplicationSettings
    {
        public static AppSettings settings;

        public static void InitializeSettings()
        {
            if (File.Exists("settings.xml"))
            {
                using (Stream stream = File.Open("settings.xml", FileMode.Open))
                {
                    SoapFormatter f = new SoapFormatter();
                    try
                    {
                        settings = (AppSettings)f.Deserialize(stream);
                        stream.Close();
                    }
                    catch (Exception)
                    {
                        EditSettings();
                    }
                }
            }
            else
            {
                EditSettings();
            }
        }

        public static void EditSettings()
        {
            if (settings == null)
                settings = new AppSettings();
            SettingsWindow s = new SettingsWindow(settings);
            s.ShowDialog();
        }

        public static AppSettings GetSettings()
        {
            if (settings == null)
                EditSettings();
            return settings;
        }

        public static void SaveSettingsToFile()
        {
            using (Stream stream = File.Open("settings.xml", FileMode.Create))
            {
                stream.Position = 0;
                SoapFormatter formatter = new SoapFormatter();

                formatter.Serialize(stream, settings);
                stream.Close();

            }
        }
    }

    [Serializable()]
    public class AppSettings
    {
        private bool useProxy;
        public bool UseProxy
        {
            get { return useProxy; }
            set { useProxy = value; }
        }

        private string proxyIP;
        public string ProxyIP
        {
            get { return proxyIP; }
            set { proxyIP = value; }
        }

        private int proxyPort;
        public int ProxyPort
        {
            get { return proxyPort; }
            set { proxyPort = value; }
        }

        private string proxyLogin;
        public string ProxyLogin
        {
            get { return proxyLogin; }
            set { proxyLogin = value; }
        }

        private string proxyPassword;
        public string ProxyPassword
        {
            get { return proxyPassword; }
            set { proxyPassword = value; }
        }

        private string tgToken;
        public string TGToken
        {
            get { return tgToken; }
            set { tgToken = value; }
        }

        private string steamLogin;
        public string SteamLogin
        {
            get { return steamLogin; }
            set { steamLogin = value; }
        }

        private string steamPassword;
        public string SteamPassword
        {
            get { return steamPassword; }
            set { steamPassword = value; }
        }

        private string steamGuard;
        public string SteamGuard
        {
            get { return steamGuard; }
            set { steamGuard = value; }
        }
        

    }
}
