using Newtonsoft.Json;
using System.Windows.Controls;
using System;
using System.Timers;
using System.Windows;
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
        DateTime lastSendedDT;
        bool cheking;
        int waitTime = 5;

        Timer t = new Timer()
        {
            Enabled = false,
            Interval = 90 * 1000
        };

        public Twitch(string channel, List<string> users, TextBox tb)
        {
            this.users = users;
            twitchUrl = $"http://tmi.twitch.tv/group/user/{channel.ToLower()}/chatters";
            t.Elapsed += OnTimedEvent;
            tbLog = tb;
            cheking = false;
            lastSendedDT = DateTime.Now.AddMinutes(-waitTime);
        }

        public void CheckViewers()
        {
            if (cheking)
            {
                return;
            }
            cheking = true;
            try
            {
                string page = HelpFunctions.LoadPage(twitchUrl);
                if ((page != "") && (page != "\"\""))
                {
                    TwitchJSON result = JsonConvert.DeserializeObject<TwitchJSON>(page);
                    if (result.chatter_count > 0)
                    {
                        string sendtext = "";
                        foreach (var userName in users)
                        {
                            if (!result.chatters.viewers.Contains(userName))
                            {
                                sendtext += $"{DateTime.Now}: пользователя {userName} нет в смотрящих стрима." + Environment.NewLine;
                            }
                        }
                        if (!String.IsNullOrEmpty(sendtext) && ((DateTime.Now - lastSendedDT) > TimeSpan.FromMinutes(waitTime)))
                        {
                            lastSendedDT = DateTime.Now;
                            tg.SendMessage(sendtext);
                        }
                        AddTextToLog("Check users - Result check users sucsessful.");
                    }
                    else
                    {
                        AddTextToLog("Check users - Result xml is empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                AddTextToLog(ex.Message);
            }
            cheking = false;
        }

        private void AddTextToLog(string text)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(
                        delegate ()
                        {
                            if (tbLog.Text.Length > 2000)
                            {
                                tbLog.Text = String.Empty;
                            }
                            tbLog.AppendText($"{DateTime.Now}: {text}{Environment.NewLine}");
                        }));
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            CheckViewers();
        }

        public void Start()
        {
            CheckViewers();
            t.Enabled = true;
        }

        public void Stop()
        {
            t.Enabled = false;
        }
    }
}
