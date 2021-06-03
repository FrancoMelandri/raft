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
                             IMessageObserver
    {
        private Dictionary<MessageType, Func<Message, Unit>> _messageActions;

        private readonly LocalNodeConfiguration _nodeConfiguration;
        private readonly IStatusRepository _statusRepository;
        private readonly IMessageListener _messageListener;
        private readonly IAgent _agent;

        public int Id => _nodeConfiguration.Id;

        public LocalNode(LocalNodeConfiguration nodeConfiguration,
                         IAgent agent,
                         IStatusRepository statusRepository,
                         IMessageListener messageListener)
        {
            _nodeConfiguration = nodeConfiguration;
            _agent = agent;
            _statusRepository = statusRepository;
            _messageListener = messageListener;

            _messageActions = new Dictionary<MessageType, Func<Message, Unit>>
            {
                { MessageType.None, _ => Unit.Default },
                { MessageType.VoteRequest, _ => Unit.Default },
                { MessageType.VoteResponse, _ => Unit.Default },
                { MessageType.LogRequest, _ => Unit.Default },
                { MessageType.LogResponse, _ => Unit.Default },
            };
        }

        public Unit Initialise()
            => _statusRepository
                .Load()
                .Match(status => _agent.OnInitialise(_nodeConfiguration, status),
                       () => _agent.OnInitialise(_nodeConfiguration))
                .Tee(_ => _messageListener.Start(this))
                .Map(_ => Unit.Default);

        public Unit Deinitialise()
            => _statusRepository
                .Save(_agent.CurrentStatus())
                .OnNone(Unit.Default)
                .Tee(_ => _messageListener.Stop());

        public Unit NotifyMessage(Message message)
            => _messageActions
                .ToOption(_ => !_.ContainsKey(message.Type))
                .Match(_ => _[message.Type](message), 
                      () => Unit.Default);
    }
}
