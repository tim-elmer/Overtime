using System.ComponentModel.DataAnnotations;

namespace Overtime.Model
{
    public class UserInformation
    {
        /// <summary>
        /// The user's Discord UID
        /// </summary>
        [Key]
        public ulong UserId { get; set; }

        [ConcurrencyCheck]
        public Guid Version { get; set; }

        /// <summary>
        /// The user's time zone ID
        /// </summary>
        public string TimeZoneId { get; set; } = string.Empty;

        /// <summary>
        /// Should the user not be reminded to set their timezone.
        /// </summary>
        public bool NoRemind { get; set; }

        /// <summary>
        /// Get a <see cref="TimeZoneInfo"/> for the user.
        /// </summary>
        /// <returns>Null if not set.</returns>
        public TimeZoneInfo? GetTimeZoneInfo() =>
            String.IsNullOrWhiteSpace(TimeZoneId) ? null : TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }
}