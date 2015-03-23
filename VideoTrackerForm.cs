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
        public int numPanels;
        public int configFileThreads;
        public VideoTrackerData videoTrackerData;
        public const string dummyFileKey = "NO FILES FOUND";
        public static VideoFile dummyFile;


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

            dummyFile = new VideoFile();
            dummyFile.title = dummyFileKey;
            dummyFile.key = dummyFileKey;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                configFile = fd.FileName;
                LoadData(configFile);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                configFile = fd.FileName;
                SaveData(configFile);
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
            FileVideoSeriesForm vsf = new FileVideoSeriesForm(videoTrackerData);
            vsf.ShowDialog();
        }

        private void addAmazonVideoOnDemandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AmazonVideoSeriesForm vsf = new AmazonVideoSeriesForm(videoTrackerData);
            vsf.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm(videoTrackerData);
            sf.ShowDialog();
        }


        public void AddTitle(VideoPlayerPanel panel, VideoSeries vs)
        {
            if (!this.videoTrackerData.videoSeriesList.Contains(vs))
            {
                this.numPanels++;
                this.videoTrackerData.videoSeriesList.Add(vs);
            }

            this.mainPanel.Controls.Add(panel);
            this.AdjustWidth();
            this.CheckAutoSave();
        }

        public void DeleteTitle(VideoPlayerPanel panel, VideoSeries vs)    
        {
            this.videoTrackerData.videoSeriesList.Remove(vs);
            this.mainPanel.Controls.Remove(panel);
            this.numPanels--;
            if (numPanels > 0)
            {
                this.AdjustWidth();
            }
            this.CheckAutoSave();
        }

        public void AdjustWidth()
        {
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
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        public void CheckAutoSave()
        {
            if (configFileThreads == 0 && videoTrackerData.autoSave && configFileValid)
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

            // File was successfully loaded; create panels and load videos. The
            // load operation will complete asynchronously, so the asynchronous
            // thread must call EnableFileOperations on completion.
            mainPanel.Controls.Clear();
            configFileThreads = videoTrackerData.videoSeriesList.Count;
            configFileValid = true;
            foreach (VideoSeries vs in videoTrackerData.videoSeriesList)
            {
                // Check for corrupted video in configuration file. This
                // shouldn't happen unless there was a bug adding a series.
                if (vs.title == "") { vs.title = "UNKNOWN";}
                if (vs.currentVideo == null) {
                    vs.currentVideo = new VideoFile();
                    vs.currentVideo.title = "NO FILE FOUND";
                    vs.currentVideo.key = "NO FILE FOUND";
                    MessageBox.Show("Invalid data for title " + vs.title + ". " +
                        "\n\nAuto-save will not be performed until configuration is saved manually");
                    configFileValid = false;
                }
                vs.LoadGlobalSettings(videoTrackerData);
                vs.Load(vs.title, vs.currentVideo.title, vs.panel);
            }
            configFile = file;
 
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
                    stream.Close();
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
        // Globals for AmazonVideoSeries
        public string awsPublicKey;
        public string awsSecretKey;
        public string awsAffiliateID;

        public bool autoSave;
        public List<VideoSeries> videoSeriesList;

        [XmlIgnore]
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
            this.videoSeriesList = new List<VideoSeries>();
        }
    }
}
