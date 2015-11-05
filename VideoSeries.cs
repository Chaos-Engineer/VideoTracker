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
// Derived classes must implement the following methods:
// * LoadGlobalSettings [Optional] - This does any required initialization based on
//   globals in the VideoTrackerData class.
// * LoadSeriesAsync - Identifies all of the video files in a series. This routine
//   runs asynchronously.
// * EditForm - Displays the form used to create a new series or edit an existing one.
// * PlayCurrent - Plays the currently-selected video.
namespace VideoTracker
{
    [Serializable]
    [XmlInclude(typeof(FileVideoSeries))]
    [XmlInclude(typeof(AmazonVideoSeries))]
    [XmlInclude(typeof(CrunchyRollVideoSeries))]
    public abstract class VideoSeries
    {

        public string title;
        public VideoFile currentVideo;
        // Not serialized - this list can be changed between invocations of the 
        // program and so must be built at run-time.
        [XmlIgnore,NonSerialized]
        public SortedList<string, VideoFile> videoFiles;
        

        private const string dummyFileKey = "NO FILES FOUND";
        private static VideoFile dummyFile;

        protected abstract void LoadSeriesAsync(object sender, DoWorkEventArgs e);
        public abstract void EditForm(VideoTrackerData videoTrackerData);
        public abstract void PlayCurrent();

        // Not serialized - dynamic values and system data structures.
        [XmlIgnore,NonSerialized]
        private VideoPlayerPanel panel;
        [XmlIgnore,NonSerialized]
        private bool valid;
        [XmlIgnore,NonSerialized]
        protected string errorString;
        [XmlIgnore,NonSerialized]
        private BackgroundWorker backgroundWorker;

        static VideoSeries()
        {
            dummyFile = new VideoFile();
            dummyFile.title = dummyFileKey;
            dummyFile.key = dummyFileKey;
        }

        public VideoSeries()
        {
            this.title = "";
            this.currentVideo = null;
            this.panel = null;
            this.videoFiles = new SortedList<string, VideoFile>();
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new DoWorkEventHandler(LoadSeriesAsync);
            this.backgroundWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(LoadDataCompleted);
        }

        public virtual bool LoadGlobalSettings(VideoTrackerData videoTrackerData)
        {
            return true;
        }


        public void LoadFiles(string title, string currentKey, VideoTrackerData videoTrackerData)
        {
            videoFiles.Clear();
            this.title = title;
            if (this.panel == null) { 
                this.panel = new VideoPlayerPanel(videoTrackerData, this);
            }
            this.panel.BeginFileLoad(this);
            this.backgroundWorker.RunWorkerAsync(currentKey);
        }

        private void LoadDataCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // If no files were found, then mark the series as invalid. If we've 
            // loaded the series successfully in the past, then the "currentVideo" field 
            // is still valid, so retain that value. Otherwise insert "dummyFile", to allow
            // a "NO FILES FOUND" message to display in the panel.
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
                    this.videoFiles.Add(dummyFileKey, dummyFile);
                    this.currentVideo = dummyFile;
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

        public bool IsValid()
        {
            return this.valid;
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
