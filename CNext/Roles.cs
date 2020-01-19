using VRSRBot.Core;
using VRSRBot.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Web;

namespace VRSRBot.CNext
{
    class Roles : BaseCommandModule
    {
        [Command("createrolemsg")]
        public async Task CreateRoleMsg(CommandContext ctx, DiscordRole role)
        {
            if (ctx.Channel.Id != Prog.Config.RoleChannel) return;

            if (ctx.Member.Roles.Any(r => r.Id == 405196930269052938 || r.Id == 667232505631604760)) // only people with admin/bot creator role
            {
                if (ctx.Guild.CurrentMember.PermissionsIn(ctx.Channel).HasFlag(DSharpPlus.Permissions.ManageMessages))
                {
                    await ctx.Message.DeleteAsync();
                }

                var emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 665860688463396864);
                var embed = new DiscordEmbedBuilder()
                {
                    Description = $"**React to this message with {emoji} to toggle the {role.Mention} role.**",
                    Color = new DiscordColor("#0165fe")
                };
                var msg = await ctx.RespondAsync("", embed: embed);
                
                var list = Prog.RoleMessages.ToList();
                list.Add(new RoleMessage(msg.Id, role.Id));
                Prog.RoleMessages = list.ToArray();

                File.WriteAllText("files/rolemessages.json", JsonConvert.SerializeObject(Prog.RoleMessages, Formatting.Indented));

                await msg.CreateReactionAsync(emoji);
            }
        }

        [Command("rolestats")]
        public async Task RoleStats(CommandContext ctx)
        {
            string chartUrl = "bkg=white&c={type:'bar',data:{labels:[_LABELS_]," +
                              "datasets:[{label:'Roles',data:[_DATA_]}]}," +
                              "options:{title:{display:false},legend:{display:false},scales:{xAxes:[{gridLines:{display:false}}],yAxes:[{ticks:{precision:0}}]}}}";

            string labels = "";
            string data = "";
            string desc = "";

            foreach (RoleMessage rm in Prog.RoleMessages)
            {
                DiscordRole role = ctx.Guild.GetRole(rm.RoleId);
                string roleName = role.Name;
                if (roleName.EndsWith(" User"))
                    roleName = roleName.Remove(roleName.LastIndexOf(" User"));
                labels += $"\"{roleName}\",";

                int userCount = ctx.Guild.Members.Where(m => m.Value.Roles.Contains(role)).Count();
                data += userCount + ",";

                string _s = "s";
                if (userCount == 1) _s = "";
                desc += $"{role.Mention}: {userCount} user{_s}.\n";
            }

            chartUrl = chartUrl.Replace("_LABELS_", labels).Replace("_DATA_", data);
            chartUrl = chartUrl.Replace(" ", "%20");

            //await ctx.RespondAsync(HttpUtility.UrlEncode(chartUrl));
            //return;

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Role Stats",
                Description = desc,
                ImageUrl = "https://quickchart.io/chart?" + chartUrl,
                Color = new DiscordColor("#0165fe")
            };
            await ctx.RespondAsync("", embed: embed);
        }
    }
}
