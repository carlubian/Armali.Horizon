using Armali.Horizon.Althes.Services;
using Armali.Horizon.Contracts.Althes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Armali.Horizon.Althes.Tests;

[TestClass]
public class AgentInboxRouterTests
{
    [TestMethod]
    public void Deliver_devuelve_false_si_el_agente_no_existe()
    {
        var router = new AgentInboxRouter();
        router.Deliver("ghost", new AgentInboxMessage()).ShouldBeFalse();
    }
    
    [TestMethod]
    public async Task GetOrCreate_devuelve_la_misma_instancia_y_entrega_mensajes()
    {
        var router = new AgentInboxRouter();
        var inbox1 = router.GetOrCreate("a");
        var inbox2 = router.GetOrCreate("A");  // case-insensitive
        inbox1.ShouldBeSameAs(inbox2);
        
        router.Deliver("a", new AgentInboxMessage { Content = "hola" }).ShouldBeTrue();
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var read = await inbox1.Reader.ReadAsync(cts.Token);
        read.Content.ShouldBe("hola");
    }
}

