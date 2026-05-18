using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Althes.Model;
using Armali.Horizon.Althes.Services;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Handlers;

// Handlers IO de la instancia Althes. Todos validan rol "althes.user".

public class ListAgentsHandler : IHorizonRequestHandler<ListAgentsRequest, ListAgentsResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly AgentRegistry Registry;
    private readonly AgentRuntimeHost Host;
    public ListAgentsHandler(HorizonAuthClient auth, AgentRegistry registry, AgentRuntimeHost host)
    { Auth = auth; Registry = registry; Host = host; }
    public async Task<ListAgentsResponse> HandleAsync(ListAgentsRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new ListAgentsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var list = Registry.All.Select(a =>
        {
            var rt = Host.GetRuntime(a.Name);
            return new AlthesAgentDto
            {
                Name = a.Name, SystemPrompt = a.SystemPrompt, Model = a.Model,
                CarryOverSummary = a.CarryOverSummary,
                AllowedSkills = (a.AllowedSkills ?? []).ToArray(),
                AllowedRecipients = (a.AllowedRecipients ?? []).ToArray(),
                MaxActionsPerHour = a.MaxActionsPerHour,
                ActiveRunId = rt?.ActiveRunId,
                PendingInboxItems = rt?.PendingInboxItems ?? 0,
                IsAwaiting = rt?.IsAwaiting ?? false,
            };
        }).ToList();
        return new ListAgentsResponse { Success = true, Agents = list };
    }
}

public class SendMessageHandler : IHorizonRequestHandler<SendMessageRequest, SendMessageResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly AgentRegistry Registry;
    private readonly AgentRuntimeHost Host;
    private readonly AgentInboxRouter Router;
    private readonly ConversationStore Store;
    public SendMessageHandler(HorizonAuthClient auth, AgentRegistry reg, AgentRuntimeHost host, AgentInboxRouter router, ConversationStore store)
    { Auth = auth; Registry = reg; Host = host; Router = router; Store = store; }
    public async Task<SendMessageResponse> HandleAsync(SendMessageRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new SendMessageResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        if (Registry.Get(req.AgentName) is null)
            return new SendMessageResponse { Success = false, Error = new() { Code = AlthesErrorCodes.AgentNotFound, Message = req.AgentName } };
        if (req.StartNewConversation) await Host.StartNewConversationAsync(ct);
        Router.Deliver(req.AgentName, new AgentInboxMessage { Kind = AgentInboxKind.Notify, Sender = "user", Content = req.Content });
        var conv = await Store.GetActiveConversationAsync(ct);
        var run = await Store.GetActiveRunAsync(req.AgentName, ct);
        return new SendMessageResponse { Success = true, RunId = run?.Id, ConversationId = conv?.Id };
    }
}

public class StartNewRunHandler : IHorizonRequestHandler<StartNewRunRequest, StartNewRunResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly AgentRuntimeHost Host;
    private readonly ConversationStore Store;
    public StartNewRunHandler(HorizonAuthClient auth, AgentRuntimeHost host, ConversationStore store)
    { Auth = auth; Host = host; Store = store; }
    public async Task<StartNewRunResponse> HandleAsync(StartNewRunRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new StartNewRunResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var rt = Host.GetRuntime(req.AgentName);
        if (rt is null)
            return new StartNewRunResponse { Success = false, Error = new() { Code = AlthesErrorCodes.AgentNotFound, Message = req.AgentName } };
        var prev = await Store.GetActiveRunAsync(req.AgentName, ct);
        await rt.CloseActiveRunAsync(ct);
        return new StartNewRunResponse { Success = true, ClosedRunId = prev?.Id };
    }
}

public class GetRunsHandler : IHorizonRequestHandler<GetRunsRequest, GetRunsResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly ConversationStore Store;
    public GetRunsHandler(HorizonAuthClient auth, ConversationStore store) { Auth = auth; Store = store; }
    public async Task<GetRunsResponse> HandleAsync(GetRunsRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new GetRunsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var runs = await Store.GetRecentRunsAsync(req.AgentName, Math.Clamp(req.MaxRuns, 1, 200), ct);
        var dtos = new List<AgentRunDto>();
        foreach (var r in runs)
        {
            var dto = MapRun(r);
            if (req.IncludeMessages)
            {
                var msgs = await Store.GetMessagesAsync(r.Id, ct);
                dto.Messages = msgs.Select(MapMessage).ToList();
            }
            dtos.Add(dto);
        }
        return new GetRunsResponse { Success = true, Runs = dtos };
    }
    internal static AgentRunDto MapRun(AgentRun r) => new()
    {
        Id = r.Id, AgentName = r.AgentName, ConversationId = r.ConversationId,
        StartedAt = r.StartedAt, EndedAt = r.EndedAt,
        Status = r.Status.ToString(), Trigger = r.Trigger.ToString(),
        Summary = r.Summary, TokenCount = r.TokenCount,
    };
    internal static AgentMessageDto MapMessage(AgentMessage m) => new()
    {
        Id = m.Id, Role = m.Role.ToString(), Skill = m.Skill,
        Sender = m.Sender, Recipient = m.Recipient, Content = m.Content,
        RawContent = m.RawContent, Visibility = m.Visibility.ToString(),
        TokenCount = m.TokenCount, CreatedAt = m.CreatedAt,
        CorrelationId = m.CorrelationId, UserQueryId = m.UserQueryId,
    };
}

public class GetRunHandler : IHorizonRequestHandler<GetRunRequest, GetRunResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly IDbContextFactory<AlthesDbContext> Factory;
    private readonly ConversationStore Store;
    public GetRunHandler(HorizonAuthClient auth, IDbContextFactory<AlthesDbContext> factory, ConversationStore store)
    { Auth = auth; Factory = factory; Store = store; }
    public async Task<GetRunResponse> HandleAsync(GetRunRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new GetRunResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        await using var db = await Factory.CreateDbContextAsync(ct);
        var run = await db.Runs.AsNoTracking().FirstOrDefaultAsync(r => r.Id == req.RunId, ct);
        if (run is null)
            return new GetRunResponse { Success = false, Error = new() { Code = AlthesErrorCodes.RunNotFound, Message = req.RunId } };
        var dto = GetRunsHandler.MapRun(run);
        var msgs = await Store.GetMessagesAsync(req.RunId, ct);
        dto.Messages = msgs.Select(GetRunsHandler.MapMessage).ToList();
        return new GetRunResponse { Success = true, Run = dto };
    }
}

public class ListUserQueriesHandler : IHorizonRequestHandler<ListUserQueriesRequest, ListUserQueriesResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly IDbContextFactory<AlthesDbContext> Factory;
    public ListUserQueriesHandler(HorizonAuthClient auth, IDbContextFactory<AlthesDbContext> factory) { Auth = auth; Factory = factory; }
    public async Task<ListUserQueriesResponse> HandleAsync(ListUserQueriesRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new ListUserQueriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        await using var db = await Factory.CreateDbContextAsync(ct);
        var q = db.UserQueries.AsNoTracking().AsQueryable();
        if (!req.IncludeAnswered) q = q.Where(x => x.Status == UserQueryStatus.Pending);
        var rows = await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return new ListUserQueriesResponse
        {
            Success = true,
            Queries = rows.Select(x => new UserQueryDto
            {
                Id = x.Id, AgentName = x.AgentName, RunId = x.RunId,
                Question = x.Question, CreatedAt = x.CreatedAt,
                Status = x.Status.ToString().ToLowerInvariant(),
                Answer = x.Answer, AnsweredAt = x.AnsweredAt,
            }).ToList(),
        };
    }
}

public class AnswerUserQueryHandler : IHorizonRequestHandler<AnswerUserQueryRequest, AnswerUserQueryResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly IDbContextFactory<AlthesDbContext> Factory;
    private readonly AgentInboxRouter Router;
    private readonly AlthesOptions Options;
    private readonly HorizonEventService Events;
    public AnswerUserQueryHandler(HorizonAuthClient auth, IDbContextFactory<AlthesDbContext> factory,
        AgentInboxRouter router, IOptions<AlthesOptions> options, HorizonEventService events)
    { Auth = auth; Factory = factory; Router = router; Options = options.Value; Events = events; }
    public async Task<AnswerUserQueryResponse> HandleAsync(AnswerUserQueryRequest req, CancellationToken ct = default)
    {
        var id = await Auth.AuthAlthesAsync(req);
        if (id is null)
            return new AnswerUserQueryResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        await using var db = await Factory.CreateDbContextAsync(ct);
        var query = await db.UserQueries.FirstOrDefaultAsync(q => q.Id == req.QueryId, ct);
        if (query is null)
            return new AnswerUserQueryResponse { Success = false, Error = new() { Code = AlthesErrorCodes.QueryNotFound, Message = req.QueryId } };
        if (query.Status != UserQueryStatus.Pending)
            return new AnswerUserQueryResponse { Success = false, Error = new() { Code = AlthesErrorCodes.AlreadyAnswered, Message = $"Status={query.Status}" } };
        query.Status = UserQueryStatus.Answered;
        query.Answer = req.Answer;
        query.AnsweredAt = DateTime.UtcNow;
        query.AnsweredBy = id.UserId;
        await db.SaveChangesAsync(ct);
        Router.Deliver(query.AgentName, new AgentInboxMessage
        {
            Kind = AgentInboxKind.UserAnswer, Sender = "user", Content = req.Answer,
            CorrelationId = query.CorrelationId,
        });
        await Events.PublishAsync(AlthesChannels.UserInbox(Options.ProjectId), new AgentInboxMessage
        {
            Kind = AgentInboxKind.Answer, Sender = "user", Content = req.Answer,
            CorrelationId = query.CorrelationId, ConversationId = query.ConversationId,
            RunId = query.RunId, UserQueryId = query.Id,
        });
        return new AnswerUserQueryResponse { Success = true };
    }
}

public class ListConversationsHandler : IHorizonRequestHandler<ListConversationsRequest, ListConversationsResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly ConversationStore Store;
    public ListConversationsHandler(HorizonAuthClient auth, ConversationStore store) { Auth = auth; Store = store; }
    public async Task<ListConversationsResponse> HandleAsync(ListConversationsRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new ListConversationsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var convs = await Store.ListConversationsAsync(req.IncludeClosed, req.MaxConversations, ct);
        var dtos = new List<ConversationDto>();
        foreach (var c in convs)
        {
            var (count, pending) = await Store.GetConversationStatsAsync(c.Id, ct);
            dtos.Add(MapConversation(c, count, pending));
        }
        return new ListConversationsResponse { Success = true, Conversations = dtos };
    }
    internal static ConversationDto MapConversation(Conversation c, int msgCount, int pending) => new()
    {
        Id = c.Id, Name = c.Name, StartedAt = c.StartedAt, EndedAt = c.EndedAt,
        Status = c.Status.ToString(), MessageCount = msgCount, PendingQuestions = pending,
    };
}

public class GetConversationHandler : IHorizonRequestHandler<GetConversationRequest, GetConversationResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly IDbContextFactory<AlthesDbContext> Factory;
    private readonly ConversationStore Store;
    public GetConversationHandler(HorizonAuthClient auth, IDbContextFactory<AlthesDbContext> factory, ConversationStore store)
    { Auth = auth; Factory = factory; Store = store; }
    public async Task<GetConversationResponse> HandleAsync(GetConversationRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new GetConversationResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        await using var db = await Factory.CreateDbContextAsync(ct);
        var conv = await db.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);
        if (conv is null)
            return new GetConversationResponse { Success = false, Error = new() { Code = AlthesErrorCodes.ConversationNotFound, Message = req.ConversationId } };
        var msgs = await Store.GetUserFacingMessagesAsync(conv.Id, ct);
        var queryIds = msgs.Where(m => m.UserQueryId is not null).Select(m => m.UserQueryId!).Distinct().ToList();
        var answeredById = queryIds.Count == 0
            ? new Dictionary<string, bool>()
            : await db.UserQueries.AsNoTracking()
                .Where(q => queryIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id, q => q.Status != UserQueryStatus.Pending, ct);
        var (count, pending) = await Store.GetConversationStatsAsync(conv.Id, ct);
        return new GetConversationResponse
        {
            Success = true,
            Conversation = ListConversationsHandler.MapConversation(conv, count, pending),
            Messages = msgs.Select(m => new ChatMessageDto
            {
                Id = m.Id, RunId = m.RunId,
                Sender = m.Visibility == UserVisibility.Incoming || m.Visibility == UserVisibility.Answer ? "user" : m.AgentName,
                Content = m.RawContent ?? m.Content,
                Visibility = m.Visibility.ToString(),
                CreatedAt = m.CreatedAt,
                UserQueryId = m.UserQueryId,
                IsAnswered = m.UserQueryId is not null && answeredById.TryGetValue(m.UserQueryId, out var ans) ? ans : null,
            }).ToList(),
        };
    }
}

public class StartNewConversationHandler : IHorizonRequestHandler<StartNewConversationRequest, StartNewConversationResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly AgentRuntimeHost Host;
    public StartNewConversationHandler(HorizonAuthClient auth, AgentRuntimeHost host) { Auth = auth; Host = host; }
    public async Task<StartNewConversationResponse> HandleAsync(StartNewConversationRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new StartNewConversationResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var closedId = await Host.StartNewConversationAsync(ct);
        return new StartNewConversationResponse { Success = true, ClosedConversationId = closedId };
    }
}

public class DeleteConversationHandler : IHorizonRequestHandler<DeleteConversationRequest, DeleteConversationResponse>
{
    private readonly HorizonAuthClient Auth;
    private readonly AgentRuntimeHost Host;
    private readonly ConversationStore Store;
    public DeleteConversationHandler(HorizonAuthClient auth, AgentRuntimeHost host, ConversationStore store) { Auth = auth; Host = host; Store = store; }
    public async Task<DeleteConversationResponse> HandleAsync(DeleteConversationRequest req, CancellationToken ct = default)
    {
        if (await Auth.AuthAlthesAsync(req) is null)
            return new DeleteConversationResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var active = await Store.GetActiveConversationAsync(ct);
        if (active is not null && active.Id == req.ConversationId)
            await Host.StartNewConversationAsync(ct);
        var ok = await Store.DeleteConversationAsync(req.ConversationId, ct);
        if (!ok)
            return new DeleteConversationResponse { Success = false, Error = new() { Code = AlthesErrorCodes.ConversationNotFound, Message = req.ConversationId } };
        return new DeleteConversationResponse { Success = true };
    }
}

