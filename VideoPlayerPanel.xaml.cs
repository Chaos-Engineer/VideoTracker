using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Media;



namespace VideoTracker
{
    public partial class VideoPlayerPanel : UserControl
    {
        private VideoSeries videoSeries;
        private VideoTrackerData videoTrackerData;
        private VideoTrackerForm videoTrackerForm;
        public  Size requiredSize = new Size(); 
        private bool updateInProgress;
        private double videoSelectorControlWidth = 0;

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


        private void videoSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VideoSeries vs = videoSeries;
            int index = videoSelector.SelectedIndex;
            vs.currentVideo = vs.videoFiles.Values[index];
            UpdatePanel();
        }

        private void videoSelector_DropDownClosed(object sender, EventArgs e)
        {
            this.playButton.Focus(); // Hack to remove highlighting from drop-down menu
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            try
            {
                videoSeries.Play();
            }
            catch (Exception ex)
            {
                App.ErrorBox( "Unable to play file\n" + ex.ToString());
            }
        }

        private void playNextButton_Click(object sender, EventArgs e)
        {
            nextButton_Click(sender, e);
            playButton_Click(sender, e);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            videoTrackerForm.DeleteTitle(this, videoSeries);
        }

        // Call the EditForm method to bring up the appropriate editing
        // form for this derived class.
        private void editButton_Click(object sender, EventArgs e)
        {
            videoSeries.EditForm(videoTrackerData);
        }

        // Panel drag-and-drop code:
        // - MouseDown event - Start a drag-and-drop sequence, with the input argument
        //   set to the index of the panel being moved.
        // - DragEnter event - Validate that we're dragging in a valid value
        // - DragDrop event - Get the index of the panel being dragged into and call the
        //   main form's MoveTitle method.

        private void flowLayoutPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = videoTrackerData.videoSeriesList.IndexOf(this.videoSeries);
            DragDrop.DoDragDrop(this, index, DragDropEffects.Move); /// This is probably wrong.
        }

        private void flowLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (((e.AllowedEffects & DragDropEffects.Move) != 0)
                    && e.Data.GetDataPresent(typeof(int)))
            {
                e.Effects = DragDropEffects.Move;
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
            // Set the panel to "Loading title..." and calculate 
            // the required panel size based on that.
            VisibleControls(false);
            this.Width = Double.NaN;        // Allow width to be dynamically calculated
            this.seriesName.Content = "Loading " + vs.seriesTitle;
            this.UpdateLayout();
            this.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            this.requiredSize.Width = this.ActualWidth;
            this.Width = this.ActualWidth;  // Set width to actual value.
            videoTrackerForm.ResizeMainPanel();
        }

        public void EndFileLoad(VideoSeries vs)
        {
            double videoSelectorWidth = 0;
            Typeface typeface = new Typeface(this.videoSelector.FontFamily,
                 this.videoSelector.FontStyle,
                 this.videoSelector.FontWeight,
                 this.videoSelector.FontStretch);

            this.UpdatePanel();
            this.VisibleControls(true);
            this.UpdateLayout();
            if (videoSelectorControlWidth == 0)
            {
                // Get the size of the ComboBox drop-down button and the margins/padding/borders. This works,
                // but try temporarily setting the videoSelector content to the longest string and 
                // then getting the size of the box.
                this.videoSelector.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                videoSelectorControlWidth =
                    this.videoSelector.Margin.Left + 
                    this.border.Margin.Left + this.border.BorderThickness.Left +
                    this.videoSelector.DesiredSize.Width +
                    this.videoSelector.Margin.Right + 
                    this.border.Margin.Right + this.border.BorderThickness.Right;
            }
            // Loop through the list of titles and add them to the combox box, also getting the 
            // length of the longest one. 
            foreach (VideoFile f in vs.videoFiles.Values)
            {
                string filename = f.episodeTitle;
                this.videoSelector.Items.Add(filename);
                FormattedText formattedText = new FormattedText(
                    filename,
                    CultureInfo.CurrentUICulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    this.videoSelector.FontSize,
                    System.Windows.Media.Brushes.Black);
                double width = formattedText.Width;
                videoSelectorWidth = Math.Max(videoSelectorWidth, width);
            }
            videoSelectorWidth += videoSelectorControlWidth;


            // Calculate the minimum required size for the panel. If this is greater 
            // than the length of the dropdown menu, then it becomes the required
            // size of the panel. Otherwise the required size is the dropdown menu
            // length
 

            this.Width = Double.NaN;        // Allow width to be dynamically calculated
            this.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            this.requiredSize.Width = Math.Max(videoSelectorWidth, this.DesiredSize.Width);

            this.videoTrackerForm.WorkerThreadComplete();
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

            using (var d = Dispatcher.DisableProcessing())
            {
                if (!videoSeries.IsValid())
                {
                    // No files were found for this video series
                    videoSelector.SelectedIndex = 0;
                    videoSelector.IsEnabled = false;
                    backButton.IsEnabled = false;
                    nextButton.IsEnabled = false;
                    playNextButton.IsEnabled = false;
                    playButton.IsEnabled = false;
                    seriesName.Content = videoSeries.seriesTitle; // Title only, no episode number available.
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
                    if (v.special == 1)
                    {
                        seriesName.Content = videoSeries.seriesTitle + " Season: " + v.season +
                                        " Special: " + v.episode +
                                        " (" + remaining + " remaining)";
                    }
                    else
                    {
                        seriesName.Content = videoSeries.seriesTitle + " Season: " + v.season +
                                        " Episode: " + v.episode +
                                        " (" + remaining + " remaining)";
                    }

                    videoSelector.IsEnabled = true;
                    videoSelector.SelectedIndex = index;
                    playButton.IsEnabled = true;

                    // Disable the "back" button for the first video, and the
                    // "next" and "play next" buttons for the last video
                    if (index == 0)
                    {
                        backButton.IsEnabled = false;
                    }
                    else
                    {
                        backButton.IsEnabled = true;
                    }

                    if (index == maxIndex)
                    {
                        playNextButton.IsEnabled = false;
                        nextButton.IsEnabled = false;
                    }
                    else
                    {
                        playNextButton.IsEnabled = true;
                        nextButton.IsEnabled = true;
                    }
                }

                videoTrackerForm.CheckAutoSave();
                updateInProgress = false;
            }
        }

        private void VisibleControls(bool flag)
        {
            this.videoSelector.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            this.nextButton.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            this.backButton.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            this.editButton.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            this.deleteButton.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            this.playButton.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            this.playNextButton.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
