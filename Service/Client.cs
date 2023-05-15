using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Overtime.Model;
using Overtime.TrapHandler;
using Overtime.Verb;
using Serilog;

namespace Overtime.Service;

public sealed class Client : BackgroundService
{
    private readonly DiscordSocketClient _client = new(
        new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.DirectMessages | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.Guilds
        }
    );

    private readonly IEnumerable<IButtonHandler> _buttonHandlers;
    private readonly Configuration _configuration;
    private readonly IEnumerable<ITrapHandler> _trapHandlers;
    private readonly IEnumerable<IVerb> _verbs;

    public Client(IOptions<Configuration> options, IEnumerable<IButtonHandler> buttonHandlers,
        IEnumerable<ITrapHandler> trapHandlers, IEnumerable<IVerb> verbs)
    {
        _buttonHandlers = buttonHandlers;
        _configuration = options.Value;
        _trapHandlers = trapHandlers;
        _verbs = verbs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Starting API client");

        async void Callback() => await _client.StopAsync();

        stoppingToken.Register(Callback);
        _client.ButtonExecuted += OnButtonExecuted;
        _client.Log += LogAsync;
        _client.MessageReceived += OnMessageReceived;
        _client.Ready += OnClientReady;
        _client.SlashCommandExecuted += OnSlashCommandExecuted;
        _client.JoinedGuild += CreateGuildCommands;

        await _client.LoginAsync(TokenType.Bot, _configuration.Token);
        await _client.StartAsync();
        Log.Information("API client started");
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        foreach (var trapHandler in _trapHandlers)
            await trapHandler.Trap(message);
    }

    private static async Task LogAsync(LogMessage message)
    {
        Log.Write(Helpers.Logging.LogLevelMap[message.Severity], message.Exception, "[{Source}] {Message}",
            message.Source, message.Message);
        await Task.CompletedTask;
    }

    private async Task OnClientReady()
    {
        foreach (var guild in _client.Guilds)
            await CreateGuildCommands(guild);
    }

    private async Task CreateGuildCommands(SocketGuild guild)
    {
        await guild.DeleteApplicationCommandsAsync();
        foreach (var verb in _verbs)
            await guild.CreateApplicationCommandAsync(verb.ApplicationCommandProperties);
    }

    private async Task OnButtonExecuted(SocketMessageComponent messageComponent)
    {
        var buttonHandler =
            _buttonHandlers.FirstOrDefault(h => h.ButtonIdList.Contains(messageComponent.Data.CustomId));
        if (buttonHandler is not null)
            await buttonHandler.OnButtonExecuted(messageComponent);
    }

    private async Task OnSlashCommandExecuted(SocketSlashCommand slashCommand)
    {
        var verb = _verbs.FirstOrDefault(verb => verb.Command == slashCommand.CommandName);
        if (verb is null)
            throw new IndexOutOfRangeException("Unknown verb.");
        await verb.OnCommandExecuted(slashCommand);
    }
}