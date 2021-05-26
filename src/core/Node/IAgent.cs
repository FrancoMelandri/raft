using RaftCore.Models;

namespace RaftCore.Node
{
    public interface IAgent
    {
        Descriptor OnInitialise(NodeConfiguration nodeConfiguration);
        Descriptor OnRecoverFromCrash(NodeConfiguration nodeConfiguration, Descriptor descriptor);
        Descriptor OnLeaderHasFailed();
        Descriptor OnElectionTimeOut();
        Descriptor OnReceivedVoteRequest(VoteRequestMessage message);
        Descriptor OnReceivedVoteResponse(VoteResponseMessage message);
        Descriptor OnBroadcastMessage(Message message);
        Descriptor OnReplicateLog();
    }
}
