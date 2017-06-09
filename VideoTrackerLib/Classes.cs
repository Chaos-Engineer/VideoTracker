using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

// Class definitions to be inherited by plugins
namespace VideoTrackerLib
{
    [Serializable]
    public class VideoFile
    {
        public string episodeTitle;     // Episode title to display
        public string internalName;     // Internal reference to episode (e.g. filename or URL)
        public int season;              // Season number
        public int episode;             // Episode number
        public int special;             // Set to "1" if this is a post-season special
        public string key;              // Unique value, used to sort episodes
    }

    // Global Plugin Keys - Predefined keys for the pluginGlobalDictionary library
    public class gpk
    {
        // Group=PLUGINS
        public const string NAME = "name";
        public const string ADD = "add";
        public const string DESC = "desc";
        public const string FORCECONFIG = "forceconfig";
    }
    // Series Plugin Keys - Predefined keys for the pluginSeriesDictionary library
    public class spk
    {
        public const string TITLE = "title";
        public const string CURRENTVIDEO = "currentvideo";
    }

    // This class allows a WebBrowser control to be accessed from within a
    // BackgroundWorker thread. Normally this is not allowed because UI
    // controls can only be accessed from STA threads, and BackgroundWorkers
    // cannot be STA threads.
    //
    // The solution is to make the WebBrowser control part of the UI class,
    // and then access it using this wrapper class to call the UI thread's
    // dispatcher.
    //
    // The UI uses the pageAvailable event flag to signal that the page 
    // load is complete and that the BackgroundWorker can continue processing.
    //
    public class DynamicHtmlLoader : IDisposable
    {
        private DynamicHtmlLoaderDialog loader;

        public bool browserRequired = false;
        public HashSet<String> inProgressList = new HashSet<String>() { "DDoS protection by CloudFlare" };
        public HashSet<String> requiredList = new HashSet<String>();
        public int timeoutSeconds = 30;
        public WindowMode windowMode = WindowMode.Hidden;

        public DynamicHtmlLoader(DynamicHtmlLoaderDialog loader)
        {
            this.loader = loader;
        }

        public void Dispose()
        {
            loader.pageAvailable.Set(); // Release any pending threads.
        }

        public string Navigate(string url)
        {
            if (!this.browserRequired) {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "GET";
                    req.UserAgent = "Wget/1.16.1 (cygwin)";
                    req.Timeout = this.timeoutSeconds * 1000;
                    Stream s = req.GetResponse().GetResponseStream();
                    StreamReader sr = new StreamReader(s);
                    return sr.ReadToEnd();
                }
                catch (WebException ex)
                {
                    HttpWebResponse resp = (HttpWebResponse) ex.Response;
                    if (resp == null || resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        this.browserRequired = true;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Failed to load ")
                          .Append(url)
                          .Append(": (")
                          .Append((int)resp.StatusCode)
                          .Append(") ")
                          .Append(resp.StatusCode.ToString());
                        return (sb.ToString());
                    }
                }
            }

            loader.SetMode(inProgressList, requiredList, timeoutSeconds, windowMode);

            // Dispatch the request to the UI, and wait for it to complete.
            loader.Dispatcher.BeginInvoke(new Action(() => loader.Navigate(url)));
            loader.pageAvailable.WaitOne();
            return loader.html;
        }
    }
}