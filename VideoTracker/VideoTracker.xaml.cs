using Ookii.Dialogs.Wpf;
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
        static public Window VideoTrackerFormWindow; // For App.Errorbox()
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
        private static TaskDialog dialog;

        public static void ErrorBox(string message)
        {
            ErrorBox(message, null);
        }
        public static void ErrorBox(string message, string details)
        {
            // Using Ookii task dialog
            dialog = new TaskDialog();
            dialog.MainIcon = TaskDialogIcon.Error;
            dialog.WindowTitle = "Error";
            //dialog.MainInstruction = message;
            dialog.Content = message; 
            dialog.ExpandedInformation = details; 
            //dialog.Footer = "Task Dialogs support footers and can even include <a href=\"http://www.ookii.org\">hyperlinks</a>.";
            //dialog.FooterIcon = TaskDialogIcon.Information;
            dialog.EnableHyperlinks = true;
            TaskDialogButton okButton = new TaskDialogButton(ButtonType.Ok);
            dialog.Buttons.Add(okButton);
            //dialog.HyperlinkClicked += new EventHandler<HyperlinkClickedEventArgs>(TaskDialog_HyperLinkClicked);
            //TaskDialogButton button = dialog.ShowDialog(this);;
            dialog.ShowDialog(VideoTrackerFormWindow);

            //MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
