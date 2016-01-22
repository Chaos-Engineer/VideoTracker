using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Windows.Threading;

namespace VideoTracker
{
    public partial class VideoTrackerForm : Window
    {
        private string configFile;
        private bool configFileValid = false;
        private int numPanels;
        private long workerThreads; // Interlocked variables must be long
        private VideoTrackerData videoTrackerData;

        private int pluginsLoaded = 0;

        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;

        public VideoTrackerForm(string launchFile)
        {
            InitializeComponent();
            string file = "UNDEFINED";
            videoTrackerData = new VideoTrackerData(this);

            // Initialize dialog boxes
            openFileDialog = new OpenFileDialog();
            this.openFileDialog.DefaultExt = "vtr";
            this.openFileDialog.Filter = "VTR files|*.vtr|All files|*.*";

            saveFileDialog = new SaveFileDialog();
            this.saveFileDialog.DefaultExt = "vtr";
            this.saveFileDialog.Filter = "VTR files|*.vtr|All files|*.*";

            // Load the default file if a file wasn't specified on the command line.
            if (launchFile.Equals(""))
            {
                file = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["DefaultFilePath"]);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file));
            }
            else
            {
                file = launchFile;
            }
            this.configFile = file;
        }

        public new void Show() {
            // Create the VTR file if it doesn't already exist and save the default global settings. Otherwise
            // open the existing VTR file.
            base.Show();
            if (!File.Exists(this.configFile))
            {
                using (File.Create(this.configFile))
                {
                    // Null body - to allow handle to get disposed.
                }
                SaveData(this.configFile);
            }
            else
            {
                LoadData(this.configFile);
            }
        }

        private void loadMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                configFile = openFileDialog.FileName;
                LoadData(this.configFile);
            }
        }

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == true)
            {
                configFile = saveFileDialog.FileName;
                SaveData(configFile);
            }
        }

        private void saveMenuItem_Click(object sender, EventArgs e)
        {

            SaveData(configFile);
        }

        private void autoSaveMenuItem_Click(object sender, EventArgs e)
        {
            // Toggle value.
            videoTrackerData.globals.Set(gdg.MAIN, gdk.AUTOSAVE,
                !videoTrackerData.globals.GetBool(gdg.MAIN, gdk.AUTOSAVE));
            autoSaveMenuItem.IsChecked = videoTrackerData.globals.GetBool(gdg.MAIN, gdk.AUTOSAVE);
            if (autoSaveMenuItem.IsChecked)
            {
                MessageBox.Show("Autosave enabled");
            }
            else
            {
                MessageBox.Show("Autosave disabled");
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Program is exiting - save current state before exiting.
        private void VideoTrackerForm_FormClosing(object sender, CancelEventArgs e)
        {
            CheckAutoSave();
        }

        //
        // Open up a New Series dialog. Do not check status here; the operation
        // completes asynchronously.
        //
        private void addVideoFileMenuItem_Click(object sender, EventArgs e)
        {
            using (FileVideoSeriesForm vsf = new FileVideoSeriesForm(videoTrackerData))
            {
                vsf.ShowDialog();
            }
        }

        private void addAmazonVideoOnDemandMenuItem_Click(object sender, EventArgs e)
        {
            AmazonVideoSeriesForm vsf = new AmazonVideoSeriesForm(videoTrackerData);
            vsf.ShowDialog();
        }


        private void addCrunchyRollVideoMenuItem_Click(object sender, EventArgs e)
        {
            using (CrunchyRollVideoSeriesForm csf = new CrunchyRollVideoSeriesForm(videoTrackerData))
            {
                csf.ShowDialog();
            }
        }

        private void refreshMenuItem_Click(object sender, EventArgs e)
        {
            LoadAllSeries();
        }

        // WPF key bindings are a pain to set up. Handle hot keys through the KeyDown event
        // instead.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) {
                LoadAllSeries();
                return;
            }
        }

        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            using (SettingsForm sf = new SettingsForm(videoTrackerData))
            {
                sf.ShowDialog();
            }
            CheckAutoSave();
        }

        public void AddTitle(VideoPlayerPanel panel, VideoSeries vs)
        {
            // AddTitle can be called from "Add new program" or from "File Load". If it's
            // called from "File Load", then the program is already in videoSeriesList and 
            // doesn't need to be added again.
            if (!this.videoTrackerData.videoSeriesList.Contains(vs))
            {
                this.videoTrackerData.videoSeriesList.Add(vs);
            }

            this.numPanels++;
            this.mainPanel.Children.Add(panel);
            //this.ResizeMainPanel();
            //this.CheckAutoSave();
        }

        public void DeleteTitle(VideoPlayerPanel panel, VideoSeries vs)
        {
            this.numPanels--;
            this.videoTrackerData.videoSeriesList.Remove(vs);
            this.mainPanel.Children.Remove(panel);
            this.ResizeMainPanel();
            this.CheckAutoSave();
        }

        // Remove a program from the list and insert it at a different
        // position.
        public void MoveTitle(int source, int dest)
        {
            if (source == dest) return;
            if (source < 0 || dest < 0) return;
            if (source >= this.numPanels || dest >= this.numPanels) return;

            UIElementCollection cc = this.mainPanel.Children;
            UIElement c = cc[source];
            cc.RemoveAt(source);
            cc.Insert(dest, c);

            List<VideoSeries> vsl = this.videoTrackerData.videoSeriesList;
            VideoSeries item = vsl[source];
            vsl.RemoveAt(source);
            vsl.Insert(dest, item);

            this.CheckAutoSave();
        }


        public void ResizeMainPanel()
        {
            if (this.numPanels == 0)
            {
                blankLabel.Visibility = Visibility.Visible;
            }
            else
            {
                blankLabel.Visibility = Visibility.Collapsed;
            }

            // Find the maximum of the requiredWidths of the panels, and set each panel
            // to that width.
            double panelWidth = 0;
            foreach (VideoPlayerPanel vp in mainPanel.Children)
            {  
                panelWidth = Math.Max(panelWidth, vp.requiredSize.Width);
            }
            foreach (VideoPlayerPanel vp in mainPanel.Children)
            {
                vp.Width = vp.Margin.Left + panelWidth + vp.Margin.Right;
            }

            // This "ConvertToInt" call assigns a default value of 1 if columns is currently undefined.
            int columns = videoTrackerData.globals.GetInt(gdg.MAIN, gdk.COLUMNS, 1);
            if (this.mainPanel.Columns != columns)
            {
                this.mainPanel.Columns = columns;
            }
        }


        // Autosave the file if it's allowed:
        // - No "Add Series" threads can currently be running.
        // - Autosave must be enabled.
        // - The configFileValid field must indicate that the configuration file loaded
        //   without errors.
        public void CheckAutoSave()
        {
            // Note: workerThreads can be updated asynchronously.
            if (Interlocked.Read(ref workerThreads) == 0
                && videoTrackerData.globals.GetBool(gdg.MAIN, gdk.AUTOSAVE, "true") // Default to true
                && configFileValid)
            {
                SaveData(configFile);
            }
        }

        private void LoadData(string file)
        {
            EnableFileOperations(false);
            try
            {
                using (Stream stream = new FileStream(file, FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VideoTrackerData));
                    videoTrackerData = (VideoTrackerData)serializer.Deserialize(stream);
                    videoTrackerData.videoTrackerForm = this;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to load " + file + "\n" + e.ToString() +
                    "\n\nAuto-save will not be performed until configuration is saved manually");
                EnableFileOperations(true);
                configFileValid = false;
                return;
            }
            // File was successfully loaded; create panels and load videos.
            mainPanel.Children.Clear();
            LoadAllSeries();

            // Initialize properties
            this.configFile = file;
            this.configFileValid = true;
            autoSaveMenuItem.IsChecked = videoTrackerData.globals.GetBool(gdg.MAIN, gdk.AUTOSAVE);

            // Add plugins to Edit menu on the first run.
            if (pluginsLoaded == 0)
            {
                foreach (string key in videoTrackerData.globals[gdg.PLUGINS].Keys)
                {
                    InsertPluginMenuItem(key, videoTrackerData.globals[key][gpk.ADD]);
                }
            }

        }

        private void LoadAllSeries()
        {
            // Don't allow this routine to be called if a previous call is still in progress.
            // The load operation will complete asynchronously, so the asynchronous
            // thread must decrement workerThreads and call EnableFileOperations 
            // on completion.
            if (Interlocked.Read(ref workerThreads) != 0)
            {
                return;
            }

            foreach (VideoSeries vs in videoTrackerData.videoSeriesList)
            {
                // Check for corrupted video in configuration file. This
                // shouldn't happen unless there was a bug adding a series.
                if (vs.seriesTitle == "") { vs.seriesTitle = "UNKNOWN"; }
                if (vs.currentVideo == null)
                {
                    vs.currentVideo = new VideoFile();
                    vs.currentVideo.key = vs.currentVideo.episodeTitle = "NO FILE FOUND";
                    MessageBox.Show("Invalid data for title " + vs.seriesTitle + ". " +
                        "\n\nAuto-save will not be performed until configuration is saved manually");
                    configFileValid = false;
                }
                vs.LoadFiles(vs.seriesTitle, vs.currentVideo.episodeTitle, videoTrackerData);
            }

        }

        private void SaveData(string file)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(VideoTrackerData));
                using (Stream stream = new FileStream(file + ".tmp", FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read))
                {
                    serializer.Serialize(stream, videoTrackerData);
                }
                File.Copy(file + ".tmp", file, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't save " + file + ".\n" + ex.ToString());
                return;
            }
            configFileValid = true;
        }

        public void RegisterWorkerThread()
        {
            Interlocked.Increment(ref this.workerThreads);
        }

        public void WorkerThreadComplete()
        {
            long threads = Interlocked.Decrement(ref this.workerThreads);
            if (threads == 0)
            {
                EnableFileOperations(true);
                CheckAutoSave();
            }
            ResizeMainPanel();
        }

        public void EnableFileOperations(bool flag)
        {
            loadMenuItem.IsEnabled = flag;
            saveMenuItem.IsEnabled = flag;
            saveAsMenuItem.IsEnabled = flag;
            autoSaveMenuItem.IsEnabled = flag;
        }

        public void InsertPluginMenuItem(string name, string add)
        {


            MenuItem plugin = new MenuItem();
            plugin.Click += (sender, e) => this.PluginLoader(name);
            plugin.Header = add;
            pluginsLoaded++;
            
            int index = 0;
            foreach (Control item in editMenuItem.Items)
            {
                index++;
                if (item is Separator)
                {
                    if (pluginsLoaded == 1) // Add a new Separator bar after the first plug-in
                    {
                        editMenuItem.Items.Insert(index, new Separator());
                    }
                    editMenuItem.Items.Insert(index, plugin);
                    break;
                }

            }
        }
        public void DeletePluginMenuItem(string name)
        {
            foreach (MenuItem item in editMenuItem.Items)
            {
                if (item.Name == name) editMenuItem.Items.Remove(item);
                break;
            }
        }

        // The PluginSeries constructor attaches the new object to videoTrackerData,
        // so it will remain in-scope after this routine exits.
        public void PluginLoader(string name)
        {
            string file = videoTrackerData.globals[gdg.PLUGINS][name];
            PluginSeries ps = new PluginSeries(name, videoTrackerData);
            string errorString;
            if (!ps.ConfigureSeries(videoTrackerData, out errorString))
            {
                if (errorString != "") MessageBox.Show(errorString);
                return;
            }
            ps.LoadFiles(ps.pluginSeriesDictionary["title"], "", videoTrackerData);
        }


        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("VideoTracker\n" +
                "Build V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n" +
                "Copyright (c) 2015 Extraordinary Popular Delusions",
                "About VideoTracker");
        }
    }

    [Serializable]
    public class VideoTrackerData
    {

        // Global settings for different VideoSeries classes. We declare them here so that
        // they can be serialized in the configuration file.
        //

        public SerializableGroupDictionary globals;
        public List<VideoSeries> videoSeriesList;

        [XmlIgnore, NonSerialized]
        public VideoTrackerForm videoTrackerForm;

        public VideoTrackerData()
        {
            return;
        }

        public VideoTrackerData(VideoTrackerForm vtf)
        {
            this.videoTrackerForm = vtf;
            this.videoSeriesList = new List<VideoSeries>();
            this.globals = new SerializableGroupDictionary();
            this.globals.Set(gdg.MAIN, gdk.COLUMNS, 1);
            this.globals.Set(gdg.MAIN, gdk.AUTOSAVE, true);
        }
    }


    // Global constants associated with elements of SerializableGroupDictionary. "GDG" (global dictionary 
    // group) contains known group names, and "GDK" (global dictionary key) contains known key names.
    public static class gdg
    {
        public const string MAIN = "main";
        public const string FILE = "fileseries";
        public const string AMAZON = "amazon";
        public const string PLUGIN_GLOBALS = "plugin_globals";
        public const string PLUGINS = "plugins";
    }

    public static class gdk
    {
        // Group=MAIN
        public const string AUTOSAVE = "autosave";
        public const string COLUMNS = "columns";

        // Group=FILE
        public const string DEFDIRLIST = "defaultdirectorylist";

        // Group=AMAZON
        public const string PUBLICKEY = "publickey";
        public const string SECRETKEY = "secretkey";
        public const string AFFILIATEID = "affiliateid";

        // Group=PLUGIN_GLOBALS
        public const string PYTHONPATH = "pythonpath";
    }

    //
    // Definition for a data dictionary. There are two keys, "group" (for a category of data) and
    // "key" (for an element within that category).
    //
    // Serialization is done as a series of "<group key1="value1" key2="value2" ...>" tags.
    //
    // The per-group dictionaries of keys are implemented using the StringDictionary class, 
    // which automatically assigns default values to undefined keys and which has helper
    // routines to convert supported data types to/from strings.
    //
    [Serializable, XmlRoot("SerializableGroupDictionary")]
    public class SerializableGroupDictionary : Dictionary<string, StringDictionary>, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                string tag = reader.Name;
                while (reader.MoveToNextAttribute())
                {
                    this[tag][reader.Name] = reader.Value;
                }
                reader.Read();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (string group in this.Keys)
            {
                writer.WriteStartElement(group);
                foreach (string key in this[group].Keys)
                {
                    writer.WriteAttributeString(key, this[group][key]);
                }
                writer.WriteEndElement();
            }
        }

        // Helper methods for data conversion. These just call the corresponding
        // methods in StringDictionary.
        public bool GetBool(string group, string key, string defval = "false")
        {
            return (this[group].GetBool(key, defval));
        }

        public int GetInt(string group, string key, int defval = 0)
        {
            return (this[group].GetInt(key, defval)); ;
        }

        public List<string> GetList(string group, string key, char delim = '|')
        {
            return (this[group].GetList(key, delim));
        }

        public string[] GetArray(string group, string key, char delim = '|')
        {
            return (this[group].GetArray(key, delim));
        }

        public void Set(string group, string key, object value, char delim = '|')
        {
            this[group].Set(key, value, delim);
        }

        public new StringDictionary this[string group]
        {
            get
            {
                if (!base.ContainsKey(group))
                {
                    base[group] = new StringDictionary();
                }
                return (base[group]);
            }
            set
            {
                base[group] = value;
            }
        }

        public new void Remove(string key)
        {
            if (this.ContainsKey(key)) base.Remove(key);
        }
    }

    //
    // Dictionary of strings.
    //
    // If a key is not defined, then return a blank string.
    //
    // There exists a set of helper routines to convert data to/from string format. Currently the
    // following data types are supported: Strings, booleans, integers, string arrays, and string lists.
    // Lists and arrays have all the elements concatenated into a string, with a user-defined character
    // used as a delimiter.
    //
    // The "set" routine converts any supported data type into a string. The "get" routines convert
    // from strings back to the requested data type, optionally returning a default value if the
    // key is undefined. 
    //
    [Serializable, XmlRoot("StringDictionary")]
    public class StringDictionary : SortedDictionary<string, string>, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                while (reader.MoveToNextAttribute())
                {
                    this[reader.Name] = reader.Value;
                }
                reader.Read();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("stringdictionary");
            foreach (string key in this.Keys)
            {
                writer.WriteAttributeString(key, this[key]);
            }
            writer.WriteEndElement();
        }


        public bool GetBool(string key, string defval = "false")
        {
            if (this[key] == "")
            {
                this[key] = defval;
            }
            return (this[key] == "true");
        }

        public int GetInt(string key, int defval = 0)
        {
            int value;
            if (!Int32.TryParse(this[key], out value))
            {
                value = defval;
                this[key] = value.ToString();
            }
            return (value);
        }

        public List<string> GetList(string key, char delim = '|')
        {
            if (this[key] == "") return (new List<string>());
            List<string> val = this[key].Split(delim).ToList<string>();
            return (val);
        }

        public string[] GetArray(string key, char delim = '|')
        {
            if (this[key] == "") return (new string[0]);
            string[] val = this[key].Split(delim);
            return (val);
        }

        public void Set(string key, object value, char delim = '|')
        {
            Type t = value.GetType();
            if (t == typeof(bool))
            {
                if ((bool)value)
                {
                    this[key] = "true";
                }
                else
                {
                    this[key] = "false";
                }
            }
            else if (t == typeof(int))
            {
                this[key] = value.ToString();
            }
            else if (t == typeof(string))
            {
                this[key] = (string)value;
            }
            else if (t == typeof(string[]))
            {
                this[key] = String.Join(delim.ToString(), (string[])value);
            }
            else if (t == typeof(List<string>))
            {
                this[key] = String.Join(delim.ToString(), (List<string>)value);
            }
            else
            {
                MessageBox.Show("VideoTrackerData.Set(): Unknown type " + value.GetType().ToString());
            }
        }

        public new string this[string key]
        {
            get
            {
                if (!base.ContainsKey(key))
                {
                    return ("");
                }
                return (base[key]);
            }
            set
            {
                base[key] = value;
            }
        }

        public new void Remove(string key)
        {
            if (this.ContainsKey(key)) base.Remove(key);
        }
    }
}
