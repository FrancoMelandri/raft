# RAFT

RAFT is a consensus algorithm.

Basically the foundation part of RAFT is a state machine where each **node** could be in one particular state: **Leader**, **Candidate** and **Follower**.



## Node state transitions

Initially, when a node starts up, or when it crashes and restarts, it is in the follower state.

Eventually the system needs to have a leader, so in order for a node to became a leader, it has to first became a candidate; nodes became a candidate if its failure detector tells it that there is a suspected leader failure (essentially if it doesn't hear the leader for a certain period of time there's a time out where the node move into the candidate state assuming the leader is dead).

When a node becomes a candidate it asks for votes from the other nodes, and if successfully obtains a quorum of votes from other nodes, then this candidate can became a leader. It could be happen that in this phase the candidate hears about some other candidate with a higher **term number** and in this case, the candidate step back to the follower state. It could be happen also that the election ends for time out not receiving any answer from other nodes, and in this case the candidate starts a new election phase with a higher term number.

![image-20210113092002671](./imgs/raft-state-machine.png)



## code 1

![image-20210113092203984](./imgs/raft-code-1.png)



## code 2

![image-20210113093022154](./imgs/raft-code-2.png)



## code 3

![image-20210113093853017](./imgs/raft-code-3.png)



