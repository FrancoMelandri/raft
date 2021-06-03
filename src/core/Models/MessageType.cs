namespace RaftCore.Models
{
    public enum MessageType
    {
        None,
        VoteRequest,
        VoteResponse,
        LogRequest,
        LogResponse,
        Application
    }
}
