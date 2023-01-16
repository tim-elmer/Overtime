using Discord;
using Discord.WebSocket;

namespace Overtime.Verb
{
    public sealed class SetTimeZone : IVerb
    {
        private static readonly IReadOnlyCollection<TimeZoneInfo> TIME_ZONES = TimeZoneInfo.GetSystemTimeZones();

        private Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> _contextFactory;
        private ApplicationCommandProperties _applicationCommandProperties;

        public string Command => "set-timezone";
        public string Description => "Set your timezone for time translation (global).";
        public ApplicationCommandProperties ApplicationCommandProperties => _applicationCommandProperties;

        public SetTimeZone(Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> contextFactory)
        {
            _contextFactory = contextFactory;

            _applicationCommandProperties = new SlashCommandBuilder()
            {
                Name = Command,
                Description = Description,
                Options = new()
                {
                    new SlashCommandOptionBuilder()
                    {
                        Name = "timezone",
                        Type = ApplicationCommandOptionType.String,
                        Description = "The timezone in which you reside, from which your messages will be converted.",
                        IsRequired = true
                    }
                }
            }.Build();
        }

        public async Task OnCommandExecuted(SocketSlashCommand command)
        {
            var timeZoneOption = command.Data.Options.FirstOrDefault();
            if (timeZoneOption is null)
            {
                await command.RespondAsync("You *will* need to provide a timezone for me to save it...", ephemeral: true);
                return;
            }
            var timeZoneIdUserValue = timeZoneOption.Value as string;
            if (string.IsNullOrWhiteSpace(timeZoneIdUserValue))
            {
                await command.RespondAsync("You *will* need to provide a timezone for me to save it...", ephemeral: true);
                return;
            }
            var timeZones = TIME_ZONES.Where(z => 
                z.Id.Contains(timeZoneIdUserValue, StringComparison.InvariantCultureIgnoreCase) ||
                z.DisplayName.Contains(timeZoneIdUserValue, StringComparison.InvariantCultureIgnoreCase) ||
                z.StandardName.Contains(timeZoneIdUserValue, StringComparison.OrdinalIgnoreCase) ||
                z.DaylightName.Contains(timeZoneIdUserValue, StringComparison.OrdinalIgnoreCase)
            );

            if (timeZones.Count() == 0)
            {
                await command.RespondAsync("I didn't recognize that timezone.", ephemeral: true);
                return;
            }

            string timeZoneId;
            if (timeZones.Count() == 1)
                timeZoneId = timeZones.First().Id;
            else
            {
                await command.RespondAsync($"Sorry, you're going to need to be more specific. I found the following similar timezones:\r{FormatListTimeZones(timeZones)}", ephemeral: true);
                return;
            }

            using var context = _contextFactory.CreateDbContext();
            var userInformation = context.UserInformation.FirstOrDefault(i => i.UserId == command.User.Id);
            if (userInformation is null)
                await context.AddAsync(userInformation = new() { UserId = command.User.Id });
            userInformation.TimeZoneId = timeZoneId;
            await context.SaveChangesAsync();
            await command.RespondAsync($"Got it! Timezone set to {timeZoneId} 👍", ephemeral: true);
        }

        private string FormatListTimeZones(IEnumerable<TimeZoneInfo> timeZones)
        {
            var stringBuilder = new System.Text.StringBuilder("  • ");
            stringBuilder.AppendJoin("\r  • ", timeZones.Select(z => $"{z.DisplayName}: {z.Id}").Take(25));
            if (timeZones.Count() > 25)
                stringBuilder.Append("\r    ...");
            return stringBuilder.ToString();
        }
    }
}