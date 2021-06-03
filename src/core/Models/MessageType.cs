namespace RaftCore.Models
{
    public enum MessageType
    {
        None,
        Application,
        VoteRequest,
        VoteResponse,
        LogRequest,
        LogResponse
    }
}
