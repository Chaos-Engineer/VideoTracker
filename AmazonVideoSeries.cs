using AmazonProductAdvtApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

/**********************************************************************************************
 * Copyright 2009 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file 
 * except in compliance with the License. A copy of the License is located at
 *
 *       http://aws.amazon.com/apache2.0/
 *
 * or in the "LICENSE.txt" file accompanying this file. This file is distributed on an "AS IS"
 * BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under the License. 
 *
 * ********************************************************************************************
 *
 *  Amazon Product Advertising API
 *  Signed Requests Sample Code
 *
 *  API Version: 2009-03-31
 *
 */

namespace VideoTracker
{
    public class AmazonVideoSeries : VideoSeries
    {
        public string keywords;

        private const string SERVICE = "AWSECommerceService";
        private const string DESTINATION = "ecs.amazonaws.com";
        private const string VERSION = "2011-08-01";
        private const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";
        private SignedRequestHelper helper;
        private string awsAffiliateID;

        public AmazonVideoSeries()
        {
            this.helper = null;
            this.awsAffiliateID = null;
        }

        public void InitializeFromForm(string keywords)
        {
            this.keywords = keywords;
        }

        public override void EditForm(VideoTrackerData videoTrackerData)
        {
            AmazonVideoSeriesForm vsf = new AmazonVideoSeriesForm(videoTrackerData, this);
            vsf.ShowDialog();
        }

        public override void Play()
        {
            string url = GetURLFromAsin(currentVideo.internalName);
            Process.Start(url);
        }

        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
        {

            // Initialize the signer object.
            VideoTrackerData vtd = (VideoTrackerData) e.Argument;

            if (String.IsNullOrWhiteSpace(vtd.globals[gdg.AMAZON][gdk.PUBLICKEY]) ||
                  String.IsNullOrWhiteSpace(vtd.globals[gdg.AMAZON][gdk.SECRETKEY]) ||
                  String.IsNullOrWhiteSpace(vtd.globals[gdg.AMAZON][gdk.AFFILIATEID]))
            {
                errorString = "Amazon affiliate ID keys have not been set.";
                return;
            }

            this.helper = new SignedRequestHelper(vtd.globals[gdg.AMAZON][gdk.PUBLICKEY],
                vtd.globals[gdg.AMAZON][gdk.SECRETKEY], DESTINATION);
            this.awsAffiliateID = vtd.globals[gdg.AMAZON][gdk.AFFILIATEID];

            //
            // An Amazon ASIN (product ID) is the letter "B", followed by two digits, followed 
            // by seven alphanumeric characters. In a URL, it is always preceded by a "/" and 
            // followed by either "/" or "?".
            //
            // Run the "keywords" argument through that regex. If it matches, then use that  
            // as the ASIN. Otherwise, do a keyword search on the argument and use the first 
            // result as the ASIN.
            //
            string asin;
            Regex r = new Regex(@"/(B\d{2}.{7})[/|\?]"); 
            MatchCollection m = r.Matches(this.keywords);


            if (m.Count > 0)
            {
                asin = m[0].Groups[1].ToString();
            }
            else
            {
                asin = GetAsinFromKeywords(keywords);
                if (asin == null)
                {
                    return;
                }
            }
            GetEpisodeListFromAsin(asin);
        }

        private string GetAsinFromKeywords(string keywords)
        {
            IDictionary<string, string> r = new Dictionary<string, String>();
            XmlDocument doc = new XmlDocument();
            string asin;

            r["Operation"] = "ItemSearch";
            r["SearchIndex"] = "Video";
            r["ResponseGroup"] = "ItemIds";
            r["Keywords"] = keywords;

            doc = SendRequest(r);
            if (doc == null) return null;
            asin = TagToString(doc, "ASIN");
            return asin;
        }

        //
        // There are three possible arrangements:
        // 1 - Stand-alone video. A RelatedItems/Episode search will fail.
        // 2 - Single-season. A RelatedItems/Episode search will succeed, giving the
        //     parent ASIN. A RelatedItems/Episode search on that will give all the
        //     videos in the season. A RelatedItems/Season search will fail.
        // 3 - Multi-season. A RelatedItems/Episode search will succeed, giving the
        //     parent ASIN. A RelatedItems/Season search on the parent ASIN will also
        //     succeed, giving the grandparent ASIN. Traversing the nodes below the
        //     grandparent will give all the videos in the series.
        //
        private bool GetEpisodeListFromAsin(string asin)
        {
            this.videoFiles = new SortedList<string, VideoFile>();

            IDictionary<string, string> r = new Dictionary<string, String>();
            XmlDocument doc = new XmlDocument();
            string seasonAsin;

            r["Operation"] = "ItemLookup";
            r["ItemId"] = asin;
            r["ResponseGroup"] = "RelatedItems,ItemAttributes";
            r["RelationshipType"] = "Episode";

            doc = SendRequest(r);
            //PrettyPrint(doc);
            if (doc == null)
            {
                return (false);
            }

            // This video is part of a season. Get the ASIN of the season.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", NAMESPACE);

            string relationship = TagToString(doc, "Relationship");
            if (relationship == null)
            {
                asin = TagToString(doc, "ASIN");
                string title = TagToString(doc, "Title");
                VideoFile v = new VideoFile();
                v.episodeTitle = title;
                // The thing we want to play is actually GetURLFromAsin(asin). However,
                // that's too many requests to send to the Amazon servers at once, so
                // we'll just save the "asin", and defer the GetURLFromAsin call until
                // the video is actually played.
                v.internalName = asin;
                v.episode = 1;
                v.season = 1;
                v.special = 0;
                v.key = String.Format("{0:D3}{1:D1}{2:D3}", v.season, v.special, v.episode);
                videoFiles.Add(v.key, v);
                return (true);
            }
            else if (relationship.Equals("Parents"))
            {
                XmlNode asinNode = doc.SelectNodes("//ns:RelatedItem/ns:Item/ns:ASIN", nsmgr).Item(0);
                seasonAsin = asinNode.InnerText;
            }
            else if (relationship.Equals("Children"))
            {
                // If we get here, then the ASIN is for a season, not an individual episode. This
                // can happen if the ASIN was parsed out of a user-entered URL. Note that we don't
                // handle the case where we get the top-level ASIN of a multi-season series; it
                // doesn't seem possible to get this from anywhere.
                seasonAsin = asin;
            }
            else
            {
                errorString = "Unexpected value for RELATIONSHIP tag.";
                return (false);
            }

            // See if the Season's ASIN is part of a Multi-Season program.
            r["Operation"] = "ItemLookup";
            r["ItemId"] = seasonAsin;
            r["ResponseGroup"] = "RelatedItems";
            r["RelationshipType"] = "Season";

            doc = SendRequest(r);
            if (doc == null)
            {
                return (false);
            }
            //PrettyPrint(doc);

            //
            // If the Season ASIN has a Parent, then this is an episode of a multi-season program. Otherwise
            // it's an episode of a single-season program.
            nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", NAMESPACE);
            relationship = TagToString(doc, "Relationship");
            if (relationship.Equals("Parents"))
            {
                XmlNode asinNode = doc.SelectNodes("//ns:RelatedItem/ns:Item/ns:ASIN", nsmgr).Item(0);
                string multiSeasonAsin = asinNode.InnerText;
                return (GetEpisodeListFromMultiSeason(multiSeasonAsin));
            }
            else
            {
                return (GetEpisodeListFromSeason(seasonAsin));
            }

        }

        private bool GetEpisodeListFromMultiSeason(string asin)
        {

            try
            {
                IDictionary<string, string> r = new Dictionary<string, String>();
                XmlDocument doc = new XmlDocument();
                int page = 1;

                r["Operation"] = "ItemLookup";
                r["ItemId"] = asin;
                r["ResponseGroup"] = "RelatedItems,ItemAttributes";
                r["RelationshipType"] = "Season";
                r["RelatedItemPage"] = page.ToString();

                while (true)
                {
                    doc = SendRequest(r);
                    if (doc == null) return (false);
                    //PrettyPrint(doc);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("ns", NAMESPACE);

                    int pageCount = Int32.Parse(TagToString(doc, "RelatedItemPageCount"));
                    XmlNodeList subNodes = doc.SelectNodes("//ns:RelatedItem/ns:Item", nsmgr);
                    foreach (XmlNode subNode in subNodes)
                    {
                        asin = subNode.SelectSingleNode("ns:ASIN", nsmgr).InnerText;
                        int season = Int32.Parse(subNode.SelectSingleNode("ns:ItemAttributes/ns:EpisodeSequence", nsmgr).InnerText);
                        GetEpisodeListFromSeason(asin, season, true);
                    }

                    if (page == pageCount) { break; }
                    r["RelatedItemPage"] = (++page).ToString();
                }
            }
            catch (Exception ex)
            {
                errorString = "Error getting episode list from ASIN " +
                    asin + ": " + ex.ToString();
                return (false);
            }
            return (true);
        }


        private bool GetEpisodeListFromSeason(string asin, int season = 1, bool multiseason = false)
        {

            try
            {
                IDictionary<string, string> r = new Dictionary<string, String>();
                XmlDocument doc = new XmlDocument();
                int page = 1;

                r["Operation"] = "ItemLookup";
                r["ItemId"] = asin;
                r["ResponseGroup"] = "RelatedItems,ItemAttributes";
                r["RelationshipType"] = "Episode";
                r["RelatedItemPage"] = page.ToString();
                while (true)
                {
                    doc = SendRequest(r);
                    if (doc == null) return (false);
                    //PrettyPrint(doc);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("ns", NAMESPACE);


                    int pageCount = Int32.Parse(TagToString(doc, "RelatedItemPageCount"));
                    XmlNodeList asinNodes = doc.SelectNodes("//ns:RelatedItem/ns:Item", nsmgr);
                    foreach (XmlNode asinNode in asinNodes)
                    {

                        asin = asinNode.SelectSingleNode("ns:ASIN", nsmgr).InnerText;
                        string title = asinNode.SelectSingleNode("ns:ItemAttributes/ns:Title", nsmgr).InnerText;
                        int episode = Int32.Parse(asinNode.SelectSingleNode("ns:ItemAttributes/ns:EpisodeSequence", nsmgr).InnerText);
                        VideoFile v = new VideoFile();
                        if (multiseason)
                        {
                            v.episodeTitle = season + "." + episode + " - " + title;
                        }
                        else
                        {
                            v.episodeTitle = episode + " - " + title;
                        }
                        // The thing we want to play is actually GetURLFromAsin(asin). However,
                        // that's too many requests to send to the Amazon servers at once, so
                        // we'll just save the "asin", and defer the GetURLFromAsin call until
                        // the video is actually played.
                        v.internalName = asin;
                        v.episode = episode;
                        v.season = season;
                        v.special = 0;
                        v.key = String.Format("{0:D3}{1:D1}{2:D3}{3}", v.season, v.special, v.episode, v.episodeTitle);
                        videoFiles.Add(v.key, v);
                    }

                    if (page == pageCount) { break; }
                    r["RelatedItemPage"] = (++page).ToString();
                }
            }
            catch (Exception ex)
            {
                errorString = "Error getting episode list from ASIN " + asin +
                    " season " + season + ": " + ex.ToString();
                return (false);
            }
            return (true);
        }

        private string GetURLFromAsin(string asin)
        {
            IDictionary<string, string> r = new Dictionary<string, String>();
            XmlDocument doc = new XmlDocument();

            r["Operation"] = "ItemLookup";
            r["ItemId"] = asin;
            r["ResponseGroup"] = "ItemAttributes";
            doc = SendRequest(r);
            if (doc == null) return (null);
            //PrettyPrint(doc);

            string detail = TagToString(doc, "DetailPageURL");
            return (detail);
        }

        private XmlDocument SendRequest(IDictionary<string, string> r)
        {
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    r["Service"] = SERVICE;
                    r["Version"] = VERSION; 
                    r["AssociateTag"] = awsAffiliateID;
                    String url = helper.Sign(r);
                    WebRequest request = HttpWebRequest.Create(url);
                    WebResponse response = request.GetResponse();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(response.GetResponseStream());

                    XmlNodeList errorMessageNodes = doc.GetElementsByTagName("Message", NAMESPACE);
                    if (errorMessageNodes != null && errorMessageNodes.Count > 0)
                    {
                        errorString = errorMessageNodes.Item(0).InnerText;
                        return null;
                    }
                    return doc;
                }
                catch (System.Net.WebException)
                {
                    //MessageBox.Show("WebException. Retrying in 5 seconds");
                    System.Threading.Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    errorString = "Unexpected communications error: " + ex.ToString();
                    return null;
                }
            }
            errorString = "Amazon web server not responding; try again later";
            return null;
        }

        private static string TagToString(XmlDocument doc, string tag)
        {
            XmlNode detailNode = doc.GetElementsByTagName(tag, NAMESPACE).Item(0);
            if (detailNode == null) return (null);
            string detail = detailNode.InnerText;
            return detail;
        }


        private static void PrettyPrint(XmlDocument doc)
        {
            // Debugging routine for the original command-line version of the program.
            // Currently needs to be modified so that writes to a message box instead
            // of the console.
            using (XmlTextWriter writer = new XmlTextWriter(Console.Out))
            {
                writer.Formatting = Formatting.Indented;
                doc.WriteContentTo(writer);
                Console.WriteLine();
            }
        }
    }
}
