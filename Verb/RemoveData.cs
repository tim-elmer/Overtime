using Discord.WebSocket;

namespace Overtime.Verb
{
    public sealed class RemoveData : IVerb
    {
        private Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> _contextFactory;

        public string Command => "remove-data";

        public string Description => "Remove all data associated with your account.";

        public RemoveData(Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> contextFactory) =>
            _contextFactory = contextFactory;

        public async Task OnCommandExecuted(SocketSlashCommand command)
        {
            using var context = _contextFactory.CreateDbContext();
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
}