using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.MAUI;

internal sealed class NSLogSink : ILogEventSink
{
    private readonly ITextFormatter _textFormatter;

    public NSLogSink(ITextFormatter textFormatter)
    {
        ArgumentNullException.ThrowIfNull(textFormatter);
        _textFormatter = textFormatter;
    }

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        using var message = new StringWriter();
        _textFormatter.Format(logEvent, message);
        var formattedMessage = message.ToString();

        if (logEvent.Level is LogEventLevel.Error or LogEventLevel.Fatal)
        {
            Console.Error.WriteLine(formattedMessage);
            return;
        }

        Console.WriteLine(formattedMessage);
    }
}