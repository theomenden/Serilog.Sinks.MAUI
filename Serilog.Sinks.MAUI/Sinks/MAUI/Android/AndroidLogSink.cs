using Android.Util;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using AndroidLog = Android.Util.Log;
namespace Serilog.Sinks.MAUI;
internal sealed class AndroidLogSink : ILogEventSink
{
    private readonly ITextFormatter _textFormatter;

    public AndroidLogSink(ITextFormatter textFormatter)
    {
        ArgumentNullException.ThrowIfNull(textFormatter);
        _textFormatter = textFormatter;
    }

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        using var message = new StringWriter();
        _textFormatter.Format(logEvent, message);

        var tag = logEvent.Properties
                .Where(x => x.Key == Constants.SourceContextPropertyName)
                .Select(x => x.Value.ToString("l", null))
                .FirstOrDefault("");

        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                AndroidLog.Debug(tag, message.ToString());
                break;
            case LogEventLevel.Information:
                AndroidLog.Info(tag, message.ToString());
                break;
            case LogEventLevel.Verbose:
                AndroidLog.Verbose(tag, message.ToString());
                break;
            case LogEventLevel.Warning:
                AndroidLog.Warn(tag, message.ToString());
                break;
            case LogEventLevel.Error:
                AndroidLog.Error(tag, message.ToString());
                break;
            case LogEventLevel.Fatal:
                AndroidLog.Wtf(tag, message.ToString());
                break;
            default:
                AndroidLog.WriteLine(LogPriority.Assert, tag, message.ToString());
                break;
        }
    }
}
