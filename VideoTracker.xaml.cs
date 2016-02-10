using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VideoTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
                MessageBox.Show("Error: Unhandled exception\n" + ex.ToString());
            }
        }


    }
}
