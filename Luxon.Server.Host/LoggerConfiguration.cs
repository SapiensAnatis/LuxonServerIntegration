using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host;

internal static class LoggerConfiguration
{
    public static ILoggerFactory Factory { get; } =
        LoggerFactory.Create(builder =>
            builder
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Debug)
        );
}
