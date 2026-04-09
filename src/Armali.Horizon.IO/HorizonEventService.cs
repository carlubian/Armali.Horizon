using System.Text.Json;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using ZstdSharp;

namespace Armali.Horizon.IO;

public class HorizonEventService: IHostedService
{
    public static HorizonEventSettings Settings { get; set; } = new();
    private ConnectionMultiplexer? _redis;
    private IDatabase? _database;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _redis = await ConnectionMultiplexer.ConnectAsync((Settings.Endpoint));
        _database = _redis.GetDatabase();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_redis != null) 
            await _redis.DisposeAsync();
            // This also removes all subscriptions
    }

    public async Task PublishAsync<T>(string channel, T payload) where T: IHorizonEventPayload
    {
        // Payload is compressed using ZSTD for efficiency
        using var compressor = new Compressor();
        var serializedPayload = JsonSerializer.SerializeToUtf8Bytes(payload);
        var compressedPayload = compressor.Wrap(serializedPayload);
        
        // It's then embedded into a HorizonEvent object
        var @event = new HorizonEvent
        {
            Payload = compressedPayload.ToArray(),
            EventId = Guid.NewGuid(),
            EventType = payload.EventType
        };
        
        // Which itself is serialized to JSON for transmission
        var readyEvent = JsonSerializer.Serialize(@event);

        await _database!.PublishAsync(RedisChannel.Literal(channel), readyEvent);
    }

    public void Subscribe(string channel, Action<IHorizonEventPayload> onReceive, Predicate<string>? eventTypeCondition)
    {
        var actualPredicate = eventTypeCondition ?? (_ => true);
        var subscriber = _redis!.GetSubscriber();
        
        subscriber.Subscribe(RedisChannel.Literal(channel), (_, message) =>
        {
            // Deserialize the incoming message to a HorizonEvent
            var jsonEvent = JsonSerializer.Deserialize<HorizonEvent>((string?)message ?? string.Empty);
            if (jsonEvent == null || !actualPredicate(jsonEvent.EventType))
                return;
            
            // Extract and decompress the payload using ZSTD
            using var decompressor = new Decompressor();
            var decompressedPayload = decompressor.Unwrap(jsonEvent.Payload);
            var payload = JsonSerializer.Deserialize<IHorizonEventPayload>(decompressedPayload);
            if (payload != null)
                onReceive(payload);
        });
    }
}
