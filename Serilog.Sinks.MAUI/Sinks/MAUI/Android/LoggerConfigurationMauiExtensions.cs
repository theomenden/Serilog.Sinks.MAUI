using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.MAUI;

namespace Serilog;
/// <summary>
/// Adds Android specific sinks to <see cref="LoggerConfiguration"/> - via <c>WriteTo.AndroidLog()</c>
/// </summary>
public static class LoggerConfigurationMauiExtensions
{
    private const string DefaultAndroidLogOutputTemplate = "[{Level}] {Message:l}{NewLine:l}{Exception:l}";

    /// <summary>
    /// Writes to the built-in Android log
    /// </summary>
    /// <param name="sinkConfiguration">The configuration that is being modified</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write a logging event to the sink</param>
    /// <param name="outputTemplate">Template for the log output</param>
    /// <param name="formatProvider">Provides a culture specific formatting information, or <see langword="null"/></param>
    /// <returns>The <see cref="LoggerConfiguration"/> for further chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sinkConfiguration" /> is <see langword="null" /></exception>
    /// <exception cref="ArgumentException"><paramref name="outputTemplate" /> is empty or consists only of white-space characters.</exception>
    public static LoggerConfiguration AndroidLog(this LoggerSinkConfiguration sinkConfiguration,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        string outputTemplate = DefaultAndroidLogOutputTemplate,
        IFormatProvider? formatProvider = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputTemplate);
        var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
        return sinkConfiguration.Sink(new AndroidLogSink(textFormatter), restrictedToMinimumLevel);
    }
}
