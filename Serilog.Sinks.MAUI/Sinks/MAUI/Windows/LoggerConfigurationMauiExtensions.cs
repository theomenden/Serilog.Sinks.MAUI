using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Sinks.MAUI;
using Serilog.Sinks.MAUI.Windows;
using System.Runtime.InteropServices;

namespace Serilog;

public static class LoggerConfigurationMauiExtensions
{
    private const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Adds a sink that writes log events to the Windows Event Log.
    /// </summary>
    /// <param name="sinkConfiguration">The provided logger configuration</param>
    /// <param name="source">The application's registered source name on the local computer</param>
    /// <param name="logName">The name of the log the source's entries are written to - possible values are <c>Application, System, or a custom log</c></param>
    /// <param name="machineName">Name of the machine hosting the event log written to. The local machine by default</param>
    /// <param name="manageEventSource">If <see langword="false"/> the we do not check/create an event source. Defaults to <see langword="true"/>, allowing the sink to manage event source creation</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required to write an event to the sink</param>
    /// <param name="outputTemplate">A provided message template that allows for a description of the formatting used to write to the sink. The default is <see cref="DefaultOutputTemplate"/></param>
    /// <param name="formatProvider">Supplies culture specific formatting information or <see langword="null"/></param>
    /// <param name="eventIdProvider">supplies event ids for emitted log events</param>
    /// <returns>The <see cref="LoggerConfiguration"/> for further chaining</returns>
    /// <exception cref="PlatformNotSupportedException">If the target platform is not Windows</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sinkConfiguration" /> is <see langword="null" />.</exception>
    public static LoggerConfiguration EventLog(
        this LoggerSinkConfiguration sinkConfiguration,
        string source,
        string? logName = null,
        string machineName = ".",
        bool manageEventSource = false,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        string outputTemplate = DefaultOutputTemplate,
        IFormatProvider? formatProvider = null,
        IEventIdHashProvider? eventIdProvider = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException(RuntimeInformation.OSDescription);
        }

        var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

        return sinkConfiguration.Sink(
           new WindowsLogSink(
            source,
            logName ?? "Application",
            formatter,
            machineName,
            manageEventSource,
            eventIdProvider ?? new EventIdHashProvider()
           ),
           restrictedToMinimumLevel);
    }

    /// <summary>
    /// Adds a sink that writes log events to the Windows Event Log.
    /// </summary>
    /// <param name="sinkConfiguration">The provided logger configuration</param>
    /// <param name="textFormatter">A provided formatter that controls how events are rendered into the file - to control plain text formatting use the overload that accepts an output template instead.</param>
    /// <param name="source">The application's registered source name on the local computer</param>
    /// <param name="logName">The name of the log the source's entries are written to - possible values are <c>Application, System, or a custom log</c></param>
    /// <param name="machineName">Name of the machine hosting the event log written to. The local machine by default</param>
    /// <param name="manageEventSource">If <see langword="false"/> the we do not check/create an event source. Defaults to <see langword="true"/>, allowing the sink to manage event source creation</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required to write an event to the sink</param>
    /// <param name="eventIdProvider">supplies event ids for emitted log events</param>
    /// <returns>The <see cref="LoggerConfiguration"/> for further chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sinkConfiguration" /> or <paramref name="textFormatter"/> is <see langword="null" />.</exception>
    public static LoggerConfiguration EventLog(
        this LoggerSinkConfiguration sinkConfiguration,
        ITextFormatter textFormatter,
        string source,
        string? logName = null,
        string machineName = ".",
        bool manageEventSource = false,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        IEventIdHashProvider? eventIdProvider = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        ArgumentNullException.ThrowIfNull(textFormatter);

        return sinkConfiguration.Sink(
            new WindowsLogSink(
                source,
                logName ?? "Application",
                textFormatter,
                machineName,
                manageEventSource,
                eventIdProvider ?? new EventIdHashProvider()
                ),
            restrictedToMinimumLevel);
    }
}
