using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Althes.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Armali.Horizon.Althes.Tests;

[TestClass]
public class RateLimiterTests
{
    [TestMethod]
    public void Limita_segun_max_acciones_por_hora()
    {
        var options = Options.Create(new AlthesOptions { Limits = new AlthesLimitsOptions { MaxActionsPerHour = 3 } });
        var rl = new RateLimiter(options);
        
        rl.TryConsume("a", null).ShouldBeTrue();
        rl.TryConsume("a", null).ShouldBeTrue();
        rl.TryConsume("a", null).ShouldBeTrue();
        rl.TryConsume("a", null).ShouldBeFalse();
        
        // Otro agente tiene su propia cuenta.
        rl.TryConsume("b", null).ShouldBeTrue();
    }
    
    [TestMethod]
    public void Override_por_agente_tiene_prioridad()
    {
        var options = Options.Create(new AlthesOptions { Limits = new AlthesLimitsOptions { MaxActionsPerHour = 100 } });
        var rl = new RateLimiter(options);
        
        rl.TryConsume("x", 1).ShouldBeTrue();
        rl.TryConsume("x", 1).ShouldBeFalse();
    }
    
    [TestMethod]
    public void Limite_cero_o_negativo_significa_sin_limite()
    {
        var options = Options.Create(new AlthesOptions { Limits = new AlthesLimitsOptions { MaxActionsPerHour = 0 } });
        var rl = new RateLimiter(options);
        for (var i = 0; i < 1000; i++)
            rl.TryConsume("z", null).ShouldBeTrue();
    }
}

