namespace Armali.Horizon.Althes.Services.Skills;

/// <summary>
/// Registro de skills disponibles. Resuelve por nombre y permite filtrar por
/// las skills permitidas a un agente concreto.
/// </summary>
public class SkillRegistry
{
    private readonly Dictionary<string, IAgentSkill> ByName;
    
    public SkillRegistry(IEnumerable<IAgentSkill> skills)
    {
        ByName = skills.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    }
    
    public IReadOnlyCollection<IAgentSkill> All => ByName.Values;
    
    public IAgentSkill? Get(string name) =>
        ByName.TryGetValue(name, out var s) ? s : null;
    
    /// <summary>
    /// Devuelve las skills que un agente puede invocar. Si <paramref name="allowed"/>
    /// está vacío, se exponen todas; en otro caso, sólo las indicadas.
    /// </summary>
    public IEnumerable<IAgentSkill> ForAgent(IReadOnlyCollection<string> allowed)
    {
        if (allowed.Count == 0) return ByName.Values;
        return ByName.Where(kv => allowed.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
            .Select(kv => kv.Value);
    }
}

