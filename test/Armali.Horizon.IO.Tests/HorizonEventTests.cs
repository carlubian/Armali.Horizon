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

        using var compressor = new Compressor();
        var compressed = compressor.Wrap(serialized);

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
    
    /// <summary>
    /// Verifica que los campos CorrelationId y ReplyTo se preservan en el roundtrip JSON.
    /// </summary>
    [TestMethod]
    public void HorizonEvent_WithCorrelation_SerializesCorrectly()
    {
        var correlationId = Guid.NewGuid();
        var original = new HorizonEvent
        {
            EventId = Guid.NewGuid(),
            EventType = "autoconfig.nodes.get",
            Payload = [0xAB],
            CorrelationId = correlationId,
            ReplyTo = "horizon:replies:abc123"
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<HorizonEvent>(json);

        restored.ShouldNotBeNull();
        restored.CorrelationId.ShouldBe(correlationId);
        restored.ReplyTo.ShouldBe("horizon:replies:abc123");
    }
    
    /// <summary>
    /// Verifica que CorrelationId y ReplyTo son null por defecto (fire-and-forget).
    /// </summary>
    [TestMethod]
    public void HorizonEvent_WithoutCorrelation_FieldsAreNull()
    {
        var original = new HorizonEvent
        {
            EventId = Guid.NewGuid(),
            EventType = "notify",
            Payload = [0x01]
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<HorizonEvent>(json);

        restored.ShouldNotBeNull();
        restored.CorrelationId.ShouldBeNull();
        restored.ReplyTo.ShouldBeNull();
    }
}

[TestClass]
public class HorizonEventFullRoundTripTests
{
    /// <summary>
    /// Simula el flujo completo: serializar payload concreto → comprimir → envolver en HorizonEvent
    /// → serializar sobre → deserializar sobre → descomprimir → deserializar payload concreto.
    /// Mismo ciclo que haría un par PublishAsync/Subscribe o RequestAsync/HandleReply.
    /// </summary>
    [TestMethod]
    public void FullRoundTrip_Request_PreservesAllFields()
    {
        // 1. Crear payload de petición
        var request = new TestPayload { EventType = "test.get", Message = "dame datos" };
        
        // 2. Comprimir (simula CreateEvent)
        using var compressor = new Compressor();
        var serialized = JsonSerializer.SerializeToUtf8Bytes(request);
        var compressed = compressor.Wrap(serialized);
        
        var correlationId = Guid.NewGuid();
        var envelope = new HorizonEvent
        {
            EventId = Guid.NewGuid(),
            EventType = request.EventType,
            Payload = compressed.ToArray(),
            CorrelationId = correlationId,
            ReplyTo = "horizon:replies:test123"
        };
        
        // 3. Serializar sobre para transmisión Redis
        var wire = JsonSerializer.Serialize(envelope);
        
        // 4. Deserializar sobre (simula recepción)
        var received = JsonSerializer.Deserialize<HorizonEvent>(wire);
        received.ShouldNotBeNull();
        received.CorrelationId.ShouldBe(correlationId);
        received.ReplyTo.ShouldBe("horizon:replies:test123");
        
        // 5. Descomprimir y deserializar payload concreto
        using var decompressor = new Decompressor();
        var decompressed = decompressor.Unwrap(received.Payload);
        var restoredRequest = JsonSerializer.Deserialize<TestPayload>(decompressed);
        
        restoredRequest.ShouldNotBeNull();
        restoredRequest.EventType.ShouldBe("test.get");
        restoredRequest.Message.ShouldBe("dame datos");
    }
    
    /// <summary>
    /// Verifica que el flujo completo funciona con un payload de respuesta más complejo
    /// (simula lo que devolvería un handler).
    /// </summary>
    [TestMethod]
    public void FullRoundTrip_Response_PreservesComplexPayload()
    {
        var response = new TestListPayload
        {
            EventType = "test.get:response",
            Items = ["nodo-1", "nodo-2", "nodo-3"]
        };
        
        using var compressor = new Compressor();
        var serialized = JsonSerializer.SerializeToUtf8Bytes(response);
        var compressed = compressor.Wrap(serialized);
        
        var correlationId = Guid.NewGuid();
        var envelope = new HorizonEvent
        {
            EventId = Guid.NewGuid(),
            EventType = response.EventType,
            Payload = compressed.ToArray(),
            CorrelationId = correlationId
        };
        
        var wire = JsonSerializer.Serialize(envelope);
        var received = JsonSerializer.Deserialize<HorizonEvent>(wire);
        received.ShouldNotBeNull();
        
        using var decompressor = new Decompressor();
        var decompressed = decompressor.Unwrap(received.Payload);
        var restoredResponse = JsonSerializer.Deserialize<TestListPayload>(decompressed);
        
        restoredResponse.ShouldNotBeNull();
        restoredResponse.Items.Count.ShouldBe(3);
        restoredResponse.Items.ShouldContain("nodo-2");
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

/// <summary>
/// Payload de prueba con una lista, simula respuestas tipo "GetNodes".
/// </summary>
internal class TestListPayload : IHorizonEventPayload
{
    public string EventType { get; set; } = string.Empty;
    public List<string> Items { get; set; } = [];
}
