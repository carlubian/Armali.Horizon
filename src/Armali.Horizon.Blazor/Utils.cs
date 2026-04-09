namespace Armali.Horizon.Blazor;

public interface Identifiable
{
    int Id { get; }
}

public interface Nameable
{
    string Name { get; }
}

public interface Colorable
{
    string Color { get; }
}

/// <summary>
/// Representa una entrada en un calendario anual.
/// Se usa junto con HorizonCalendarMonth y HorizonCalendarYear.
/// </summary>
public record HorizonCalendarEntry(int Month, int Day, string Label);
