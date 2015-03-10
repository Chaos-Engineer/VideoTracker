using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace VideoTracker
{
    [Serializable]
    [XmlInclude(typeof(FileVideoSeries))]
    public abstract class VideoSeries
    {

        public string title;
        public VideoFile currentVideo;

        public bool allowUndelimitedEpisodes;
        public bool noSeasonNumber;
        public List<string> postSeasonStrings;

        protected static string addDelay = ConfigurationManager.AppSettings["AddDelay"];

        protected abstract void LoadSeriesAsync(object sender, DoWorkEventArgs e);
        public abstract void EditForm(VideoTrackerForm videoTrackerForm);
        public abstract void PlayCurrent();

        // Not serialized - this list can be changed between invocations of the 
        // program and so must be built at run-time.
        [XmlIgnore]
        public SortedList<string, VideoFile> videoFiles;
        // Not serialized - this depends on the contents of "videoFiles".
        [XmlIgnore]
        public VideoPlayerPanel panel;
        [XmlIgnore]
        public bool valid;
        [XmlIgnore]
        public string errorString;
        // Not serialized - system data structure
        [XmlIgnore]
        public BackgroundWorker backgroundWorker;

        public VideoSeries()
        {
            this.title = "";
            this.currentVideo = null;
            this.allowUndelimitedEpisodes = true;
            this.noSeasonNumber = false;
            this.panel = null;
            this.postSeasonStrings = new List<String>();
            this.videoFiles = new SortedList<string, VideoFile>();
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new DoWorkEventHandler(LoadSeriesAsync);
            this.backgroundWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(LoadDataCompleted);
        }

        public void Load(string title, string currentFile, VideoPlayerPanel panel)
        {
            videoFiles.Clear();
            this.title = title;
            this.allowUndelimitedEpisodes = true; // Make this configurable;
            this.noSeasonNumber = false; // Make this configurable;
            this.postSeasonStrings = new List<string>();
            this.postSeasonStrings.Add("SPECIAL"); // Make this configurable;

            this.panel = panel;
            if (panel != null)
            {
                panel.BeginFileLoad(this);
            }

            this.backgroundWorker.RunWorkerAsync(currentFile);
        }

        private void LoadDataCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // No files were found, so disable everything on the panel except the 
            // "edit" and "delete" buttons. If we've previously loaded the series
            // successfully in the past, then the "currentVideo" field is still
            // valid, so retain the value.
            if (this.videoFiles.Count == 0)
            {
                MessageBox.Show("Error loading " + this.title + ".\n" + errorString);
                this.valid = false;
                if (this.currentVideo != null)
                {
                    this.videoFiles.Add(this.currentVideo.key, this.currentVideo);
                }
                else
                {
                    string dummyFile = "NO FILES FOUND";
                    VideoFile v = new VideoFile();
                    v.title = dummyFile;
                    this.videoFiles.Add(dummyFile, v);
                    this.currentVideo = v;
                }
            }
            else
            {
                this.valid = true;
            }
            this.panel.EndFileLoad(this);
        }

    }

    public class FileVideoSeries : VideoSeries
    {
        public List<string> directoryList;

  
        public FileVideoSeries()
        {
            this.directoryList = null;
	    }

        public void Load(string title, string currentFile, VideoPlayerPanel panel, List<string> directoryList)
        {
            this.directoryList = directoryList;
            base.Load(title, currentFile, panel);
        } 

        public override void PlayCurrent()
        {
            Process.Start(currentVideo.internalName);
        }

        public override void EditForm(VideoTrackerForm videoTrackerForm)
        {
            FileVideoSeriesForm vsf = new FileVideoSeriesForm(videoTrackerForm, this);
            vsf.ShowDialog();
        }


        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
        {
            try
            {
                Regex whitespace = new Regex(@"\s+");
                string fileSearchString = whitespace.Replace(title, "*");
                string regexSearchString = whitespace.Replace(Regex.Escape(title), ".*");
                string seasonEpisodeRegex = regexSearchString + @"\D*?(\d+)\D+?(\d+)";
                string EpisodeOnlyRegex = regexSearchString + @"\D*?(\d+)";
                string currentFile = (String)e.Argument;
                Dictionary<int, int> seasons = new Dictionary<int, int>();
                bool seasonValid = true;
                bool parsingEpisode = true;

                if (addDelay.Equals("true"))
                {
                    Thread.Sleep(1000 * title.Length); // Force delay 
                }
                foreach (string directory in directoryList)
                {
                    string[] files = System.IO.Directory.GetFiles(directory, fileSearchString + "*");
                    foreach (string file in files)
                    {
                        VideoFile v = new VideoFile();
                        v.internalName = v.title = file;

                        parsingEpisode = true;
                        // This handles strings with season and episode numbers, in formats
                        // like S01E01 and 1x01. It is executed if the "noSeasonNumber" flag
                        // has not been set, and if the episode name contains at least two
                        // digit strings.
                        if (noSeasonNumber == false)
                        {
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
                        if (parsingEpisode)
                        {
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
                        v.postSeason = 0;
                        foreach (string s in postSeasonStrings)
                        {
                            if (file.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                v.postSeason = 1;
                            }
                        }

                        v.key = String.Format("{0:D3}{1:D1}{2:D3}{3}", v.season, v.postSeason, v.episode, v.title);

                        videoFiles.Add(v.key, v);
                        if (v.internalName == currentFile)
                        {
                            this.currentVideo = v;
                        }
                    }
                }
                // The current video file has been deleted from disk, but other files were found.
                // Reset the current video to the first file in the series. (If no other files
                // were found, then leave currentVideo unchanged - it might be that the directory
                // is a network share that's temporarily unavailable, and the files will become
                // visible later.)
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
                if (videoFiles.Count == 0)
                {
                    errorString = "No files found.";
                }
            }
            catch (Exception ex)
            {
                errorString = ex.ToString();
            }
        }


    }

    [Serializable]
    public class VideoFile
    {
        public string title;
        public string internalName;
        public int postSeason;
        public int season;
        public int episode;
        public string key;
    }
}
