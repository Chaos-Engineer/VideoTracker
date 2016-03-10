import urllib2
# Return the HTML from a web page, or report an error if the site
# is unavailable.
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
            # Note: Use e.read() here to get full HTML
            self.error = "Request returns HTTP code " + str(e.code)
    
    def read(self):
        return self.handle.read()

