using VRSRBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming;

namespace VRSRBot.Util
{
    class TwitterListener
    {
        public IFilteredStream TwitterStream;

        public TwitterListener(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            Prog.Log("Initializing TwitterListener...", "&3");
            
            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            TwitterStream = Stream.CreateFilteredStream();
            TwitterStream.AddFollow(User.GetUserFromScreenName("VRSpeedruns"));
            TwitterStream.StreamStarted += (sender, args) =>
            {
                Prog.Log("TwitterListener started...", "&3");
            };
            TwitterStream.StreamStopped += (sender, args) =>
            {
                Prog.Log("TwitterListener stopped. Restarting...", "&c");
                TwitterStream.StartStreamMatchingAllConditions();
            };
        }

        public void Init()
        {
            Prog.Log("Initializing TwitterListener...", "&3");
            TwitterStream.StartStreamMatchingAllConditions();
        }
    }
}
