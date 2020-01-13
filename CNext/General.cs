using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRSRBot.CNext
{
    class General : BaseCommandModule
    {
        [Command("about")]
        public async Task About(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Description = "Bot made by <@101384280122351616>\n\n[View on Github](https://github.com/bigfoott/VRSRBot)"
            };
            await ctx.RespondAsync("", embed: embed);
        }

        [Command("say")]
        public async Task Say(CommandContext ctx, [RemainingText] string msg)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasFlag(DSharpPlus.Permissions.ManageMessages))
            {
                if (ctx.Guild.CurrentMember.PermissionsIn(ctx.Channel).HasFlag(DSharpPlus.Permissions.ManageMessages))
                {
                    await ctx.Message.DeleteAsync();
                }

                await ctx.RespondAsync(msg);
            }
        }
    }
}
