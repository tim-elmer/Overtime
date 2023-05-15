using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Overtime.Model;
using Serilog;

namespace Overtime.TrapHandler;

public sealed partial class ConvertibleTime : ITrapHandler, IButtonHandler
{
    private const string NoRemindTimezoneButtonId = "noRemindTimeZone",
        NoRemindTimezoneButtonText = "Don't remind me again";

    private static readonly MessageComponent UnknownTimezoneNoRemindButton = new ComponentBuilder()
            .WithButton(NoRemindTimezoneButtonText, customId: NoRemindTimezoneButtonId, ButtonStyle.Danger)
            .Build(),
        UnknownTimezoneNoRemindButtonDisabled = new ComponentBuilder()
            .WithButton(NoRemindTimezoneButtonText, customId: NoRemindTimezoneButtonId,
                ButtonStyle.Danger, disabled: true)
            .Build();

    private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<Context> _contextFactory;

    public List<string> ButtonIdList => new() { NoRemindTimezoneButtonId };

    public ConvertibleTime(Microsoft.EntityFrameworkCore.IDbContextFactory<Context> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task OnButtonExecuted(SocketMessageComponent messageComponent) =>
        await OnNoRemindTimezoneButtonExecuted(messageComponent);

    private async Task OnNoRemindTimezoneButtonExecuted(SocketMessageComponent messageComponent)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var userInformation = context.UserInformation.FirstOrDefault(t => t.UserId == messageComponent.User.Id);
        if (userInformation is null)
            context.UserInformation.Add(userInformation = new UserInformation { UserId = messageComponent.User.Id });
        userInformation.NoRemind = true;
        await context.SaveChangesAsync();

        await messageComponent.Channel.ModifyMessageAsync(messageComponent.Message.Id,
            messageProperties =>
            {
                messageProperties.Components = new Optional<MessageComponent>(UnknownTimezoneNoRemindButtonDisabled);
            });
        await messageComponent.RespondAsync("I see how it is. I won't remind you again. 😭");
    }

    public async Task Trap(SocketMessage message)
    {
        Log.Debug("Received: {CleanMessage}", message.CleanContent);

        if (message.Author.IsBot)
            return;

        var matches = TimeRegex().Matches(message.CleanContent)
            .Select(t => DateTime.Parse(t.Value));

        var dateTimes = matches.ToList();
        if (!dateTimes.Any())
            return;

        // * Check for user TZ known, notify if not.
        await using var context = await _contextFactory.CreateDbContextAsync();
        var userInformation = context.UserInformation.FirstOrDefault(i => i.UserId == message.Author.Id);
        if (userInformation is null || userInformation.GetTimeZoneInfo() is null && !userInformation.NoRemind)
        {
            await (await message.Author.CreateDMChannelAsync()).SendMessageAsync(
                "That looks like a time, but you haven't told me what timezone you're in. To do so, use the command `/set-timezone`. If you'd like me to *stop* notifying you about this, click the button below 😢 (you can still use `/set-timezone` if you change your mind 😉).",
                components: UnknownTimezoneNoRemindButton);
            return;
        }

        if (userInformation.GetTimeZoneInfo() is null && userInformation.NoRemind)
            return;

        System.Text.StringBuilder stringBuilder = new();
        stringBuilder.AppendJoin(", ", dateTimes.Select(t => TimestampTag.FormatFromDateTime(
            TimeZoneInfo.ConvertTime(t, sourceTimeZone: userInformation.GetTimeZoneInfo()!,
                destinationTimeZone: TimeZoneInfo.Local), TimestampTagStyles.ShortTime)
        ));
        await message.Channel.SendMessageAsync(stringBuilder.ToString(),
            messageReference: new MessageReference(message.Id) /*, components: RemoveTimeButton*/);
    }

    [GeneratedRegex("(?i)\\b\\d{1,2}(?:(?::\\d{2}(?: ?[ap]m)?)|(?: ?[ap]m))\\b", RegexOptions.None, "en-US")]
    private static partial Regex TimeRegex();
}