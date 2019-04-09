#
# This is a minimal plug-in that you can use as a base when developing
# new plug-ins. See the Sample plug-in for documentation and more
# detailed example code.
#
import sys, os
import re

import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Diagnostics import Process
from System.Collections.Generic import List

clr.AddReference('VideoTrackerLib')
import VideoTrackerLib
from VideoTrackerLib import VideoFile, gpk, spk, DynamicHtmlLoader, WindowMode

def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "Amazon"
    pluginRegisterDictionary[gpk.ADD]   = "Add Amazon Series"
    pluginRegisterDictionary[gpk.DESC]  = "Amazon Prime Videos"
    #pluginRegisterDictionary[gpk.FORCECONFIG] = "true"

# ConfigureGlobals is optional

#def ConfigureGlobals(parent, pluginGlobalDictionary) :
#    form = TemplateConfigureGlobals()
#    form.Owner = parent
#    form.launcher.Text = pluginGlobalDictionary["launcher"]
#    if form.ShowDialog() :
#        pluginGlobalDictionary["launcher"] = form.launcher.Text
#        return True
#    else :
#        return False

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = AmazonConfigureSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    form.UrlBox.Text = pluginSeriesDictionary["URL"]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       pluginSeriesDictionary["URL"] = form.UrlBox.Text
       #pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       return True
    else :
       return False

def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, dynamicHtmlLoader, videoFiles) :
    series = pluginSeriesDictionary[spk.TITLE]
    url = pluginSeriesDictionary["URL"]

    dynamicHtmlLoader.browserRequired = True
    #dynamicHtmlLoader.windowMode = WindowMode.Visible
    #
    # If the URL wasn't specified, then search for the series name and get the
    # URL from the first desktop-auto-sparkle-single block.
    #

    if url == "":
        url = "https://www.amazon.com/s?url=search-alias%3Dinstant-video&field-keywords=" + series
        html = dynamicHtmlLoader.Navigate(url)
        m = re.search('id="desktop-auto-sparkle-single".*?//www.amazon.com/dp/(.*?)\?', html, flags=re.DOTALL|re.IGNORECASE)
        if m is None:
            return List[str](["Series not found", html])
        asin = m.group(1)
        url = "https://www.amazon.com/gp/video/detail/" + asin
        pluginSeriesDictionary["URL"] = url
        
    html = dynamicHtmlLoader.Navigate(url)
    if html.find('Hello. Sign in') != -1:
        savedWindowMode = dynamicHtmlLoader.windowMode
        dynamicHtmlLoader.windowMode = WindowMode.Interactive
        html = dynamicHtmlLoader.Navigate(url)
        dynamicHtmlLoader.windowMode = savedWindowMode
        return List[str](["You must be logged into Amazon in order to use this plug-in. " +
                            "Close this dialog, and then log in using the provided window. " +
                            "Reload the series after logging in.", html])

    #
    # If this is a multi-season show, then the page will have a set of aiv_dp_season_select options.
    # Extract the season ASIN numbers and then get the episodes from each page. Otherwise,
    # get the episodes from the current page.
    #

    #MessageBox.Show("Series ASN=" + asin)
    html = dynamicHtmlLoader.Navigate(url)
    m = re.findall('<option value="(.*?):aiv_dp_season_select".*?<span class="dv-season-selector-text">(.*?)</span>', html)
    if not m:
        return GetSeason(1, "", url, html, videoFiles)
    season = 0
    for item in m:
        season = season+1
        seasonASIN = item[0]
        seasonName = item[1] + " "
        url = "https://www.amazon.com/dp/" + seasonASIN
        html = dynamicHtmlLoader.Navigate(url);
        status = GetSeason(season, seasonName, url, html, videoFiles)
        if status: # An error was detected
            return status 
     
    #return List[str](["error", "details"])  # Sends an error with details
    return "" # Indicates no error

def GetSeason(season, seasonName, url, html, videoFiles):
    #MessageBox.Show("Season URL = " + url)
    blocks = getblocks('<div class="dv-episode-playback-title', '<div class="dv-episode-playback-title', html)
    if not blocks:
        return List[str](["Can't find episode list in " + url, html])
    #
    # For each season, get the episode list
    #
    episode=0
    for block in blocks:
        m2 = re.search('"(/gp/video/detail/.*?)/.*?<div class="dv-el-title"> <!-- Title -->\s*(.*?)\s*</div>', block, flags=re.IGNORECASE|re.DOTALL)
        if m2 is None:
            return List[str](["Can't find episode in block at " + url, block])
        #MessageBox.Show(item[1] + " " + m2.group(1) + " " + m2.group(2))
        episode = episode+1
        episodeASIN = m2.group(1)
        episodeName = m2.group(2)
        v = VideoFile()
        v.episodeTitle = seasonName + episodeName
        v.internalName = "https://www.amazon.com/" + episodeASIN
        v.season = season
        v.episode = episode
        v.special = 0
        v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
        videoFiles.Add(v.key, v)
    return ""

#
# Get a list of blocks of text from string, where the startToken
# and endToken strings delimit the blocks
#
def getblocks(startToken, endToken, text):
    begin = text.find(startToken, 0)
    end = 0
    list = []
    while (begin != -1):
        end = text.find(endToken, begin)
        if (end == -1):
            return list
        else:
            list.append(text[begin:end])
            begin = text.find(startToken, end)
    return list

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
class AmazonConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'Amazon.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return



    
