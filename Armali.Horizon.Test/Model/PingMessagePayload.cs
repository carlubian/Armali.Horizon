using Armali.Horizon.Messaging.Model;

namespace Armali.Horizon.Test.Model;

public class PingMessagePayload : MessagePayload
{
    public string Message { get; set; } = "Ping from Horizon Test";
}
