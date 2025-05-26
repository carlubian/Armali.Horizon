using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.Messaging;

public delegate void MessageReceivedDelegate(object message);

public interface IHorizonMessaging : IHostedService
{
    Task SendMessage(object message);
    event MessageReceivedDelegate OnMessageReceived;
}
