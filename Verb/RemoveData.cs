using Discord.WebSocket;

namespace Overtime.Verb;

public sealed class RemoveData : IVerb
{
    private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> _contextFactory;

    public string Command => "z-remove-all-user-data";

    public string Description => "Remove all data associated with your account.";

    public RemoveData(Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task OnCommandExecuted(SocketCommandBase command)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var userInformation = context.UserInformation.FirstOrDefault(i => i.UserId == command.User.Id);
        if (userInformation is null)
        {
            await command.RespondAsync("I don't know you... 🤔\r\r(No user data to remove.)", ephemeral: true);
            return;
        }

        context.UserInformation.Remove(userInformation);
        await context.SaveChangesAsync();
        await command.RespondAsync("Done.", ephemeral: true);
    }
}