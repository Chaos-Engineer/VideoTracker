import subprocess
import re
# Return the HTML from a web page, or report an error if the site
# is unavailable. Use the phantomjs headless browser to bypass 
# CloudFlare protections.

class HtmlLoader_CloudFlare:
    def __init__(self, url, directory):
         self.error = ""
         self.html = ""
         args = [directory + "\\phantomjs.exe", "--cookies-file=" + directory + "\\CloudFlare-Cookies.txt", directory + "\\CloudFlare-Bypass.js", url]
         try:
            CREATE_NO_WINDOW = 0x08000000
            self.html = subprocess.check_output(args, shell=False, creationflags=CREATE_NO_WINDOW)
         except subprocess.CalledProcessError as ex:
            self.error = self.html
            return
         m = re.search('<title>([^<]*?Error.*?)>', self.html, flags=re.IGNORECASE)
         if (m is not None):
             self.error = re.sub("(<.*?>)+", "\r\n", self.html) # Simple HTML tag remover

    def read(self):
         return self.html