using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnInitialise(NodeConfiguration nodeConfiguration)
            => this
                .Tee(_ => _configuration = nodeConfiguration)
                .Tee(_ => _status = new Status
                {
                    CurrentTerm = INIT_TERM,
                    VotedFor = INIT_VOTED_FOR,
                    Log = INIT_LOG,
                    CommitLenght = INIT_COMMIT_LENGTH,
                    CurrentRole = INIT_STATE,
                    CurrentLeader = INIT_CURRENT_LEADER,
                    VotesReceived = INIT_VOTES_RECEIVED,
                    SentLength = INIT_SENT_LENGTH,
                    AckedLength = INIT_ACKED_LENGTH
                })
                .Map(_ => _status);

        public Status OnInitialise(NodeConfiguration nodeConfiguration, Status status)
            => this
                .Tee(_ => _configuration = nodeConfiguration)
                .Tee(_ => _status = new Status
                {
                    CurrentTerm = status.CurrentTerm,
                    VotedFor = status.VotedFor,
                    Log = status.Log,
                    CommitLenght = status.CommitLenght,
                    CurrentRole = INIT_STATE,
                    CurrentLeader = INIT_CURRENT_LEADER,
                    VotesReceived = INIT_VOTES_RECEIVED,
                    SentLength = INIT_SENT_LENGTH,
                    AckedLength = INIT_ACKED_LENGTH
                })
                .Map(_ => _status);
    }
}
