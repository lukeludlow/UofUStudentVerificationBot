using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace UofUStudentVerificationBot
{
    public class RoleAssignmentService : IRoleAssignmentService
    {
        private readonly DiscordSocketClient discordClient;
        private readonly IConfiguration config;

        public RoleAssignmentService(DiscordSocketClient discordClient, IConfiguration config)
        {
            this.discordClient = discordClient;
            this.config = config;
        }

        public void AssignVerifiedRoleToDiscordUser(ulong discordID)
        {
            if (GetGuildAndVerifiedRole(out IGuild guild, out IRole verifiedRole)) {
                SocketGuildUser user = guild.GetUserAsync(discordID).Result as SocketGuildUser;
                user.AddRoleAsync(verifiedRole);
            }
        }

        public void RemoveVerifiedRoleFromDiscordUser(ulong discordID)
        {
            if (GetGuildAndVerifiedRole(out IGuild guild, out IRole verifiedRole)) {
                SocketGuildUser user = guild.GetUserAsync(discordID).Result as SocketGuildUser;
                user.RemoveRoleAsync(verifiedRole);
            }
        }

        private bool GetGuildAndVerifiedRole(out IGuild guild, out IRole verifiedRole)
        {
            guild = discordClient.Guilds
                .Where(guild => guild.Id == config.GetValue<ulong>("Discord:GuildID"))
                .FirstOrDefault();
            verifiedRole = guild.Roles
                .Where(role => role.Name == config["Discord:VerifiedRoleName"])
                .FirstOrDefault();
            // check if the guild and role were actually found (defaults won't have the matching id/name)
            if (guild.Id == config.GetValue<ulong>("Discord:GuildID") && verifiedRole.Name == config["Discord:VerifiedRoleName"]) {
                return true;
            } else {
                // this means that the guild id or verified channel name are incorrect or not found.
                // this is a problem with the config file and/or the discord server settings.
                return false;
            }
        }

        // TODO
        // private delegate GuildUserRoleAction ...

    }
}