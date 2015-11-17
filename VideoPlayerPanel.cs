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
        private VideoSeries videoSeries;
        private VideoTrackerData videoTrackerData;
        private VideoTrackerForm videoTrackerForm;
        public  int initialWidth;
        private bool updateInProgress;

        public VideoPlayerPanel(VideoTrackerData vtd, VideoSeries vs)
        {
            InitializeComponent();
            this.updateInProgress = false;  // Set when a call to UpdatePanel is in progress

            this.videoSeries = vs;
            this.videoTrackerForm = vtd.videoTrackerForm;
            this.videoTrackerData = vtd;

            BeginFileLoad(vs);
            this.videoTrackerForm.AddTitle(this, vs);
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
            videoSeries.PlayCurrent();        }

        private void playNextButton_Click(object sender, EventArgs e)
        {
            nextButton_Click(sender, e);
            playButton_Click(sender, e);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            videoTrackerForm.DeleteTitle(this, videoSeries);
            this.Dispose();
        }

        // Call the EditForm method to bring up the appropriate editing
        // form for this derived class.
        private void editButton_Click(object sender, EventArgs e)
        {
            videoSeries.EditForm(videoTrackerData);
        }

        // Panel drag-and-drop code:
        // - MouseDown event - Start a drop-and-drop sequence, with the input argument
        //   set to the index of the panel being moved.
        // - DragEnter event - Validate that we're dragging in a valid value
        // - DragDrop event - Get the index of the panel being dragged into and call the
        //   main form's MoveTitle method.
        private void flowLayoutPanel_MouseDown(object sender, MouseEventArgs e)
        {
            int index = videoTrackerData.videoSeriesList.IndexOf(this.videoSeries);
            DoDragDrop(index, DragDropEffects.Move);
        }

        private void flowLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (((e.AllowedEffect & DragDropEffects.Move) != 0)
                    && e.Data.GetDataPresent(typeof(int)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void flowLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            int source = (int)e.Data.GetData(typeof(int));
            int dest = videoTrackerData.videoSeriesList.IndexOf(this.videoSeries);
            videoTrackerForm.MoveTitle(source, dest);
        }



        public void BeginFileLoad(VideoSeries vs)
        {
            this.seriesName.Text = "Loading " + vs.title;
            VisibleControls(false);
            this.initialWidth = Width;
        }

        public void EndFileLoad(VideoSeries vs)
        {
            int temp = 0, maxWidth = 0;
            this.videoSelector.Items.Clear();
            foreach (VideoFile f in vs.videoFiles.Values)
            {
                string filename = f.title;
                this.videoSelector.Items.Add(filename);
                temp = TextRenderer.MeasureText(filename, this.videoSelector.Font).Width;
                if (temp > maxWidth) { maxWidth = temp; }
            }
            this.seriesName.Text = vs.title;
            this.SetSelectorWidth(maxWidth);
            this.VisibleControls(true);
            this.UpdatePanel();

            // Allow the panel to autosize itself and save the resulting width. Afterwards, 
            // turn autosizing off again. The main application window will set all panels
            // to match the width of the largest one.
            this.flowLayoutPanel.AutoSize = true;
            this.ResumeLayout(false);
            this.PerformLayout();
            this.initialWidth = flowLayoutPanel.Width;
            this.flowLayoutPanel.AutoSize = false;

            // Update the sizes of the panels in the main application window.
            this.videoTrackerForm.ResizeMainPanel();
            this.videoTrackerForm.WorkerThreadComplete();
        }

        private void SetSelectorWidth(int width)
        {
            this.videoSelector.Width = width + SystemInformation.VerticalScrollBarWidth;
        }
        
        private void UpdatePanel()
        {
            // Prevent recursive calls using the "updateInProgress" variable.
            //
            // Reason: If this routine changes the SelectedIndex field, it triggers
            // a recursive call because it's the handler for the SelectedIndexChanged
            // event.
            if (updateInProgress) { return;  }
            updateInProgress = true;

            SuspendLayout();
            if (!videoSeries.IsValid())
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
                if (v == null)
                {
                    v = videoSeries.videoFiles.First().Value;
                    videoSeries.currentVideo = v;
                }
                int maxIndex = videoSeries.videoFiles.Count - 1;
                int index = videoSeries.videoFiles.IndexOfKey(v.key);
                int remaining = maxIndex - index;
                if (v.postSeason == 1)
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
                playButton.Enabled = true;

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

        private void VisibleControls(bool flag)
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
