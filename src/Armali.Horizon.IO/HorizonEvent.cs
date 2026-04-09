namespace Armali.Horizon.IO;

public class HorizonEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = [];
}
