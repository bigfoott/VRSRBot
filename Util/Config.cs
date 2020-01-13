using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRSRBot.Util
{
    [Serializable]
    class Config
    {
        public string Token;
        public string ApiKey;
        
        public string Prefix;
        public ulong WRChannel;
        public ulong RoleChannel;

        public TwitterConfig TwitterCfg;

        public Config()
        {
            Token = "";
            ApiKey = "";

            Prefix = "!";
            WRChannel = 0;
            RoleChannel = 0;

            TwitterCfg = new TwitterConfig();
        }
    }
}
