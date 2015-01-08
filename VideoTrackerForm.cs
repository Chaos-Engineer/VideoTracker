using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Text;

namespace VideoTracker
{
    public partial class VideoTrackerForm : Form
    {
        //<summary>
        //Number of panels on main page
        //</summary>
        private int numPanels = 0;
        public VideoTrackerData videoTrackerData = new VideoTrackerData();

        public VideoTrackerForm()
        {
            InitializeComponent();
        }

        private void AddNewTitle_Click(object sender, EventArgs e)
        {
            VideoSeriesForm vs = new VideoSeriesForm();
            if (vs.ShowDialog() == DialogResult.OK)
            {
                videoTrackerData.videoSeriesList.Add(vs.videoSeries);
                AddOrUpdateVideoPanel(vs.videoSeries);
            }
        }
 
        //
        // This is called when we click the "Add New" button, or when a
        // configuration file is imported.
        //
        public void AddOrUpdateVideoPanel(VideoSeries v)
        {
            bool add;
            VideoPlayerPanel panel;

            add = false;
            if (v.panel == null) {
                // New title
                add = true;
                numPanels++;
                panel = new VideoPlayerPanel();
                v.panel = panel;
                v.panel.videoSeries = v;
                v.panel.videoTrackerData = videoTrackerData;
                v.panel.videoTrackerForm = this;
            }
            int temp = 0, maxWidth = 0;
            foreach (VideoFile f in v.videoFiles.Values)
            {
                string filename = Path.GetFileName(f.filename);
                v.panel.AddFile(filename);
                temp = TextRenderer.MeasureText(filename, Font).Width;
                if (temp > maxWidth) { maxWidth = temp; }
            }
            v.panel.SetSelectorWidth(maxWidth);
            v.panel.UpdatePanel();
 
            if (add)
            {
                mainPanel.Controls.Add(v.panel);
            }
        }

        public void DeleteTitle(VideoPlayerPanel panel)
        {
            videoTrackerData.videoSeriesList.Remove(panel.videoSeries);
            mainPanel.Controls.Remove(panel);
            numPanels--;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        public void CheckAutoSave()
        {
            if (videoTrackerData.autoSave)
            {
                SaveData();
            }
        }

        private void SaveData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(VideoTrackerData));
            Stream stream = new FileStream("MyFile.xml", FileMode.Create,
                FileAccess.Write,
                FileShare.Read);
            serializer.Serialize(stream, videoTrackerData);
            stream.Close();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(VideoTrackerData));
            Stream stream = new FileStream("MyFile.xml", FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            mainPanel.SuspendLayout();
            mainPanel.Controls.Clear();
            videoTrackerData = (VideoTrackerData) serializer.Deserialize(stream);
            stream.Close();
            foreach (VideoSeries v in videoTrackerData.videoSeriesList)
            {
                // These fields aren't serialized and must be recreated.
                v.videoFiles.Clear();
                v.panel = null; 
                v.Update(v.title, v.currentVideo.filename, v.directoryList, v.panel);
                AddOrUpdateVideoPanel(v);
            }
            mainPanel.PerformLayout();
            mainPanel.ResumeLayout();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

    [Serializable]
    public class VideoFile
    {
        public string filename;
        public string season;
        public string episode;
        public string key;
    }

    [Serializable]
    public class VideoSeries
    {

        public string title;
        public VideoFile currentVideo;
        public List<string> directoryList;
        [XmlIgnore] public SortedList<string,VideoFile> videoFiles;
        [XmlIgnore] public VideoPlayerPanel panel;

        public VideoSeries()
        {
            this.title = "";
            this.currentVideo = null;
            this.directoryList = null;
            this.panel = null;
            this.videoFiles = new SortedList<string,VideoFile>();
        }

        public VideoSeries(string title, string currentFile, List<string> directoryList)
        {

            this.videoFiles = new SortedList<string,VideoFile>();
            this.panel = null;
            this.Update(title, currentFile, directoryList, this.panel);
        }

        public void Update(string title, string currentFile, List<string> directoryList, VideoPlayerPanel panel)
        {
            videoFiles.Clear();
            this.title = title;
            this.currentVideo = null;
            this.directoryList = directoryList;
            this.panel = panel;

            Regex whitespace = new Regex(@"/\s+/g");
            string fileSearchString = whitespace.Replace(title, "*");
            string regexSearchString = whitespace.Replace(Regex.Escape(title), ".*");
            string seasonEpisodeRegex = regexSearchString + @"\D*?(\d+)\D+?(\d+)";
            string EpisodeOnlyRegex = regexSearchString + @"\D*?(\d+)";

            Dictionary<string, int> seasons = new Dictionary<string, int>();
            int special = 1;
            bool seasonValid = true;
            foreach (string directory in directoryList)
            {
                string[] files = Directory.GetFiles(directory, fileSearchString + "*");
                foreach (string file in files)
                {
                    VideoFile v = new VideoFile();
                    v.filename = file;
             
                    Match m = Regex.Match(file, seasonEpisodeRegex, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        GroupCollection g = m.Groups;
                        v.season = g[1].Value;
                        v.episode = g[2].Value;
                        if (seasons.ContainsKey(v.season))
                        {
                            seasons[v.season] += 1;
                        }
                        else
                        {
                            seasons[v.season] = 1;
                        }
                    }
                    else
                    {
                        m = Regex.Match(file, EpisodeOnlyRegex, RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            GroupCollection g = m.Groups;
                            v.season = g[1].Value;
                            v.episode = "Special " + special++;
                            if (seasons.ContainsKey(v.season))
                            {
                                seasons[v.season] += 1;
                            }
                            else
                            {
                                seasons[v.season] = 1;
                            }
                        }
                        else
                        {
                            v.season = "NONE";
                            v.episode = "Special " + special++;
                        }
                    }
                    v.key = this.title + " S:" + v.season + " E:" + v.episode;


                    videoFiles.Add(v.key, v);
                    if (v.filename == currentFile)
                    {
                        this.currentVideo = v;
                    }
                }
            }
            // The current video file has been deleted. Reset to the beginning of the series.
            // If all videos have been deleted, then an empty panel will be displayed for editting.
            if (this.currentVideo == null && videoFiles.Count > 0)
            {
                this.currentVideo = videoFiles.Values[0];
            }

            //
            // If there are three or more episodes, and if no two episodes have the same season 
            // number, then assume that the first number in the filename is the episode, and any
            // remaining numbers are meaningless.
            //
            if (videoFiles.Count >= 3)
            {
                seasonValid = false;
                foreach (int i in seasons.Values)
                {
                    if (i > 1)
                    {
                        seasonValid = true;
                    }
                }
            }
            if (!seasonValid)
            {
                foreach (VideoFile v in videoFiles.Values)
                {
                    v.episode = v.season;
                    v.season = "NONE";
                }
            }
        }
    }

    [Serializable]
    public class VideoTrackerData
    {
        public bool autoSave;
        public string configPath;
        public List<VideoSeries> videoSeriesList;

        public VideoTrackerData()
        {
            autoSave = true;
            configPath = "";
            videoSeriesList = new List<VideoSeries>();
        }
    }
}
