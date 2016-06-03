using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using System.Windows;
// Plays videos from the CrunchyRoll website, using screen-scraping to determine the URLs associated
// with a series.

namespace VideoTracker
{
    public class CrunchyRollVideoSeries : VideoSeries
    {
        public string URL;
        public const string CrunchyRollUrlPrefix = "http://www.crunchyroll.com";


        public CrunchyRollVideoSeries()
        {

        }

        public CrunchyRollVideoSeries(string URL, string title)
        {
            this.URL = URL;
            this.seriesTitle = title;
        }

        public override void EditForm(VideoTrackerData vtd)
        {
            CrunchyRollVideoSeriesForm cf = new CrunchyRollVideoSeriesForm(vtd, this);
            cf.ShowDialog();
        }

        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
        {
            string key, html;
            VideoTrackerData vtd = (VideoTrackerData) e.Argument;
            //
            // Crunchyroll lists all episodes of a season on a single page, in <div> blocks like the one below.
            //
            // We use the CsQuery NuGet package to parse the HTML and return a collection of <div> blocks. For each 
            // block, we extract the URL from the <A> tag, the title from "span.series-title", and the description 
            // from "p.short-desc" 
            //         
            //  <div class="wrapper container-shadow hover-classes" data-classes="container-shadow-dark">
            //      <a href="/polar-bear-cafe/episode-1-polar-bears-caf-595187" title="Polar Bear Cafe Episode 1" class="portrait-element block-link titlefix episode">
            //                  <img src="http://img1.ak.crunchyroll.com/i/spire1-tmb/ae6baca891168d026842fa0d5fecbcb91333523692_wide.jpg" class="landscape"/>
            //          <div class="episode-progress-bar">
            //              <div class="episode-progress" media_id="595187" style="width: 0%;"></div>
            //          </div>
            //      <span class="series-title block ellipsis" dir="auto">Episode 1</span>
            //      <p class="short-desc" dir="auto">Polar Bear's Café</p>
            //      </a>
            //  </div>


            WebClient c = new WebClient();
            try
            {
                html = c.DownloadString(this.URL);
            }
            catch (WebException)
            {
                errorString = "Failed to load " + this.URL;
                return;
            }
            CQ dom = CQ.Create(html);
            CQ divs = dom.Find("div.wrapper.container-shadow.hover-classes");
            foreach (IDomObject div in divs)
            {
                IDomElement atag = div.Cq().Find("a").FirstElement();
                if (atag == null) { continue; }
                string episodeUrl = CrunchyRollUrlPrefix + atag.GetAttribute("href").ToString(); 
                // URL is the user-entered URL for the series. At entry time, it was converted to 
                // all-lowercase and so will match the beginning of the episode URL returned in div.
                if (!episodeUrl.Contains(this.URL)) { continue; }  // Ignore "Viewers Also Liked" links.

                IDomElement ttag = div.Cq().Find("span.series-title").FirstElement();
                if (ttag == null) { continue; }
                string title = ttag.InnerText.Trim();

                IDomElement ptag = div.Cq().Find("p.short-desc").FirstElement();
                if (ptag == null) { continue; }

                // CrunchyRoll sends this string in UTF8 format, but the HtmlDecode routine assumes that
                // it's in ASCII format. This causes problems with some special characters. The two lines
                // of code below convert to the correct format.
                byte[] bytes = Encoding.Default.GetBytes(HttpUtility.HtmlDecode(ptag.InnerText.Trim()));
                string desc = Encoding.UTF8.GetString(bytes);

                VideoFile v = new VideoFile();
                string episodeRegex = @"\D*?(\d+)\D+?";
                Match m = Regex.Match(episodeUrl, episodeRegex);
                if (m.Success)
                {
                    GroupCollection g = m.Groups;
                    v.episode = Int32.Parse(g[1].Value);
                    key = String.Format("{0:D5}", v.episode);
                }
                else
                {
                    App.ErrorBox("Can't extract episode number from " + this.URL);
                    continue;
                }

                v.key = key;
                v.internalName = episodeUrl;
                v.episodeTitle = v.episode + " - " + desc;
                v.season = 1;
                videoFiles.Add(key, v);

            }
        }
    }
}
