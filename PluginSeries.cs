using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace VideoTracker
{
    public class PluginSeries : VideoSeries
    {
        public StringDictionary pluginSeriesDictionary = new StringDictionary();

        public string pluginName;

        private dynamic runtime;
        private static Dictionary<string, dynamic> pluginRuntimeDictionary = new Dictionary<string, dynamic>();

        public PluginSeries()
        {
            return;
        }

        public PluginSeries(string pluginName, VideoTrackerData vtd)
        {
            this.pluginName = pluginName;
        }

        public static bool Register(string pluginFile, VideoTrackerData vtd, out string pluginName)
        {
            dynamic runtime;
            const string NEW = "**NEW**";
            pluginName = "UNDEFINED";

            try
            {
                runtime = PluginSeries.CreateRuntime(NEW, pluginFile);
            }
                catch (Exception ex)
            {
                MessageBox.Show("Unable to load Python file " + pluginFile + ":\n" + ex.ToString());
                return false;
            }

            StringDictionary pluginRegisterDictionary = new StringDictionary();
            try {
                runtime.Register(pluginRegisterDictionary);
            } catch (Exception ex) {
                pluginRuntimeDictionary.Remove(NEW);
                MessageBox.Show("Error calling Register in " + pluginFile + ":\n" + ex.ToString());
                return false;
            }

            if (pluginRegisterDictionary[gpk.NAME] == "" || pluginRegisterDictionary[gpk.ADD] == ""
                || pluginRegisterDictionary[gpk.DESC] == "") 
            {
                pluginRuntimeDictionary.Remove(NEW);
                MessageBox.Show("Invalid Register routine in " + pluginFile + 
                        ":\nMust set values for 'name', 'add', and 'desc' arguments");
                return false;
            }

            pluginName = pluginRegisterDictionary[gpk.NAME];

            pluginRuntimeDictionary[pluginName] = pluginRuntimeDictionary[NEW];
            pluginRuntimeDictionary.Remove(NEW);

            vtd.globals[gdg.PLUGINS][pluginName] = pluginFile;
            foreach (string key in pluginRegisterDictionary.Keys)
            {
                if (key != gpk.NAME)
                {
                    vtd.globals[pluginName][key] = pluginRegisterDictionary[key];
                }
            }
            return true;

        }

        
        private static dynamic CreateRuntime(string name, string file)
        {
            if (!PluginSeries.pluginRuntimeDictionary.ContainsKey(name))
            {
                Dictionary<string, object> options = new Dictionary<string, object>();
                options["Debug"] = true;
                ScriptRuntime python = Python.CreateRuntime(options);
                pluginRuntimeDictionary[name] = python.UseFile(file);
            }
            return pluginRuntimeDictionary[name];
        }


        public override void EditForm(VideoTrackerData vtd)
        {
            string errorString;
            if (!this.PerformConfiguration(vtd, out errorString))
            {
                if (errorString != "") MessageBox.Show(errorString);
                return;
            }
            // We always want an alert if there's an error here, so reset the timer. 
            VideoSeries.lastAlert = DateTime.Now.AddDays(-1);
            this.LoadFiles(this.pluginSeriesDictionary["title"], "", vtd);
        }

        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
        {
            VideoTrackerData vtd = (VideoTrackerData)e.Argument;
            if (!vtd.globals[gdg.PLUGINS].ContainsKey(pluginName)) {
                errorString = "Plugin '" + pluginName + "' is no longer registered.";
                return;
            }
            string pluginFile = vtd.globals[gdg.PLUGINS][pluginName];


            // Initialize the Python runtime 
            try
            {
                runtime = PluginSeries.CreateRuntime(pluginName, pluginFile);
            }
            catch (Exception ex)
            {
                errorString = "Unable to load Python file " + pluginFile + ":\n" + ex.ToString();
                return;
            }

            // Get the global variables for this plug-in
            StringDictionary pluginGlobalDictionary = vtd.globals[pluginName];

            // Load the series episodes.
            try
            {
                if (!runtime.LoadSeries(pluginGlobalDictionary, this.pluginSeriesDictionary,
                        out this.videoFiles)) return;
            }
            catch (Exception ex)
            {
                this.videoFiles.Clear(); // Make sure error is reported if list was partially loaded.
                errorString = "Error calling LoadSeries in " + pluginFile + ":\n" + ex.ToString();
                return;
            }

            // If the CURRENTVIDEO entry is set, then set the current video and then clear the
            // entry. (This is so that we only allow the plug-in to change the current video
            // when these series is re-configured. Afterwards, the current video will stay
            // as the last video watched.
            string currentVideoKey = this.pluginSeriesDictionary[spk.CURRENTVIDEO];
            foreach (VideoFile v in this.videoFiles.Values)
            {
                if (v.episodeTitle == currentVideoKey)
                {
                    this.currentVideo = v;
                }
            }
            this.pluginSeriesDictionary[spk.CURRENTVIDEO] = "";

            return;
        }

        public static bool PerformGlobalConfiguration(string pluginName, VideoTrackerData vtd, out string errorString)
        {
            string pluginFile = vtd.globals[gdg.PLUGINS][pluginName];
            StringDictionary pluginGlobalDictionary = vtd.globals[pluginName];
            dynamic runtime;

            errorString = "";
            // Initialize the Python runtime 
            try
            {
                runtime = PluginSeries.CreateRuntime(pluginName, pluginFile);
            }
            catch (Exception ex)
            {
                errorString = "Unable to load Python file " + pluginFile + ":\n" + ex.ToString();
                return false;
            }

            // Get the local variables for this plugin
            try
            {
                pluginGlobalDictionary = vtd.globals[pluginName];
                if (!runtime.ConfigureGlobals(out pluginGlobalDictionary))
                {
                    // Operation cancelled - no need to report error
                    errorString = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorString = "Unable to call ConfigureGlobals in " + pluginFile + ":\n" + ex.ToString();
                return false;
            }
            vtd.globals[pluginName] = pluginGlobalDictionary;
            return true;
        }

        public bool PerformConfiguration(VideoTrackerData vtd, out string errorString)
        {
            string pluginFile = vtd.globals[gdg.PLUGINS][pluginName]; 
            errorString = "";
            // Initialize the Python runtime 
            try
            {
                runtime = PluginSeries.CreateRuntime(pluginName, pluginFile);
            }
            catch (Exception ex)
            {
                errorString = "Unable to load Python file " + pluginFile + ":\n" + ex.ToString();
                return false;
            }

            // Get the local variables for this plugin
            try
            {
                if (!runtime.ConfigureSeries(out this.pluginSeriesDictionary))
                {
                    // Operation cancelled - no need to report error
                    errorString = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorString = "Unable to call ConfigureSeries in " + pluginFile + ":\n" + ex.ToString();
                return false;
            }

            seriesTitle = pluginSeriesDictionary[spk.TITLE];
            if (seriesTitle == "")
            {
                errorString = "ERROR: ConfigureSeries call in " + pluginFile + "\n" + "did not set the 'spk.TITLE' field";
                return false;
            }
            return true;
        }

    }
}
