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

namespace TelegramBot
{
    class SteamMarketItem : Grid, IDisposable
    {
        TextBlockValues textblockValues;
        List<PriceDate> lPrices;
        readonly string jsonLink, itemName;

        TextBlock tbItemName = new TextBlock()
        {
            FontSize = 8,
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        TextBlock tbCurrentPrice = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
        };

        TextBlock tbTen = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
        };

        TextBlock tbThirty = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
        };

        TextBlock tbHour = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
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
            Interval = 60 * 1000,
            Enabled = false
        };

        public SteamMarketItem(List<string> itemInfo)
        {
            border.Child = this;
            this.Width = 150;
            this.Height = 100;
            t.Elapsed += t_Tick;

            textblockValues = new TextBlockValues(itemInfo[3]);
            lPrices = new List<PriceDate>();

            Style tbStyle = new Style(typeof(TextBlock));
            tbStyle.Setters.Add(new Setter { Property = Control.FontSizeProperty, Value = 10.0 });
            tbStyle.Setters.Add(new Setter { Property = Control.FontFamilyProperty, Value = new FontFamily("Consolas") });
            tbStyle.Setters.Add(new Setter { Property = Control.HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
            tbStyle.Setters.Add(new Setter { Property = Control.VerticalAlignmentProperty, Value = VerticalAlignment.Center });


            tbCurrentPrice.Style = tbStyle;
            tbTen.Style = tbStyle;
            tbThirty.Style = tbStyle;
            tbHour.Style = tbStyle;

            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(60, GridUnitType.Pixel)
            });
            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(90, GridUnitType.Pixel)
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

            SetRow(bDelete, 0);
            gPrices.Children.Add(bDelete);

            itemName = itemInfo[2];
            tbItemName.Text = itemName;
            ico.Source = HelpFunctions.GetBitmap(itemInfo[1]);
            jsonLink = itemInfo[0];

            SetUpTextBlock(gPrices, 0, 2, tbStyle, "Price", "PriceColor", "IReceivePrice");
            SetUpTextBlock(gPrices, 0, 3, tbStyle, "TenMinutes", "TenMinutesColor", "IReceiveTenMinutes");
            SetUpTextBlock(gPrices, 0, 4, tbStyle, "ThirtyMinutes", "ThirtyMinutesColor", "IReceiveThirtyMinutes");
            SetUpTextBlock(gPrices, 0, 5, tbStyle, "OneHour", "OneHourColor", "IReceiveOneHour");

            PriceDate res;
            string prefix, suffix;
            CheckPrice(out res, out prefix, out suffix);
            if(res != null)
            {
                textblockValues.startPrice = res.price;
                textblockValues.pricePreffix = prefix;
                textblockValues.priceSuffix = suffix;
                textblockValues.Price = res.price.ToString();
            }
            t.Enabled = true;
        }

        /// <summary>
        /// Добавляет TextBlock на указанный grid, в указанные колонку col и строку row. Задает стиль style. Биндит данные по именам (propTextName, PropBackgroundName, propToolTipText) к классу с ценами.
        /// </summary>
        /// <param name="grid">Таблица на которую надо ддобавить TextBlock</param>
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

        private void t_Tick(object source, ElapsedEventArgs e)
        {
            PriceDate res; string prefix, suffix;
            CheckPrice(out res, out prefix, out suffix);
            if (res != null)
            {
                lPrices.Insert(0, res); 
            }
            try
            {
                UpdatePrices();
            }
            catch (Exception ex)
            {
                Logger.Write($"{DateTime.Now}: {ex.Message}");
            }
        }

        private void CheckPrice(out PriceDate pd, out string prefix, out string suffix)
        {
            pd = null; prefix = suffix = "";
            try
            {
                string response = HelpFunctions.LoadPage(jsonLink);
                if (response != "")
                {
                    MarketResponse resp = JsonConvert.DeserializeObject<MarketResponse>(response);
                    if (resp != null && resp.success)
                    {
                        if(resp.sog != null && resp.sog.Count > 0)
                        {
                            prefix = resp.price_prefix;
                            suffix = resp.price_suffix;
                            pd = new PriceDate(double.Parse(resp.sog[0][0], CultureInfo.InvariantCulture), DateTime.Now);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"{DateTime.Now}: {ex.Message}");
            }
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

        public void Dispose()
        {
            t.Enabled = false;
        }

        ~SteamMarketItem()
        {
#if DEBUG
            Logger.Write($"{itemName} has been deleted.");
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
                if (price != Convert.ToDouble(value))
                {
                    dbh.Insert($"insert into [Items] ([market_id], [dt], [price]) values ({id}, '{DateTime.Now}', '{value}')");
                }
                price = Convert.ToDouble(value);
                percentPrice = Convert.ToInt32((price - startPrice) / (startPrice / 100));
                PriceColor = GetColor(percentPrice);
                RaisePropertyChanged("Price");
                IReceivePrice = (price * multiplier).ToString($"{pricePreffix}0.00{priceSuffix}");
                RaisePropertyChanged("IReceivePrice");
            }
        }

        public string IReceivePrice { get; set; }

        public string TenMinutes
        {
            get { return $"{pricePreffix}{tenMinutes}{priceSuffix}({percentTenMinutes}%)"; }
            set
            {
                tenMinutes = Convert.ToDouble(value);
                percentTenMinutes = Convert.ToInt32((tenMinutes - price) / (price / 100));
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
                percentThirtyMinutes = Convert.ToInt32((thirtyMinutes - price) / (price / 100));
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
                percentOneHour = Convert.ToInt32((oneHour - price) / (price / 100));
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
            else if (dif < 10)
                return Brushes.White;
            else if (dif < 20)
                return Brushes.GreenYellow;
            else
                return Brushes.Pink;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string propertyName)
        {
            var handlers = PropertyChanged;

            handlers(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
