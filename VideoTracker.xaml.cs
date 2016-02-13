using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace VideoTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
     
        static public object writeInProgress = new Object();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                string launchFile = "";
                if (e.Args.Count() > 0)
                {
                    launchFile = e.Args[0];
                }
                VideoTrackerForm videoTrackerForm = new VideoTrackerForm(launchFile);
                videoTrackerForm.Show();
            }
            catch (Exception ex)
            {
                App.ErrorBox("Error: Unhandled exception\n" + ex.ToString());
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Don't exit while we're writing to the data file.
            lock (App.writeInProgress)
            {
                return;
            }
        }

        // Global method for error logging.
        public static void ErrorBox(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
