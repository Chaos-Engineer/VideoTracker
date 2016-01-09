#
# SAMPLE PLUG-IN FOR VIDEOTRACKER.
#
# This creates a video series with two programs. 
# The first is hard-coded as a link to Youtube.
# The second is given a name and URL provided by the user.
# The user also has the ability to specify an alternate launcher. If no launcer
# is specific then the default web browser will be used.
import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
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
#
# You may also assign additional keys if you want. The keyname can be any text string, for example:
#    pluginRegisterDictionary["other"] = "Additional data is stored like this"
#
def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "sample"
    pluginRegisterDictionary[gpk.ADD]   = "Add Sample series"
    pluginRegisterDictionary[gpk.DESC]  = "Sample plug-in to launch a user-specified URL"

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
def ConfigureGlobals(pluginGlobalDictionary) :
    form = SampleConfigureGlobals()
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
# In this example, we also prompt the user for a URL associated with the title.

def ConfigureSeries(pluginSeriesDictionary) :
    form = SampleConfigureSeries()
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    form.URLBox.Text =  pluginSeriesDictionary["url"]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       pluginSeriesDictionary["url"]  = form.URLBox.Text
       return True
    else :
       return False

 
def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, videoFiles) :
    v = VideoFile()

    v.episodeTitle = "Launch Youtube"
    v.internalName = "https://www.youtube.com"
    v.season = 1
    v.episode = 1
    v.postSeason = 0
    v.key = "001001"
    videoFiles.Add(v.key, v);

    # For this plug-in, there is only a single generated episode, populated 
    # from the values in the SeriesDictionary. Normally this routine would 
    # generate a list of episodes based on a web lookup or something similar.
    v = VideoFile()
    v.episodeTitle = pluginSeriesDictionary[spk.TITLE]
    v.internalName = pluginSeriesDictionary["url"]
    v.season = 1
    v.episode = 2
    v.postSeason = 0
    v.key = "001002"
    videoFiles.Add(v.key, v)

    return True


class SampleConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, 'D:\MyProjects\VideoTracker\Plugins\SampleConfigureSeries.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Website name must be specified")
            return
        if self.URLBox.Text == "" :
            MessageBox.Show("URL must be specified")
            return
        self.DialogResult = True
        return

class SampleConfigureGlobals(Window):
    def __init__(self):
        wpf.LoadComponent(self, 'D:\MyProjects\VideoTracker\Plugins\SampleConfigureGlobals.xaml')
    
    def OKButton_Click(self, sender, e):
        self.DialogResult = True
        return

    def ClearButton_Click(self, sender, e):
        self.launcher.Text = ""

    def BrowseButton_Click(self, sender, e):
        dialog = OpenFileDialog()
        result = dialog.ShowDialog();
        if result == True :
            self.launcher.Text = dialog.FileName
        return
