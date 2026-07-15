using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host;

internal sealed partial class LogImports
{
    [SuppressMessage(
        "Microsoft.Performance",
        "CA1823:AvoidUnusedPrivateFields",
        Justification = "Used by the logging source generator"
    )]
    private readonly ILogger nativeLogger = LoggerConfiguration.Factory.CreateLogger(
        "LuxonServerNative"
    );

    public void CustomLogSink(LuxonLogLevel luxonLevel, string message)
    {
        LogLevel level = luxonLevel switch
        {
            LuxonLogLevel.Trace => LogLevel.Trace,
            LuxonLogLevel.Debug => LogLevel.Debug,
            LuxonLogLevel.Info => LogLevel.Information,
            LuxonLogLevel.Warn => LogLevel.Warning,
            LuxonLogLevel.Error => LogLevel.Error,
            LuxonLogLevel.Critical => LogLevel.Critical,
            LuxonLogLevel.Off => LogLevel.None,
            _ => throw new UnreachableException("Invalid LuxonLogLevel."),
        };

        this.LogNativeMessage(level, message);
    }

    [LoggerMessage("{NativeMessage}")]
    private partial void LogNativeMessage(LogLevel level, string nativeMessage);
}
