using System;
using System.Net.Http;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace TelegramBot
{
    public static class HelpFunctions
    {
        /// <summary>
        /// Загружает страницу
        /// </summary>
        /// <param name="result">Результат запроса</param>
        /// <param name="link">Ссылка на страницу</param>
        public static string LoadPage(string link)
        {
            using (var client = new HttpClient())
            {
                return client
                    .GetAsync(link)
                    .Result
                    .Content
                    .ReadAsStringAsync()
                    .Result;
            }
        }

        public static BitmapImage GetBitmap(string urlImg)
        {
            if (urlImg == "empty")
                return null;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(urlImg, UriKind.Absolute);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            return bi;
        }

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                return random.Next(min, max);
            }
        }
    }
}
