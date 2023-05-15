using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Overtime.Model;

namespace Overtime.Verb;

public class RemoveTranslation : IVerb
{
    private readonly Configuration _configuration;

    public string Command => "remove-translation";

    public string Description =>
        $"Remove last translation in channel. Limited to last {_configuration.LookBackLimit} messages, last {_configuration.LookBackTime} minutes from you.";

    public RemoveTranslation(IOptions<Configuration> options) =>
        _configuration = options.Value ??
                         throw new ArgumentNullException(nameof(options));

    public async Task OnCommandExecuted(SocketCommandBase command)
    {
        var message = command.Channel.GetMessagesAsync(_configuration.LookBackLimit).FlattenAsync().Result
            .Where(m => m.CreatedAt >
                        new DateTimeOffset(DateTime.Now - TimeSpan.FromMinutes(_configuration.LookBackTime)))
            .OrderBy(m => m.CreatedAt)
            .LastOrDefault(m => m.Author.Id == command.ApplicationId);
        if (message is null)
        {
            await command.RespondAsync("You have no recently translated messages in this channel.", ephemeral: true);
            return;
        }

        await command.Channel.DeleteMessageAsync(message.Id);
        await command.RespondAsync("Translation removed.", ephemeral: true);
    }
}