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
        public int numPanels;
        public int configFileThreads;
        public VideoTrackerData videoTrackerData = new VideoTrackerData();

        public VideoTrackerForm(string launchFile)
        {
            string file = "UNDEFINED";
            InitializeComponent();
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
        private void addProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileVideoSeriesForm vsf = new FileVideoSeriesForm(this);
            vsf.ShowDialog();
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
            if (configFileThreads == 0 && videoTrackerData.autoSave)
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
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to load " + file + "\n" + e.ToString());
                EnableFileOperations(true);
                return;
            }

            // File was successfully loaded; create panels and load videos. The
            // load operation will complete asynchronously, so the asynchronous
            // thread must call EnableFileOperations on completion.
            mainPanel.Controls.Clear();
            configFileThreads = videoTrackerData.videoSeriesList.Count;
            foreach (VideoSeries vs in videoTrackerData.videoSeriesList)
            {
                vs.panel = new VideoPlayerPanel(this, vs);
                vs.Load(vs.title, vs.currentVideo.internalName, vs.panel);
            }
            configFile = file;
        }

        private void SaveData(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(VideoTrackerData));
            using (Stream stream = new FileStream(file, FileMode.Create,
                FileAccess.Write,
                FileShare.Read))
            {
                serializer.Serialize(stream, videoTrackerData);
                stream.Close();
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
        public bool autoSave;
        public List<VideoSeries> videoSeriesList;

        public VideoTrackerData()
        {
            autoSave = true;
            videoSeriesList = new List<VideoSeries>();
        }
    }
}
