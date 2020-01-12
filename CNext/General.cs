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
                Title = "About",
                Description = "Made by <@101384280122351616>\n\n[View on Github](https://github.com/bigfoott/VRSRBot)"
            };
            await ctx.RespondAsync("", embed: embed);
        }
    }
}
