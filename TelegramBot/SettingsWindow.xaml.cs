using System;
using System.Windows;

namespace TelegramBot
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            this.DataContext = settings;
        }

        ~SettingsWindow()
        {
            Console.WriteLine("Settings deleted");
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.SaveSettingsToFile();
        }
    }
}
