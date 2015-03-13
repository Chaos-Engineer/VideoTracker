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

// CLASS: VideoSeries
//
// This is an abstract class representing a set of video files. Derived classes represent
// different sources for files.
//
// Derived classes must implment the following methods:
// * Load [Optional] - This assigns values to any class-specific configuration fields 
//   for the series.
// * LoadSeriesAsynch - Identifies all of the video files in a series. This routine
//   runs asynchronously.
// * EditForm - Displays the form used to create a new series or edit an existing one.
// * PlayCurrent - Plays the currently-selected video.
namespace VideoTracker
{
    [Serializable]
    [XmlInclude(typeof(FileVideoSeries))]
    [XmlInclude(typeof(AmazonVideoSeries))]
    public abstract class VideoSeries
    {

        public string title;
        public VideoFile currentVideo;

        public bool allowUndelimitedEpisodes;
        public bool noSeasonNumber;
        public List<string> postSeasonStrings;

        protected static string addDelay = ConfigurationManager.AppSettings["AddDelay"];

        protected abstract void LoadSeriesAsync(object sender, DoWorkEventArgs e);
        public abstract void EditForm(VideoTrackerData videoTrackerData);
        public abstract void PlayCurrent();

        public virtual void LoadGlobalSettings(VideoTrackerData videoTrackerData)
        {
            return; // Can be overridden.
        }

        // Not serialized - this list can be changed between invocations of the 
        // program and so must be built at run-time.
        [XmlIgnore]
        public SortedList<string, VideoFile> videoFiles;
        // Not serialized - dynamic values and system data structures.
        [XmlIgnore]
        public VideoPlayerPanel panel;
        [XmlIgnore]
        public bool valid;
        [XmlIgnore]
        public string errorString;
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

        public void Load(string title, string currentTitle, VideoPlayerPanel panel)
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

            this.backgroundWorker.RunWorkerAsync(currentTitle);
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
                    v.key = dummyFile;
                    this.videoFiles.Add(dummyFile, v);
                    this.currentVideo = v;
                }
            }
            else
            {
                // If the current video no longer exists, then reset to the
                // beginning of the series.
                if (this.currentVideo == null || !this.videoFiles.ContainsKey(this.currentVideo.key))
                {
                    this.currentVideo = this.videoFiles.First().Value;
                }
                this.valid = true;
            }
            this.panel.EndFileLoad(this);
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
