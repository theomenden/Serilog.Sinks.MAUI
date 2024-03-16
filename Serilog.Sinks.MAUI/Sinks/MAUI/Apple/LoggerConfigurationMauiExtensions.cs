using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.MAUI;

namespace Serilog;
/// <summary>
/// Adds Apple specific sinks to <see cref="LoggerConfiguration"/> - via <c>WriteTo.AppleNSLog()</c>
/// </summary>
public static class LoggerConfigurationMauiExtensions
{
    private const string DefaultOutputTemplate = "[{Level}] {Message:l}{NewLine:l}{Exception:l}";

    /// <summary>
    /// Adds a sink that writes log events to the Apple NSLog system.
    /// </summary>
    /// <param name="sinkConfiguration">The configuration that is being modified</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write a logging event to the sink</param>
    /// <param name="outputTemplate">Template for the log output</param>
    /// <param name="formatProvider">Provides a culture specific formatting information, or <see langword="null"/></param>
    /// <returns>The <see cref="LoggerConfiguration"/> for further chaining</returns>
    /// <exception cref="ArgumentNullException">The required <paramref name="sinkConfiguration"/> is null</exception>
    /// <exception cref="ArgumentException">The required <paramref name="outputTemplate"/> is null</exception>
    /// <remarks>Default logging template is supplied, and is formatted as <c>[{Level}] {Message:l}{NewLine:l}{Exception:l}</c></remarks>
    public static LoggerConfiguration AppleNSLog(this LoggerSinkConfiguration sinkConfiguration,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        string outputTemplate = DefaultOutputTemplate,
        IFormatProvider? formatProvider = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputTemplate);

        var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

        return sinkConfiguration.Sink(new NSLogSink(textFormatter), restrictedToMinimumLevel);
    }

}
