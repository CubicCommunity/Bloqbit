const SysAssets = require("../../assets.json");
const SysSettings = require("../../settings.json");
const { BotDatabase } = require("../../classes.js");
const { ChatInputCommandInteraction, Invite } = require("discord.js");
const { SlashCommandBuilder } = require("@discordjs/builders");
const { ChannelType, PermissionFlagsBits } = require('discord-api-types/v10');

module.exports = {
    data: new SlashCommandBuilder()
        .setName("invite")
        .setDescription("Create a permanent invite for this server.")
        .addChannelOption((c) => c
            .setName("channel")
            .setDescription("Channel to create the invite in.")
            .addChannelTypes([ChannelType.GuildText, ChannelType.GuildVoice, ChannelType.GuildStageVoice, ChannelType.GuildAnnouncement, ChannelType.GuildForum, ChannelType.GuildMedia]))
        .setDefaultMemberPermissions(PermissionFlagsBits.CreateInstantInvite),
    /**
     * 
     * @param {ChatInputCommandInteraction} interaction The interaction for the slash command.
     * @param {typeof SysAssets} assets The configuration of the client's visual assets.
     * @param {typeof SysSettings} system The settings model for the bot's configuration.
     * @param {BotDatabase} db The database information.
     * 
     * @returns {void}
     */
    execute: async (interaction, assets, system, db) => {
        const channel = interaction.options?.getChannel("channel", false);

        let invite = null;

        if (channel) {
            invite = await channel.createInvite({
                maxAge: 0,
                maxUses: 0,
                unique: true,
            });
        } else {
            invite = await interaction.channel?.createInvite({
                maxAge: 0,
                maxUses: 0,
                unique: true,
            });
        };

        if (invite) {
            return await interaction.reply({
                "content": "",
                "embeds": [
                    {
                        "description": `${assets.icons.check} | **${interaction.user?.username}** - Successfully created permanent invite code \`${invite.code}\``,
                        "color": assets.colors.primary,
                    },
                ],
            });
        } else {
            return await interaction.reply({
                "content": "",
                "embeds": [
                    {
                        "description": `${assets.icons.xmark} | **${interaction.user?.username}** - Failed to create invite code.`,
                        "color": assets.colors.primary,
                    },
                ],
            });
        };
    },
};