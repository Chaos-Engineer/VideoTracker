using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

// Class definitions to be inherited by plugins
namespace VideoTracker
{
    [Serializable]
    public class VideoFile
    {
        public string episodeTitle;     // Episode title to display
        public string internalName;     // Internal reference to episode (e.g. filename or URL)
        public int season;              // Season number
        public int episode;             // Episode number
        public int special;             // Set to "1" if this is a post-season special
        public string key;              // Unique value, used to sort episodes
    }

    // Global Plugin Keys - Predefined keys for the pluginGlobalDictionary library
    public class gpk
    {
        // Group=PLUGINS
        public const string NAME = "name";
        public const string ADD  = "add";
        public const string DESC = "desc";
        public const string FORCECONFIG = "forceconfig";
    }
    // Series Plugin Keys - Predefined keys for the pluginSeriesDictionary library
    public class spk
    {
        public const string TITLE = "title";
        public const string CURRENTVIDEO = "currentvideo";
    }
}
