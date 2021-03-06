using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Constants.Logs;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnInitialise(BaseNodeConfiguration nodeConfiguration)
            => this
                .Tee(_ => _nodeConfiguration = nodeConfiguration)
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
                .Map(_ => _status)
                .Tee(_ => _logger.Information(NODE_INITIALISE));

        public Status OnInitialise(BaseNodeConfiguration nodeConfiguration, Status status)
            => this
                .Tee(_ => _nodeConfiguration = nodeConfiguration)
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
                .Map(_ => _status)
                .Tee(_ => _logger.Information(NODE_INITIALISE_WITH_STATUS));
    }
}
