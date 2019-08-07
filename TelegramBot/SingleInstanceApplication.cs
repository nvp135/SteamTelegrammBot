using System;
using Microsoft.VisualBasic.ApplicationServices;

namespace TelegramBot
{
    public class StartupClass
    {
        [STAThread]
        static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }

    // WindowsFormsApplicationBase из псборки Microsoft.VisualBasic
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private WpfApplication _application;
        private System.Collections.ObjectModel.ReadOnlyCollection<string> _commandLine;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            // First time _application is launched
            _commandLine = eventArgs.CommandLine;
            _application = new WpfApplication();
            _application.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            _commandLine = eventArgs.CommandLine;
            _application.Activate();
        }
    }
}
