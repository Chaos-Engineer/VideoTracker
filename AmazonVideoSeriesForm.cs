using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoTracker
{
    public partial class AmazonVideoSeriesForm : Form
    {
        private AmazonVideoSeries amazonVideoSeries;
        private VideoTrackerData videoTrackerData;

        // One-argument constructor to create a new series
        public AmazonVideoSeriesForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            this.videoTrackerData = vtd;
        }

        // Two-argument constructor to edit an existing series.
        public AmazonVideoSeriesForm(VideoTrackerData vtd, AmazonVideoSeries vs)
        {
            InitializeComponent();
            this.amazonVideoSeries = vs;
            this.videoTrackerData = vtd;
            this.titleBox.Text = vs.title;
            this.keywordBox.Text = vs.keywords;
        }
        
        private void AmazonVideoSeriesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string currentKey;
            // User selected cancel, don't validate or save results.
            if (this.DialogResult == DialogResult.Cancel)
            {
                e.Cancel = false;
                return;
            }

            // Check input for validity
            if (titleBox.Text.Equals(""))
            {
                MessageBox.Show("Title must be specified");
                e.Cancel = true;
                return;
            }
            if (keywordBox.Text.Equals(""))
            {
                keywordBox.Text = titleBox.Text;
            }

            // Create a new series object, or update an existing one.
            if (amazonVideoSeries == null)
            {
                amazonVideoSeries = new AmazonVideoSeries();
                if (!amazonVideoSeries.LoadGlobalSettings(videoTrackerData)) {
                    e.Cancel = true;
                    return;
                }
                currentKey = null;
            } else {
                currentKey = amazonVideoSeries.currentVideo.key;
            }
              
            amazonVideoSeries.Initialize(keywordBox.Text);
            amazonVideoSeries.LoadFiles(titleBox.Text, currentKey, videoTrackerData);

            e.Cancel = false;
        }

    }
}
