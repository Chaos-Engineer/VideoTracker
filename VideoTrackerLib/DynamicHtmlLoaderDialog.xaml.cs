using mshtml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


//
// This is an automated web browser control. It has the ability to return the final
// HTML for pages generated via Javascript, and so is suitable for bypassing basic
// CloudFlare protection. CloudFlare captcha codes can be bypassed by loading the
// page in interactive mode and relying on direct user input.
//
// Methods:
// - Navigate(url) - navigates to the specified URL.
//
// Input properties:
// - timeoutSeconds - the timeout interval in seconds.
// - windowMode - the display mode:
//    - WindowMode.Hidden - hide the web browser window.
//    - WindowMode.Visible - Display the web browser window.
//    - WindowMode.Interactive - Display the web browser window. Disable timeout code.
// - inProgressList - A list of strings. If a loaded page contains one of these strings,
//      then wait for the page to reload (e.g. via a CloudFlare Javascript forward). This 
//      is implemented as a HashSet to prevent duplicates from being inserted.
// - requiredList - A list of strings. If the loaded page does not contain one of these
//      strings, then wait for the page to reload
//
// Output properties:
// - html - The page HTML
//
namespace VideoTrackerLib
{
    public enum WindowMode { Hidden, Visible, Interactive }
    public partial class DynamicHtmlLoaderDialog: Window, IDisposable
    {
        public static bool shutdownInProgress = false;

        public string html;
        private HashSet<String> inProgressList;
        private HashSet<String> requiredList;
        private int timeoutSeconds;
        private WindowMode windowMode;

        public AutoResetEvent pageAvailable = new AutoResetEvent(false);
        private bool initialized = false;
        private bool inUse = true;
        private DispatcherTimer dispatcherTimer;

        public DynamicHtmlLoaderDialog()
        {
            InitializeComponent();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += Browser_LoadCompleted;
        }

        public void SetMode(HashSet<String> inProgressList,
                        HashSet<String> requiredList,
                        int timeoutSeconds,
                        WindowMode windowMode)
        {
            this.inProgressList = inProgressList;
            this.requiredList = requiredList;
            this.timeoutSeconds = timeoutSeconds;
            this.windowMode = windowMode;
        }



        public void Navigate(String url)
        {
            try
            {
                dispatcherTimer.Interval = new TimeSpan(0, 0, timeoutSeconds);
                switch (windowMode)
                {
                    case WindowMode.Hidden:
                        this.ShowInTaskbar = false;
                        this.WindowState = WindowState.Minimized;
                        this.Visibility = Visibility.Hidden;
                        dispatcherTimer.Start();
                        break;
                    case WindowMode.Visible:
                        this.ShowInTaskbar = true;
                        this.WindowState = WindowState.Normal;
                        this.Visibility = Visibility.Visible;
                        dispatcherTimer.Start();
                        break;
                    case WindowMode.Interactive:
                        this.ShowInTaskbar = false;
                        this.WindowState = WindowState.Normal;
                        this.Visibility = Visibility.Visible;
                        break;
                }
                if (!this.IsActive)
                {
                    this.initialized = false;
                    this.Show();
                }
                this.browser.Navigate(url);

                if (!this.initialized)
                {
                    // Undocumented Windows feature to disable Javascript error
                    // messages within WebBrowser objects.
                    dynamic activeX = this.browser.GetType().InvokeMember("ActiveXInstance",
                             BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                             null, this.browser, new object[] { });
                    activeX.Silent = true;
                }
                this.initialized = true;
            }
            catch (Exception ex)
            {
                this.html = "Error in HtmlLoaderDialog:\n" + ex.ToString();
                this.initialized = false;
                this.pageAvailable.Set();
            }
        }

        // The page has finished loading. Check the contents to see if they're
        // what we're expecting. If not, then wait for the page to reload.
        // Otherwise, set the pageAvailable event to allow the Navigate() 
        // routine's caller to finish.
        private void Browser_LoadCompleted(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();
            HTMLDocument doc = (HTMLDocument)this.browser.Document;
            if (doc == null || !this.browser.IsLoaded)
            {
                this.html = "Operation timed out";
            }
            else
            {
                this.html = doc.body.outerHTML;
                foreach (string s in inProgressList)
                {
                    if (this.html.Contains(s)) return;
                }
                foreach (string s in requiredList)
                {
                    if (!this.html.Contains(s)) return;
                }
            }
            this.pageAvailable.Set();
        }

        public void Dispose()
        {
            this.inUse = false;
            this.Close();
        }

        // Don't let the user close the window while the application is running.
        // When the main application exists, it should set the shutdownInProgress
        // static flag.
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.inUse && !shutdownInProgress)
            {
                MessageBox.Show("Can't close active HtmlLoaderDialog window");
                e.Cancel = true;
            }
        }
    }
}