using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Threading;
using TelegramBot.Properties;
using System.Windows.Media;

namespace TelegramBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Telegram tg;
        Twitch tw;
        List<SteamProfileItem> profilesItems = new List<SteamProfileItem>();

        bool auth;
        Timer tCheckAuth = new Timer()
        {
            Interval = 60000,
            Enabled = false
        };

        public MainWindow()
        {
            InitializeComponent();
            tg = Telegram.TelegramInit();
            tCheckAuth.Elapsed += tCheckAuth_Tick;
            CheckAuth();
            tCheckAuth.Enabled = true;
        }

        private void bStartTwitchMonitor_Click(object sender, RoutedEventArgs e)
        {
            List<string> users = new List<string>(tbUsers.Text.Split(',').Where(x => x.Length > 0));
            tw = new Twitch(tbChannel.Text, users, tbLogSteam);
            tw.CheckViewers();
        }

        private void bTest_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //tb.SendMessage(tbSend.Text);
        }

        private async void bAddSteamProfile_Click(object sender, RoutedEventArgs e)
        {
            if (!auth)
            {
                return;
            }
            string link = tbLink.Text;
            if (link.StartsWith("http"))
            {
                link = $"{tbLink.Text.Replace("https://", "http://")}";
            }
            else
            {
                link = $"http://{tbLink.Text}";
            }
            string accountNick = CheckNick(link);
            if (!String.IsNullOrEmpty(accountNick))
            {
                try
                {
                    link = $"{link}/inventory/json/730/2";
                    ResultCheckItem res = await SteamProfileItem.CheckLastItem(link);
                    //SteamProfileItem.CheckLastItem(out itemId, out picUrl, out itemName, out error, link);
                    if (String.IsNullOrEmpty(res.error))
                    {
                        if (!String.IsNullOrEmpty(res.itemId) && !String.IsNullOrEmpty(res.picUrl) && !String.IsNullOrEmpty(res.itemName))
                        {
                            SteamProfileItem item = new SteamProfileItem(link, accountNick, res.picUrl, res.itemName, res.itemId, tbLogSteam);
                            item.bDelete.Click += Del;
                            profilesItems.Add(item);
                            spContent.Children.Add(item);
                        }
                        else
                        {
                            tbLogSteam.AppendText(DateTime.Now + ": " + "Ссылка на аккаунт верна, но не могу загрузить инвентарь. Возможно он недоступен." + Environment.NewLine);
                        }
                    }
                    else
                    {
                        tbLogSteam.AppendText($"{DateTime.Now}: {res.error}{Environment.NewLine}");
                    }
                }
                catch (Exception ex)
                {
                    tbLogSteam.AppendText(DateTime.Now + ": " + ex.Message + Environment.NewLine);
                }
            }
            else
            {
                tbLogSteam.AppendText(DateTime.Now + ": " + "Ссылка на аккаунт не верна." + Environment.NewLine);
            }
            tbLink.Text = "";
        }

        private void Del(object sender, RoutedEventArgs e)
        {
            foreach (SteamProfileItem item in profilesItems)
            {
                if ((Button)sender == item.bDelete)
                {
                    spContent.Children.Remove(item);
                    profilesItems.Remove(item);
                    break;
                }
            }
        }

        private string CheckNick(string link)
        {
            try
            {
                string page = HelpFunctions.LoadPage(link);
                if (page != "")
                {
                    Regex r = new Regex("<span class=\"actual_persona_name\">(.*)</span>");
                    Match match = r.Match(page);
                    return match.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                tbLogSteam.AppendText(ex.Message + Environment.NewLine);
            }
            return null;
        }

        private List<string> CheckMarketId(string link)
        {
            List<string> result = new List<string>()
            {
                "", "", ""
            };
            try
            {
                string page = HelpFunctions.LoadPage(link);
                if (page != "")
                {
                    Regex rId = new Regex("Market_LoadOrderSpread\\((.*)\\);");
                    Match mId = rId.Match(page);
                    result[0] = $"http://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=5&item_nameid={mId.Groups[1].Value.Trim()}&two_factor=0";
                    Regex rIcoUri = new Regex("\"icon_url\":\"([^\"]*)");
                    Match mIcoUri = rIcoUri.Match(page);
                    result[1] = $"http://steamcommunity-a.akamaihd.net/economy/image/{mIcoUri.Groups[1].Value}/60fx60f";
                    Regex rName = new Regex("<span class=\"market_listing_item_name\" style=\"\">(.*)</span><br/>");
                    Match mName = rName.Match(page);
                    result[2] = mName.Groups[1].Value.Trim();
                }
            }
            catch (Exception ex)
            {
                tbLogSteam.AppendText(ex.Message + Environment.NewLine);
            }
            return result;
        }

        private void testbtn_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void testbtn1_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        public void tCheckAuth_Tick(object source, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                            () =>
                            {
                                CheckAuth();
                            }));
        }

        private void bAuth_Click(object sender, RoutedEventArgs e)
        {
            SignInSteam f = new SignInSteam();
            f.ShowDialog();
            CheckAuth();
        }

        void CheckAuth()
        {
            auth = !string.IsNullOrWhiteSpace(Settings.Default.sessionid) && !string.IsNullOrWhiteSpace(Settings.Default.steamLogin);
            SteamProfileItem.auth = auth;
            if (auth)
            {
                bAddSteamAccount.Visibility = Visibility.Visible;
                bAuth.Visibility = Visibility.Collapsed;
            }
            else
            {
                bAddSteamAccount.Visibility = Visibility.Collapsed;
                bAuth.Visibility = Visibility.Visible;
            }
        }

        private void bAddMarketItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> itemInfo = CheckMarketId(tbLinkMarket.Text);
            if(!String.IsNullOrEmpty(itemInfo[0]) && !String.IsNullOrEmpty(itemInfo[1]) && !String.IsNullOrEmpty(itemInfo[2]))
            {
                SteamMarketItem si = new SteamMarketItem(itemInfo);
                wpMarketItems.Children.Add(si.border);
            }

        }
    }
}
