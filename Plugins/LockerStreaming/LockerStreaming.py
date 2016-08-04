import re, sys, os
import clr
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Collections.Generic import List
from System.Diagnostics import Process

clr.AddReference('VideoTrackerLib')
import VideoTrackerLib
from VideoTrackerLib import VideoFile, gpk, spk, DynamicHtmlLoader

def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "LockerStreaming"
    pluginRegisterDictionary[gpk.ADD]   = "Add Locker Streaming Series"
    pluginRegisterDictionary[gpk.DESC]  = "Add series from a Locker Streaming site"
    pluginRegisterDictionary[gpk.FORCECONFIG] = "TRUE"


def ConfigureGlobals(parent, pluginGlobalDictionary) :
    form = LockerStreamingConfigureGlobals()
    form.Owner = parent
    form.URL.Text = pluginGlobalDictionary["URL"]
    if form.ShowDialog() :
        pluginGlobalDictionary["URL"] = form.URL.Text
        return True
    else :
        return False

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = LockerStreamingConfigureSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       #pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       return True
    else :
       return False

def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, dynamicHtmlLoader, videoFiles) :
    #
    # Search for the desired program. Output from the search string is:
    # 
    # <a href="[BASE]/tvshow.html" title="TV Show">
    #  
    #
    series = pluginSeriesDictionary[spk.TITLE]
    base = pluginGlobalDictionary["URL"]
    if base == "":
        return "Must set URL in Plugins/Configure"

    url = base + "/search/search.php?q=" + series
    html = dynamicHtmlLoader.Navigate(url)
 
    m = re.search('<a href="(.*?)" title=(.*?' + series + '.*?)>', html, flags=re.IGNORECASE)
    #m = re.search('<a title=(.*?' + series + '.*?) href="(.*?)">', html, flags=re.IGNORECASE)
    if m is None:
        detailString = html
        return List[str](["Series not found at " + url, detailString])
    url = m.group(1)
    title = m.group(2)



    # Find the episodes within the series:
    # 
    # <a href="[base]/TVShow-season-1-episode1.html" 
    # title="TV Show Season 1 Episode 1"><strong>Episode 1</strong></a>
    #
    html = dynamicHtmlLoader.Navigate(url)

    episode = 0
    m = re.findall('<a href="(.*?)" title="(.*?' + series + '.*?)">', html, flags=re.IGNORECASE)
    #m = re.findall('<a title="(.*?' + series + '.*?)" href="(.*?)">', html, flags=re.IGNORECASE)
    for item in (m):
        if item is None:
            continue
        link = item[0]
        episodeTitle = item[1]
        e = re.search('season-(\d*)-episode-(\d*)', link, flags=re.IGNORECASE)
        if e is None:
            continue
        v = VideoFile()
        v.episodeTitle = episodeTitle
        v.internalName = link
        v.season = int(e.group(1))
        v.episode = int(e.group(2))
        v.special = 0
        v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
        videoFiles.Add(v.key, v);
        episode = episode + 1

    if episode == 0:
        detailString = html
        return List[str](["No episodes found at " + url, html])

    return # Indicates no error

#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name and a URL. Both values are required.
#
# 
class LockerStreamingConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'LockerStreaming.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return

class LockerStreamingConfigureGlobals(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'LockerStreamingConfigureGlobals.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.URL.Text == "" :
            MessageBox.Show("URL must be specified")
            return
        self.DialogResult = True
        return


    
