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
            VideoSeriesForm vsf = new VideoSeriesForm(this);
            vsf.ShowDialog();
        }

        public void DeleteTitle(VideoPlayerPanel panel)
        {
            videoTrackerData.videoSeriesList.Remove(panel.videoSeries);
            mainPanel.Controls.Remove(panel);
            numPanels--;
            if (numPanels > 0)
            {
                AdjustWidth();
            }
            CheckAutoSave();
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

            // File was successfully loaded; create panels. This will complete
            // asynchronously, so the asynchronous thread must call
            // EnableFileOperations on completion.
            mainPanel.Controls.Clear();
            configFileThreads = videoTrackerData.videoSeriesList.Count;
            foreach (VideoSeries vs in videoTrackerData.videoSeriesList)
            {
                vs.InitializeVideoPanel(this);
                vs.UpdateFiles(vs.title, vs.currentVideo.filename, vs.directoryList, vs.panel);
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
