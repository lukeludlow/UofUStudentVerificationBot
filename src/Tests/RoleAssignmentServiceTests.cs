using System;
using System.Collections.Generic;
using System.IO;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UofUStudentVerificationBot;

namespace UofUStudentVerificationBotTests
{
    // [TestClass]
    public class RoleAssignmentServiceTests
    {

        // [TestMethod]
        // public void AssignVerifiedRoleToDiscordUser_UserDoesntHaveRole_ShouldLookupGuildFromConfig()
        // {
        //     DiscordSocketClient discordClient = Mock.Of<DiscordSocketClient>();
        //     IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        //     config["Discord:GuildID"] = "420";
        //     IRoleAssignmentService roleAssignmentService = new RoleAssignmentService(discordClient, config);
        //     roleAssignmentService.AssignVerifiedRoleToDiscordUser(69);
        //     Mock.Get(discordClient).Verify(m => m.Guilds, Times.Once());
        //     Mock.Get(discordClient).Verify(m => m.GetGuild(420), Times.Once());
        //     // Mock.Get(discordClient).Verify(m => m.GetUser(69), Times.Once());
        // }

    }
}