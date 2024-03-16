using System;
using System.Diagnostics;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.MAUI.Windows;
using static Serilog.Events.LogEventLevel;

namespace Serilog.Sinks.MAUI;

/// <summary>
/// Writes log events to the Windows Event Log.
/// </summary>
/// <remarks>CAUTION: Changing the source/logname can have unexpected results within the Windows Event Log</remarks>
public sealed class WindowsLogSink : ILogEventSink
{
    private const string ApplicationLogName = "Application";
    private const int MaximumPayloadLengthCharacters = 31839;
    private const int MaximumSourceNameLengthCharacters = 212;
    private const int SourceMovedEventId = 3;

    private readonly IEventIdHashProvider _eventIdHashProvider;
    private readonly EventLog _eventLog;
    private readonly ITextFormatter _formatter;

    /// <inheritdoc />
    public WindowsLogSink(
        string source,
        string logName,
        ITextFormatter textFormatter,
        string machineName,
        bool manageEventSource)
    : this(source, logName, textFormatter, machineName, manageEventSource, new EventIdHashProvider())
    {
    }

    /// <summary>
    /// Construct a <see cref="WindowsLogSink"/> that writes to the Windows Event Log, with the specified <paramref name="source"/> if one does not exist.
    /// </summary>
    /// <param name="source">The source name which the application is registered to on the local computer</param>
    /// <param name="logName">The name of the log that the source's entries are written to - e.g. <c>Application, System</c>, or a custom event log</param>
    /// <param name="textFormatter">Supplies culture-specific formatting information or <see langword="null"/></param>
    /// <param name="machineName">The name of the machine hosting the event log that we're writing to</param>
    /// <param name="manageEventSource">If <see langword="false" /> then we don't check/create the event source. Otherwise, <see langword="true" /> allows the sink to manage event source creation</param>
    /// <param name="eventIdHashProvider">Supplies event ids for the emitted log events</param>
    public WindowsLogSink(
        string source,
        string logName,
        ITextFormatter textFormatter,
        string machineName,
        bool manageEventSource,
        IEventIdHashProvider eventIdHashProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(logName);
        ArgumentNullException.ThrowIfNull(textFormatter);
        ArgumentNullException.ThrowIfNull(eventIdHashProvider);

        if (source.Length > MaximumSourceNameLengthCharacters)
        {
            SelfLog.WriteLine("The source name '{0}' is too long. Trimming down to {1} characters",
                                              source, MaximumSourceNameLengthCharacters);

            source = source[..MaximumSourceNameLengthCharacters];
        }

        source = source.Replace('<', '_');
        source = source.Replace('>', '_');

        var resolvedLogName = String.IsNullOrWhiteSpace(logName) ? ApplicationLogName : logName;

        _eventIdHashProvider = eventIdHashProvider;
        _formatter = textFormatter;
        _eventLog = new EventLog(resolvedLogName, machineName, source);

        if (manageEventSource)
        {
            ConfigureEventSource(_eventLog, source);
        }

        _eventLog.Source = source;
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="logEvent">The log event to write</param>
    /// <remarks>
    /// <list type="bullet">
    ///     <listheader>
    ///         <term>Serilog Event Level</term>
    ///         <description>EventLog Entry Type</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Debug,Information, and Verbose</term>
    ///         <description>These have been mapped to <see cref="System.Diagnostics.EventLogEntryType.Information"/></description>
    ///     </item>
    ///     <item>
    ///         <term>Error and Fatal</term>
    ///         <description>Have been mapped to <see cref="System.Diagnostics.EventLogEntryType.Error"/></description>
    ///     </item>
    ///     <item>
    ///         <term>Warning</term>
    ///         <description>Has of course been registered as <see cref="System.Diagnostics.EventLogEntryType.Warning"/></description>
    ///     </item>
    /// </list>
    /// </remarks>
    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        var type = LevelToEventLogEntryType(logEvent.Level);

        using var payloadWriter = new StringWriter();
        _formatter.Format(logEvent, payloadWriter);

        var payload = payloadWriter.ToString();

        if (payload.Length > MaximumPayloadLengthCharacters)
        {
            SelfLog.WriteLine("The payload is too long. Trimming down to {0} characters",
                                                             MaximumPayloadLengthCharacters);
            payload = payload[..MaximumPayloadLengthCharacters];
        }

        _eventLog.WriteEntry(payload, type, _eventIdHashProvider.ComputeEventIdHash(logEvent));

    }

    private static void ConfigureEventSource(EventLog eventLog, string source)
    {
        var sourceData = new EventSourceCreationData(source, eventLog.Log)
        {
            MachineName = eventLog.MachineName
        };

        var oldLogName = String.Empty;

        if (!EventLog.SourceExists(source, eventLog.MachineName))
        {
            EventLog.CreateEventSource(sourceData);
        }
        else
        {
            var existingLogWithSourceName = EventLog.LogNameFromSourceName(source, eventLog.MachineName);

            if (String.IsNullOrWhiteSpace(existingLogWithSourceName)
                && eventLog.Log.Equals(existingLogWithSourceName, StringComparison.OrdinalIgnoreCase))
            {
                EventLog.DeleteEventSource(source, eventLog.MachineName);
                oldLogName = existingLogWithSourceName;
            }
        }

        if (!string.IsNullOrEmpty(oldLogName))
        {
            var metaSource = $"serilog-{eventLog.Log}";
            if (!EventLog.SourceExists(metaSource, eventLog.MachineName))
            {
                EventLog.CreateEventSource(new EventSourceCreationData(metaSource, eventLog.Log)
                {
                    MachineName = eventLog.MachineName
                });
            }

            eventLog.Source = metaSource;
            eventLog.WriteEntry(
                               $"The source '{source}' has been moved from the log '{oldLogName}' to the log '{eventLog.Log}'.",
                                              EventLogEntryType.Information,
                                              SourceMovedEventId);
        }

        eventLog.Source = source;
    }

    private static EventLogEntryType LevelToEventLogEntryType(LogEventLevel level)
    {
        return level switch
        {
            Verbose => EventLogEntryType.Information,
            LogEventLevel.Debug => EventLogEntryType.Information,
            Information => EventLogEntryType.Information,
            Warning => EventLogEntryType.Warning,
            Error => EventLogEntryType.Error,
            Fatal => EventLogEntryType.Error,
            _ => EventLogEntryType.Information
        };
    }
}