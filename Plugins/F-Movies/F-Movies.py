import sys, os
import re

import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Diagnostics import Process
from System.Collections.Generic import List
from urlparse import urlparse

clr.AddReference('VideoTrackerLib')
import VideoTrackerLib
from VideoTrackerLib import VideoFile, gpk, spk, DynamicHtmlLoader, WindowMode

def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "FMovies"
    pluginRegisterDictionary[gpk.ADD]   = "Add F-Movies Streaming Series"
    pluginRegisterDictionary[gpk.DESC]  = "Series from an F-Movies Streaming Site"
    pluginRegisterDictionary[gpk.FORCECONFIG] = "true"


# ConfigureGlobals is optional

def ConfigureGlobals(parent, pluginGlobalDictionary) :
    form = FMovieConfigureGlobals()
    form.Owner = parent
    form.URL.Text = pluginGlobalDictionary["URL"]
    if form.ShowDialog() :
        pluginGlobalDictionary["URL"] = form.URL.Text
        return True
    else :
        return False

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = FMovieConfigureSeries()
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
    base = pluginGlobalDictionary["URL"]
    if base == "":
        return "Must set URL in Plugins/Configure"
    series = pluginSeriesDictionary[spk.TITLE]

    #
    # Series URL can be entered by the user, or found by using a site
    # search for the program. If it's done via a site search, then add
    # the URL to the dictionary for future use.
    #
    # The series URL can be on a different host than the F-Movies default.
    # Extract the host name from the series URL to use when constructing the
    # invididual episode URLs
    #
    if pluginSeriesDictionary["URL"] != "":
        title = pluginSeriesDictionary[spk.TITLE]
        url = pluginSeriesDictionary["URL"]
        up = urlparse(url)
        base = up.scheme + "://" + up.netloc
    else:
        url =  base + "/sitemap"
        dynamicHtmlLoader.browserRequired = True
        dynamicHtmlLoader.inProgressList.Add("window.CloudFlare")
        #dynamicHtmlLoader.windowMode = WindowMode.Visible
        html = dynamicHtmlLoader.Navigate(url)

        # Series: <a ... href="url" ...>Title<
        nab="[^<]*?" # Matches string with no "<" angle bracket
        m=re.search('<a' + nab + 'href="(' + nab + ')"'+nab+'>('+nab+series+nab+')<',html,flags=re.IGNORECASE)
        if m is None:
            return List[str](["Series not found at " + url, html])

        url = base + m.group(1)
        title = m.group(2)
        pluginSeriesDictionary[spk.TITLE] = title
        pluginSeriesDictionary["URL"] = url


    # Series is in the first block of HTML at:
    #    <i class="fa fa-server"> ... 
    # This can span multiple lines, so use the "re.DOTALL" option for the search to work.
    # At a later time, we may wish to allow the server block to be selected by the user.
    html = dynamicHtmlLoader.Navigate(url)
    
    m = re.search('<I class="fa fa\-server">(.*?)<I class', html, flags=re.DOTALL|re.IGNORECASE)
    if m is None:
        return List[str](["Episode list not found at " + url, html])

    #
    # HTML blocks contain:
    # <A href="/film/url.zmj2/kk4vlw" data-id="kk4vlw">1</A> 
    seriesHtml = m.group(1)
    m = re.findall('<a[^<]*?href="([^<]*?)"[^<]*?>([^<]*?)<', seriesHtml, flags=re.IGNORECASE)

    episode = 0
    for item in m:
        episode += 1
        v = VideoFile()
        v.episodeTitle = "Episode " + item[1]
        v.internalName = base + item[0]
        v.season = 1
        v.episode = episode
        v.special = 0
        v.key = "%04d%04d%04d%s" % (v.season, v.special, v.episode, v.episodeTitle)
        videoFiles.Add(v.key, v)

    if episode == 0:
        return List[str](["No episodes found at " + url, html])

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

class FMovieConfigureGlobals(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'F-MoviesConfigureGlobals.xaml')

        # User hit OK, return the text field to the caller.
    def OKButton_Click(self, sender, e):
        if self.URL.Text == "":
            MessageBox.Show("Base URL must be specified")
            return
        self.DialogResult = True
        return

#
# WPF Form controlled by the ConfigureSeries call.
#
# Allow the user to enter an episode name and a URL. Both values are required.
#
# 
class FMovieConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'F-Movies.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return



    
