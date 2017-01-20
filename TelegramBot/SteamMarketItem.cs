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
    class SteamMarketItem : Grid
    {
        TextBlockValues textblockValues;
        List<PriceDate> lPrices;
        readonly string jsonLink, itemName;

        public SteamMarketItem(List<string> itemInfo)
        {
            border.Child = this;
            this.Width = 140;
            this.Height = 100;
            t.Elapsed += t_Tick;

            textblockValues = new TextBlockValues();
            lPrices = new List<PriceDate>();

            Style tbStyle = new Style(typeof(TextBlock));
            tbStyle.Setters.Add(new Setter { Property = Control.FontSizeProperty, Value = 10.0 });
            tbStyle.Setters.Add(new Setter { Property = Control.FontFamilyProperty, Value = new FontFamily("Consolas") });
            tbStyle.Setters.Add(new Setter { Property = Control.HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
            tbStyle.Setters.Add(new Setter { Property = Control.VerticalAlignmentProperty, Value = VerticalAlignment.Center });

            tbPrice.Style = tbStyle;
            tb1.Style = tbStyle;
            tb2.Style = tbStyle;
            tb3.Style = tbStyle;

            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(60, GridUnitType.Pixel)
            });
            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(80, GridUnitType.Pixel)
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

            SetRow(tbPrice, 2);
            gPrices.Children.Add(tbPrice);

            SetRow(tb1, 3);
            gPrices.Children.Add(tb1);

            SetRow(tb2, 4);
            gPrices.Children.Add(tb2);

            SetRow(tb3, 5);
            gPrices.Children.Add(tb3);

            itemName = itemInfo[2];
            tbItemName.Text = itemName;
            ico.Source = HelpFunctions.GetBitmap(itemInfo[1]);
            jsonLink = itemInfo[0];

            DataContext = this;
            tbPrice.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Path = new PropertyPath("Price"),
                Source = textblockValues
            });
            tb1.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Path = new PropertyPath("TenMinutes"),
                Source = textblockValues
            });
            tb2.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Path = new PropertyPath("ThirtyMinutes"),
                Source = textblockValues
            });
            tb3.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Path = new PropertyPath("OneHour"),
                Source = textblockValues
            });
            tbPrice.SetBinding(TextBlock.BackgroundProperty, new Binding()
            {
                Path = new PropertyPath("PriceColor"),
                Source = textblockValues
            });
            tb1.SetBinding(TextBlock.BackgroundProperty, new Binding()
            {
                Path = new PropertyPath("TenMinutesColor"),
                Source = textblockValues
            });
            tb2.SetBinding(TextBlock.BackgroundProperty, new Binding()
            {
                Path = new PropertyPath("ThirtyMinutesColor"),
                Source = textblockValues
            });
            tb3.SetBinding(TextBlock.BackgroundProperty, new Binding()
            {
                Path = new PropertyPath("OneHourColor"),
                Source = textblockValues
            });

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

        private void t_Tick(object source, ElapsedEventArgs e)
        {
            PriceDate res; string prefix, suffix;
            CheckPrice(out res, out prefix, out suffix);
            if (res != null)
            {
                lPrices.Insert(0, res); 
            }
            UpdatePrices();
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
            catch (Exception)
            {

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

        TextBlock tbItemName = new TextBlock()
        {
            FontSize = 8,
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        TextBlock tbPrice = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
        };

        TextBlock tb1 = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
        };

        TextBlock tb2 = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
        };

        TextBlock tb3 = new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap
        };

        Image ico = new Image();

        Button bDelete = new Button()
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
            Interval = 30 * 1000,
            Enabled = false
        };
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
        public double startPrice = 0.0;
        public string pricePreffix;
        public string priceSuffix;

        private double price = 0.0;
        private double tenMinutes = 0.0;
        private double thirtyMinutes = 0.0;
        private double oneHour = 0.0;

        private Brush priceColor = Brushes.White;
        private Brush tenMinutesColor = Brushes.White;
        private Brush thirtyMinutesColor = Brushes.White;
        private Brush oneHourColor = Brushes.White;

        public string Price
        {
            get { return $"{pricePreffix}{price}{priceSuffix}"; }
            set
            {
                double x = Convert.ToDouble(value);
                PriceColor = GetColor(Convert.ToInt32((x - startPrice) / (startPrice / x)));
                price = Convert.ToDouble(value);
                RaisePropertyChanged("Price");
            }
        }

        public string TenMinutes
        {
            get { return $"{pricePreffix}{tenMinutes}{priceSuffix}"; }
            set
            {
                double x = Convert.ToDouble(value);
                TenMinutesColor = GetColor(Convert.ToInt32((x - price) / (price / x)));
                tenMinutes = Convert.ToDouble(value);
                RaisePropertyChanged("TenMinutes");
            }
        }

        public string ThirtyMinutes
        {
            get { return $"{pricePreffix}{thirtyMinutes}{priceSuffix}"; }
            set
            {
                double x = Convert.ToDouble(value);
                ThirtyMinutesColor = GetColor(Convert.ToInt32((x - price) / (price / x)));
                thirtyMinutes = Convert.ToDouble(value);
                RaisePropertyChanged("ThirtyMinutes");
            }
        }

        public string OneHour
        {
            get { return $"{pricePreffix}{oneHour}{priceSuffix}"; }
            set
            {
                double x = Convert.ToDouble(value);
                OneHourColor = GetColor(Convert.ToInt32((x - price) / (price / x)));
                oneHour = Convert.ToDouble(value);
                RaisePropertyChanged("OneHour");
            }
        }

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
            else if (dif < 5)
                return Brushes.White;
            else if (dif < 10)
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
