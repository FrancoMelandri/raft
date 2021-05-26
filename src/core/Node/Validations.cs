using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Node.Utils;
using static System.Math;

namespace RaftCore.Node
{
    public static class Validations
    {
        public static Either<string, int> ValidateLog(Descriptor descriptor, VoteRequestMessage message)
            => ValidateLogTerm(descriptor, message)
                .Map(_ => _.Match(_ => _, _ => ValidateLogLength(descriptor, message)));

        private static Either<string, int> ValidateLogTerm(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm > LastEntryOrZero(descriptor.Log) ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        private static Either<string, int> ValidateLogLength(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm == LastEntryOrZero(descriptor.Log) &&
               message.LogLength >= descriptor.Log.Length ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateTerm(Descriptor descriptor, VoteRequestMessage message)
            => ValidateCurrentTerm(descriptor, message)
                .Map(_ => _.Match(_ => _, _ => ValidateVoteFor(descriptor, message)));

        private static Either<string, int> ValidateCurrentTerm(Descriptor descriptor, VoteRequestMessage message)
            => message.CurrentTerm > descriptor.CurrentTerm ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        private static Either<string, int> ValidateVoteFor(Descriptor descriptor, VoteRequestMessage message)
            => message.CurrentTerm == descriptor.CurrentTerm &&
               descriptor.VotedFor != message.NodeId ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateVoteGrant(Descriptor descriptor, VoteResponseMessage message)
            => descriptor.CurrentRole == States.Candidate && 
               descriptor.CurrentTerm == message.CurrentTerm &&
               message.Granted ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateTerm(Descriptor descriptor, VoteResponseMessage message)
            => message.CurrentTerm > descriptor.CurrentTerm ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateVotesQuorum(Descriptor descriptor, ICluster cluster)
        => descriptor.VotesReceived.ToOption(_ => _.Length, _ => _.Length == 0).OnNone(0) > (int)Floor(((decimal)cluster.Nodes.Length + 1) / 2) ?
            Either<string, int>.Right(0) :
            Either<string, int>.Left("");
    }
}
