﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VRSRBot.Util;

namespace VRSRBot.Core
{
    class Bot
    {
        public Config Config;

        public DiscordClient Client;
        public InteractivityExtension Interactivity;
        public CommandsNextExtension CommandsNext;

        public TwitterListener Twitter;

        private static ulong BotId;
        private static Dictionary<DiscordMember, KeyValuePair<ulong, DateTime>> lastReaction;

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
            CommandsNext.RegisterCommands<CNext.General>();
            CommandsNext.RegisterCommands<CNext.Roles>();

            Client.MessageReactionAdded += async e =>
            {
                await ToggleRole((DiscordMember)e.User, e.Guild.GetRole(Prog.RoleMessages.FirstOrDefault(r => r.MessageId == e.Message.Id).RoleId));
                await e.Message.DeleteReactionAsync(e.Emoji, e.User);
            };
            Client.MessageDeleted += async e =>
            {
                if (Prog.RoleMessages.Any(r => r.MessageId == e.Message.Id))
                {
                    var list = Prog.RoleMessages.ToList();
                    list.RemoveAll(r => r.MessageId == e.Message.Id);
                    Prog.RoleMessages = list.ToArray();

                    File.WriteAllText("files/rolemessages.json", JsonConvert.SerializeObject(Prog.RoleMessages, Formatting.Indented));
                }
            };

            // when a guild becomes available, check for any role messages that got deleted while the bot was offline
            Client.GuildAvailable += async e =>
            {
                if (e.Guild.Id != 405196443519811584) return; //ignore if its not the main vrsr server

                Prog.Log($"Checking for deleted role messages while offline in server '{e.Guild.Name}'...", "&e");
                
                List<RoleMessage> tempRoleMessages = Prog.RoleMessages.ToList();
                var channel = await Client.GetChannelAsync(Prog.Config.RoleChannel);
                
                foreach (RoleMessage rm in Prog.RoleMessages)
                {
                    DiscordMessage msg = null;

                    try { msg = await channel.GetMessageAsync(rm.MessageId); continue; }
                    catch { }
                    
                    if (msg == null)
                        tempRoleMessages.Remove(rm);
                }

                int diff = Prog.RoleMessages.Length - tempRoleMessages.Count;
                if (diff > 0)
                {
                    string _s = "s";
                    string _them = "them";
                    if (diff == 1)
                    {
                        _s = "";
                        _them = "it";
                    }

                    Prog.Log($"Found {Prog.RoleMessages.Length - tempRoleMessages.Count} deleted message{_s} in server '{e.Guild.Name}' and removed {_them}.", "&e");
                    Prog.RoleMessages = tempRoleMessages.ToArray();
                    File.WriteAllText("files/rolemessages.json", JsonConvert.SerializeObject(Prog.RoleMessages, Formatting.Indented));
                }
                else
                    Prog.Log($"No role messages were deleted in server '{e.Guild.Name}'.", "&e");
            };

            Client.Ready += async e =>
            {
                await Client.UpdateStatusAsync(new DiscordActivity("speedrun.com", ActivityType.Watching), UserStatus.Online);
                
                BotId = Client.CurrentUser.Id;
                lastReaction = new Dictionary<DiscordMember, KeyValuePair<ulong, DateTime>>();
                
            };

            Prog.Log("Bot initialization complete. Connecting...", "&3");

            Client.ConnectAsync();

            Prog.Log("Connected.", "&3");

            Twitter = new TwitterListener(Config.TwitterCfg.ConsumerKey, Config.TwitterCfg.ConsumerSecret, Config.TwitterCfg.AccessToken, Config.TwitterCfg.AccessTokenSecret);
            Twitter.TwitterStream.MatchingTweetReceived += async (sender, args) =>
            {
                //if (args.Tweet.Text.Contains("[__TEST__]")) return; // if testing tweets, dont post them on the public bot
                
                try
                {
                    DiscordChannel channel = await Client.GetChannelAsync(Config.WRChannel);
                    string[] url = args.Tweet.Urls[0].ExpandedURL.Split('/');
                    await HandleNewWR(url[url.Length - 1], channel);
                }
                catch { }
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
                        Timestamp = run.TimeStamp,
                        Color = new DiscordColor("#0165fe")
                };

                    embed.AddField("Category", $"**[{run.GameName}]({run.GameLink})** - **[{run.Category}]({run.CategoryLink})**");
                    embed.AddField("Time", $"**[{time}]({run.Link})**");
                    
                    if (run.RunnerLink != "")
                        embed.AddField("Runner", $"**[{run.Runner}]({run.RunnerLink})**");
                    else
                        embed.AddField("Runner", $"**{run.Runner}**");

                    if (run.DeviceType != "null")
                        embed.AddField(run.DeviceType, $"**{run.DeviceValue}**");
                    
                    if (run.Comment != null)
                        embed.AddField("Runner Comment", $"\"{run.Comment}\"");

                    await channel.SendMessageAsync("", embed: embed);
                };
                wc.DownloadStringAsync(new Uri("https://www.speedrun.com/api/v1/runs/" + id + "?embed=category,players,variables,category.variables,game,platform"));
            }
        }

        private static async Task ToggleRole(DiscordMember member, DiscordRole role)
        {
            if (member.Id != BotId)
            {
                // Clear out the lastReaction list if it starts to get too full
                if (lastReaction.Count > 10)
                {
                    List<DiscordMember> remove = new List<DiscordMember>();
                    foreach (KeyValuePair<DiscordMember, KeyValuePair<ulong, DateTime>> reaction in lastReaction)
                    {
                        if (DateTime.Now.Subtract(reaction.Value.Value).TotalSeconds > 1)
                            remove.Add(reaction.Key);
                    }
                    foreach (DiscordMember reaction in remove)
                        lastReaction.Remove(reaction);
                }

                if (lastReaction.ContainsKey(member))
                {
                    if (DateTime.Now.Subtract(lastReaction[member].Value).TotalSeconds < 1 && lastReaction[member].Key == role.Id)
                        return;
                    
                    lastReaction.Remove(member);
                }
                var kvp = new KeyValuePair<ulong, DateTime>(role.Id, DateTime.Now);
                
                lastReaction.Add(member, kvp);
                
                if (member.Roles.Contains(role))
                    await member.RevokeRoleAsync(role);
                else
                    await member.GrantRoleAsync(role);
            }
        }
    }
}
