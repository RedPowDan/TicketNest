using TicketNest.Shared.Guard;

namespace TicketNest.Shared;

public class SequentialGuidFactory
{
    public static Guid Create(DateTime dateTime)
    {
        Ensure.NotDefault(dateTime, nameof(dateTime));

        var tickBytes = BitConverter.GetBytes(dateTime.Ticks);
        var guidBytes = Guid.NewGuid().ToByteArray();

        Array.Copy(tickBytes, 0, guidBytes, 0, 8);

        return new Guid(guidBytes);
    }
}