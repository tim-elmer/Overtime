using System.Collections.ObjectModel;
using Discord;
using Serilog.Events;

namespace Overtime.Helpers;

public static class Logging
{
    public static readonly ReadOnlyDictionary<LogSeverity, LogEventLevel> LogLevelMap = new(
        new Dictionary<LogSeverity, LogEventLevel>
        {
            { LogSeverity.Critical, LogEventLevel.Fatal },
            { LogSeverity.Error, LogEventLevel.Error },
            { LogSeverity.Warning, LogEventLevel.Warning },
            { LogSeverity.Info, LogEventLevel.Information },
            { LogSeverity.Verbose, LogEventLevel.Verbose },
            { LogSeverity.Debug, LogEventLevel.Debug }
        });
}