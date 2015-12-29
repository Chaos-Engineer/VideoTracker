Instructions for adding a new series type:

1 - Declare a new class representing the series type. It should be derived from VideoSeries.
2 - Add an empty constructor that takes no arguments (called during deserialization), and a constructor to be
    called by the Form class, where the arguments are the values defined by the form. (Title, Current Episode,
	and all series-specific arguments)
3 - Add these method headers:
        public override bool LoadGlobalSettings(VideoTrackerData vtd)  [OPTIONAL: IF GLOBAL CONFIGURATION VARIABLES NEEDED]
        public override void PlayCurrent() [OPTIONAL: DEFAULT ROUTINE IS "Process.Start(currentVideo.internalName)"]
        public override void EditForm(VideoTrackerData vtd)
        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
4 - Add an [XmlInclude] for the new class in VideoSeries.cs, to allow it to be serializable.
5 - Declare a new Windows Form class (xxxVideoSeriesForm) to define the form used to enter the series 
    information.
6 - Add a new option to the VideoTrackerForm menu to display that form class.
7 - If there are any new global variables for this class, add them to the VideoTrackerData class.
8 - Add an interface to set these new global variables to the SettingsForm class
9 - Write the (VideoTrackerData) and (VideoTrackerData, xxxVideoSeries) constructors for the Form Class. The
    first one is used to add a new series. The second one is used to edit an existing series and presets the 
	displayed fields to the current series values.
10 - Add the FormClosing event to the Form Class. This should call the multi-argument constructor, the 
    LoadGlobalSettings method if needed and the base LoadFiles method. (LoadFiles will set up a call to 
	the class-specific LoadSeriesAsync method, which will do the actual file load.)
10 - Write the PlayCurrent() routine if the default can't be used
11 - Write the LoadSeriesAsync() routine. This should getting information about each video based on the 
	series values, convert them into VideoFile objects, and add them to the videoFiles collection.

	NUGET PACKAGES:
	- csquery: Used for HTML screen-scraping (CrunchyRollVideoSeries)
	- ookii: Allows access to the Vista-style directory selector dialog, not otherwise available in .NET.

Use "git push origin master" to publish updated package.