using System.Reflection;

using Bloqbit;
using Bloqbit.Include;

using System;

using Discord;
using Discord.WebSocket;
using Discord.Webhook;

class Program
{
    private DiscordSocketClient? _client;
    private static List<Command> _commands = new List<Command>();

    private static readonly string? token = Environment.GetEnvironmentVariable("MAIN_TOKEN");
    private static readonly string? _webhookUrl = Environment.GetEnvironmentVariable("MAIN_LOG_WH");

    public Program()
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("MAIN_TOKEN environment variable is not set");
        }
        else
        {
            Log.Info("Token variable is set, starting...");
        }
    }

    static async Task Main(string[] _)
    {
        Log.Print("Starting Bloqbit ...");
        await new Program().RunBotAsync();
    }

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent,
                HandlerTimeout = null
            }
        );

        Log.Debug("Setting up event listeners...");

        _client.Log += Debug;

        _client.Ready += OnReadyAsync;

        Log.Debug("Logging in...");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.SlashCommandExecuted += OnSlashCommand;
        _client.MessageReceived += OnMessage;

        _client.JoinedGuild += OnJoinedGuildAsync;
        _client.LeftGuild += OnLeftGuildAsync;

        await Task.Delay(-1); // Keeps the bot running
    }

    static public List<Command> LoadAllCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Command)));

        Log.Info($"Adding {commandTypes.Count()} commands to loading queue");

        _commands = commandTypes
            .Select(t => Activator.CreateInstance(t) as Command)
            .Where(c => c is not null)
            .Cast<Command>()
            .ToList();

        return _commands;
    }

    private async Task OnReadyAsync()
    {
        await _client!.SetGameAsync("Nothing to see here...", "https://bloqbit.cubicstudios.xyz/", ActivityType.Watching);
        await RegisterCommandsAsync();

        try
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                Log.Warn("MAIN_LOG_WH environment variable is not set, using default logging method");
            }
            else
            {
                var webhook = new DiscordWebhookClient(_webhookUrl);

                if (webhook is not null)
                {
                    Embed logEmbed = new EmbedBuilder()
                    .WithAuthor(
                        new EmbedAuthorBuilder()
                        .WithName("Service Status")
                    )
                    .WithDescription($"{Assets.Icons.Check} **{_client.CurrentUser.GlobalName ?? "Bloqbit "}** is now __online__")
                    .WithColor(Assets.Colors.Primary)
                    .WithFooter(
                        new EmbedFooterBuilder()
                        .WithText(_client.CurrentUser.GlobalName ?? "Bloqbit ")
                        .WithIconUrl(_client.CurrentUser.GetDisplayAvatarUrl(ImageFormat.Auto, 512))
                    )
                    .Build();

                    await webhook.SendMessageAsync(embeds: [logEmbed], avatarUrl: _client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 512));
                }
                else
                {
                    Log.Error("Failed to create webhook client");
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }

        Log.Success($"Bloqbit is now online, running v{Bloqbit.Include.Version.Get()} on {_client?.Guilds.Count} servers!");
    }

    private Task Debug(LogMessage log)
    {
        switch (log.Severity)
        {
            case LogSeverity.Critical:
                Log.Critical(log.Message);
                break;

            case LogSeverity.Error:
                Log.Error(log.Message);
                break;

            case LogSeverity.Warning:
                Log.Warn(log.Message);
                break;

            case LogSeverity.Info:
                Log.Info(log.Message);
                break;

            case LogSeverity.Verbose:
                Log.Print(log.Message);
                break;

            case LogSeverity.Debug:
                Log.Debug(log.Message);
                break;

            default:
                Log.Print(log.Message);
                break;
        }

        return Task.CompletedTask;
    }

    private async Task RegisterCommandsAsync()
    {
        if (_client is not null)
        {
            var cmds = LoadAllCommands();
            Log.Info($"Registering {cmds.Count} commands globally (bulk)");

            var builtCommands = cmds
                .Select(c => c.Builder?.Build())
                .Where(b => b is not null)
                .Cast<ApplicationCommandProperties>()
                .ToList();

            foreach (var bc in builtCommands)
                Log.Debug($"Built command /{bc.Name}");

            if (builtCommands.Count > 0)
            {
                try
                {
                    await _client!.Rest.BulkOverwriteGlobalCommands([.. builtCommands]);
                    Log.Info($"Bulk registered {builtCommands.Count} commands globally");
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
        }
        else
        {
            Log.Error("Bloqbit client not found to register slash commands");
        }
    }

    private async Task OnSlashCommand(SocketSlashCommand command)
    {
        try
        {
            var matched = _commands.FirstOrDefault((c) => c.Builder.Build().Name.GetValueOrDefault("invalid") == command.Data.Name);
            var guild = _client?.GetGuild(command.GuildId.GetValueOrDefault());

            if (guild?.OwnerId == 358673524661157897 || command.ContextType != InteractionContextType.Guild)
            {
                if (matched is not null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_webhookUrl))
                        {
                            Log.Warn("MAIN_LOG_WH environment variable is not set, using default logging method");
                        }
                        else
                        {
                            var webhook = new DiscordWebhookClient(_webhookUrl);

                            if (webhook is not null)
                            {
                                string useDesc = $"Used **/{command.Data.Name}** in user context";

                                if (guild is not null) useDesc = $"Used **/{command.Data.Name}** in __{guild?.Name}__";

                                Embed logEmbed = new EmbedBuilder()
                                .WithAuthor(
                                    new EmbedAuthorBuilder()
                                    .WithName("Interaction")
                                )
                                .WithDescription(useDesc)
                                .WithColor(Assets.Colors.Tertiary)
                                .AddField(
                                new EmbedFieldBuilder()
                                    .WithName("Used At")
                                    .WithValue($"<t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:F> • <t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:R>")
                                    .WithIsInline(false)
                                )
                                .WithFooter(
                                    new EmbedFooterBuilder()
                                    .WithText(command.User.Username)
                                    .WithIconUrl(command.User.GetDisplayAvatarUrl(ImageFormat.Auto, 512))
                                )
                                .Build();

                                await webhook.SendMessageAsync(embeds: [logEmbed], avatarUrl: _client?.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 512));
                            }
                            else
                            {
                                Log.Error("Failed to create webhook client");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }

                    Log.Debug($"Running command /{command.Data.Name}");
                    await matched.ExecuteAsync(command, _client!);
                }
                else
                {
                    await command.RespondAsync(text: $"{Assets.Icons.XMark} Unknown command.", flags: MessageFlags.Ephemeral);
                    Log.Error($"Command /{command.CommandName} not found");
                }
            }
            else
            {
                await command.RespondAsync(text: $"{Assets.Icons.XMark} Bloqbit is currently in private testing and cannot be used in your server.", flags: MessageFlags.Ephemeral);
                Log.Error("Unauthorized guild tried to use a command");
            }
        }
        catch (Exception e)
        {
            string errRes = $"{Assets.Icons.XMark} An error occurred while executing this command.";

            if (command.HasResponded)
            {
                await command.FollowupAsync(text: errRes, flags: MessageFlags.Ephemeral);
            }
            else
            {
                await command.RespondAsync(text: errRes, flags: MessageFlags.Ephemeral);
            }

            Log.Error(e.Message);
        }
    }

    private async Task OnMessage(SocketMessage msg)
    {
        var msgs = await msg.Channel.GetMessagesAsync(100, CacheMode.AllowDownload).ToArrayAsync();
        Log.Debug($"Cached {msgs.Length}/{msg.Channel.GetCachedMessages().Count} messages from guild channel");
    }

    private async Task OnJoinedGuildAsync(SocketGuild guild)
    {
        try
        {
            var webhook = new DiscordWebhookClient(_webhookUrl);

            if (webhook is not null)
            {
                var date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                Embed logEmbed = new EmbedBuilder()
                .WithAuthor(
                    new EmbedAuthorBuilder()
                    .WithName("Servers")
                )
                .WithDescription($"{Assets.Icons.Plus} **{_client?.CurrentUser.GlobalName ?? "Bloqbit "}** was authorized to join the guild __{guild.Name}__")
                .WithColor(Assets.Colors.Primary)
                .AddField(
                    new EmbedFieldBuilder()
                    .WithName("Time of Join")
                    .WithValue($"<t:{date}:F> • <t:{date}:R>")
                    .WithIsInline(false)
                )
                .WithFooter(
                    new EmbedFooterBuilder()
                    .WithText(guild.Name)
                    .WithIconUrl(guild.IconUrl)
                )
                .Build();

                await webhook.SendMessageAsync(embeds: [logEmbed], avatarUrl: _client?.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 512));
            }
            else
            {
                Log.Error("Failed to create webhook client");
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }

        Log.Info($"Authorized to join guild {guild.Name} ({guild.Id})");
    }

    private async Task OnLeftGuildAsync(SocketGuild guild)
    {
        try
        {
            var webhook = new DiscordWebhookClient(_webhookUrl);

            if (webhook is not null)
            {
                var date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                Embed logEmbed = new EmbedBuilder()
                .WithAuthor(
                    new EmbedAuthorBuilder()
                    .WithName("Servers")
                )
                .WithDescription($"{Assets.Icons.Minus} **{_client?.CurrentUser.GlobalName ?? "Bloqbit "}** was forced to leave the guild __{guild.Name}__")
                .WithColor(Assets.Colors.Secondary)
                .AddField(
                    new EmbedFieldBuilder()
                    .WithName("Time of Leave")
                    .WithValue($"<t:{date}:F> • <t:{date}:R>")
                    .WithIsInline(false)
                )
                .WithFooter(
                    new EmbedFooterBuilder()
                    .WithText(guild.Name)
                    .WithIconUrl(guild.IconUrl)
                )
                .Build();

                await webhook.SendMessageAsync(embeds: [logEmbed], avatarUrl: _client?.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 512));
            }
            else
            {
                Log.Error("Failed to create webhook client");
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }

        Log.Info($"Forced to leave guild {guild.Name} ({guild.Id})");
    }
}