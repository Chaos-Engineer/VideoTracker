using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoTracker
{
    public partial class AmazonVideoSeriesForm : Window
    {
        private AmazonVideoSeries amazonVideoSeries;
        private VideoTrackerData videoTrackerData;

        // One-argument constructor to create a new series
        public AmazonVideoSeriesForm(VideoTrackerData vtd)
        {
            InitializeComponent();
            this.videoTrackerData = vtd;
            this.Owner = vtd.videoTrackerForm;
        }

        // Two-argument constructor to edit an existing series.
        public AmazonVideoSeriesForm(VideoTrackerData vtd, AmazonVideoSeries vs)
        {
            InitializeComponent();
            this.amazonVideoSeries = vs;
            this.videoTrackerData = vtd;
            this.Owner = vtd.videoTrackerForm;
            this.titleBox.Text = vs.seriesTitle;
            this.keywordBox.Text = vs.keywords;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string currentKey;

            // Check global variables
            if (String.IsNullOrWhiteSpace(videoTrackerData.globals[gdg.AMAZON][gdk.PUBLICKEY]) ||
                  String.IsNullOrWhiteSpace(videoTrackerData.globals[gdg.AMAZON][gdk.SECRETKEY]) ||
                  String.IsNullOrWhiteSpace(videoTrackerData.globals[gdg.AMAZON][gdk.AFFILIATEID]))
            {
                App.ErrorBox("Amazon Affiliate ID parameters must be set before loading " +
                    "Amazon On-Demand Video programs");
                SettingsForm s = new SettingsForm(videoTrackerData);
                foreach (TabItem t in s.tabControl.Items) {
                    if (t.Name.Equals("amazon")) s.tabControl.SelectedItem = t;
                }
                s.ShowDialog();
                return;
            }


            // Check input for validity
            if (titleBox.Text.Equals(""))
            {
                App.ErrorBox("Title must be specified");
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
                currentKey = null;
            }
            else
            {
                currentKey = amazonVideoSeries.currentVideo.key;
            }

            amazonVideoSeries.InitializeFromForm(keywordBox.Text);
            amazonVideoSeries.LoadSeries(titleBox.Text, currentKey, videoTrackerData);

            this.DialogResult = true;
        }
    }
}
