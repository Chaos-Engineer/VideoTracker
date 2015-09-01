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

    public partial class CrunchyRollVideoSeriesForm : Form
    {
        private VideoTrackerData videoTrackerData;
        private CrunchyRollVideoSeries crunchyRollVideoSeries;

        public CrunchyRollVideoSeriesForm(VideoTrackerData vtd)
        {
            this.videoTrackerData = vtd;
            InitializeComponent();
        }

        public CrunchyRollVideoSeriesForm(VideoTrackerData vtd, CrunchyRollVideoSeries cs)
        {
            InitializeComponent();
            this.crunchyRollVideoSeries = cs;
            this.videoTrackerData = vtd;
            this.Title.Text = cs.title;
            this.URL.Text = cs.URL;
        }

        private void CrunchyRollVideoSeriesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // User selected cancel, don't validate or save results.
            if (this.DialogResult == DialogResult.Cancel)
            {
                e.Cancel = false;
                return;
            }

            // Check input for validity
            if (Title.Text.Equals(""))
            {
                MessageBox.Show("Title must be set");
                e.Cancel = true;
                return;
            }

            //
            // If no URL is specified, try to deduce it from the title.
            //
            // Convert the URL to a standard all-lowercase form for future pattern-matching.
            //
            if (URL.Text.Equals("")) URL.Text = Title.Text.Replace(' ','-').ToLower();
            int index = URL.Text.LastIndexOf('/');
            if (index < 0) {
                URL.Text = VideoTrackerData.CrunchyRollUrlPrefix + "/" + URL.Text.ToLower();
            } else {
                URL.Text = VideoTrackerData.CrunchyRollUrlPrefix + URL.Text.Substring(index).ToLower();
            }

            // Create a new series object, or update an existing one.

            if (crunchyRollVideoSeries == null) {
                crunchyRollVideoSeries = new CrunchyRollVideoSeries(URL.Text, Title.Text);
                if (!crunchyRollVideoSeries.LoadGlobalSettings(videoTrackerData))
                {
                    e.Cancel = true;
                    return;
                }

            }
            crunchyRollVideoSeries.LoadFiles(Title.Text, "", videoTrackerData);
            e.Cancel = false;
        }
    }
}
