# RAFT

RAFT is a consensus algorithm

> The goal of a **distributed consensus algorithm** is to allow a set of computers to all agree on a single value that one of the nodes in the **system** proposed (as opposed to making up a random value). The challenge in doing this in a **distributed system** is that messages can be lost or machines can fail.

Basically the foundation part of RAFT is a state machine where each **node** could be in one particular state: **Leader**, **Candidate** and **Follower**.



## Node state transitions

Initially, when a node starts up, or when it crashes and restarts, it is in the follower state.

Eventually the system needs to have a leader, so in order for a node to became a leader, it has to first became a candidate; nodes became a candidate if its failure detector tells it that there is a suspected leader failure (essentially if it doesn't hear the leader for a certain period of time there's a time out where the node move into the candidate state assuming the leader is dead).

When a node becomes a candidate it asks for votes from the other nodes, and if successfully obtains a quorum of votes from other nodes, then this candidate can became a leader. It could be happen that in this phase the candidate hears about some other candidate with a higher **term number** and in this case, the candidate step back to the follower state. It could be happen also that the election ends for time out not receiving any answer from other nodes, and in this case the candidate starts a new election phase with a higher term number.

Once a node is leader it remains leader for potentially a very long time, until that node crashes, get shut down or another candidate starts a new election phase with a higher term.

![raft-state-machine](./imgs/raft-state-machine.png)

> State transitions displayed

## Code

Let's start digging into the code step by step spread over nine part.

### Initialisation

![raft-code-1](./imgs/raft-code-1.png)

During this phase we are going to initialise a bunch of variables to let the raft works properly. The first four variables **currentTerm**, **votedFor**, **log** and **commitLenght** has to be store in a persistent storage. These variables must be updated on disk before the node do anything else; this is important for the crash recovery strategy. The other five variable should be stored in memory because they are going to initialized during a recovery from crash.

 







