namespace Armali.Horizon.IO;

/// <summary>
/// Handler que procesa una petición de tipo <typeparamref name="TRequest"/>
/// y devuelve una respuesta de tipo <typeparamref name="TResponse"/>.
/// <para>
/// Implementar esta interfaz para cada operación que un servicio quiera
/// exponer de forma remota a través de eventos Horizon.
/// El handler se resuelve desde DI con un scope nuevo por cada petición.
/// </para>
/// </summary>
public interface IHorizonRequestHandler<in TRequest, TResponse>
    where TRequest : IHorizonEventPayload
    where TResponse : IHorizonEventPayload
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}

