using Discord;
using Discord.WebSocket;
using Serilog;

namespace Overtime.TrapHandler
{
    public sealed class ConvertibleTime : ITrapHandler, IButtonHandler
    {
        private const string PATTERN = @"(?i)\b\d{1,2}(?:(?::\d{2}(?: ?[ap]m)?)|(?: ?[ap]m))\b",
        NO_REMIND_TIMEZONE_BUTTON_ID = "noRemindTimeZone",
        NO_REMIND_TIMEZONE_BUTTON_TEXT = "Don't remind me again";

        private static readonly MessageComponent UNKNOWN_TIMEZONE_NO_REMIND_BUTTON = new ComponentBuilder()
            .WithButton(NO_REMIND_TIMEZONE_BUTTON_TEXT, customId: NO_REMIND_TIMEZONE_BUTTON_ID, ButtonStyle.Danger)
            .Build(),
        UNKNOWN_TIMEZONE_NO_REMIND_BUTTON_DISABLED = new ComponentBuilder()
            .WithButton(NO_REMIND_TIMEZONE_BUTTON_TEXT, customId: NO_REMIND_TIMEZONE_BUTTON_ID,
                ButtonStyle.Danger, disabled: true)
            .Build();

        private Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> _contextFactory;

        public string ButtonId => NO_REMIND_TIMEZONE_BUTTON_ID;

        public ConvertibleTime(Microsoft.EntityFrameworkCore.IDbContextFactory<Model.Context> contextFactory) =>
            _contextFactory = contextFactory;

        public async Task OnButtonExecuted(SocketMessageComponent messageComponent)
        {
            using var context = _contextFactory.CreateDbContext();
            var userInformation = context.UserInformation.FirstOrDefault(t => t.UserId == messageComponent.User.Id);
            if (userInformation is null)
                context.UserInformation.Add(userInformation = new() { UserId = messageComponent.User.Id});
            userInformation.NoRemind = true;
            await context.SaveChangesAsync();

            await messageComponent.Channel.ModifyMessageAsync(messageComponent.Message.Id, (messageProperties) =>
            {
                messageProperties.Components = new(UNKNOWN_TIMEZONE_NO_REMIND_BUTTON_DISABLED);
            });
            await messageComponent.RespondAsync("I see how it is. I won't remind you again. 😭");
        }

        public async Task Trap(SocketMessage message)
        {
            Log.Debug("Received: {message}", message.CleanContent);

            if (message.Author.IsBot)
                return;

            var matches = System.Text.RegularExpressions.Regex
                .Matches(message.CleanContent, PATTERN)
                .Select(t => DateTime.Parse(t.Value));

            if (matches.Count() == 0)
                return;

            // * Check for user TZ known, notify if not.
            using var context = _contextFactory.CreateDbContext();
            var userInformation = context.UserInformation.FirstOrDefault(i => i.UserId == message.Author.Id);
            if (userInformation is null || (userInformation.GetTimeZoneInfo() is null && !userInformation.NoRemind))
            {
                await (await message.Author.CreateDMChannelAsync()).SendMessageAsync("That looks like a time, but you haven't told me what timezone you're in. To do so, use the command `/set-timezone`. If you'd like me to *stop* notifying you about this, click the button below 😢 (you can still use `/set-timezone` if you change your mind 😉).", components: UNKNOWN_TIMEZONE_NO_REMIND_BUTTON);
                return;
            }
            if (userInformation.GetTimeZoneInfo() is null && userInformation!.NoRemind)
                return;

            System.Text.StringBuilder stringBuilder = new();
            stringBuilder.AppendJoin(", ", matches.Select(t => TimestampTag.FormatFromDateTime(
            TimeZoneInfo.ConvertTime(t, sourceTimeZone: userInformation.GetTimeZoneInfo()!, destinationTimeZone: TimeZoneInfo.Local), TimestampTagStyles.ShortTime)
            ));
            await message.Channel.SendMessageAsync(stringBuilder.ToString(), messageReference: new(message.Id));
        }
    }
}