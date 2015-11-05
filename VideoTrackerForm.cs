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

namespace VideoTracker
{
    public partial class VideoTrackerForm : Form
    {
        private string configFile;
        private bool configFileValid = false;
        private int numPanels;
        private long configFileThreads; // Interlocked variables must be long
        private VideoTrackerData videoTrackerData;

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

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog fd = new OpenFileDialog())
            {
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    configFile = fd.FileName;
                    LoadData(configFile);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    configFile = fd.FileName;
                    SaveData(configFile);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveData(configFile);
        }

        private void autoSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoTrackerData.autoSave = !videoTrackerData.autoSave;
            autoSaveToolStripMenuItem.Checked = videoTrackerData.autoSave;
            if (videoTrackerData.autoSave)
            {
                MessageBox.Show("Autosave enabled");
            }
            else
            {
                MessageBox.Show("Autosave disabled");
            }
        }

        //
        // Open up a New Series dialog. Do not check status here; the operation
        // completes asynchronously.
        //
        private void addVideoFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FileVideoSeriesForm vsf = new FileVideoSeriesForm(videoTrackerData))
            {
                vsf.ShowDialog();
            }
        }

        private void addAmazonVideoOnDemandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AmazonVideoSeriesForm vsf = new AmazonVideoSeriesForm(videoTrackerData))
            {
                vsf.ShowDialog();
            }
        }


        private void addCrunchyRollVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CrunchyRollVideoSeriesForm csf = new CrunchyRollVideoSeriesForm(videoTrackerData))
            {
                csf.ShowDialog();
            }

        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadAllSeries();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SettingsForm sf = new SettingsForm(videoTrackerData)) {
                sf.ShowDialog();
            }
        }


        public void AddTitle(VideoPlayerPanel panel, VideoSeries vs)
        {
            // AddTitle can be called from "Add new program" or from "File Load". If it's
            // called from "File Load", then it's already in videoSeriesList and doesn't
            // need to be added again.
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
            if (numPanels > 0)
            {
                this.ResizeMainPanel();
            }
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

            // Find the maximum initial width of the VideoPlayerPanel controls, and set
            // the width of each panel to that maximum.
            int max = 0;
            foreach (VideoPlayerPanel vp in mainPanel.Controls)
            {
                if (vp.initialWidth > max)
                {
                    max = vp.initialWidth;
                }
            }
            foreach (VideoPlayerPanel vp in mainPanel.Controls)
            {
                vp.SetWidth(max);
            }

            // Adjust the number of columns if it has changed.
            if (this.mainPanel.ColumnCount != this.videoTrackerData.columns)
            {
                this.mainPanel.ColumnCount = this.videoTrackerData.columns;
                this.mainPanel.PerformLayout();
            }
            // Update the main window
            this.PerformLayout();
        }

        // Autosave the file if it's allowed:
        // - No "Add Series" threads can currently be running.
        // - Autosave must be enabled.
        // - The configFileValid field must indicate that the configuration file loaded
        //   without errors.
        public void CheckAutoSave()
        {
            // Note: configFileThreads can be updated asynchronously.
            if (Interlocked.Read(ref configFileThreads) == 0 && videoTrackerData.autoSave && configFileValid)
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
            // thread must decrement configFileThreads and call EnableFileOperations 
            // on completion.
            if (Interlocked.Read(ref configFileThreads) != 0)
            {
                return;
            }
            Interlocked.Add(ref configFileThreads, (long)videoTrackerData.videoSeriesList.Count);

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
                vs.LoadGlobalSettings(videoTrackerData);
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

        public void ThreadComplete()
        {
            long threads = Interlocked.Decrement(ref this.configFileThreads);
            if (threads == 0)
            {
                EnableFileOperations(true);
            }
        }

        public void EnableFileOperations(bool flag)
        {
            loadToolStripMenuItem.Enabled = flag;
            saveAsMenuItem.Enabled = flag;
            autoSaveToolStripMenuItem.Enabled = flag;
        }

        // Program is exiting.
        private void VideoTrackerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                CheckAutoSave();
            }
        }
    }

    [Serializable]
    public class VideoTrackerData
    {

        // Global settings for different VideoSeries classes. We declare them here so that
        // they can be serialized in the configuration file.
        //

        // General Globals
        public int columns;

        // Globals for FileSeries
        public List<string> defaultDirectoryList;

        // Globals for AmazonVideoSeries
        public string awsPublicKey;
        public string awsSecretKey;
        public string awsAffiliateID;

        public bool autoSave;
        public List<VideoSeries> videoSeriesList;

        [XmlIgnore,NonSerialized]
        public VideoTrackerForm videoTrackerForm;

        public VideoTrackerData()
        {
            return;
        }

        public VideoTrackerData(VideoTrackerForm vtf)
        {
            this.autoSave = true;
            this.awsPublicKey = null;
            this.awsSecretKey = null;
            this.awsAffiliateID = null;
            this.videoTrackerForm = vtf;
            this.defaultDirectoryList = new List<string>();
            this.videoSeriesList = new List<VideoSeries>();
        }

      
    }
}
