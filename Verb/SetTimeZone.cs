using Discord;
using Discord.WebSocket;
using Overtime.Model;

namespace Overtime.Verb;

public sealed class SetTimeZone : IVerb
{
    private static readonly IReadOnlyCollection<TimeZoneInfo> TimeZones = TimeZoneInfo.GetSystemTimeZones();

    private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<Context> _contextFactory;

    public string Command => "set-timezone";
    public string Description => "Set your timezone for time translation (global).";
    public ApplicationCommandProperties ApplicationCommandProperties { get; }

    public SetTimeZone(Microsoft.EntityFrameworkCore.IDbContextFactory<Context> contextFactory)
    {
        _contextFactory = contextFactory;

        ApplicationCommandProperties = new SlashCommandBuilder
        {
            Name = Command,
            Description = Description,
            Options = new List<SlashCommandOptionBuilder>
            {
                new()
                {
                    Name = "timezone",
                    Type = ApplicationCommandOptionType.String,
                    Description = "The timezone in which you reside, from which your messages will be converted.",
                    IsRequired = true
                }
            }
        }.Build();
    }

    public async Task OnCommandExecuted(SocketCommandBase command)
    {
        var slashCommand = command as SocketSlashCommand ??
                           throw new InvalidCastException();

        var timeZoneOption = slashCommand.Data.Options.FirstOrDefault();
        if (timeZoneOption is null)
        {
            await slashCommand.RespondAsync("You *will* need to provide a timezone for me to save it...",
                ephemeral: true);
            return;
        }

        var timeZoneIdUserValue = timeZoneOption.Value as string;
        if (string.IsNullOrWhiteSpace(timeZoneIdUserValue))
        {
            await slashCommand.RespondAsync("You *will* need to provide a timezone for me to save it...",
                ephemeral: true);
            return;
        }

        var timeZones = TimeZones.Where(z =>
            z.Id.Contains(timeZoneIdUserValue, StringComparison.InvariantCultureIgnoreCase) ||
            z.DisplayName.Contains(timeZoneIdUserValue, StringComparison.InvariantCultureIgnoreCase) ||
            z.StandardName.Contains(timeZoneIdUserValue, StringComparison.OrdinalIgnoreCase) ||
            z.DaylightName.Contains(timeZoneIdUserValue, StringComparison.OrdinalIgnoreCase)
        );

        var timeZoneInfos = timeZones.ToList();
        if (!timeZoneInfos.Any())
        {
            await slashCommand.RespondAsync("I didn't recognize that timezone.", ephemeral: true);
            return;
        }

        string timeZoneId;
        if (timeZoneInfos.Count == 1)
            timeZoneId = timeZoneInfos.First().Id;
        else
        {
            await slashCommand.RespondAsync(
                $"Sorry, you're going to need to be more specific. I found the following similar timezones:\r{FormatListTimeZones(timeZoneInfos)}",
                ephemeral: true);
            return;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var userInformation = context.UserInformation.FirstOrDefault(i => i.UserId == slashCommand.User.Id);
        if (userInformation is null)
            await context.AddAsync(userInformation = new UserInformation { UserId = slashCommand.User.Id });
        userInformation.TimeZoneId = timeZoneId;
        await context.SaveChangesAsync();
        await slashCommand.RespondAsync($"Got it! Timezone set to {timeZoneId} 👍", ephemeral: true);
    }

    private static string FormatListTimeZones(IEnumerable<TimeZoneInfo> timeZones)
    {
        var stringBuilder = new System.Text.StringBuilder("  • ");
        var timeZoneInfos = timeZones.ToList();
        stringBuilder.AppendJoin("\r  • ", timeZoneInfos.Select(z => $"{z.DisplayName}: {z.Id}").Take(25));
        if (timeZoneInfos.Count > 25)
            stringBuilder.Append("\r    ...");
        return stringBuilder.ToString();
    }
}