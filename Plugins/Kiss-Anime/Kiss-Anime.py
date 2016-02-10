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
    pluginRegisterDictionary[gpk.NAME]  = "Kiss-Anime"
    pluginRegisterDictionary[gpk.ADD]   = "Add Kiss-Anime Series"
    pluginRegisterDictionary[gpk.DESC]  = "Find programs on http://kiss-anime.tv"

def ConfigureSeries(parent, pluginSeriesDictionary) :
    form = SampleConfigureSeries()
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
    # Grab the full series list and search for the title. Extract the series URL
    # 
    # Regex matches:
    #    <a class="bigChar" href="http://kiss-anime.tv/Anime/mirai-nikki">Mirai Nikki</a>
    #
    series = pluginSeriesDictionary[spk.TITLE]
    url = "http://kiss-anime.tv/full-anime-list"
    h = HtmlLoader(url);
    if (h.error != ""):
        return h.error

    m = re.search('<a class="bigChar" href="(\S*)">.*' + series + '.*</a>', h.html, flags=re.IGNORECASE)
    if m is None:
        return "Series not found at " + url
    url = m.group(1)
    
    #
    # Load the episode page and build the series list. The "href" is the internal name of the series, and the link
    # text is the epsiode name. Episodes are not guaranteed to have a numeric field (as shown by the "redial-episode"
    # below), but are always listed in reverse order - so we can build the list and then reverse it to get the 
    # episode numbers.
    #
    # <a href="http://kiss-anime.tv/Anime-mirai-nikki-redial-episode" title="Watch anime Mirai Nikki Redial Episode online in high quality">
    #    Mirai Nikki Redial Episode</a>
    # <a href="http://kiss-anime.tv/Anime-mirai-nikki-episode-26" title="Watch anime Mirai Nikki Episode 26 online in high quality">
    #    Mirai Nikki Episode 26</a>
    h = HtmlLoader(url);
    if (h.error != ""):
        return h.error

    m = re.findall('<a href="(.*)" title="Watch', h.html)
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

def Play(pluginGlobalDictionary, name) :
    # Get MP4 file. HTML format is:
    #
    # Download (Save as...): <a href="http://kiss-anime.tv/download.php?id=73002">
    # Mirai_Nikki_Episode_19.mp4</a></div>
    h = HtmlLoader(name);
    if (h.error != ""):
        return h.error

    m = re.search('<a href="(http://kiss-anime.tv/download.php\?id=\S*)"', h.html)
    if m is None:
        return False

    file = m.group(1)
    Process.Start(file)
    return True;


# Return the HTML from a web page, or report an error if the site
# is unavailable.
class HtmlLoader:
    def __init__(self, url):
        #
        # The kiss-anime website returns a 403 error if no user agent
        # is specified.
        #
        userAgent = "Wget/1.16.1 (cygwin)"
        headers = { 'User-Agent' : userAgent }
        req = urllib2.Request(url, "", headers)
        self.error = ""
        try:
            handle = urllib2.urlopen(req)
        except urllib2.HTTPError as e:
            self.error = "Request returns HTTP code " + str(e.code)
        self.html = handle.read()


#
# SampleConfigureGlobals is optional.
#

#class SampleConfigureGlobals(Window):
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
class SampleConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'Kiss-Anime.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return



    
