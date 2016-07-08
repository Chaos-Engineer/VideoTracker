#
# SAMPLE PLUG-IN FOR VIDEOTRACKER.
#
# This creates a video series with two programs. 
# The first is hard-coded as a link to Youtube.
# The second is given a name and URL provided by the user.
# The user also has the ability to specify an alternate launcher. If no launcer
# is specific then the default web browser will be used.

#
# Import WPF and some .NET classes. Since Python might not be installed on the
# client system, 
#
import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Diagnostics import Process
from Microsoft.Win32 import OpenFileDialog

#
# Import some classes. 
# - "VideoFile" contains information about a single episode
# - "GPK" is Global Plugin Keys, a set of text constants used to populate a data dictionary
# - "SPK" is Series Plugin Keys, another set of text constants used to popular a data dictionary.
#
clr.AddReference('VideoTrackerLib')
import VideoTracker
from VideoTracker import VideoFile, gpk, spk

#
# The plug-in routines below must be at the top level of the file, not contained in any class.
# The routines are:
# - Register         - Required - Provides basic information about the plugin.
# - ConfigureGlobals - Optional - Provides configuration settings that are applied to all
#                                 programs that use this plug-in type.
# - ConfigureSeries  - Required - Provides configuration settings that are applied to a
#                                 single series. This can be used to create a new series
#                                 or edit an existing one.
# - LoadSeries       - Required - Generates a list of VideoFile objects based on the
#                                 contents of the Global and Series dictionaries.
# - Play             - Optional - Plays a specified VideoFile. If this routine is not
#                                 specified, then the file or URL will be launched with 
#                                 the Windows default handler.
#

#
# REQUIRED ROUTINE: REGISTER
#
# This routine is called when a new plug-in is registered with the VideoTracker software. It must 
# assign values to the three Global Plugin Keys. These are stored in a dictionary<string,string> object.
#
# The gpk class just contains the definitions of constant strings to help prevent typoes. For
# example, gpk.NAME is the string "name".
#
# Values should be assigned as follows:
# - gpk.NAME - The name of the plug-in. This must be unique.
# - gpk.ADD  - The string to insert into the "Edit" menu so that the user can invoke the plug-in
# - gpk.DESC - A text description of the plug-in.
# - gkp.FORCECONFIG - If non-null, the ConfigureGlobals routine will be called immediately after 
#   registration.
#
# You may also assign additional keys if you want. The keyname can be any text string, for example:
#    pluginRegisterDictionary["other"] = "Additional data is stored like this"
#
def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "Sample"
    pluginRegisterDictionary[gpk.ADD]   = "Add Sample series"
    pluginRegisterDictionary[gpk.DESC]  = "Sample plug-in to launch a user-specified URL"
    #pluginRegisterDictionary[gpk.FORCECONFIG] = "true"

#
# OPTIONAL ROUTINE: ConfigureGlobals
#
# This routine sets key-value pairs in pluginGlobalDictionary, which are made available
# to every video series that uses this plug-in.
#
# These values are typically set by a form. On entry, the routine should copy the values 
# into the form and then display it. If the form is modified (ShowDialog returns true), then
# the new form values should be copied back into the dictionary. 
#
# In this example, there's a single global called "launcher", which will be used to allow
# the user to override the default file/URL handler when playing the video.
#
# This routine must contain a "form.Owner = parent" line to keep us from going into a state
# where the main program is visible, but this dialog box is hidden behind some other Window
def ConfigureGlobals(parent, pluginGlobalDictionary) :
    form = SampleConfigureGlobals()
    form.Owner = parent
    form.launcher.Text = pluginGlobalDictionary["launcher"]
    if form.ShowDialog() :
        pluginGlobalDictionary["launcher"] = form.launcher.Text
        return True
    else :
        return False

#
# REQUIRED ROUTINE: ConfigureSeries
#
# This routine sets key-value pairs in pluginSeriesDictionary, providing information 
# about the configuration of a single series. 
#
# These values are typically set by a form. On entry, the routine should copy the values 
# into the form and then display it. If the form is modified (ShowDialog returns true), then
# the new form values should be copied back into the dictionary. 
#
# There is one required key: spk.TITLE, which must be set to the name of the series.
# If the spk.CURRENTVIDEO key is set, then if there's a VideoFile.episodeTitle field that
# matches it, it will be set to the current video. This is optional; if it is not set 
# then the current video will be the last video played, or the first video of a new series.
#
# This routine must contain a "form.Owner = parent" line to keep us from going into a state
# where the main program is visible, but this dialog box is hidden behind some other Window
#
# In this example, we also prompt the user for a URL associated with the title.

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = SampleConfigureSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    form.URLBox.Text =  pluginSeriesDictionary["url"]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       pluginSeriesDictionary["url"]  = form.URLBox.Text
       return True
    else :
       return False

#
# REQUIRED ROUTINE: LoadSeries
#
# This routine generates a list of files in a series. 
#
# It takes two inputs, pluginGlobalDictionary and pluginSeriesDictionary. These are the
# data dictionaries built by the previous ConfigureGlobals and ConfigureSeries calls.
#
# The output, videoFiles, is an object of type SortedList<VideoFile>
#
# detailString holds additional information in the event of an error. If the plugin does
# screen-scraping, then this typically holds the HTML of the last page loaded.
#
# SortedList is a standard C# data structure. To add a new element to the list, you must
# provide both the value of a sort key and the object to be added.
#
# VideoFile is an class with the following definition:
#
#    public class VideoFile
#    {
#        public string episodeTitle;     // Episode title to display
#        public string internalName;     // Internal reference to episode (e.g. filename or URL)
#        public int season;              // Season number
#        public int episode;             // Episode number
#        public int special;             // Set to "1" if this is a post-season special
#        public string key;              // Unique value, used to sort episodes
#    }
#
# 
# This sample routine create a series containing two programs. "Season 1 Episode 1" is
# a link to Youtube. "Season 1 Episode 2" is the link specified by the user in the
# ConfigureSeries call.
#
def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, videoFiles, detailString) :

    # Episode 1
    v = VideoFile()
    v.episodeTitle = "Launch Youtube"
    v.internalName = "https://www.youtube.com"
    v.season = 1
    v.episode = 1
    v.special = 0
    v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
    videoFiles.Add(v.key, v);

    # Episode 2.
    #
    # For this plug-in, there is only a single generated episode, populated 
    # from the values in the SeriesDictionary. Normally this routine would 
    # generate a list of episodes based on a web lookup or something similar.
    v = VideoFile()
    v.episodeTitle = pluginSeriesDictionary[spk.TITLE]
    v.internalName = pluginSeriesDictionary["url"]
    v.season = 1
    v.episode = 2
    v.special = 0
    v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
    videoFiles.Add(v.key, v)

    return "" # Indicate no error

def Play(pluginGlobalDictionary, name) :
    launcher = pluginGlobalDictionary["launcher"]
    if launcher == "" :
        return False
    Process.Start(launcher + " " + name)
    return True;



#
# WPF Form controlled by the ConfigureGlobals call.
#
# This allows the user to enter the path of an executable that will be used to
# launch the video. No data validatation is done.
#
class SampleConfigureGlobals(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'SampleConfigureGlobals.xaml')

    # User hit OK, return the text field to the caller.
    def OKButton_Click(self, sender, e):
        self.DialogResult = True
        return

    # Clear text field.
    def ClearButton_Click(self, sender, e):
        self.launcher.Text = ""

    # Open a file browser dialog, and store the result in the text field.
    def BrowseButton_Click(self, sender, e):
        dialog = OpenFileDialog()
        result = dialog.ShowDialog();
        if result == True :
            self.launcher.Text = dialog.FileName
        return
#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name and a URL. Both values are required.
#
# 
class SampleConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'Sample.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Website name must be specified")
            return
        if self.URLBox.Text == "" :
            MessageBox.Show("URL must be specified")
            return
        self.DialogResult = True
        return



    
