using RaftCore.Models;

namespace RaftCore.Node
{
    public interface IAgent
    {
        Status OnInitialise(LocalNodeConfiguration nodeConfiguration);
        Status OnInitialise(LocalNodeConfiguration nodeConfiguration, Status status);
        Status OnLeaderHasFailed();
        Status OnElectionTimeOut();
        Status OnReceivedVoteRequest(VoteRequestMessage message);
        Status OnReceivedVoteResponse(VoteResponseMessage message);
        Status OnBroadcastMessage(Message message);
        Status OnReplicateLog();
        Status OnReceivedLogRequest(LogRequestMessage message);
        Status OnReceivedLogResponse(LogResponseMessage message);
    }
}
