using Newtonsoft.Json;
using System.Windows.Controls;
using System;
using System.Timers;
using System.Windows.Threading;
using System.Collections.Generic;

namespace TelegramBot
{
    class Twitch
    {
        private readonly string twitchUrl;
        private List<string> users;
        private Telegram tg = Telegram.TelegramInit();
        private TextBox tbLog;
        DateTime lastSendedDT = DateTime.Now.AddMinutes(-1);

        Timer t = new Timer()
        {
            Enabled = false,
            Interval = 15000
        };

        public Twitch(string channel, List<string> users, TextBox tb)
        {
            this.users = users;
            twitchUrl = $"http://tmi.twitch.tv/group/user/{channel.ToLower()}/chatters";
            t.Elapsed += OnTimedEvent;
            tbLog = tb;
            CheckViewers();
        }

        public void CheckViewers()
        {
            
            try
            {
                string page = HelpFunctions.LoadPage(twitchUrl);
                if (page != "")
                {
                    TwitchJSON result = JsonConvert.DeserializeObject<TwitchJSON>(page);
                    string sendtext = "";
                    foreach (var userName in users)
                    {
                        if (!result.chatters.viewers.Contains(userName))
                        {
                            sendtext += $"{DateTime.Now}: пользователя {userName} нет в смотрящих стрима." + Environment.NewLine;
                        }
                    }
                    if (!String.IsNullOrEmpty(sendtext) && (DateTime.Now - lastSendedDT > TimeSpan.FromMinutes(1)))
                    {
                        tg.SendMessage(sendtext);
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                        () =>
                        {
                            tbLog.AppendText(ex.Message + Environment.NewLine);
                        }));
            }
            
            t.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            t.Enabled = false;
            CheckViewers();
        }
    }
}
