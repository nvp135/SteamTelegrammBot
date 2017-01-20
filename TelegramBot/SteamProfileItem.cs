using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Timers;
using System.Threading.Tasks;

namespace TelegramBot
{
    class SteamProfileItem : DockPanel
    {
        string accountNick, itemId;
        readonly string linkToJSON;
        private Telegram tg = Telegram.TelegramInit();
        TextBox tbLog;
        Image ico = new Image();
        public static bool auth;

        public SteamProfileItem(string link, string accountNickname, string picUrl, string itemName, string itemId, TextBox tbLog)
        {
            this.tbLog = tbLog;
            this.linkToJSON = link;
            this.itemId = itemId;
            accountNick = accountNickname;
            tbNick.Text = $"Account nick {accountNick} / Last updated {DateTime.Now}";
            t.Elapsed += t_Tick;

            ico.Source = HelpFunctions.GetBitmap(picUrl);
            tbItemName.Text = itemName;

            LastChildFill = true;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(150, GridUnitType.Auto)
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(10, GridUnitType.Star)
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(50, GridUnitType.Auto)
            });
            grid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(20, GridUnitType.Pixel)
            });
            grid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(45, GridUnitType.Auto)
            });

            Grid.SetRow(bDelete, 0);
            Grid.SetColumn(bDelete, 2);
            grid.Children.Add(bDelete);
            Grid.SetRow(tbNick, 0);
            Grid.SetColumn(tbNick, 0);
            Grid.SetColumnSpan(tbNick, 2);
            grid.Children.Add(tbNick);
            Grid.SetRow(ico, 1);
            Grid.SetColumn(ico, 0);
            grid.Children.Add(ico);
            Grid.SetRow(tbItemName, 1);
            Grid.SetColumn(tbItemName, 1);
            Grid.SetColumnSpan(tbItemName, 2);
            grid.Children.Add(tbItemName);
            //grid.ShowGridLines = true;
            SetDock(grid, Dock.Top);
            Children.Add(grid);

            t.Enabled = true;
        }

        Timer t = new Timer()
        {
            Enabled = false,
            Interval = 60000
        };

        public Button bDelete = new Button()
        {
            Width = 30,
            Background = new SolidColorBrush(Colors.Red),
            Content = "DEL"
        };

        TextBlock tbNick = new TextBlock()
        {
            FontSize = 12,
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.NoWrap,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        TextBlock tbItemName = new TextBlock()
        {
            FontSize = 20,
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        private void t_Tick(object source, ElapsedEventArgs e)
        {
            if(!auth)
            {
                return;
            }
            t.Enabled = false;
            CheckInventory();
        }

        async void CheckInventory()
        {
            try
            {
                ResultCheckItem res = await CheckLastItem(linkToJSON);
                if(res.error == "")
                {
                    if (res.itemId != "" && res.itemId != itemId)
                    {
                        itemId = res.itemId;
                        await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                            () =>
                            {
                                ChangeItem(res.picUrl, res.itemName);
                            }));
                    }
                    else
                    {
                        await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               () =>
                               {
                                   tbNick.Text = $"Account nick {accountNick} / Last updated {DateTime.Now}";
                               }));
                    }
                }
                else
                {
                    await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                        () =>
                        {
                            tbLog.AppendText($"{DateTime.Now}: {res.error}{Environment.NewLine}");
                        }));
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                        () =>
                        {
                            tbLog.AppendText($"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
                        }));
            }
            t.Enabled = true;
        }

        public async static Task<ResultCheckItem> CheckLastItem(string link)
        {
            string response = await CookieClient.GetHttpAsync(link);
            if (response != "")
            {
                InventoryResponse resp = JsonConvert.DeserializeObject<InventoryResponse>(response);
                if (resp != null)
                {
                    if (resp.success)
                    { 
                        if (resp.dicInventory.Count > 0)
                        {
                            foreach (var item in resp.dicInventory)
                            {
                                if (item.Value.pos == 1)
                                {
                                    var firstDsc = resp.dicDescriptions[$"{item.Value.classid}_{item.Value.instanceid}"];
                                    return new ResultCheckItem()
                                    {
                                        itemId = item.Value.id,
                                        picUrl = $"http://steamcommunity-a.akamaihd.net/economy/image/{firstDsc.icon_url}/96fx96f",
                                        itemName = $"{firstDsc.name}",
                                        error = ""
                                    };
                                }
                            }
                        }
                        else
                        {
                            return new ResultCheckItem()
                            {
                                itemId = "0",
                                picUrl = "empty",
                                itemName = "Empty CS:GO inventory :(",
                                error = ""
                            };
                        }
                    }
                    else
                    {
                        return new ResultCheckItem()
                        {
                            itemId = "",
                            picUrl = "",
                            itemName = "",
                            error = resp.Error
                        };
                    }
                    //Logger.Write($"{DateTime.Now} - Normal update - {link}{Environment.NewLine}");
                }
                else
                {
                    //Logger.Write($"{DateTime.Now} - {response} - {link}{Environment.NewLine}");
                }
            }
            else
            {
                //Logger.Write($"{DateTime.Now} - Empty response {link}{Environment.NewLine}");
            }
            return null;
        }

        void ChangeItem(string urlImg, string name)
        {
            ico.Source = HelpFunctions.GetBitmap(urlImg);
            tbItemName.Text = name;
            tg.SendMessage($"{accountNick} получил CS:GO итем {name}");
            tg.SendPicture(urlImg.Replace("/96fx96f", ""));
            tbNick.Text = $"Account nick {accountNick} / Last updated {DateTime.Now}";
        }
    }

    class ResultCheckItem
    {
        public string itemId { get; set; }
        public string picUrl { get; set; }
        public string itemName { get; set; }
        public string error { get; set; }
    }
}