using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace VideoTracker
{
    public partial class VideoPlayerPanel : UserControl
    {
        public VideoSeries videoSeries;
        public VideoTrackerData videoTrackerData;
        public VideoTrackerForm videoTrackerForm;
        public int initialWidth;
        private bool updateInProgress;

        public VideoPlayerPanel()
        {
            InitializeComponent();
            this.videoSeries = null;
            updateInProgress = false;
        }
  
        private void backButton_Click(object sender, EventArgs e)
        {
            VideoSeries vs = this.videoSeries;
            int index = vs.videoFiles.IndexOfKey(vs.currentVideo.key);
            vs.currentVideo = vs.videoFiles.Values[--index];
            UpdatePanel();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            VideoSeries vs = this.videoSeries;
            int index = vs.videoFiles.IndexOfKey(vs.currentVideo.key);
            vs.currentVideo = vs.videoFiles.Values[++index];
            UpdatePanel();
        }

        private void videoSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            VideoSeries vs = videoSeries;
            int index = videoSelector.SelectedIndex;
            vs.currentVideo = vs.videoFiles.Values[index];
            UpdatePanel();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            Process.Start(videoSeries.currentVideo.filename);
        }

        private void playNextButton_Click(object sender, EventArgs e)
        {
            nextButton_Click(sender, e);
            playButton_Click(sender, e);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            videoTrackerForm.DeleteTitle(this);
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            VideoSeriesForm vsf = new VideoSeriesForm(videoTrackerForm, videoSeries);
            vsf.ShowDialog();
        }

        public void AddFile(string filename)
        {
            this.videoSelector.Items.Add(filename);
        }

        public void ClearFiles()
        {
            this.videoSelector.Items.Clear();
        }

        public void SetSelectorWidth(int width)
        {
            this.videoSelector.Width = width + SystemInformation.VerticalScrollBarWidth;
        }

        public void SetSeriesName(string name)
        {
            this.seriesName.Text = name;
        }

        
        public void UpdatePanel()
        {
            // Prevent recursive calls using the "updateInProgress" variable.
            //
            // Reason: If this routine changes the SelectedIndex field, it triggers
            // a recursive call because it's the handler for the SelectedIndexChanged
            // event.
            if (updateInProgress) { return;  }
            updateInProgress = true;

            SuspendLayout();
            if (!videoSeries.valid)
            {
                // No files were found for this video series
                videoSelector.SelectedIndex = 0;
                videoSelector.Enabled = false;
                backButton.Enabled = false;
                nextButton.Enabled = false;
                playNextButton.Enabled = false;
                playButton.Enabled = false;
                seriesName.Text = videoSeries.title; // Title only, no episode number available.
            }
            else
            {
                VideoFile v = videoSeries.currentVideo;
                int maxIndex = videoSeries.videoFiles.Count - 1;
                int index = videoSeries.videoFiles.IndexOfKey(v.key);
                int remaining = maxIndex - index;
                if (v.postseason == 1)
                {
                    seriesName.Text = videoSeries.title + " Season: " + v.season + 
                                    " Special: " + v.episode + 
                                    " (" + remaining + " remaining)";
                }
                else
                {
                    seriesName.Text = videoSeries.title + " Season: " + v.season + 
                                    " Episode: " + v.episode +
                                    " (" + remaining + " remaining)";
                }

                videoSelector.Enabled = true;
                videoSelector.SelectedIndex = index;

                // Disable the "back" button for the first video, and the
                // "next" and "play next" buttons for the last video
                if (index == 0)
                {
                    backButton.Enabled = false;
                }
                else
                {
                    backButton.Enabled = true;
                }

                if (index == maxIndex)
                {
                    playNextButton.Enabled = false;
                    nextButton.Enabled = false;
                }
                else
                {
                    playNextButton.Enabled = true;
                    nextButton.Enabled = true;
                }
            }
            flowLayoutPanel.AutoSize = true;
            ResumeLayout(false);
            PerformLayout();
            initialWidth = flowLayoutPanel.Width;
            flowLayoutPanel.AutoSize = false;
            videoTrackerForm.CheckAutoSave();
            updateInProgress = false;
        }

        public void SetWidth(int width)
        {
            FlowLayoutPanel p = this.flowLayoutPanel;
            p.AutoSize = false;
            p.Width = width;
            p.PerformLayout();
        }

        public void VisibleControls(bool flag)
        {
            this.videoSelector.Visible = flag;
            this.nextButton.Visible = flag;
            this.backButton.Visible = flag;
            this.editButton.Visible = flag;
            this.deleteButton.Visible = flag;
            this.playButton.Visible = flag;
            this.playNextButton.Visible = flag;
        }

    }
}
