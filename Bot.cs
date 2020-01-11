using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VRSRBot.Util;

namespace VRSRBot
{
    class Bot
    {
        public Config Config;

        public DiscordClient Client;
        public InteractivityExtension Interactivity;
        public CommandsNextExtension CommandsNext;

        public TwitterListener Twitter;

        public Bot(Config cfg)
        {
            Prog.Log("Initializing Bot...", "&3");
            Config = cfg;

            var config = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Critical,
                AutoReconnect = true
            };

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
            {
                config.WebSocketClientFactory = WebSocketSharpClient.CreateNew;
                Prog.Log("Switched websocket to WebSocketSharp. (Windows 7)", "&a");
            }

            Prog.Log("Initializing components...", "&3");
            Client = new DiscordClient(config);
            Interactivity = Client.UseInteractivity(new InteractivityConfiguration { Timeout = new TimeSpan(0, 1, 30) });
            CommandsNext = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDms = false,
                EnableDefaultHelp = false,
                PrefixResolver = PrefixPredicateAsync,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true
            });
            //CommandsNext.RegisterCommands<General>();

            Client.Ready += async e =>
            {
                await Client.UpdateStatusAsync(new DiscordActivity("speedrun.com", ActivityType.Watching), UserStatus.Online);
            };

            Prog.Log("Bot initialization complete. Connecting...", "&3");

            Client.ConnectAsync();

            Prog.Log("Connected.", "&3");

            Twitter = new TwitterListener(Config.TwitterCfg.ConsumerKey, Config.TwitterCfg.ConsumerSecret, Config.TwitterCfg.AccessToken, Config.TwitterCfg.AccessTokenSecret);
            Twitter.TwitterStream.MatchingTweetReceived += async (sender, args) =>
            {
                DiscordChannel channel = await Client.GetChannelAsync(Config.WRChannel);
                string[] url = args.Tweet.Urls[0].ExpandedURL.Split('/');
                await HandleNewWR(url[url.Length - 1], channel);
            };
            Twitter.Init();
        }
        
        private static Task<int> PrefixPredicateAsync(DiscordMessage m)
        {
            string pref = Prog.Config.Prefix;
            return Task.FromResult(m.GetStringPrefixLength(pref));
        }

        private static async Task HandleNewWR(string id, DiscordChannel channel)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadStringCompleted += async (sender, e) =>
                {
                    Run run = new Run(e.Result);

                    string time = "";
                    if (run.Time.Hours != 0)
                        time += run.Time.Hours + "h ";
                    if (run.Time.Minutes != 0)
                        time += run.Time.Minutes + "m ";
                    if (run.Time.Seconds != 0)
                        time += run.Time.Seconds + "s";
                    time = time.Trim();

                    var embed = new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor()
                        {
                            Name = "NEW WORLD RECORD!",
                            IconUrl = "https://bigft.io/vrsrbot_icon.png"
                        },
                        Description = "[View the run on Speedrun.com](" + run.Link + ")",
                        ThumbnailUrl = "https://www.speedrun.com/themes/" + run.GameAbbr + "/cover-256.png",
                        Timestamp = DateTime.Now
                    };

                    embed.AddField("Category", $"**[{run.GameName}]({run.GameLink})** - **[{run.Category}]({run.CategoryLink})**");
                    embed.AddField("Time", $"**[{time}]({run.Link})**");


                    if (run.RunnerLink != "")
                        embed.AddField("Runner", $"**[{run.Runner}]({run.RunnerLink})**");
                    else
                        embed.AddField("Runner", $"**{run.Runner}**");

                    embed.AddField(run.DeviceType, $"**{run.DeviceValue}**");
                    embed.AddField("Runner Comment", $"\"{run.Comment}\"");

                    await channel.SendMessageAsync("", embed: embed);
                };
                wc.DownloadStringAsync(new Uri("https://www.speedrun.com/api/v1/runs/" + id + "?embed=category,players,variables,category.variables,game,platform"));
            }
        }
    }
}
