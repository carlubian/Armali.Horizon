using System.Text.Json;
using ZstdSharp;

namespace Armali.Horizon.IO.Tests;

[TestClass]
public class HorizonEventCompressionTests
{
    /// <summary>
    /// Verifica que el ciclo completo de compresión y descompresión Zstd
    /// preserva el payload original (mismo flujo que usa HorizonEventService).
    /// </summary>
    [TestMethod]
    public void CompressAndDecompress_PreservesPayload()
    {
        var original = new TestPayload { EventType = "test.created", Message = "Hola Mundo" };
        var serialized = JsonSerializer.SerializeToUtf8Bytes(original);

        // Compresión (mismo flujo que PublishAsync)
        using var compressor = new Compressor();
        var compressed = compressor.Wrap(serialized);

        // Descompresión (mismo flujo que Subscribe)
        using var decompressor = new Decompressor();
        var decompressed = decompressor.Unwrap(compressed);
        var restored = JsonSerializer.Deserialize<TestPayload>(decompressed);

        restored.ShouldNotBeNull();
        restored.EventType.ShouldBe("test.created");
        restored.Message.ShouldBe("Hola Mundo");
    }

    [TestMethod]
    public void CompressEmptyPayload_WorksCorrectly()
    {
        var original = new TestPayload { EventType = "test.empty", Message = "" };
        var serialized = JsonSerializer.SerializeToUtf8Bytes(original);

        using var compressor = new Compressor();
        var compressed = compressor.Wrap(serialized);

        using var decompressor = new Decompressor();
        var decompressed = decompressor.Unwrap(compressed);
        var restored = JsonSerializer.Deserialize<TestPayload>(decompressed);

        restored.ShouldNotBeNull();
        restored.Message.ShouldBeEmpty();
    }
}

[TestClass]
public class HorizonEventSerializationTests
{
    /// <summary>
    /// Verifica que HorizonEvent se serializa y deserializa correctamente con JSON,
    /// incluyendo el array de bytes del payload comprimido.
    /// </summary>
    [TestMethod]
    public void HorizonEvent_SerializationRoundTrip()
    {
        var original = new HorizonEvent
        {
            EventId = Guid.NewGuid(),
            EventType = "order.placed",
            Payload = [0x01, 0x02, 0xFF]
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<HorizonEvent>(json);

        restored.ShouldNotBeNull();
        restored.EventId.ShouldBe(original.EventId);
        restored.EventType.ShouldBe("order.placed");
        restored.Payload.ShouldBe(original.Payload);
    }

    [TestMethod]
    public void HorizonEvent_EmptyPayload_SerializesCorrectly()
    {
        var original = new HorizonEvent
        {
            EventId = Guid.NewGuid(),
            EventType = "ping",
            Payload = []
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<HorizonEvent>(json);

        restored.ShouldNotBeNull();
        restored.Payload.ShouldBeEmpty();
    }
}

/// <summary>
/// Payload de prueba que implementa IHorizonEventPayload.
/// </summary>
internal class TestPayload : IHorizonEventPayload
{
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

