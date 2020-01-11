using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRSRBot.Util
{
    [Serializable]
    class TwitterConfig
    {
        public string ConsumerKey;
        public string ConsumerSecret;
        public string AccessToken;
        public string AccessTokenSecret;

        public TwitterConfig()
        {
            ConsumerKey = "";
            ConsumerSecret = "";
            AccessToken = "";
            AccessTokenSecret = "";
        }
    }
}
