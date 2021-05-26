using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnInitialise(NodeConfiguration nodeConfiguration)
            => this
                .Tee(_ => _.Configuration = nodeConfiguration)
                .Tee(_ => _descriptor = new Descriptor
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
                .Map(_ => _descriptor);

        public Descriptor OnInitialise(NodeConfiguration nodeConfiguration, Descriptor descriptor)
            => this
                .Tee(_ => _.Configuration = nodeConfiguration)
                .Tee(_ => _descriptor = new Descriptor
                {
                    CurrentTerm = descriptor.CurrentTerm,
                    VotedFor = descriptor.VotedFor,
                    Log = descriptor.Log,
                    CommitLenght = descriptor.CommitLenght,
                    CurrentRole = INIT_STATE,
                    CurrentLeader = INIT_CURRENT_LEADER,
                    VotesReceived = INIT_VOTES_RECEIVED,
                    SentLength = INIT_SENT_LENGTH,
                    AckedLength = INIT_ACKED_LENGTH
                })
                .Map(_ => _descriptor);
    }
}
