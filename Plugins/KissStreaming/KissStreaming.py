﻿import sys, os
import re

import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Collections.Generic import List
from System.Diagnostics import Process
from Microsoft.Win32 import OpenFileDialog

clr.AddReference('VideoTrackerLib')
import VideoTrackerLib
from VideoTrackerLib import VideoFile, gpk, spk, DynamicHtmlLoader, WindowMode


def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "KissStreaming"
    pluginRegisterDictionary[gpk.ADD]   = "Add Kiss Streaming Series"
    pluginRegisterDictionary[gpk.DESC]  = "Find programs on a Kiss streaming site."
    pluginRegisterDictionary[gpk.FORCECONFIG] = "TRUE";

def ConfigureGlobals(parent, pluginGlobalDictionary) :
    form = KissAnimeConfigureGlobals()
    form.Owner = parent
    form.URL.Text = pluginGlobalDictionary["URL"]
    if form.ShowDialog() :
        pluginGlobalDictionary["URL"] = form.URL.Text
        return True
    else :
        return False

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = KissAnimeConfigureSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    form.URLBox.Text = pluginSeriesDictionary["URL"]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       pluginSeriesDictionary["URL"] = form.URLBox.Text
       return True
    else :
       return False

def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, dynamicHtmlLoader, videoFiles) :

    series = pluginSeriesDictionary[spk.TITLE]
    #
    # Grab the full series list and search for the title. Extract the canonical title
    # and the series URL.
    # 
    # <a href="<base>/Anime/tv-show"><span class=\"title\">TV Show</span></a>
    # <a class="bigChar" href="<base>/Anime/tv-show">TV Show</a>
    base = pluginGlobalDictionary["URL"]
    if base == "":
        return "Must set URL in Plugins/Configure"

    dynamicHtmlLoader.inProgressList.Add("Checking your browser before accessing")
    #
    # Series URL can be entered by the user, or found by using a site
    # search for the program. If it's done via a site search, then add
    # the URL to the dictionary for future use.
    #
    if pluginSeriesDictionary["URL"] != "":
        title = pluginSeriesDictionary[spk.TITLE]
        url = pluginSeriesDictionary["URL"]
    else:
        #url = base + "/search/" + series
        url =  base + "/full-anime-list"
        ##dynamicHtmlLoader.windowMode = WindowMode.Visible
        html = dynamicHtmlLoader.Navigate(url)
        #m = re.search('<a[^<]*?href="([^<]*?)"><span class="title">([^<]*?' + series + '[^<]*?)</span></a>', html, flags=re.IGNORECASE)
        m = re.search('<a[^<]*?href="([^<]*?)">([^<]*?' + series + '[^<]*?)</a>', html, flags=re.IGNORECASE)
        if m is None:
            return List[str](["Series not found at " + url, html])
        url = m.group(1)
        title = m.group(2).strip()
        pluginSeriesDictionary["URL"] = url
        pluginSeriesDictionary[spk.TITLE] = title

    #
    # Load the episode page and build the series list. Episode titles are not guaranteed to have a numeric field
    # (as shown by the "extra-episode" below), but are guaranteed to be listed in reverse order - so build the
    # list and then reverse it to get the episode numbers.
    #
    # <a href="http://base/TV-Show-extra-episode" title="Watch extra episode">
    #    Extra Episode</a>
    # <a href="http://base/TV-Show-episode-26" title="Watch Episode 26">
    #    Episode 26</a>
    #
    html = dynamicHtmlLoader.Navigate(url)
    #m = re.findall('<a href="(.*?)" title="Watch', html)
    m = re.findall('<a[^<]*?href="([^<]*?)"[^<]*?>([^<]*?' + title + '[^<]*?Episode[^<]*?)</a>', html, flags=re.IGNORECASE)
 
    episode = 0
    for item in reversed(m):
        episode += 1
        v = VideoFile()
        v.episodeTitle = item[1].strip()
        v.internalName = item[0]
        v.season = 1
        v.episode = episode
        v.special = 0
        
        v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
        videoFiles.Add(v.key, v)

    if episode == 0:
        return List[str](["No episodes found at " + url, html])

    return # Indicates no error

#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name.
#
# 
class KissAnimeConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'KissStreaming.xaml')

    # User hit OK, return the text field to the caller.
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return

#
# WPF Form controlled by CongigureGlobals call
#
# Allow the user to enter an MP4 player application. If none is specified, the 
# episode will be launched from the web browser.
#
class KissAnimeConfigureGlobals(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'KissStreamingConfigureGlobals.xaml')

    def BrowseButton_Click(self, sender, e):
        openFileDialog = OpenFileDialog()
        if (openFileDialog.ShowDialog() == True):
            self.playerBox.Text = openFileDialog.FileName;

    def OKButton_Click(self, sender, e):
        self.DialogResult = True
        return    
