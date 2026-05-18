using Armali.Horizon.Althes.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Althes.Services;

/// <summary>
/// Acceso a la persistencia de conversaciones, runs y mensajes. Encapsula
/// las queries más usadas por el runtime y los handlers IO.
/// </summary>
public class ConversationStore
{
    private readonly IDbContextFactory<AlthesDbContext> Factory;
    
    public ConversationStore(IDbContextFactory<AlthesDbContext> factory) => Factory = factory;
    
    // ── Conversations ───────────────────────────────────────────────
    
    /// <summary>Devuelve la conversación activa, o null si no hay ninguna abierta.</summary>
    public async Task<Conversation?> GetActiveConversationAsync(CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        return await db.Conversations.AsNoTracking()
            .Where(c => c.Status == ConversationStatus.Active)
            .OrderByDescending(c => c.StartedAt)
            .FirstOrDefaultAsync(ct);
    }
    
    /// <summary>
    /// Devuelve la conversación activa; si no hay ninguna, crea una nueva.
    /// </summary>
    public async Task<Conversation> GetOrCreateActiveConversationAsync(string? name = null, CancellationToken ct = default)
    {
        var existing = await GetActiveConversationAsync(ct);
        if (existing is not null) return existing;
        
        await using var db = await Factory.CreateDbContextAsync(ct);
        var conv = new Conversation { Name = name };
        db.Conversations.Add(conv);
        await db.SaveChangesAsync(ct);
        return conv;
    }
    
    /// <summary>Cierra una conversación. No-op si ya está cerrada.</summary>
    public async Task CloseConversationAsync(string conversationId, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId, ct);
        if (conv is null || conv.Status == ConversationStatus.Closed) return;
        conv.Status = ConversationStatus.Closed;
        conv.EndedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
    
    /// <summary>Hard-delete: borra la conversación, sus runs, mensajes y queries.</summary>
    public async Task<bool> DeleteConversationAsync(string conversationId, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId, ct);
        if (conv is null) return false;
        
        // Mensajes pertenecen a Runs de la conversación.
        var runIds = await db.Runs.Where(r => r.ConversationId == conversationId)
            .Select(r => r.Id).ToListAsync(ct);
        
        if (runIds.Count > 0)
        {
            var messages = db.Messages.Where(m => runIds.Contains(m.RunId));
            db.Messages.RemoveRange(messages);
        }
        var queries = db.UserQueries.Where(q => q.ConversationId == conversationId);
        db.UserQueries.RemoveRange(queries);
        var runs = db.Runs.Where(r => r.ConversationId == conversationId);
        db.Runs.RemoveRange(runs);
        db.Conversations.Remove(conv);
        await db.SaveChangesAsync(ct);
        return true;
    }
    
    public async Task<List<Conversation>> ListConversationsAsync(bool includeClosed, int max, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var q = db.Conversations.AsNoTracking();
        if (!includeClosed) q = q.Where(c => c.Status == ConversationStatus.Active);
        return await q.OrderByDescending(c => c.StartedAt).Take(Math.Clamp(max, 1, 500)).ToListAsync(ct);
    }
    
    /// <summary>Mensajes user-facing de una conversación, agregados de todos sus runs.</summary>
    public async Task<List<AgentMessage>> GetUserFacingMessagesAsync(string conversationId, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var runIds = await db.Runs.Where(r => r.ConversationId == conversationId)
            .Select(r => r.Id).ToListAsync(ct);
        if (runIds.Count == 0) return [];
        return await db.Messages.AsNoTracking()
            .Where(m => runIds.Contains(m.RunId) && m.Visibility != UserVisibility.Hidden)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }
    
    public async Task<(int messageCount, int pendingQuestions)> GetConversationStatsAsync(string conversationId, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var runIds = await db.Runs.Where(r => r.ConversationId == conversationId)
            .Select(r => r.Id).ToListAsync(ct);
        var msgCount = runIds.Count == 0 ? 0 : await db.Messages
            .Where(m => runIds.Contains(m.RunId) && m.Visibility != UserVisibility.Hidden)
            .CountAsync(ct);
        var pending = await db.UserQueries
            .Where(q => q.ConversationId == conversationId && q.Status == UserQueryStatus.Pending)
            .CountAsync(ct);
        return (msgCount, pending);
    }
    
    // ── Runs ────────────────────────────────────────────────────────
    
    /// <summary>Run activo del agente, o null si no hay ninguno abierto.</summary>
    public async Task<AgentRun?> GetActiveRunAsync(string agentName, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        return await db.Runs.AsNoTracking()
            .Where(r => r.AgentName == agentName && (r.Status == AgentRunStatus.Active || r.Status == AgentRunStatus.Compacted))
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync(ct);
    }
    
    /// <summary>Devuelve todos los runs activos (de todos los agentes).</summary>
    public async Task<List<AgentRun>> GetAllActiveRunsAsync(CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        return await db.Runs.AsNoTracking()
            .Where(r => r.Status == AgentRunStatus.Active || r.Status == AgentRunStatus.Compacted)
            .ToListAsync(ct);
    }
    
    /// <summary>Crea un nuevo run activo dentro de la conversación indicada.</summary>
    public async Task<AgentRun> StartRunAsync(string agentName, string conversationId, AgentRunTrigger trigger, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var run = new AgentRun { AgentName = agentName, ConversationId = conversationId, Trigger = trigger };
        db.Runs.Add(run);
        await db.SaveChangesAsync(ct);
        return run;
    }
    
    /// <summary>Cierra un run con el resumen indicado.</summary>
    public async Task CloseRunAsync(string runId, AgentRunStatus status, string? summary, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var run = await db.Runs.FirstOrDefaultAsync(r => r.Id == runId, ct);
        if (run is null) return;
        run.Status = status;
        run.Summary = summary;
        run.EndedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
    
    /// <summary>Añade un mensaje al run y actualiza el conteo de tokens.</summary>
    public async Task AppendAsync(AgentMessage msg, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        db.Messages.Add(msg);
        var run = await db.Runs.FirstOrDefaultAsync(r => r.Id == msg.RunId, ct);
        if (run is not null) run.TokenCount += msg.TokenCount;
        await db.SaveChangesAsync(ct);
    }
    
    /// <summary>Mensajes del run en orden cronológico.</summary>
    public async Task<List<AgentMessage>> GetMessagesAsync(string runId, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        return await db.Messages.AsNoTracking()
            .Where(m => m.RunId == runId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }
    
    /// <summary>Sustituye un bloque de mensajes por un único resumen (compresión).</summary>
    public async Task ReplaceWithSummaryAsync(string runId, IReadOnlyList<string> messageIdsToRemove, string summary, int summaryTokens, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var toRemove = await db.Messages.Where(m => messageIdsToRemove.Contains(m.Id)).ToListAsync(ct);
        var removedTokens = toRemove.Sum(m => m.TokenCount);
        db.Messages.RemoveRange(toRemove);
        
        db.Messages.Add(new AgentMessage
        {
            RunId = runId,
            AgentName = toRemove.FirstOrDefault()?.AgentName ?? "",
            Role = AgentMessageRole.System,
            Skill = "summary",
            Content = summary,
            TokenCount = summaryTokens,
            CreatedAt = DateTime.UtcNow,
            Visibility = UserVisibility.Hidden,
        });
        
        var run = await db.Runs.FirstOrDefaultAsync(r => r.Id == runId, ct);
        if (run is not null)
        {
            run.TokenCount = run.TokenCount - removedTokens + summaryTokens;
            run.Status = AgentRunStatus.Compacted;
        }
        await db.SaveChangesAsync(ct);
    }
    
    /// <summary>Devuelve los runs recientes de un agente.</summary>
    public async Task<List<AgentRun>> GetRecentRunsAsync(string agentName, int max, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        return await db.Runs.AsNoTracking()
            .Where(r => r.AgentName == agentName)
            .OrderByDescending(r => r.StartedAt)
            .Take(max)
            .ToListAsync(ct);
    }
}

