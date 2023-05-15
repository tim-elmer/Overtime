namespace Overtime.Model;

public class Configuration
{
    private string _database = "./overtime.sqlite";
    private int _lookBackLimit = 10;
    private int _lookBackTime = 30;

    /// <summary>
    /// The path to the database file.
    /// </summary>
    public string Database
    {
        get => _database;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Database));
            _database = value;
        }
    }

    /// <summary>
    /// Your Discord token for the application.
    /// </summary>
    /// <remarks>Sensitive (duh).</remarks>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The number of prior messages that will be checked when a user requests removal of a translation.
    /// </summary>
    /// <remarks>
    /// Do not fetch too many messages at once! This may cause unwanted preemptive rate limit or even actual rate limit, causing your bot to freeze! 
    /// </remarks>
    public int LookBackLimit
    {
        get => _lookBackLimit;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(LookBackLimit),
                    "The look-back limit cannot be less than 1.");
            _lookBackLimit = value;
        }
    }

    /// <summary>
    /// The maximum age of a message that may have a translation removed, in minutes.
    /// </summary>
    public int LookBackTime
    {
        get => _lookBackTime;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(LookBackTime),
                    "The look-back time cannot be less than 1 minute.");
            _lookBackTime = value;
        }
    }
}