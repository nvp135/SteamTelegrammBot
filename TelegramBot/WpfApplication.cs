using System.Windows;

namespace TelegramBot
{
    public class WpfApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Call the OnStartup event on our base class
            base.OnStartup(e);

            // Create our MainWindow and show it
            MainWindow window = new MainWindow();
            window.Show();
        }

        public void Activate()
        {
            // Reactivate the main window
            MainWindow.Activate();
        }
    }
}
