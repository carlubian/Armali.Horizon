using System.Text.Json;
using Microsoft.JSInterop;

namespace Armali.Horizon.Blazor.Services;

public class HorizonSessionService
{
    private readonly IJSRuntime _js;

    public HorizonSessionService(IJSRuntime js)
    {
        _js = js;
    }
    
    /// <summary>
    /// Guarda cualquier objeto en LocalStorage convirtiéndolo a JSON.
    /// </summary>
    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    /// <summary>
    /// Recupera un objeto de LocalStorage. Si no existe, devuelve el valor por defecto de T.
    /// </summary>
    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string>("localStorage.getItem", key);

        if (string.IsNullOrEmpty(json))
            return default;

        try 
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch 
        {
            return default;
        }
    }

    /// <summary>
    /// Elimina una clave específica.
    /// </summary>
    public async Task RemoveItemAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}