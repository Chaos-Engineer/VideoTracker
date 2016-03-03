#
# This is a minimal plug-in that you can use as a base when developing
# new plug-ins. See the Sample plug-in for documentation and more
# detailed example code.
#
import urllib2, re
import clr 
clr.AddReference('IronPython.Wpf')
import wpf
from System.Windows import Application, Window, MessageBox
from System.IO import Path
from System.Diagnostics import Process

clr.AddReference('VideoTrackerLib')
import VideoTracker
from VideoTracker import VideoFile, gpk, spk

def Register(pluginRegisterDictionary) :
    pluginRegisterDictionary[gpk.NAME]  = "Putlocker"
    pluginRegisterDictionary[gpk.ADD]   = "Add Putlocker Series"
    pluginRegisterDictionary[gpk.DESC]  = "Add series from putlocker.is"
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
    form = PutLockerConfigureSeries()
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
    # Search for the desired program. Output from the search string is:
    # 
    # <a href="http://putlocker.is/watch-orange-is-the-new-black-tvshow-online-free-putlocker.html"
    # title="Orange Is the New Black (2013)">
    #  
    #
    series = pluginSeriesDictionary[spk.TITLE]
    url = "http://putlocker.is/search/search.php?q=" + series
    h = HtmlLoader(url);
    if (h.error != ""):
        return h.error
    html = h.read();

    m = re.search('<a href="(.*?)" title=(.*?' + series + '.*?)>', html, flags=re.IGNORECASE)
    if m is None:
        return "Series not found"
    url = m.group(1)
    title = m.group(2)

    # Find the episodes within the series:
    # 
    # <a href="http://putlocker.is/watch-orange-is-the-new-black-tvshow-season-1-episode-1-online-free-putlocker.html" 
    # title="Orange Is the New Black Season 1 Episode 1 - I Wasn't Ready"><strong>Episode 1</strong></a>
    #
    h = HtmlLoader(url);
    if (h.error != ""):
        return h.error
    html = h.read();

    m = re.findall('<a href="(.*?)" title="(.*?' + series + '.*?)">', html, flags=re.IGNORECASE)
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

    return "" # Indicates no error

class HtmlLoader:
    def __init__(self, url):
        #
        # Some websites return a 403 error if no user agent
        # is specified.
        #
        userAgent = "Wget/1.16.1 (cygwin)"
        headers = { 'User-Agent' : userAgent }
        req = urllib2.Request(url, "", headers)
        self.error = ""
        try:
            self.handle = urllib2.urlopen(req)
        except urllib2.HTTPError as e:
            self.error = "Request returns HTTP code " + str(e.code)
    
    def read(self):
        return self.handle.read()


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
class PutLockerConfigureSeries(Window):
    def __init__(self):
        wpf.LoadComponent(self, Path.GetDirectoryName(__file__) + '\\' + 'PutLocker.xaml')
    
    def OKButton_Click(self, sender, e):
        if self.NameBox.Text == "" :
            MessageBox.Show("Series name must be specified")
            return
        self.DialogResult = True
        return



    
