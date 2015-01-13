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
            VideoSeriesForm vsf = new VideoSeriesForm();
            if (vsf.ShowDialog() == DialogResult.OK)
            {
                videoTrackerData.videoSeriesList.Add(vsf.videoSeries);
                AddOrUpdateVideoPanel(vsf.videoSeries);
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
            AdjustWidth();
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

        }

        private void AdjustWidth()
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
                FlowLayoutPanel p = vp.flowLayoutPanel;
                p.AutoSize = false;
                p.Width = max;
                p.PerformLayout();
            }
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
        public int postseason;
        public int season;
        public int episode;
        public string key;
    }

    [Serializable]
    public class VideoSeries
    {

        public string title;
        public VideoFile currentVideo;
        public List<string> directoryList;
        public bool allowUndelimitedEpisodes;
        public bool noSeasonNumber;
        public List<string> postSeasonStrings;
        [XmlIgnore] public SortedList<string,VideoFile> videoFiles;
        [XmlIgnore] public VideoPlayerPanel panel;

        public VideoSeries()
        {
            this.title = "";
            this.currentVideo = null;
            this.directoryList = null;
            this.allowUndelimitedEpisodes = true;
            this.noSeasonNumber = false;
            this.panel = null;
            this.postSeasonStrings = new List<String>();
            this.videoFiles = new SortedList<string,VideoFile>();
        }

        public VideoSeries(string title, string currentFile, List<string> directoryList)
        {

            this.videoFiles = new SortedList<string,VideoFile>();
            this.panel = null;
            this.Update(title, currentFile, directoryList, this.panel);
        }

        private int Sort(string key)
        {
            return 0;
        }

        public void Update(string title, string currentFile, List<string> directoryList, VideoPlayerPanel panel)
        {
            videoFiles.Clear();
            this.title = title;
            this.allowUndelimitedEpisodes = true; // Make this configurable;
            this.noSeasonNumber = false; // Make this configurable;
            this.postSeasonStrings = new List<string>();
            this.postSeasonStrings.Add("SPECIAL"); // Make this configurable;
            this.currentVideo = null;
            this.directoryList = directoryList;
            this.panel = panel;

            Regex whitespace = new Regex(@"\s+");
            string fileSearchString = whitespace.Replace(title, "*");
            string regexSearchString = whitespace.Replace(Regex.Escape(title), ".*");
            string seasonEpisodeRegex = regexSearchString + @"\D*?(\d+)\D+?(\d+)";
            string EpisodeOnlyRegex = regexSearchString + @"\D*?(\d+)";

            Dictionary<int, int> seasons = new Dictionary<int, int>();
            bool seasonValid = true;
            bool parsingEpisode = true;
            foreach (string directory in directoryList)
            {
                string[] files = Directory.GetFiles(directory, fileSearchString + "*");
                foreach (string file in files)
                {
                    VideoFile v = new VideoFile();
                    v.filename = file;
             
                    parsingEpisode = true;
                    // This handles strings with season and episode numbers, in formats
                    // like S01E01 and 1x01. It is executed if the "noSeasonNumber" flag
                    // has not been set, and if the episode name contains at least two
                    // digit strings.
                    if (noSeasonNumber == false) {
                        Match m = Regex.Match(file, seasonEpisodeRegex, RegexOptions.IgnoreCase);
                        if (m.Success)
                        {

                             GroupCollection g = m.Groups;
                             v.season = Int32.Parse(g[1].Value);
                             v.episode = Int32.Parse(g[2].Value);
                             if (seasons.ContainsKey(v.season))
                             {
                                  seasons[v.season] += 1;
                             }
                             else
                             {
                                  seasons[v.season] = 1;
                             }
                             parsingEpisode = false;
                        }
                    }

                    // This handles strings with episode numbers only. It is executed if
                    // the previous block failed - meaning that the "noSeasonNumber" flag 
                    // has been set, or the filename contains a single digit string.
                    if (parsingEpisode) {
                        Match m = Regex.Match(file, EpisodeOnlyRegex, RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            GroupCollection g = m.Groups;
                            v.season = Int32.Parse(g[1].Value);
                            if (seasons.ContainsKey(v.season))
                            {
                                seasons[v.season] += 1;
                            }
                            else
                            {
                                seasons[v.season] = 1;
                            }
                            parsingEpisode = false;
                        }
                    }
                    //
                    // This code is executed if the filename contains no digits at all.
                    // Assume this is a single episode of something.
                    if (parsingEpisode)
                    {
                            v.season = 1;
                            v.episode = 1;
                     }                   

                    // If the first number is 3 or more digits, then this usually indicates
                    // that it contains both the season and episode numbers, e.g. 101 is
                    // Season 1, Episode 1, not season 101.
                    if (allowUndelimitedEpisodes)
                    {
                        if (v.season > 100)
                        {
                            v.episode = v.season % 100;
                            v.season = v.season / 100;
                        }
                    }

                    // End-of-season special. May have the same episode number as a regular
                    // season episode.
                    v.postseason = 0;
                    foreach (string s in postSeasonStrings)
                    {
                        if (file.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            v.postseason = 1;
                        }
                    }

                    v.key = String.Format("{0:D3}{1:D1}{2:D3}", v.season, v.postseason, v.episode);

                    videoFiles.Add(v.key, v);
                    if (v.filename == currentFile)
                    {
                        this.currentVideo = v;
                    }
                }
            }
            // The current video file has been deleted from disk. Reset to the beginning of the 
            // series. If all videos have been deleted, then an empty panel will be displayed 
            // for editting.
            if (this.currentVideo == null && videoFiles.Count > 0)
            {
                this.currentVideo = videoFiles.Values[0];
            }

            //
            // If there are three or more episodes, and if no two episodes have the same season 
            // number, then assume that the first number in the filename is the episode, and that
            // any remaining numbers are meaningless.
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
                    v.season = 1;
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
