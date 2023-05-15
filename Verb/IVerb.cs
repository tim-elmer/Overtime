using Discord;
using Discord.WebSocket;

namespace Overtime.Verb;

public interface IVerb
{
    public string Command { get; }
    public string Description { get; }

    public ApplicationCommandProperties ApplicationCommandProperties =>
        new SlashCommandBuilder()
            .WithName(Command)
            .WithDescription(Description)
            .Build();

    public Task OnCommandExecuted(SocketCommandBase command);
}