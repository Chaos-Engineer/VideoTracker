#
# This is a minimal plug-in that you can use as a base when developing
# new plug-ins. See the Sample plug-in for documentation and more
# detailed example code.
#
import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Diagnostics import Process
from System.Collections.Generic import List

clr.AddReference('VideoTrackerLib')
import VideoTracker
from VideoTracker import VideoFile, gpk, spk

def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "NAME"
    pluginRegisterDictionary[gpk.ADD]   = "Add [NAME] Series"
    pluginRegisterDictionary[gpk.DESC]  = "Description of [NAME]"
    #pluginRegisterDictionary[gpk.FORCECONFIG] = "true"


# ConfigureGlobals is optional

#def ConfigureGlobals(pluginGlobalDictionary, parent) :
#    form = SampleConfigureGlobals()
#    form.Owner = parent
#    form.launcher.Text = pluginGlobalDictionary["launcher"]
#    if form.ShowDialog() :
#        pluginGlobalDictionary["launcher"] = form.launcher.Text
#        return True
#    else :
#        return False

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = TemplateConfigureSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       #pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       return True
    else :
       return False

def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, videoFiles) :
    # Episode 1
    v = VideoFile()
    v.episodeTitle = "[NAME]"
    v.internalName = "[INTERNAL NAME]"
    v.season = 1
    v.episode = 1
    v.special = 0
    v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
    videoFiles.Add(v.key, v);

    #return List[str](["error", "details"])  # Sends an error with details
    return "" # Indicates no error

#
# Play is optional
#

#def Play(pluginGlobalDictionary, name) :
#    launcher = pluginGlobalDictionary["launcher"]
#    if launcher == "" :
#        return False
#    Process.Start(launcher + " " + name)
#    return True;

#
# TemplateConfigureGlobals form is optional.
#

#class TemplateConfigureGlobals(Window):
#    def __init__(self):
#        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + '<CONFIGURE GLOBALS>.xaml')
#
#    # User hit OK, return the text field to the caller.
#    def OKButton_Click(self, sender, e):
#        # Validation comes here
#        self.DialogResult = True
#        return

#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name and a URL. Both values are required.
#
# 
class TemplateConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'Template.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return



    
