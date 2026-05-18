using Armali.Horizon.Althes.Model;
using Armali.Horizon.Althes.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Armali.Horizon.Althes.Tests;

[TestClass]
public class ConversationStoreTests
{
    [TestMethod]
    public async Task GetActiveRun_devuelve_null_si_no_hay_run_abierto()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        (await store.GetActiveRunAsync("agentA")).ShouldBeNull();
    }
    
    [TestMethod]
    public async Task StartRun_crea_un_run_activo_dentro_de_una_conversacion()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        var conv = await store.GetOrCreateActiveConversationAsync();
        
        var run = await store.StartRunAsync("agentA", conv.Id, AgentRunTrigger.Manual);
        run.Status.ShouldBe(AgentRunStatus.Active);
        run.ConversationId.ShouldBe(conv.Id);
        
        var active = await store.GetActiveRunAsync("agentA");
        active!.Id.ShouldBe(run.Id);
    }
    
    [TestMethod]
    public async Task AppendAsync_incrementa_token_count_del_run()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        var conv = await store.GetOrCreateActiveConversationAsync();
        var run = await store.StartRunAsync("agentA", conv.Id, AgentRunTrigger.Manual);
        
        await store.AppendAsync(new AgentMessage
        {
            RunId = run.Id, AgentName = "agentA", Role = AgentMessageRole.User,
            Content = "hola", TokenCount = 5,
        });
        
        var refreshed = await store.GetActiveRunAsync("agentA");
        refreshed!.TokenCount.ShouldBe(5);
    }
    
    [TestMethod]
    public async Task CloseRunAsync_marca_status_y_summary()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        var conv = await store.GetOrCreateActiveConversationAsync();
        var run = await store.StartRunAsync("agentA", conv.Id, AgentRunTrigger.Manual);
        
        await store.CloseRunAsync(run.Id, AgentRunStatus.Completed, "resumen", default);
        var runs = await store.GetRecentRunsAsync("agentA", 10);
        runs.Single().Status.ShouldBe(AgentRunStatus.Completed);
        runs.Single().Summary.ShouldBe("resumen");
    }
    
    [TestMethod]
    public async Task ReplaceWithSummary_compacta_y_marca_run_como_compacted()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        var conv = await store.GetOrCreateActiveConversationAsync();
        var run = await store.StartRunAsync("agentA", conv.Id, AgentRunTrigger.Manual);
        
        var ids = new List<string>();
        for (var i = 0; i < 5; i++)
        {
            var m = new AgentMessage
            {
                RunId = run.Id, AgentName = "agentA", Role = AgentMessageRole.User,
                Content = $"msg{i}", TokenCount = 10,
            };
            await store.AppendAsync(m);
            ids.Add(m.Id);
        }
        
        await store.ReplaceWithSummaryAsync(run.Id, ids, "S", 7);
        
        var msgs = await store.GetMessagesAsync(run.Id);
        msgs.Count.ShouldBe(1);
        msgs[0].Skill.ShouldBe("summary");
        
        var refreshed = await store.GetActiveRunAsync("agentA");
        refreshed!.Status.ShouldBe(AgentRunStatus.Compacted);
        refreshed.TokenCount.ShouldBe(7);
    }
    
    [TestMethod]
    public async Task GetOrCreateActiveConversation_es_idempotente()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        
        var c1 = await store.GetOrCreateActiveConversationAsync();
        var c2 = await store.GetOrCreateActiveConversationAsync();
        c1.Id.ShouldBe(c2.Id);
    }
    
    [TestMethod]
    public async Task CloseConversation_permite_crear_una_nueva_en_la_siguiente_llamada()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        
        var c1 = await store.GetOrCreateActiveConversationAsync();
        await store.CloseConversationAsync(c1.Id);
        var c2 = await store.GetOrCreateActiveConversationAsync();
        
        c2.Id.ShouldNotBe(c1.Id);
        (await store.GetActiveConversationAsync())!.Id.ShouldBe(c2.Id);
    }
    
    [TestMethod]
    public async Task DeleteConversation_borra_runs_mensajes_y_queries()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        var conv = await store.GetOrCreateActiveConversationAsync();
        var run = await store.StartRunAsync("agentA", conv.Id, AgentRunTrigger.Manual);
        await store.AppendAsync(new AgentMessage
        {
            RunId = run.Id, AgentName = "agentA", Role = AgentMessageRole.User,
            Content = "x", TokenCount = 1,
        });
        
        var ok = await store.DeleteConversationAsync(conv.Id);
        ok.ShouldBeTrue();
        
        (await store.ListConversationsAsync(true, 10)).ShouldBeEmpty();
        (await store.GetMessagesAsync(run.Id)).ShouldBeEmpty();
        (await store.GetRecentRunsAsync("agentA", 10)).ShouldBeEmpty();
    }
    
    [TestMethod]
    public async Task GetUserFacingMessages_filtra_hidden()
    {
        using var factory = new TestDbContextFactory();
        var store = new ConversationStore(factory);
        var conv = await store.GetOrCreateActiveConversationAsync();
        var run = await store.StartRunAsync("agentA", conv.Id, AgentRunTrigger.Manual);
        
        await store.AppendAsync(new AgentMessage
        {
            RunId = run.Id, AgentName = "agentA", Role = AgentMessageRole.System,
            Content = "[thought] internal", TokenCount = 3,
            Visibility = UserVisibility.Hidden,
        });
        await store.AppendAsync(new AgentMessage
        {
            RunId = run.Id, AgentName = "agentA", Role = AgentMessageRole.Tool,
            Content = "[user notified] hola", RawContent = "hola", TokenCount = 5,
            Visibility = UserVisibility.Outgoing,
        });
        
        var visible = await store.GetUserFacingMessagesAsync(conv.Id);
        visible.Count.ShouldBe(1);
        visible[0].Visibility.ShouldBe(UserVisibility.Outgoing);
        visible[0].RawContent.ShouldBe("hola");
    }
}

