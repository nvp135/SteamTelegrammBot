using System.IO;
using System.Collections.Specialized;
using System.Net;

namespace TelegramBot
{
    class Telegram
    {
        static Telegram tb;

        readonly string token, baseUrl = @"https://api.telegram.org/bot", userId;

        Telegram()
        {
            try
            {
                token = File.ReadAllText("tokentelegram");
            }
            catch (System.Exception)
            {

            }
            userId = "1597881";
        }

        public static Telegram TelegramInit()
        {
            if (tb != null)
                return tb;
            tb = new Telegram();
            return tb;
        }

        /// <summary>
        /// Отправить сообщение пользователю
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        public void SendMessage(string message)
        {   
            using (var webClient = new WebClient())
            {
                var pars = new NameValueCollection();
                pars.Add("text", message);
                pars.Add("chat_id", userId);
                webClient.UploadValues(baseUrl + token + "/sendMessage", pars);
            }
        }

        public void SendPicture(string link)
        {
            using (var webClient = new WebClient())
            {
                var pars = new NameValueCollection();
                pars.Add("photo", link);
                pars.Add("chat_id", userId);
                webClient.UploadValues(baseUrl + token + "/sendPhoto", pars);
            }
        }

    }
}
