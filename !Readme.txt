Instructions for adding a new series type:

1 - Declare a new class representing the series type. It should be derived from VideoSeries.
2 - Add these routine headers:
        public override bool LoadGlobalSettings(VideoTrackerData vtd)  [ONLY IF GLOBAL CONFIGURATION VARIABLES NEEDED]
        public override void PlayCurrent()
        public override void EditForm(VideoTrackerData vtd)
        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
3 - Add an XmlInclude for the new class in VideoSeries.cs, to allow it to be serializable.
4 - Declare a new Windows Form class (xxxVideoSeriesForm) to allow the series information to be entered.
5 - Add a new option to the VideoTrackerForm menu to launch that form class.
6 - Write the (VideoTrackerData) and (VideoTrackerData, xxxVideoSeries) initializers for the Form Class
7 - Add the FormClosing event to the Form Class
8 - If there are any new global variables for this class, add them to the VideoTrackerData class.
9 - Add an interface to set the new global variables to SettingsForm
10 - Write the series type routines. Most of them can be copied from existing code with slight modifications. 
    LoadSeriesAsync should do the actual work of getting information about each video and adding it to the
	videoFiles member.
