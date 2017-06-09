using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using VideoTrackerLib;

namespace VideoTracker
{
    public class PluginSeries : VideoSeries, IDisposable
    {
        public string pluginName;
        public string pluginFileName;
        public StringDictionary pluginSeriesDictionary = new StringDictionary();
        private StringDictionary pluginGlobalDictionary;

        private DynamicHtmlLoaderDialog dynamicHtmlLoaderDialog;
        [NonSerialized, XmlIgnore]
        public DynamicHtmlLoader dynamicHtmlLoader;

        private Window parentWindow;        // Used by plugins to set their forms' "Owner" field.
        private Plugin plugin = null;

        public PluginSeries()
        {
            dynamicHtmlLoaderDialog = new DynamicHtmlLoaderDialog();
            dynamicHtmlLoader = new DynamicHtmlLoader(dynamicHtmlLoaderDialog);
            return;
        }

        public PluginSeries(string pluginName, VideoTrackerData vtd)
        {
            dynamicHtmlLoaderDialog = new DynamicHtmlLoaderDialog();
            dynamicHtmlLoader = new DynamicHtmlLoader(dynamicHtmlLoaderDialog);

            this.pluginName = pluginName;
            this.pluginGlobalDictionary = vtd.globals[pluginName];
            this.plugin = new Plugin(pluginName, vtd);
            this.parentWindow = vtd.videoTrackerForm;
        }

        public override void EditForm(VideoTrackerData vtd)
        {
            if (!this.ConfigureSeries(vtd))
            {
                return;
            }
            // We always want an alert if there's an error here, so reset the timer. 
            VideoSeries.lastAlert = DateTime.Now.AddDays(-1);
            this.LoadSeries(this.pluginSeriesDictionary["title"], "", vtd);
        }

        public override void Play()
        {
            dynamic scope;
            // Initialize the Python runtime 
            try
            {
                scope = plugin.LoadPlugin();
            }
            catch (Exception ex)
            {
                ErrorDialog.Show("Unable to load Python file " + plugin.pluginFileName, ex.ToString());
                base.Play();
                return;
            }

            if (!scope.ContainsVariable("Play"))
            {
                base.Play();
                return;
            }
            try
            {
                if (!scope.Play(pluginGlobalDictionary, currentVideo.internalName)) base.Play();
            }
            catch (Exception ex)
            {
                ErrorDialog.Show("Unable to call Play() in " + plugin.pluginFileName, ex.ToString());
            }
        }

        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
        {
            dynamic scope;
            VideoTrackerData vtd = (VideoTrackerData)e.Argument;
            if (!vtd.globals[gdg.PLUGINS].ContainsKey(pluginName))
            {
                this.errorString = "Plugin '" + pluginName + "' is no longer registered.";
                return;
            }
            if (this.plugin == null)
            {
                this.plugin = new Plugin(pluginName, vtd);
            }
            // Initialize the Python runtime 
            try
            {
                scope = plugin.LoadPlugin();
            }
            catch (Exception ex)
            {
                this.errorString = "Unable to load Python file " + plugin.pluginFileName;
                this.detailString = ex.ToString();
                return;
            }

            // Get the global variables for this plug-in
            this.pluginGlobalDictionary = vtd.globals[pluginName];
 
            // Load the series episodes.
            try
            {
                object status;
                status = scope.LoadSeries(this.pluginGlobalDictionary, this.pluginSeriesDictionary,
                        this.dynamicHtmlLoader, out this.videoFiles);
                if (status == null)
                {
                    this.errorString = ""; // Success
                }
                else if (status is string)
                {
                    this.errorString = (string)status;
                }
                else if (status is List<string>)
                {
                    List<string> temp = (List<string>)status;
                    this.errorString = temp[0];
                    this.detailString = temp[1];
                }
                else
                {
                    ErrorDialog.Show("LoadSeries returns unexpected type " + status.GetType().ToString());
                }

                if (this.errorString != "")
                {
                    this.videoFiles.Clear();
                    return;
                }

                // The LoadSeries call can potentially change the series title 
                // from the user-specified string to the canonical title.
                this.seriesTitle = pluginSeriesDictionary[spk.TITLE];
            }
            catch (Exception ex)
            {
                this.videoFiles.Clear(); // Make sure error is reported if list was partially loaded.
                this.errorString = "Error calling LoadSeries in " + plugin.pluginFileName;
                this.detailString = ex.ToString();
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


        public bool ConfigureSeries(VideoTrackerData vtd)
        {
            dynamic scope;
            // Initialize the Python runtime
            if (plugin == null)
            {
                ErrorDialog.Show("No registered plugin for this program");
                return false;
            }

            try
            {
                using (new WaitCursor())
                {
                    scope = plugin.LoadPlugin();
                }
            }
            catch (Exception ex)
            {
                ErrorDialog.Show("Unable to load Python file " + plugin.pluginFileName, ex.ToString());
                return false;
            }
            // Get the local variables for this plugin
            try
            {
                if (!scope.ConfigureSeries(parentWindow, out this.pluginSeriesDictionary))
                {
                    // Operation cancelled - no need to report error
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorDialog.Show("Unable to call ConfigureSeries in " + plugin.pluginFileName, ex.ToString());
                return false;
            }

            seriesTitle = pluginSeriesDictionary[spk.TITLE];
            if (seriesTitle == "")
            {
                ErrorDialog.Show("ERROR: ConfigureSeries call in " + plugin.pluginFileName + " did not set the 'spk.TITLE' field");
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            dynamicHtmlLoader.Dispose();
            dynamicHtmlLoaderDialog.Dispose();
        }
    }

    //
    // PLUGIN class
    //
    // This class is used in three ways:
    // 1 - At registration time, the "Register" method is used to associate a file with the plugin and
    //     create the associated ScriptRuntime object. We need a different ScriptRuntime for each plugin
    //     to account for the different library paths.
    // 2 - During global configuration, the "ConfigureGlobals()" method updates the global variables associated 
    //     with the plugin. They are not stored locally to the class, they're in the VideoTrackerData dictionary.
    // 3 - An individual video series can call the GetScope() method and use this to call the "ConfigureLocals()"
    //     "LoadSeriesAsync()" and "Play()" methods in the plug-in file.
    //
    // ScriptRuntime objects can be shared among all threads that use a plug-in.
    // ScopeRuntime objects are not thread-safe, so a new object is needed for each thread.
    //
    public class Plugin
    {
        private const string NEW = "**NEW**";
        public string pluginName = NEW;
        public string pluginFileName;

        private Window parentWindow;
        private dynamic scope = null;
        private string pythonLib = "";

        // A single plug-in file (and the associated ScriptRuntime) can be shared between multiple instances.
        // Use a static dictionary indexed on the plug-in name to track that information.
        //
        // This section of code must be synched: If there are multiple series that use the same plugin,
        // then there's can be a race condition on updates to pluginDictionary[pluginName].

        private static readonly object pluginDictionaryLock = new object();
        private static Dictionary<string, PluginData> pluginDictionary = new Dictionary<string, PluginData>();

        // Creates an empty plug-in. This will be initialized later by the Register call.
        public Plugin()
        {
            return;
        }

        // Gets a previously-registered plugin.
        public Plugin(string pluginName, VideoTrackerData vtd)
        {
            this.pluginName = pluginName;
            this.parentWindow = vtd.videoTrackerForm;
            this.pluginFileName = vtd.globals[gdg.PLUGINS][pluginName];
            this.pythonLib = vtd.globals[gdg.PLUGIN_GLOBALS][gdk.PYTHONPATH];
        }

        // Registers a new plug-in
        public bool Register(string pluginFileName, VideoTrackerData vtd)
        {
            lock (pluginDictionaryLock)
            {
                this.pluginFileName = pluginFileName;
                pluginDictionary[NEW] = new PluginData(pluginFileName, vtd.globals[gdg.PLUGIN_GLOBALS][gdk.PYTHONPATH]);

                // Load the plug-in file and call the "Register" method.

                try
                {
                    using (new WaitCursor())
                    {
                        LoadPlugin();
                    }
                }
                catch (IronPython.Runtime.Exceptions.ImportException ex)
                {
                    ErrorDialog.Show("Import exception: This usually means that IronPython has not been " +
                        "installed on your system (download it from http://ironpython.net), or that the " +
                        "IronPython library path on this page is incorrect. The other possibility is that " +
                        "this plug-in relies on a Python library module that is not installed on your system\n\n"
                        + ex.ToString());
                }
                catch (Exception ex)
                {
                    ErrorDialog.Show("Unable to load Python file " + pluginFileName + ":\n" + ex.ToString());
                    return false;
                }


                StringDictionary pluginRegisterDictionary = new StringDictionary();
                try
                {
                    using (new WaitCursor())
                    {
                        scope.Register(pluginRegisterDictionary);
                    }
                }
                catch (Exception ex)
                {
                    pluginDictionary.Remove(NEW);
                    ErrorDialog.Show("Error calling Register in " + pluginFileName + ":\n" + ex.ToString());
                    return false;
                }

                // Validate the output of the "Register" method and update the dictionaries.
                if (pluginRegisterDictionary[gpk.NAME] == "" || pluginRegisterDictionary[gpk.ADD] == ""
                    || pluginRegisterDictionary[gpk.DESC] == "")
                {
                    pluginDictionary.Remove(NEW);
                    ErrorDialog.Show("Invalid Register routine in " + pluginFileName +
                            ":\nMust set values for 'name', 'add', and 'desc' arguments");
                    return false;
                }

                pluginName = pluginRegisterDictionary[gpk.NAME];
                if (!Regex.IsMatch(pluginName, @"^[a-zA-Z0-9]+$"))
                {
                    ErrorDialog.Show("NAME field '" + pluginName + "' contains non-alphanumeric characters");
                    return false;
                }

                if (vtd.globals.ContainsKey(pluginName))
                {
                    ErrorDialog.Show(pluginName + " is already registered.");
                    pluginDictionary.Remove(NEW);
                    return false;
                }


                pluginDictionary[pluginName] = pluginDictionary[NEW];
                pluginDictionary.Remove(NEW);

                vtd.globals[gdg.PLUGINS][pluginName] = pluginFileName;
                foreach (string key in pluginRegisterDictionary.Keys)
                {
                    if (key != gpk.NAME)
                    {
                        vtd.globals[pluginName][key] = pluginRegisterDictionary[key];
                    }
                }

                // If the FORCECONFIG option is set, then run the ConfigureGlobals method.
                // The plug-in will still be considered successfully registered even if this 
                // operation fails.
                if (pluginRegisterDictionary[gpk.FORCECONFIG] != "")
                {
                    string errorString, detailString;
                    if (!ConfigureGlobals(vtd, out errorString, out detailString))
                    {
                        if (errorString != "") { ErrorDialog.Show(errorString, detailString); }
                    }
                }
                return true;
            }
        }


        // If the ScriptRuntime for this plug-in hasn't been created, or if the source file has
        // changed, then create a new ScriptRuntime.
        //
        // If the ScriptRuntime has been changed or if the "scope" property for this instance
        // isn't defined, then create a new scope.
        //
        public dynamic LoadPlugin()
        {
            lock (pluginDictionaryLock)
            {

                if (!pluginDictionary.ContainsKey(pluginName))
                {
                    pluginDictionary[pluginName] = new PluginData(pluginFileName, pythonLib);
                }

                DateTime lastMod = File.GetLastWriteTime(pluginFileName);
                if (scope == null)
                {
                    this.scope = pluginDictionary[pluginName].runtime.UseFile(pluginFileName);
                }
                else if (pluginDictionary[pluginName].fileMod < lastMod)
                {
                    using (new TransientMessage(this.parentWindow, 
                        Path.GetFileName(pluginFileName) + " has been modified. Reloading."))
                    {
                        pluginDictionary[pluginName] = new PluginData(pluginFileName, pythonLib);
                        this.scope = pluginDictionary[pluginName].runtime.UseFile(pluginFileName);
                    }
                }

            }
            return this.scope;

        }

        public bool ConfigureGlobals(VideoTrackerData vtd, out string errorString, out string detailString)
        {
            StringDictionary pluginGlobalDictionary = vtd.globals[pluginName];


            errorString = detailString = "";
            // Initialize the Python runtime 
            try
            {
                using (new WaitCursor())
                {
                    LoadPlugin();
                }
            }
            catch (Exception ex)
            {
                errorString = "Unable to load Python file " + pluginFileName;
                detailString = ex.ToString();
                return false;
            }
            // Get the global variables for this plugin
            if (!scope.ContainsVariable("ConfigureGlobals"))
            {
                errorString = "Configuration not possible.\nNo ConfigureGlobals entry point in " + pluginFileName;
                return false;
            }

            try
            {
                pluginGlobalDictionary = vtd.globals[pluginName];
                if (!this.scope.ConfigureGlobals(parentWindow, out pluginGlobalDictionary))
                {
                    // Operation cancelled - no need to report error
                    errorString = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorString = "Unable to call ConfigureGlobals in " + pluginFileName;
                detailString = ex.ToString();
                return false;
            }
            vtd.globals[pluginName] = pluginGlobalDictionary;
            return true;
        }



        private class PluginData
        {
            public string file;
            public ScriptRuntime runtime;
            public DateTime fileMod;
            public PluginData(string file, string lib)
            {
                Dictionary<string, object> options = new Dictionary<string, object>();
                options["Debug"] = true;
                ScriptRuntime python = Python.CreateRuntime(options);

                // Add the script directory to the path list.
                ScriptEngine pythonEngine = python.GetEngine("Python");
                ICollection<string> paths = pythonEngine.GetSearchPaths();
                paths.Add(Path.GetDirectoryName(file));
                if (lib != "")
                {
                    paths.Add(lib);
                }
                pythonEngine.SetSearchPaths(paths);

                this.file = file;
                this.fileMod = File.GetLastWriteTime(file);
                this.runtime = python;
            }
        }
    }
}
