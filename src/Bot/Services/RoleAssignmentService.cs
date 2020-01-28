using System;
using System.Linq;
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
            IGuild guild = discordClient.Guilds
                .Where(guild => guild.Id == config.GetValue<ulong>("Discord:GuildID"))
                .FirstOrDefault();
            IRole verifiedRole = guild.Roles
                .Where(role => role.Name == config["Discord:VerifiedRoleName"])
                .FirstOrDefault();
            SocketGuildUser user = guild.GetUserAsync(discordID).Result as SocketGuildUser;
            user.AddRoleAsync(verifiedRole);
        }

        public void RemoveVerifiedRoleFromDiscordUser(ulong discordID)
        {
            IGuild guild = discordClient.Guilds
                .Where(guild => guild.Id == config.GetValue<ulong>("Discord:GuildID"))
                .FirstOrDefault();
            IRole verifiedRole = guild.Roles
                .Where(role => role.Name == config["Discord:VerifiedRoleName"])
                 .FirstOrDefault();
            SocketGuildUser user = guild.GetUserAsync(discordID).Result as SocketGuildUser;
            user.RemoveRoleAsync(verifiedRole);
        }

        // TODO
        // private SocketGuildUser GetGuildUser
        // create a delegate for something


    }
}