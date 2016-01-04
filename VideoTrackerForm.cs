using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing;
using System.Configuration;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace VideoTracker
{
    public partial class VideoTrackerForm : Form
    {
        private string configFile;
        private bool configFileValid = false;
        private int numPanels;
        private long workerThreads; // Interlocked variables must be long
        private VideoTrackerData videoTrackerData;

        private int currentHeight;      // Saved height of main window (used when resizing)
        private int currentWidth;       // Saved width of main window (used when resizing)
        private int panelWidth;         // Width of individual panel controls, excluding margin (used to change width)
        private int actualPanelWidth;   // Width of individual panel controls, including margin
        private int actualPanelHeight;  // Height of individual panel controls, including margin

        public VideoTrackerForm(string launchFile)
        {
            InitializeComponent();
            string file = "UNDEFINED";
            videoTrackerData = new VideoTrackerData(this);

            if (launchFile.Equals(""))
            {
                file = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["DefaultFilePath"]);
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            else
            {
                file = launchFile;
            }
            this.configFile = file;
            if (!File.Exists(file))
            {
                using (File.Create(file))
                {
                    // Null body - to allow handle to get disposed.
                }
                SaveData(file);
            }
            else
            {
                LoadData(file);
            }
        }

        private void loadMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                configFile = openFileDialog.FileName;
                LoadData(configFile);
            }
        }

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
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
            autoSaveMenuItem.Checked = videoTrackerData.globals.GetBool(gdg.MAIN, gdk.AUTOSAVE);
            if (autoSaveMenuItem.Checked)
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
            Application.Exit();
        }

        // Program is exiting - save current state before exiting.
        private void VideoTrackerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                CheckAutoSave();
            }
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
            using (AmazonVideoSeriesForm vsf = new AmazonVideoSeriesForm(videoTrackerData))
            {
                vsf.ShowDialog();
            }
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

        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            using (SettingsForm sf = new SettingsForm(videoTrackerData))
            {
                sf.ShowDialog();
            }
        }

        // If the resize movement was mostly left-to-right, then assume that the number of
        // columns should change, and if the resize movement was mostly up-and-down, then
        // assume that the number of rows should change.
        //
        // Based on that information, calculate the number of columns desired and resize the
        // display.
        private void VideoTrackerForm_ResizeEnd(object sender, EventArgs e)
        {
            int columns;
            int dWidth = Math.Abs(this.Width - this.currentWidth);
            int dHeight = Math.Abs(this.Height - this.currentHeight);

            if (dWidth >= dHeight)
            {
                // Change number of columns
                columns = (int)Math.Round((float)this.mainPanel.ClientSize.Width / (float)this.actualPanelWidth);
            }
            else
            {
                // Change number of rows (and calculate corresponding number of rows.)
                int rows = (int)Math.Round((float)this.mainPanel.ClientSize.Height / (float)this.actualPanelHeight);
                if (rows == 0) rows = 1;
                columns = ((this.numPanels - 1) / rows) + 1;
            }
            if (columns <= 0) columns = 1;
            this.videoTrackerData.globals[gdg.MAIN][gdk.COLUMNS] = columns.ToString();
            ResizeMainPanel();
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
            this.mainPanel.Controls.Add(panel);
            this.ResizeMainPanel();
            this.CheckAutoSave();
        }

        public void DeleteTitle(VideoPlayerPanel panel, VideoSeries vs)
        {
            this.videoTrackerData.videoSeriesList.Remove(vs);
            this.mainPanel.Controls.Remove(panel);
            this.numPanels--;
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

            Control.ControlCollection cc = this.mainPanel.Controls;
            Control c = cc[source];
            cc.SetChildIndex(c, dest);

            List<VideoSeries> vsl = this.videoTrackerData.videoSeriesList;
            VideoSeries item = vsl[source];
            vsl.RemoveAt(source);
            vsl.Insert(dest, item);

            this.CheckAutoSave();
        }

        // Function: Update the size of the main window. This is called when we add or remove a
        // program, or change the number of columns in the display.
        public void ResizeMainPanel()
        {
            this.SuspendLayout();

            blankLabel.Visible = (this.numPanels == 0); // Display a caption if there are no panels.

            // Find the maximum initial width of the VideoPlayerPanel controls, and set
            // the width of each panel to that maximum.
            this.panelWidth = 0;
            this.actualPanelWidth = 0;
            this.actualPanelHeight = 0;
            foreach (VideoPlayerPanel vp in mainPanel.Controls)
            {
                if (vp.initialWidth > this.panelWidth)
                {
                    this.panelWidth = vp.initialWidth;
                    this.actualPanelWidth = vp.Margin.Left + vp.initialWidth + vp.Margin.Right;
                    this.actualPanelHeight = vp.Margin.Top + vp.DisplayRectangle.Height + vp.Margin.Bottom;
                }
            }
            foreach (VideoPlayerPanel vp in mainPanel.Controls)
            {
                vp.SetWidth(this.panelWidth);
            }

            // Adjust the number of columns if it has changed.

            // This "ConvertToInt" call assigns a default value of 1 if columns is currently undefined.
            int columns = videoTrackerData.globals.GetInt(gdg.MAIN, gdk.COLUMNS, 1);
            if (this.mainPanel.ColumnCount != columns)
            {
                this.mainPanel.ColumnCount = columns;
            }
            // Update the main window with auto-sizing enabled.
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.AutoSize = true;
            this.ResumeLayout(false);
            this.PerformLayout();

            // Get the dimensions of the auto-sized window, set the fixed dimensions
            // to those values, and then disable auto-sizing.
            //
            // Note: In order for the user to be able to change the size of the window,
            // AutoSizeMode must be sent to GrowOnly. (This is a bug in Windows; the
            // AutoSizeMode property should be ignored when AutoSize is set to false.)
            this.SuspendLayout();
            this.currentHeight = this.Height;
            this.currentWidth = this.Width;
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.Height = this.currentHeight;
            this.Width = this.currentWidth;
            this.ResumeLayout(false);
            this.PerformLayout();
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
            mainPanel.Controls.Clear();
            configFileValid = true;
            LoadAllSeries();
            configFile = file;
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
                if (vs.title == "") { vs.title = "UNKNOWN"; }
                if (vs.currentVideo == null)
                {
                    vs.currentVideo = new VideoFile();
                    vs.currentVideo.key = vs.currentVideo.title = "NO FILE FOUND";
                    MessageBox.Show("Invalid data for title " + vs.title + ". " +
                        "\n\nAuto-save will not be performed until configuration is saved manually");
                    configFileValid = false;
                }
                vs.LoadFiles(vs.title, vs.currentVideo.title, videoTrackerData);
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
        }

        public void EnableFileOperations(bool flag)
        {
            loadMenuItem.Enabled = flag;
            saveMenuItem.Enabled = flag;
            saveAsMenuItem.Enabled = flag;
            autoSaveMenuItem.Enabled = flag;
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
        public const string MAIN   = "main";
        public const string FILE   = "fileseries";
        public const string AMAZON = "amazon";
    }

    public static class gdk
    {
        public const string AUTOSAVE    = "autosave";
        public const string COLUMNS     = "columns";

        public const string DEFDIRLIST  = "defaultdirectorylist";

        public const string PUBLICKEY   = "publickey";
        public const string SECRETKEY   = "secretkey";
        public const string AFFILIATEID = "affiliateid";
    }

    //
    // Definition for a data dictionary. There are two keys, "group" (for a category of data) and
    // "key" (for an element within that category).
    //
    // Serialization is done as a series of "<item key="value">" tags.
    //
    // There exists a set of helper routines to convert data to/from string format. Currently the
    // following data types are supported: Strings, booleans, integers, string arrays, and string lists.
    //
    // If a value for a particular group/key pair is not found, a blank string is returned.
    //
    [XmlRoot("SerializableGroupDictionary")]
    public class SerializableGroupDictionary : SortedDictionary<string, StringDictionary>, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(string));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(string));
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
            XmlSerializer keySerializer = new XmlSerializer(typeof(string));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(string));
            foreach (string group in this.Keys)
            {
                foreach (string key in this[group].Keys)
                {
                    writer.WriteStartElement(group);
                    writer.WriteAttributeString(key, this[group][key]);
                    writer.WriteEndElement();
                }
            }
        }
        
        public bool GetBool(string group, string key, string defval="false")
        {
            if (this[group][key] == "")
            {
                this[group][key] = defval;
            }    
            return (this[group][key] == "true");
        }


        public int GetInt(string group, string key, int defval = 0)
        {
            int value;
            if (!Int32.TryParse(this[group][key], out value))
            {
                value = defval;
                this[group][key] = value.ToString();
            }
            return (value);
        }

        public List<string> GetList(string group, string key, char delim = '|')
        {
            if (this[group][key] == "") return (new List<string>());
            List<string> val = this[group][key].Split(delim).ToList<string>();
            return (val);
        }

        public string[] GetArray(string group, string key, char delim = '|')
        {
            if (this[group][key] == "") return (new string[0]);
            string[] val = this[group][key].Split(delim);
            return (val);
        }

        public void Set(string group, string key, object value, char delim = '|')
        {
            Type t = value.GetType();
            if (t == typeof(bool))
            {
                if ((bool)value)
                {
                    this[group][key] = "true";
                }
                else
                {
                    this[group][key] = "false";
                }
            }
            else if (t == typeof(int))
            {
                this[group][key] = value.ToString();
            }
            else if (t == typeof(string)) {
                this[group][key] = (string) value;
            }
            else if (t == typeof(string[]))
            {
                this[group][key] = String.Join(delim.ToString(), (string[]) value);
            }
            else if (t == typeof(List<string>))
            {
                this[group][key] = String.Join(delim.ToString(), (List<string>) value);
            }
            else
            {
                MessageBox.Show("VideoTrackerData.Set(): Unknown type " + value.GetType().ToString());
            }
        }

        public new StringDictionary this[string group]
        {
            get
            {
                if (!base.ContainsKey(group))
                {
                    base[group] = new StringDictionary();
                }
                return base[group];
            }
            set
            {
                base[group] = value;
            }
        }
    }

    //
    // Dictionary of strings.
    //
    // If a key is not defined, then return a blank string.
    //
    public class StringDictionary : SortedDictionary<string,string>
    {
        public new string this[string key]
        {
            get
            {
                if (base.ContainsKey(key))
                {
                    return base[key];
                }
                return ("");
            }
            set
            {
                base[key] = value;
            }
        }
    }
}