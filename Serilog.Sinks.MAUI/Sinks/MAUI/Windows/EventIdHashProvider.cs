using Serilog.Events;

namespace Serilog.Sinks.MAUI.Windows;

public interface IEventIdHashProvider
{
    ushort ComputeEventIdHash(LogEvent logEvent);
}

internal sealed class EventIdHashProvider : IEventIdHashProvider
{
    public ushort ComputeEventIdHash(LogEvent logEvent)
    => (ushort)JenkinsOneAtATimeHash(logEvent.MessageTemplate.Text);

    private static int JenkinsOneAtATimeHash(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        unchecked
        {
            ushort hash = 0;
            foreach (var c in value.AsSpan())
            {
                hash += c;
                hash += (ushort)(hash << 10);
                hash ^= (ushort)(hash >> 6);
            }
            hash += (ushort)(hash << 3);
            hash ^= (ushort)(hash >> 11);
            hash += (ushort)(hash << 15);
            return hash;
        }
    }
}