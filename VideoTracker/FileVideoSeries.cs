using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VideoTrackerLib;

namespace VideoTracker
{

    public class FileVideoSeries : VideoSeries
    {

        // If set, 3-digit episode numbers are allowed. Otherwise, they're interpreted 
        // as a season/episode combination. For example, "Episode101.avi" is taken as
        // Season 1, Episode 01.
        private bool allowThreeDigitEpisodes;  // Make this configurable
        // If set, the file name contains only an episode number, not a season number.
        // The first digit string is interpreted as the episode number and any
        // subsequent digit strings are ignored.
        private bool noSeasonNumber;            // Make this configurable
        // If the file name contains one of the specialStrings, then it is interpreted
        // as a post-season special and appears after the regular episodes. (For example,
        // EpisodeS01E02.avi comes before Episode-S01-Christmas-Special.avi.)
        private List<string> specialStrings; // Make this configurable.

        public List<string> directoryList;

        // addDelay is a debugging flag used to test threading. If set, then the 
        // LoadSeriesAsync call will have an additional semi-random delay added before
        // completion, so we can watch how the display gets updated on the completion
        // of each thread.
        [XmlIgnore, NonSerialized]
        private static bool addDelay = false;

        [XmlIgnore, NonSerialized]
        private string currentFileName;

        public FileVideoSeries()
        {
            this.allowThreeDigitEpisodes = true; 
            this.noSeasonNumber = false;
            this.specialStrings = new List<string>();
            this.specialStrings.Add("SPECIAL");

            this.directoryList = null;
        }

        public void InitializeFromForm(List<string> directoryList, string currentFileName)
        {
            this.directoryList = directoryList;
            this.currentFileName = currentFileName;
        }

        public override void EditForm(VideoTrackerData videoTrackerData)
        {
            FileVideoSeriesForm vsf = new FileVideoSeriesForm(videoTrackerData, this);
            vsf.ShowDialog();
        }


        protected override void LoadSeriesAsync(object sender, DoWorkEventArgs e)
        {
            try
            {
                VideoTrackerData vtd = (VideoTrackerData)e.Argument;

                Regex whitespace = new Regex(@"\s+");
                string fileSearchString = whitespace.Replace(seriesTitle, "*");
                string regexSearchString = whitespace.Replace(Regex.Escape(seriesTitle), ".*");
                string seasonEpisodeRegex = regexSearchString + @"\D*?(\d+)\D+?(\d+)";  // Find first two digit strings
                string EpisodeOnlyRegex = regexSearchString + @"\D*?(\d+)";             // Find first digit string only
                Dictionary<int, int> seasons = new Dictionary<int, int>();
                bool seasonValid = true;
                bool parsingEpisode = true;
                string[] files;

                if (addDelay)
                {
                    Thread.Sleep(1000 * seriesTitle.Length); // Force delay 
                }
                foreach (string directory in directoryList)
                {
                    try
                    {
                        files = System.IO.Directory.GetFiles(directory, "*" + fileSearchString + "*");
                    }
                    catch (DirectoryNotFoundException)
                    {
                        continue; // Silently ignore - directory not found
                    }
                    catch (IOException)
                    {
                        continue; // Silently ignore - network share not found
                    }

                    foreach (string file in files)
                    {
                        VideoFile v = new VideoFile();
                        v.internalName = file;
                        v.episodeTitle = Path.GetFileName(file);

                        parsingEpisode = true;
                        // This handles strings with season and episode numbers, in formats
                        // like S01E01 and 1x01. It is executed if the "noSeasonNumber" flag
                        // has not been set, and if the episode name contains at least two
                        // digit strings.
                        if (noSeasonNumber == false)
                        {
                            Match m = Regex.Match(file, seasonEpisodeRegex, RegexOptions.IgnoreCase);
                            if (m.Success)
                            {

                                GroupCollection g = m.Groups;
                                v.season = Int32.Parse(g[1].Value);
                                v.episode = Int32.Parse(g[2].Value);
                                if (seasons.ContainsKey(v.season))
                                {
                                    seasons[v.season] += 1;
                                }
                                else
                                {
                                    seasons[v.season] = 1;
                                }
                                parsingEpisode = false;
                            }
                        }

                        // This handles strings with episode numbers only. It is executed if
                        // the previous block failed - meaning that the "noSeasonNumber" flag 
                        // has been set, or the filename contains a single digit string.
                        if (parsingEpisode)
                        {
                            Match m = Regex.Match(file, EpisodeOnlyRegex, RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                GroupCollection g = m.Groups;
                                v.season = Int32.Parse(g[1].Value);
                                if (seasons.ContainsKey(v.season))
                                {
                                    seasons[v.season] += 1;
                                }
                                else
                                {
                                    seasons[v.season] = 1;
                                }
                                parsingEpisode = false;
                            }
                        }
                        //
                        // This code is executed if the filename contains no digits at all.
                        // Assume this is a single episode of something.
                        if (parsingEpisode)
                        {
                            v.season = 1;
                            v.episode = 1;
                        }

                        // If the first number is 3 or more digits, then this usually indicates
                        // that it contains both the season and episode numbers, e.g. 101 is
                        // Season 1, Episode 1, not season 101.
                        if (allowThreeDigitEpisodes)
                        {
                            if (v.season > 100)
                            {
                                v.episode = v.season % 100;
                                v.season = v.season / 100;
                            }
                        }

                        // End-of-season special. May have the same episode number as a regular
                        // season episode.
                        v.special = 0;
                        foreach (string s in specialStrings)
                        {
                            if (file.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                v.special = 1;
                                if (v.episode == 0) { v.episode = 1; }
                            }
                        }
                        v.key = String.Format("{0:D3}{1:D1}{2:D3}{3}", v.season, v.special, v.episode, v.episodeTitle);

                        videoFiles.Add(v.key, v);
                        // Note that we identify the current video using the internal file 
                        // name rather than the key. If this is a new video series, then the 
                        // user entered a filename on the form. The key name is unknown at
                        // that time, because it can't be calculated until this routine has
                        // been executed.
                        if (v.internalName == this.currentFileName)
                        {
                            this.currentVideo = v;
                        }
                    }
                }

                //
                // If there are three or more episodes, and if no two episodes have the same season 
                // number, then assume that the first number in the filename is the episode, and that
                // any remaining numbers are meaningless.
                //
                // We don't need to recalculate the key, as the renumbering being done here will not 
                // change the sort order of the series.
                //
                if (videoFiles.Count >= 3)
                {
                    seasonValid = false;
                    foreach (int i in seasons.Values)
                    {
                        if (i > 1)
                        {
                            seasonValid = true;
                        }
                    }
                }
                if (!seasonValid)
                {
                    foreach (VideoFile v in videoFiles.Values)
                    {
                        v.episode = v.season;
                        v.season = 1;
                    }
                }
                if (videoFiles.Count == 0)
                {
                    errorString = "No files found.";
                }
            }
            catch (Exception ex)
            {
                this.videoFiles.Clear(); // Make sure error is reported if list was partially loaded.
                errorString = ex.ToString();
            }
        }

    }
}
