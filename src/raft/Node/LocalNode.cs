using RaftCore.Cluster;
using RaftCore.Node;
using TinyFp;
using TinyFp.Extensions;
using RaftCore.Adapters;
using RaftCore.Models;
using System;
using System.Collections.Generic;

namespace Raft.Node
{
    public class LocalNode : ILocalNode,
                             IMessageObserver,
                             ILeaderFailureObserver
    {
        private Dictionary<MessageType, Func<Message, Unit>> _messageActions;

        private readonly LocalNodeConfiguration _nodeConfiguration;
        private readonly IStatusRepository _statusRepository;
        private readonly IMessageListener _messageListener;
        private readonly ILeaderFailureDetector _leaderFailure;
        private readonly IAgent _agent;

        public int Id => _nodeConfiguration.Id;

        public LocalNode(LocalNodeConfiguration nodeConfiguration,
                         IAgent agent,
                         IStatusRepository statusRepository,
                         IMessageListener messageListener,
                         ILeaderFailureDetector leaderFailure)
        {
            _nodeConfiguration = nodeConfiguration;
            _agent = agent;
            _statusRepository = statusRepository;
            _messageListener = messageListener;
            _leaderFailure = leaderFailure;
            _messageActions = new Dictionary<MessageType, Func<Message, Unit>>
            {
                { MessageType.None, _ => Unit.Default },
                { MessageType.VoteRequest, _ => Unit.Default },
                { MessageType.VoteResponse, HandleVoteResponse },
                { MessageType.LogRequest, HandleLogRequest },
                { MessageType.LogResponse, HandleLogResponse }
            };
        }

        public Unit Initialise()
            => _statusRepository
                .Load()
                .Match(status => _agent.OnInitialise(_nodeConfiguration, status),
                       () => _agent.OnInitialise(_nodeConfiguration))
                .Tee(_ => _messageListener.Start(this))
                .Tee(_ => _leaderFailure.Start(this))
                .Map(_ => Unit.Default);

        public Unit Deinitialise()
            => _statusRepository
                .Save(_agent.CurrentStatus())
                .OnNone(Unit.Default)
                .Tee(_ => _messageListener.Stop())
                .Tee(_ => _leaderFailure.Stop());

        public Unit NotifyMessage(Message message)
            => _messageActions
                .ToOption(_ => !_.ContainsKey(message.Type))
                .Match(_ => _[message.Type](message), 
                      () => Unit.Default);

        public Unit NotifyFailure()
            => _agent
                .OnLeaderHasFailed()
                .Map( _ => Unit.Default);

        private Unit HandleLogRequest(Message message)
            => message
                .Map(_ => message as LogRequestMessage)
                .ToOption()
                .Match(_ => _agent
                                .OnReceivedLogRequest(_)
                                .Tee(_ => _leaderFailure.Reset())
                                .Map(_ => Unit.Default),
                        () => Unit.Default);

        private Unit HandleVoteResponse(Message message)
            => message
                .Map(_ => message as VoteResponseMessage)
                .ToOption()
                .Match(_ => _agent
                                .OnReceivedVoteResponse(_)
                                .Map(_ => Unit.Default),
                        () => Unit.Default);

        private Unit HandleLogResponse(Message message)
            => message
                .Map(_ => message as LogResponseMessage)
                .ToOption()
                .Match(_ => _agent
                                .OnReceivedLogResponse(_)
                                .Map(_ => Unit.Default),
                        () => Unit.Default);
    }
}
