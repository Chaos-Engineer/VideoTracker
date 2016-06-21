import subprocess
import re
import urllib2
import os.path
# Return the HTML from a web page, or report an error if the site
# is unavailable. Use the phantomjs headless browser to bypass 
# CloudFlare protections.

class HtmlLoader_CloudFlare:
    userAgent = "Wget/1.16.1 (cygwin)"
    headers = { 'User-Agent' : userAgent }
    cloudflareRequired = False

    def __init__(self, url, directory):
         self.error = ""
         self.html = ""

         #
         # Try doing a simple urllib request first. If this returns a "503" error,
         # then retry using CloudFlare bypass


         if (HtmlLoader_CloudFlare.cloudflareRequired == False):
            try:
                req = urllib2.Request(url, "", HtmlLoader_CloudFlare.headers)
                handle = urllib2.urlopen(req)
                self.html = handle.read()
                return
            except urllib2.HTTPError as e:
                # Note: Use e.read() here to get full HTML
                if (e.code != 503):
                    self.error = "Cloudflare-ByPass request on " + url + " returns HTTP code " + str(e.code)
                    return

         phantomjs = directory + "\\phantomjs.exe"
         if (not os.path.exists(phantomjs)):
             self.error = ("\"" + phantomjs + "\" required but does not exist.\n" +
                "Download the latest version from http://phantomjs.org/download.html\n" +
                "and copy the file to that directory")
             return
                
         HtmlLoader_CloudFlare.cloudflareRequired = True;
         args = [phantomjs, "--cookies-file=" + directory + "\\CloudFlare-Cookies.txt", directory + "\\CloudFlare-Bypass.js", url]
         try:
            CREATE_NO_WINDOW = 0x08000000
            self.html = subprocess.check_output(args, shell=False, creationflags=CREATE_NO_WINDOW)
         except subprocess.CalledProcessError as ex:
            self.error = "Cloudflare-ByPass on " + url + " generates exception " + self.html
            return

    def read(self):
         return self.html