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

namespace VRSRBot.CNext
{
    class Roles : BaseCommandModule
    {
        [Command("createrolemsg")]
        public async Task Help(CommandContext ctx, DiscordRole role)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasFlag(DSharpPlus.Permissions.ManageRoles))
            {
                if (ctx.Guild.CurrentMember.PermissionsIn(ctx.Channel).HasFlag(DSharpPlus.Permissions.ManageMessages))
                {
                    await ctx.Message.DeleteAsync();
                }

                var emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 665860688463396864);
                var embed = new DiscordEmbedBuilder()
                {
                    Description = $"**React to this message with {emoji} to toggle the {role.Mention} role.**"
                };
                var msg = await ctx.RespondAsync("", embed: embed);

                var list = Prog.RoleMessages.ToList();
                list.Add(new RoleMessage(msg.Id, role.Id));
                Prog.RoleMessages = list.ToArray();

                File.WriteAllText("files/rolemessages.json", JsonConvert.SerializeObject(Prog.RoleMessages, Formatting.Indented));

                await msg.CreateReactionAsync(emoji);
            }
        }
    }
}
