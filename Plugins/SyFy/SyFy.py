import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Diagnostics import Process

clr.AddReference('VideoTrackerLib')
import VideoTracker
from VideoTracker import VideoFile, gpk, spk

import re
import urllib2

def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "SyFy"
    pluginRegisterDictionary[gpk.ADD]   = "Add SyFy Series"
    pluginRegisterDictionary[gpk.DESC]  = "Find programs on http://www.syfy.com"

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = SyFyConfigureSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       #pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       return True
    else :
       return False

def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, videoFiles) :

    #
    # Grab the full series list and search for the title. Extract the canonical title
    # and the series URL.
    # 
    # <a href="/themagicians">The Magicians</a>
    # The HREF string is matched with /[^/]*, which gets URLs that contain only one slash
    # at the beginning. 
    #
    series = pluginSeriesDictionary[spk.TITLE]
    url = "http://syfy.com/episodes"
    h = urllib2.urlopen(url);
    html = h.read()

    m = re.search('<a href="(/[^/]*)">(.*' + series + '.*)</a>', html, flags=re.IGNORECASE)
    if m is None:
        return "Series not found at " + url
    url = "http://syfy.com" + m.group(1) + "/episodes"
    
    #
    # Load the episode page and build the series list. The "href" is the internal name of the series, and the link
    # text is the epsiode name. Episodes are not guaranteed to have a numeric field (as shown by the "redial-episode"
    # below), but are always listed in reverse order - so we can build the list and then reverse it to get the 
    # episode numbers.
    #
    # <a href="http://www.syfy.com/themagicians/videos/101-unauthorized-magic-0">Watch Full Episode</a>
    #
    h = urllib2.urlopen(url);
    html = h.read()

    m = re.findall('<a href="(.*)">Watch Full Episode</a>', html)
    episode = 0
    for item in reversed(m):
        episode += 1
        v = VideoFile()
        v.episodeTitle = item.split('/')[-1].replace("-"," ").title()
        v.internalName = item
        v.season = 1
        v.episode = episode
        v.special = 0
        
        v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
        videoFiles.Add(v.key, v)

    return "" # Indicates no error

#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name and a URL. Both values are required.
#
# 
class SyFyConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'SyFy.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return



    
