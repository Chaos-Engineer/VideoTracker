import sys, os
import re

import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Diagnostics import Process
from Microsoft.Win32 import OpenFileDialog

clr.AddReference('VideoTrackerLib')
import VideoTracker
from VideoTracker import VideoFile, gpk, spk

# Append the "..\VideoTrackerUtils" directory relative to this file.
sys.path.append(os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "\\VideoTrackerUtils")
from HtmlLoader import HtmlLoader


def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "Kiss-Anime"
    pluginRegisterDictionary[gpk.ADD]   = "Add Kiss-Anime Series"
    pluginRegisterDictionary[gpk.DESC]  = "Find programs on http://kiss-anime.tv. (Use CONFIGURE to set media player)"

def ConfigureGlobals(parent, pluginGlobalDictionary) :
    form = KissAnimeConfigureGlobals()
    form.Owner = parent
    form.playerBox.Text = pluginGlobalDictionary["player"]
    if form.ShowDialog() :
        pluginGlobalDictionary["player"] = form.playerBox.Text
        return True
    else :
        return False

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = KissAnimeConfigureSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       #pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       return True
    else :
       return False

def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, videoFiles) :

    series = pluginSeriesDictionary[spk.TITLE]
    #
    # Grab the full series list and search for the title. Extract the canonical title
    # and the series URL.
    # 
    # <a class="bigChar" href="http://kiss-anime.tv/Anime/mirai-nikki">Mirai Nikki</a>
    url = "http://kiss-anime.tv/full-anime-list"
    h = HtmlLoader(url);
    if (h.error != ""):
        return h.error

    html = h.read()
    m = re.search('<a class="bigChar" href="(.*)">(.*' + series + '.*)</a>', html, flags=re.IGNORECASE)
    if m is None:
        return "Series not found"
    url = m.group(1)
    title = m.group(2)
    
    #
    # Load the episode page and build the series list. Episode titles are not guaranteed to have a numeric field
    # (as shown by the "redial-episode" below), but are guaranteed to be listed in reverse order - so build the
    # list and then reverse it to get the episode numbers.
    #
    # <a href="http://kiss-anime.tv/Anime-mirai-nikki-redial-episode" title="Watch anime Mirai Nikki Redial Episode online in high quality">
    #    Mirai Nikki Redial Episode</a>
    #<a href="http://kiss-anime.tv/Anime-mirai-nikki-episode-26" title="Watch anime Mirai Nikki Episode 26 online in high quality">
    #    Mirai Nikki Episode 26</a>
    h = HtmlLoader(url);
    if (h.error != ""):
        return h.error
    html = h.read();
    m = re.findall('<a href="(.*)" title="Watch', html)
    episode = 0
    for item in reversed(m):
        episode += 1
        v = VideoFile()
        v.episodeTitle = item.split('/')[-1]
        v.internalName = item
        v.season = 1
        v.episode = episode
        v.special = 0
        
        v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
        videoFiles.Add(v.key, v)

    return "" # Indicates no error

#
# Play is optional
#
# Launching through a streaming media player doesn't seem to work well - video halts after
# 5-10 minutes and needs to be reloaded. Playing through the browser is fine.
#
def Play(pluginGlobalDictionary, url) :
    player = pluginGlobalDictionary["player"]
    if (player == ""):
        return False

    # Get MP4 file. HTML format is:
    #
    # Download (Save as...): <a href="http://kiss-anime.tv/download.php?id=73002">
    # Mirai_Nikki_Episode_19.mp4</a></div>
    h = HtmlLoader(url);
    if (h.error != ""):
        return h.error
    html = h.read();
    m = re.search('<a href="(http://kiss-anime.tv/download.php\?id=.*)"', html)
    if m is None:
        return "Episode not found"
    file = m.group(1)

    # The link will contain a redirect - this code resolves it.
    h = HtmlLoader(file)
    file = h.handle.url

    Process.Start(player, h.handle.url)
    return True;


#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name.
#
# 
class KissAnimeConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'Kiss-Anime.xaml')

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
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'Kiss-AnimeConfigureGlobals.xaml')

    def BrowseButton_Click(self, sender, e):
        openFileDialog = OpenFileDialog()
        if (openFileDialog.ShowDialog() == True):
            self.playerBox.Text = openFileDialog.FileName;

    def OKButton_Click(self, sender, e):
        self.DialogResult = True
        return



    
