using Armali.Horizon.Blazor;
using Armali.Horizon.Segaris.Model;

namespace Armali.Horizon.Segaris.Services;

public static class Utils
{
    public static T? Index<T>(IEnumerable<T> collection, int index) where T : Identifiable
    {
        return collection.FirstOrDefault(i => i.Id == index);
    }

    /// <summary>
    /// Devuelve el nombre del icono FA según el estado de privacidad de la entidad.
    /// </summary>
    public static string PrivacyIcon(bool? isPrivate, string creator, string currentUser) => isPrivate switch
    {
        true => "fa-lock",
        false when creator == currentUser => "fa-globe",
        false => "fa-share-nodes",
        _ => string.Empty
    };

    /// <summary>
    /// Devuelve la clase de color para el icono de privacidad de la entidad.
    /// </summary>
    public static string PrivacyColor(bool? isPrivate, string creator, string currentUser) => isPrivate switch
    {
        true => "text-hz-tribunal",
        false when creator == currentUser => "text-hz-opera",
        false => "text-hz-sol",
        _ => string.Empty
    };
}