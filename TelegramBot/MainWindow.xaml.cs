using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using TelegramBot.Properties;
using System.Threading;
using System.Net.Http;

namespace TelegramBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cts;
        Telegram tg;
        Twitch tw;
        List<SteamProfileItem> profilesItems = new List<SteamProfileItem>();
        List<SteamMarketItem> marketItems = new List<SteamMarketItem>();

        readonly string xmlItemsMarketName = "ItemsMarket.xml";

        bool auth, marketItemListChange;

        System.Timers.Timer tCheckAuth = new System.Timers.Timer()
        {
            Interval = 60000,
            Enabled = false
        };

        public MainWindow()
        {
            InitializeComponent();
            tc.SelectedIndex = 2;
            tg = Telegram.TelegramInit();
            tCheckAuth.Elapsed += tCheckAuth_Tick;
            CheckAuth();
            tCheckAuth.Enabled = true;
            marketItemListChange = false;
            LoadTwitchSettings();
        }

        private void LoadTwitchSettings()
        {
            string configPath = $"{Directory.GetCurrentDirectory()}\\config.xml";
            if (!File.Exists(configPath))
            {
                SaveTwitchSettings();
            }
            try
            {
                XmlDocument configXmlDocument = new XmlDocument();
                configXmlDocument.Load(configPath);
                XmlNodeList xnl = configXmlDocument.GetElementsByTagName("TwitchChannel");
                if (xnl != null && xnl.Count == 1)
                {
                    tbTwitchChannel.Text = xnl[0].InnerText.Trim();
                }
                xnl = configXmlDocument.GetElementsByTagName("TwitchUsers");
                if (xnl != null && xnl.Count == 1)
                {
                    tbTwitchUsers.Text = xnl[0].InnerText.Trim();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SaveTwitchSettings()
        {
            string configPath = $"{Directory.GetCurrentDirectory()}\\config.xml";
            using (XmlWriter writer = XmlWriter.Create(configPath))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("TwitchSettings");

                writer.WriteElementString("TwitchUsers", tbTwitchUsers.Text);
                writer.WriteElementString("TwitchChannel", tbTwitchChannel.Text);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void bStartTwitchMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (tw == null)
            {
                SaveTwitchSettings();
                List<string> users = new List<string>(tbTwitchUsers.Text.Split(',').Where(x => x.Length > 0));
                tw = new Twitch(tbTwitchChannel.Text, users, tbLogTwitch);
                tw.Start();
                bStartTwitchMonitor.Content = "Stop Monitor";
            }
            else
            {
                tw.Stop();
                tw = null;
                bStartTwitchMonitor.Content = "Start Monitor";
            }

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
                            item.bDelete.Click += DelAccount;
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

        private void DelAccount(object sender, RoutedEventArgs e)
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

        private void DelItem(object sender, RoutedEventArgs e)
        {
            foreach (SteamMarketItem item in marketItems)
            {
                if ((Button)sender == item.bDelete)
                {
                    marketItemListChange = true;
                    item.Dispose();
                    wpMarketItems.Children.Remove(item.border);
                    marketItems.Remove(item);
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
                var res = Task.Run(() => HelpFunctions.LoadPage(link));
                res.Wait();
                string page = res.Result;
                //string page = HelpFunctions.LoadPage(link);
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
            System.Diagnostics.Process.Start("www.yahoo.com");
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


        private List<string> AddMarketItem(string link)
        {
            if (link.StartsWith("http"))
            {
                link = link.Replace("https://", "http://");
            }
            else
            {
                return null;
            }
            return CheckMarketId(link);
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            LoadSavedItemsList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!marketItemListChange)
            {
                return;
            }
            try
            {
                var root = new XElement("Items");
                foreach (var item in marketItems)
                {
                    root.Add(item.GetXmlNode());
                }
                root.Save(xmlItemsMarketName);
            }
            catch (Exception ex)
            {
                Logger.Write($"{DateTime.Now}: Ошибка при сохранении списка маркет итемов в файл {ex.Message}");
            }
        }

#region Items Market 

        private async void bAddMarketItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> link = new List<string>();
            link.Add(tbLinkMarket.Text);
            await CheckMarketItemsHistory(link);
            tbLinkMarket.Text = String.Empty;
            marketItemListChange = true;
        }

        void LoadSavedItemsList()
        {
            XmlDocument doc = new XmlDocument();
            List<XmlAttributeCollection> values = null;
            if (File.Exists(xmlItemsMarketName))
            {
                values = new List<XmlAttributeCollection>();
                doc.Load(xmlItemsMarketName);
                foreach (XmlNode node in doc.SelectNodes("//Item"))
                {
                    values.Add(node.Attributes);
                }
            }
            if (values != null && values.Count > 0)
            {
                foreach (var item in values)
                {
                    List<string> itm = new List<string>()
                    {
                        item["itemJsonLink"].Value,
                        item["itemIcoLink"].Value,
                        item["itemName"].Value,
                        item["itemId"].Value,
                        item["itemMarketLink"].Value
                    };
                    AddSteamMarketItem(new SteamMarketItem(itm));
                }
            }

            /*IEnumerable<string> itemsMarket = null;
            try
            {
                itemsMarket = File.ReadAllText(marketItemsHistory).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            }
            catch (Exception ex)
            {
                tbLogError.AppendText($"{DateTime.Now}: Возникла ошибка при чтении файла с товарами для отслеживания: {ex.Message}{Environment.NewLine}");
            }
            await CheckMarketItemsHistory(itemsMarket);*/
        }

        async Task<SteamMarketItem> CheckMarketItemAsync(string url, HttpClient client, CancellationToken ct)
        {
            string page = "";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url, ct);
                var task = response.Content.ReadAsStringAsync();
                page = await task;
            }
            catch (Exception ex)
            {
                tbLogError.AppendText($"{DateTime.Now}: Возникла ошибка при загрузке страницы товара для отслеживания: {ex.Message}{Environment.NewLine}");
            }
            if (page != "")
            {
                List<string> result = new List<string>()
                {
                    "", "", "", "", ""
                };
                Regex rId = new Regex("Market_LoadOrderSpread\\((.*)\\);");
                Match mId = rId.Match(page);
                result[0] = $"http://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=5&item_nameid={mId.Groups[1].Value.Trim()}&two_factor=0";
                Regex rIcoUri = new Regex("\"icon_url\":\"([^\"]*)");
                Match mIcoUri = rIcoUri.Match(page);
                result[1] = $"http://steamcommunity-a.akamaihd.net/economy/image/{mIcoUri.Groups[1].Value}/60fx60f";
                Regex rName = new Regex("<span class=\"market_listing_item_name\" style=\"\">(.*)</span><br/>");
                Match mName = rName.Match(page);
                result[2] = mName.Groups[1].Value.Trim();
                result[3] = mId.Groups[1].Value.Trim();
                result[4] = url;

                //Regex rSpamResponse = new Regex( = "<h3>You've made too many requests recently. Please wait and try your request again later.</h3>");

                if (!String.IsNullOrEmpty(result[0]) && !String.IsNullOrEmpty(result[1]) && !String.IsNullOrEmpty(result[2]))
                {
                    return new SteamMarketItem(result);
                }
            }
            return null;
        }

        async Task CheckMarketItemsHistory(IEnumerable<string> itemsMarket)
        {
            cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            if (itemsMarket != null)
            {
                var client = new HttpClient();

                IEnumerable<Task<SteamMarketItem>> loadItemsQuery =
                    from url in itemsMarket select CheckMarketItemAsync(url, client, ct);

                List<Task<SteamMarketItem>> downloadTasks = loadItemsQuery.ToList();

                while (downloadTasks.Count > 0)
                {
                    Task<SteamMarketItem> firstFinishedTask = await Task.WhenAny(downloadTasks);
                    downloadTasks.Remove(firstFinishedTask);
                    var steamItem = firstFinishedTask.Result;
                    if (steamItem != null)
                    {
                        AddSteamMarketItem(steamItem);
                    }
                }
            }
            cts = null;
        }

        private void tbFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var item in marketItems)
            {
                item.Visibility = item.tbItemName.Text.ToUpper().Contains((sender as TextBox).Text.ToUpper()) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void AddSteamMarketItem(SteamMarketItem steamItem)
        {
            steamItem.InitItem(); //t_Tick(this, null);
            steamItem.bDelete.Click += DelItem;
            wpMarketItems.Children.Add(steamItem.border);
            marketItems.Add(steamItem);
        }

#endregion
    }
}
