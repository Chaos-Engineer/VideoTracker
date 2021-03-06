﻿import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Collections.Generic import List
from System.Diagnostics import Process

clr.AddReference('VideoTrackerLib')
import VideoTrackerLib
from VideoTrackerLib import VideoFile, gpk, spk, DynamicHtmlLoader

import urllib2
import json

def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "Funimation"
    pluginRegisterDictionary[gpk.ADD]   = "Add Funimation Series"
    pluginRegisterDictionary[gpk.DESC]  = "Load series from Funimation website"

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = ConfigureFunimationSeries()
    form.Owner = parent
    form.NameBox.Text = pluginSeriesDictionary[spk.TITLE]
    if form.ShowDialog() :
       pluginSeriesDictionary[spk.TITLE] = form.NameBox.Text
       pluginSeriesDictionary[spk.CURRENTVIDEO] = form.NameBox.Text
       return True
    else :
       return False

def LoadSeries(pluginGlobalDictionary, pluginSeriesDictionary, dynamicHtmlLoader, videoFiles) :
    # Get the website index. Data is returned in JSON format, which can be converted to
    # a list of dictionaries with the json.load call.
    index_url = "http://www.funimation.com/feeds/ps/shows?ut=FunimationSubscriptionUser&limit=9999"
    try:
        handle = urllib2.urlopen(index_url)
    except:
        errorString = "Can't open " + index_url
        return errorString
    if handle.info()['content-type'] != 'application/json':
        errorString = "Can't open " + index_url    
        return errorString

    # Find the requested series in the index. Get the "asset_id" to build the episode list,
    # and the "link" field which will be part of the episode URL.
    series = pluginSeriesDictionary[spk.TITLE]
    series_url = ""
    content = json.load(handle)
    for member in content:
        if series.upper() in member["series_name"].upper():
            asset_id = member["asset_id"]
            # Sometimes the Funimation "welcome" page causes urllib2 to go into a "302" 
            # redirect loop. When that happens, member["link"] is usually valid...
            try:
                handle = urllib2.urlopen(member["link"])
                series_url = handle.url + "/videos/official/"
            except:
                # Ignore exception
                series_url = member["link"] + "/vidoes/official/"
    if series_url == "":
        errorString = "Series not found"
        return errorString

    # Get the episodes and build the VideoFiles list.
    url = "http://www.funimation.com/feeds/ps/videos?ut=FunimationSubscriptionUser&limit=3000&show_id=" + asset_id
    try:
        handle = urllib2.urlopen(url)
    except:
        return "Unable to load " + url
    content = json.load(handle)
    content = content["videos"]
    for member in content:
            # Ignore dubs.
            if member["dub_sub"] == "dub":
                continue
            v = VideoFile();
            v.episodeTitle = member["sequence"] + " - " + member["title"]
            v.internalName = series_url + member["url"] + "?watch=sub"
            v.season = int(member["season_number"])
            v.episode = int(member["sequence"])
            v.special = 0
            v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)           
            videoFiles.Add(v.key, v);
    return "" #  Indicates no error

#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name and a URL. Both values are required.
#
# 
class ConfigureFunimationSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'Funimation.xaml')

    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return



    
