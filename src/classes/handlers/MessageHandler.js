const SysSettings = require("../../settings.json");
const { Message, Client, PermissionsBitField, PermissionFlagsBits, Events } = require("discord.js");
const ModActionType = require("../enum/ModActionType");
const cache = require("../../cache");
const resolve = require("../../modules/resolve");
const moderation = require("../../modules/moderation");

class MessageHandler {
    /**
     * 
     * @param {Client} client Discord bot client.
     */
    constructor(client) {
        console.debug("Initiating global message handler...");

        client.on(Events.MessageCreate, async (m) => {
            const s = cache.fetch(m.guild?.id);

            if (s && m) {
                await this.messageSend(s, m);
            };
        });
    };

    /**
     * @param {typeof SysSettings} system Server settings.
     * @param {Message} message Discord message.
     */
    messageSend = async (system, message) => {
        if (message.guild) {
            if (message.author.bot) {
                return;
            } else {
                const inF = moderation.inFilter(system, message);
                if (inF.punishment) return await moderation.punish(inF.punishment, message.member, inF.warning.value);
            };
        };
    };
};

module.exports = MessageHandler;