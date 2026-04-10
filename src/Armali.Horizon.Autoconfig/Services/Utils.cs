using Armali.Horizon.Blazor;

namespace Armali.Horizon.Autoconfig.Services;

public static class Utils
{
    public static T? Index<T>(IEnumerable<T> collection, int index) where T : Identifiable
    {
        return collection.FirstOrDefault(i => i.Id == index);
    }
}