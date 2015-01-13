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

        public VideoPlayerPanel()
        {
            InitializeComponent();
            this.videoSeries = null;
        }
  
        private void deleteButton_Click(object sender, EventArgs e)
        {
            videoTrackerForm.DeleteTitle(this);
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            VideoSeries vs = videoSeries;
            int index = vs.videoFiles.IndexOfKey(vs.currentVideo.key);
            index--;
            vs.currentVideo = vs.videoFiles.Values[index];
            UpdatePanel();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            VideoSeries vs = videoSeries;
            int index = vs.videoFiles.IndexOfKey(vs.currentVideo.key);
            index++;
            vs.currentVideo = vs.videoFiles.Values[index];
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

        public void AddFile(string filename)
        {
            videoSelector.Items.Add(filename);
        }

        public void SetSelectorWidth(int width)
        {
            videoSelector.Width = width + SystemInformation.VerticalScrollBarWidth;
        }

        public void UpdatePanel()
        {
            SuspendLayout();

            if (videoSeries.currentVideo == null)
            {
                string noFiles = "NO FILES FOUND";
                AddFile(noFiles);
                SetSelectorWidth(TextRenderer.MeasureText(noFiles, Font).Width);
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
                if (v.postseason == 1)
                {
                    seriesName.Text = videoSeries.title + " Season: " + v.season + " Special: " + v.episode;
                }
                else
                {
                    seriesName.Text = videoSeries.title + " Season: " + v.season + " Episode: " + v.episode;
                }
                int index = videoSeries.videoFiles.IndexOfKey(v.key);
                videoSelector.SelectedIndex = index;
                if (index == 0)
                {
                    backButton.Enabled = false;
                }
                else
                {
                    backButton.Enabled = true;
                }

                if (index == videoSeries.videoFiles.Count - 1)
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
        }

        private void editButton_Click(object sender, EventArgs e)
        {
           VideoSeriesForm vsf = new VideoSeriesForm(videoSeries);
           VideoTrackerForm vtf = (VideoTrackerForm) this.Parent.Parent;
           if (vsf.ShowDialog() == DialogResult.OK)
           {
               vtf.AddOrUpdateVideoPanel(vsf.videoSeries);
           }
        }
 

    }
}
