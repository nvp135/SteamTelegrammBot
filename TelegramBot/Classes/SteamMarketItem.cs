using System;
using System.Linq;
using System.Timers;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace TelegramBot
{
    class SteamMarketItem : Grid, IDisposable
    {
        TextBlockValues textblockValues;
        List<PriceDate> lPrices;
        readonly string ITEM_JSON_LINK, ITEM_NAME, ITEM_ID, ITEM_ICO_LINK, ITEM_MARKET_LINK;
        readonly static int UPDATE_INTERVAL = 90 * 1000;
        readonly static int WIDTH = 160;

        public TextBlock tbItemName = new TextBlock()
        {
            FontSize = 8,
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        TextBlock tbLastUpdate = new TextBlock()
        {
            FontSize = 10,
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        Image ico = new Image();

        public Button bDelete = new Button()
        {
            Width = 10,
            Background = new SolidColorBrush(Colors.Red),
            HorizontalAlignment = HorizontalAlignment.Right
        };

        public Border border = new Border()
        {
            BorderBrush = Brushes.Black,
            Background = Brushes.CadetBlue,
            BorderThickness = new Thickness(1)
        };

        Timer t = new Timer()
        {
            Interval = UPDATE_INTERVAL,
            Enabled = false
        };

        public SteamMarketItem(List<string> itemInfo)
        {
            border.Child = this;
            this.Width = WIDTH;
            this.Height = 100;
            t.Elapsed += t_Tick;

            ITEM_JSON_LINK = itemInfo[0];
            ITEM_ICO_LINK = itemInfo[1];
            ITEM_NAME = itemInfo[2];
            ITEM_ID = itemInfo[3];
            ITEM_MARKET_LINK = itemInfo[4];

            tbItemName.MouseDown += TbItemName_MouseDown;
            textblockValues = new TextBlockValues(ITEM_ID);

            tbItemName.Text = ITEM_NAME;
            ico.Source = HelpFunctions.GetBitmap(ITEM_ICO_LINK);

            lPrices = new List<PriceDate>();

            Style tbStyle = new Style(typeof(TextBlock));
            tbStyle.Setters.Add(new Setter { Property = Control.FontSizeProperty, Value = 10.0 });
            tbStyle.Setters.Add(new Setter { Property = Control.FontFamilyProperty, Value = new FontFamily("Consolas") });
            tbStyle.Setters.Add(new Setter { Property = Control.HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
            tbStyle.Setters.Add(new Setter { Property = Control.VerticalAlignmentProperty, Value = VerticalAlignment.Center });

            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(60, GridUnitType.Pixel)
            });
            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(WIDTH - 60, GridUnitType.Pixel)
            });

            Grid gDescr = new Grid();
            gDescr.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(40, GridUnitType.Pixel)
            });
            gDescr.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(60, GridUnitType.Pixel)
            });

            Grid gPrices = new Grid();
            gPrices.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(10, GridUnitType.Pixel)
            });
            gPrices.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(10, GridUnitType.Pixel)
            });
            gPrices.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(20, GridUnitType.Pixel)
            });
            gPrices.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(20, GridUnitType.Pixel)
            });
            gPrices.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(20, GridUnitType.Pixel)
            });
            gPrices.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(20, GridUnitType.Pixel)
            });

            SetColumn(gDescr, 0);
            this.Children.Add(gDescr);

            SetColumn(gPrices, 1);
            this.Children.Add(gPrices);

            SetRow(tbItemName, 0);
            gDescr.Children.Add(tbItemName);

            SetRow(ico, 1);
            gDescr.Children.Add(ico);

            DockPanel dp = new DockPanel();
            SetRow(dp, 0);
            gPrices.Children.Add(dp);
            DockPanel.SetDock(bDelete, Dock.Right);
            dp.Children.Add(bDelete);
            TextBlock tbUpdate = new TextBlock() { Text = "UPDATED", Style = tbStyle };
            tbUpdate.ToolTip = tbLastUpdate;
            DockPanel.SetDock(tbUpdate, Dock.Left);
            dp.Children.Add(tbUpdate);

            SetUpTextBlock(gPrices, 0, 2, tbStyle, "Price", "PriceColor", "IReceivePrice");
            SetUpTextBlock(gPrices, 0, 3, tbStyle, "TenMinutes", "TenMinutesColor", "IReceiveTenMinutes");
            SetUpTextBlock(gPrices, 0, 4, tbStyle, "ThirtyMinutes", "ThirtyMinutesColor", "IReceiveThirtyMinutes");
            SetUpTextBlock(gPrices, 0, 5, tbStyle, "OneHour", "OneHourColor", "IReceiveOneHour");
        }

        public async Task InitItem()
        {
            PriceDate res = await CheckPriceAsync();
            if (res != null)
            {
                textblockValues.startPrice = res.price;
                textblockValues.Price = res.price.ToString();
            }
            t_Tick(this, null);
            t.Enabled = true;
        }

        /// <summary>
        /// Добавляет TextBlock на указанный grid, в указанные колонку col и строку row. Задает стиль style. Биндит данные по именам (propTextName, PropBackgroundName, propToolTipText) к классу с ценами.
        /// </summary>
        /// <param name="grid">Таблица на которую надо добавить TextBlock</param>
        /// <param name="col">Колонка</param>
        /// <param name="row">Строка</param>
        /// <param name="style">Стиль</param>
        /// <param name="propTextName">Имя свойства текста в TextBlock</param>
        /// <param name="propBackgroungName">Имя свойства цвета в TexBlock</param>
        /// <param name="propToolTipText">Имя свойства ToolTip в TextBlock</param>
        private void SetUpTextBlock(Grid grid, int col, int row, Style style, string propTextName, string propBackgroungName, string propToolTipText)
        {
            var tb = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap
            };
            tb.Style = style;
            SetRow(tb, row);
            SetColumn(tb, col);
            grid.Children.Add(tb);

            DataContext = this;

            tb.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Path = new PropertyPath(propTextName),
                Source = textblockValues
            });

            tb.SetBinding(TextBlock.BackgroundProperty, new Binding()
            {
                Path = new PropertyPath(propBackgroungName),
                Source = textblockValues
            });

            ToolTip toolTip = new ToolTip();

            var tbtt = new TextBlock();
            tbtt.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Path = new PropertyPath(propToolTipText),
                Source = textblockValues
            });
            toolTip.Content = tbtt;
            tb.ToolTip = toolTip;
        }

        public async void t_Tick(object source, ElapsedEventArgs e)
        {
            try
            {
                PriceDate res = await CheckPriceAsync();
                if (res != null)
                {
                    lPrices.Insert(0, res);
                }
                UpdatePrices();
            }
            catch (Exception ex)
            {
                Logger.Write($"t_Tick error: {ex.Message} / {ITEM_ID} / {ITEM_NAME}");
            }
        }

        private void CheckPrice(out PriceDate pd, out string prefix, out string suffix)
        {
            pd = null; prefix = suffix = "";
            try
            {
                string response = HelpFunctions.LoadPage(ITEM_JSON_LINK);
                if (response == "" || response == "[]")
                {
                    SetToolTipUpdate($"{DateTime.Now} empty json");

                }
                else
                {
                    MarketResponse resp = JsonConvert.DeserializeObject<MarketResponse>(response);
                    if (resp != null && resp.success)
                    {
                        if (resp.sog != null && resp.sog.Count > 0)
                        {
                            prefix = resp.price_prefix;
                            suffix = resp.price_suffix;
                            pd = new PriceDate(double.Parse(resp.sog[0][0], CultureInfo.InvariantCulture), DateTime.Now);
                            SetToolTipUpdate($"{DateTime.Now} success updated");
                        }
                    }
                    else
                    {
                        SetToolTipUpdate($"{DateTime.Now} error json");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"CheckPrice error: {ex.Message} / {ITEM_ID} / {ITEM_NAME}");
            }
        }

        private async Task<PriceDate> CheckPriceAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                using (var stream = await httpClient.GetStreamAsync(ITEM_JSON_LINK))
                using (var reader = new StreamReader(stream))
                {
                    string response = await reader.ReadToEndAsync();

                    if (response == "" || response == "[]")
                    {
                        SetToolTipUpdate($"{DateTime.Now} empty json");
                    }
                    else
                    {
                        MarketResponse resp = JsonConvert.DeserializeObject<MarketResponse>(response);
                        if (resp != null && resp.success)
                        {
                            if (resp.sog != null && resp.sog.Count > 0)
                            {
                                SetToolTipUpdate($"{DateTime.Now} success updated");
                                textblockValues.pricePreffix = resp.price_prefix;
                                textblockValues.priceSuffix = resp.price_suffix;
                                return new PriceDate(double.Parse(resp.sog[0][0], CultureInfo.InvariantCulture), DateTime.Now);
                            }
                        }
                        else
                        {
                            SetToolTipUpdate($"{DateTime.Now} error json");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"CheckPriceAsync error: {ex.Message} / {ITEM_ID} / {ITEM_NAME}");
            }
            return null;
        }

        private void UpdatePrices()
        {
            var now = lPrices.FirstOrDefault();
            if (now != null)
            {
                textblockValues.Price = now.price.ToString();
                var ten = lPrices.FirstOrDefault(p => p.dt <= now.dt.AddMinutes(-10));
                var thirty = lPrices.FirstOrDefault(p => p.dt <= now.dt.AddMinutes(-30));
                var hour = lPrices.FirstOrDefault(p => p.dt <= now.dt.AddMinutes(-60));
                if (ten != null)
                {
                    textblockValues.TenMinutes = ten.price.ToString();
                }
                if (thirty != null)
                {
                    textblockValues.ThirtyMinutes = thirty.price.ToString();
                }
                if (hour != null)
                {
                    textblockValues.OneHour = hour.price.ToString();
                }
            }
        }

        void SetToolTipUpdate(string message)
        {
            Dispatcher.Invoke(() => {
                tbLastUpdate.Text = message;
            });
        }

        private void TbItemName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if( e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 2)
            {
                System.Diagnostics.Process.Start(ITEM_MARKET_LINK);
            }
        }

        public XElement GetXmlNode()
        {
            return new XElement("Item",
								new XAttribute("itemMarketLink", ITEM_MARKET_LINK),
								new XAttribute("itemName", ITEM_NAME),
								new XAttribute("itemIcoLink", ITEM_ICO_LINK),
								new XAttribute("itemJsonLink", ITEM_JSON_LINK),
								new XAttribute("itemId", ITEM_ID)
            );
        }

        public void Dispose()
        {
            t.Enabled = false;
        }

        ~SteamMarketItem()
        {
#if DEBUG
            Logger.Write($"{ITEM_NAME} has been deleted.");
#endif
        }
    }

    class PriceDate
    {
        public double price;
        public DateTime dt;

        public PriceDate(double price, DateTime dt)
        {
            this.price = price;
            this.dt = dt;
        }
    }

    class CurrentPrice
    {
        public double price;
        public DateTime dt;
        string suffix, prefix;

        public CurrentPrice(string pref, string suf, double pr, DateTime date)
        {
            prefix = pref;
            suffix = suf;
            price = pr;
            dt = date;
        }
    }

    class TextBlockValues: INotifyPropertyChanged
    {
        DBHelper dbh = DBHelper.Init();

        public double startPrice = 0.0;
        public string pricePreffix;
        public string priceSuffix;

        private double price = 0.0;
        private double tenMinutes = 0.0;
        private double thirtyMinutes = 0.0;
        private double oneHour = 0.0;

        private int percentPrice = 0;
        private int percentTenMinutes = 0;
        private int percentThirtyMinutes = 0;
        private int percentOneHour = 0;

        private Brush priceColor = Brushes.White;
        private Brush tenMinutesColor = Brushes.White;
        private Brush thirtyMinutesColor = Brushes.White;
        private Brush oneHourColor = Brushes.White;

        private int id;
        private static readonly double multiplier = 0.8697;

        public TextBlockValues(string id)
        {
            this.id = Convert.ToInt32(id);
        }

        public string Price
        {
            get { return $"{pricePreffix}{price}{priceSuffix}({percentPrice}%)"; }
            set
            {
                double result;
                if (Double.TryParse(value, out result))
                {
                    if (price != result)
                    {
                        dbh.Insert($"insert into [Items] ([market_id], [dt], [price]) values ({id}, '{DateTime.Now}', '{value}')");
                    }
                    if (startPrice == 0)
                    {
                        startPrice = result;
                    }
                    price = result;
                    try
                    {
                        percentPrice = Convert.ToInt32((price - startPrice) / (startPrice / 100));
                    }
                    catch (OverflowException)
                    {
                        percentPrice = 100;
                    }
                    PriceColor = GetColor(percentPrice);
                    RaisePropertyChanged("Price");
                    IReceivePrice = (price * multiplier).ToString($"{pricePreffix}0.00{priceSuffix}");
                    RaisePropertyChanged("IReceivePrice");
                }
            }
        }

        public string IReceivePrice { get; set; }

        public string TenMinutes
        {
            get { return $"{pricePreffix}{tenMinutes}{priceSuffix}({percentTenMinutes}%)"; }
            set
            {
                tenMinutes = Convert.ToDouble(value);
                try
                { 
                    percentTenMinutes = Convert.ToInt32((tenMinutes - price) / (price / 100));
                }
                catch (OverflowException)
                {
                    percentTenMinutes = 100;
                }
                TenMinutesColor = GetColor(percentTenMinutes);
                RaisePropertyChanged("TenMinutes");
                IReceiveTenMinutes = (tenMinutes * multiplier).ToString($"{pricePreffix}0.00{priceSuffix}");
                RaisePropertyChanged("IReceiveTenMinutes");
            }
        }

        public string IReceiveTenMinutes { get; set; }

        public string ThirtyMinutes
        {
            get { return $"{pricePreffix}{thirtyMinutes}{priceSuffix}({percentThirtyMinutes}%)"; }
            set
            {
                thirtyMinutes = Convert.ToDouble(value);
                try
                {
                    percentThirtyMinutes = Convert.ToInt32((thirtyMinutes - price) / (price / 100));
                }
                catch (OverflowException)
                {
                    percentThirtyMinutes = 100;
                }
                ThirtyMinutesColor = GetColor(percentThirtyMinutes);
                RaisePropertyChanged("ThirtyMinutes");
                IReceiveThirtyMinutes = (thirtyMinutes * multiplier).ToString($"{pricePreffix}0.00{priceSuffix}");
                RaisePropertyChanged("IReceiveThirtyMinutes");
            }
        }

        public string IReceiveThirtyMinutes { get; set; }

        public string OneHour
        {
            get { return $"{pricePreffix}{oneHour}{priceSuffix}({percentOneHour}%)"; }
            set
            {
                oneHour = Convert.ToDouble(value);
                try
                {
                    percentOneHour = Convert.ToInt32((oneHour - price) / (price / 100));
                }
                catch (OverflowException)
                {
                    percentOneHour = 100;
                }
                OneHourColor = GetColor(percentOneHour);
                RaisePropertyChanged("OneHour");
                IReceiveOneHour = (oneHour * multiplier).ToString($"{pricePreffix}0.00{priceSuffix}");
                RaisePropertyChanged("IReceiveOneHour");
            }
        }

        public string IReceiveOneHour { get; set; }

        public Brush PriceColor
        {
            get { return priceColor; }
            set
            {
                priceColor = value;
                RaisePropertyChanged("PriceColor");
            }
        }

        public Brush TenMinutesColor
        {
            get { return tenMinutesColor; }
            set
            {
                tenMinutesColor = value;
                RaisePropertyChanged("TenMinutesColor");
            }
        }

        public Brush ThirtyMinutesColor
        {
            get { return thirtyMinutesColor; }
            set
            {
                thirtyMinutesColor = value;
                RaisePropertyChanged("ThirtyMinutesColor");
            }
        }

        public Brush OneHourColor
        {
            get { return oneHourColor; }
            set
            {
                oneHourColor = value;
                RaisePropertyChanged("OneHourColor");
            }
        }

        Brush GetColor(int dif)
        {
            if (dif < 0)
                return Brushes.Yellow;
            else if (dif < 20)
                return Brushes.White;
            else if (dif < 50)
                return Brushes.GreenYellow;
            else if (dif < 100)
                return Brushes.Pink;
            else
                return Brushes.Plum;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string propertyName)
        {
            var handlers = PropertyChanged;

            handlers(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
