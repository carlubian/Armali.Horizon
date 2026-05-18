using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Althes;

/// <summary>
/// Cliente de alto nivel para una instancia de Horizon.Althes sobre el bus IO.
/// <para>
/// Toda llamada requiere un token Identity con rol <see cref="AlthesChannels.UserRole"/>
/// (o admin). El cliente apunta a un <c>projectId</c> concreto: cada instancia
/// Althes (cada proyecto) escucha en su propio canal.
/// </para>
/// </summary>
public class HorizonAlthesClient
{
    private readonly HorizonEventService Events;
    private readonly string Channel;
    private readonly TimeSpan? Timeout;
    
    /// <summary>Token bearer actual. Puede actualizarse desde fuera.</summary>
    public string? Token { get; set; }
    
    public HorizonAlthesClient(HorizonEventService events, string projectId, string? token = null, TimeSpan? timeout = null)
    {
        Events = events;
        Channel = AlthesChannels.For(projectId);
        Token = token;
        Timeout = timeout;
    }
    
    private string RequireToken() => Token
        ?? throw new InvalidOperationException("No hay token configurado en HorizonAlthesClient.");
    
    private Task<TRes> Send<TRes>(IHorizonEventPayload req) where TRes : IHorizonEventPayload =>
        Events.RequestAsync<TRes>(Channel, req, Timeout);
    
    public Task<ListAgentsResponse> ListAgentsAsync() =>
        Send<ListAgentsResponse>(new ListAgentsRequest { Token = RequireToken() });
    
    public Task<SendMessageResponse> SendMessageAsync(string agentName, string content, bool startNewConversation = false) =>
        Send<SendMessageResponse>(new SendMessageRequest
        {
            Token = RequireToken(),
            AgentName = agentName,
            Content = content,
            StartNewConversation = startNewConversation,
        });
    
    public Task<StartNewRunResponse> StartNewRunAsync(string agentName) =>
        Send<StartNewRunResponse>(new StartNewRunRequest
        {
            Token = RequireToken(),
            AgentName = agentName,
        });
    
    public Task<StartNewConversationResponse> StartNewConversationAsync(string? name = null) =>
        Send<StartNewConversationResponse>(new StartNewConversationRequest
        {
            Token = RequireToken(),
            Name = name,
        });
    
    public Task<ListConversationsResponse> ListConversationsAsync(bool includeClosed = true, int maxConversations = 50) =>
        Send<ListConversationsResponse>(new ListConversationsRequest
        {
            Token = RequireToken(),
            IncludeClosed = includeClosed,
            MaxConversations = maxConversations,
        });
    
    public Task<GetConversationResponse> GetConversationAsync(string conversationId) =>
        Send<GetConversationResponse>(new GetConversationRequest
        {
            Token = RequireToken(),
            ConversationId = conversationId,
        });
    
    public Task<DeleteConversationResponse> DeleteConversationAsync(string conversationId) =>
        Send<DeleteConversationResponse>(new DeleteConversationRequest
        {
            Token = RequireToken(),
            ConversationId = conversationId,
        });
    
    public Task<GetRunsResponse> ListRunsAsync(string agentName, int maxRuns = 20, bool includeMessages = false) =>
        Send<GetRunsResponse>(new GetRunsRequest
        {
            Token = RequireToken(),
            AgentName = agentName,
            MaxRuns = maxRuns,
            IncludeMessages = includeMessages,
        });
    
    public Task<GetRunResponse> GetRunAsync(string runId) =>
        Send<GetRunResponse>(new GetRunRequest { Token = RequireToken(), RunId = runId });
    
    public Task<ListUserQueriesResponse> ListUserQueriesAsync(bool includeAnswered = false) =>
        Send<ListUserQueriesResponse>(new ListUserQueriesRequest
        {
            Token = RequireToken(),
            IncludeAnswered = includeAnswered,
        });
    
    public Task<AnswerUserQueryResponse> AnswerUserQueryAsync(string queryId, string answer) =>
        Send<AnswerUserQueryResponse>(new AnswerUserQueryRequest
        {
            Token = RequireToken(),
            QueryId = queryId,
            Answer = answer,
        });
}

