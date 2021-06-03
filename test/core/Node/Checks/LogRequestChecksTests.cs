using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftTest.Core.Checks
{
    [TestFixture]
    public class LogRequestChecksTests
    {
        [Test]
        public void IsTermGreater_WhenTerm_GT_CurrentTerm_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                Term = 5
            };
            var status = new Status
            {
                CurrentTerm = 4
            };
            LogRequestChecks
                .IsTermGreater(message, status)
                .IsRight.Should().BeTrue();
        }

        [TestCase(5)]
        [TestCase(6)]
        public void IsTermGreater_WhenTerm_LE_CurrentTerm_ReturnError(int current)
        {
            var message = new LogRequestMessage
            {
                Term = 5
            };
            var status = new Status
            {
                CurrentTerm = current
            };
            LogRequestChecks
                .IsTermGreater(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsLengthOk_WhenLogLength_GT_Message_And_Message_GT_Zero_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1
            };
            var status = new Status
            {
                Log = new LogEntry[] 
                {
                    new LogEntry(),
                    new LogEntry()
                }
            };
            LogRequestChecks
                .IsLengthOk(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsLengthOk_WhenLogLength_LT_Message_And_Message_GT_Zero_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 5
            };
            var status = new Status
            {
                Log = new LogEntry[] 
                {
                    new LogEntry(),
                    new LogEntry()
                }
            };
            LogRequestChecks
                .IsLengthOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsLengthOk_WhenLogLength_GT_Message_And_Message_EQ_Zero_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 0
            };
            var status = new Status
            {
                Log = new LogEntry[] 
                {
                    new LogEntry(),
                    new LogEntry()
                }
            };
            LogRequestChecks
                .IsLengthOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsTermOk_WhenLastLogTerm_EQ_LogTerm_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                LogTerm = 5
            };
            var status = new Status
            {
                Log = new LogEntry[] 
                {
                    new LogEntry{ Term = 5 }
                }
            };
            LogRequestChecks
                .IsTermOk(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsTermOk_WhenLastLogTerm_NEQ_LogTerm_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                LogTerm = 5
            };
            var status = new Status
            {
                Log = new LogEntry[] 
                {
                    new LogEntry{ Term = 6 }
                }
            };
            LogRequestChecks
                .IsTermOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsTermOk_WhenLogLength_LT_LogLength_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 2,
                LogTerm = 5
            };
            var status = new Status
            {
                Log = new LogEntry[] 
                {
                    new LogEntry{ Term = 6 }
                }
            };
            LogRequestChecks
                .IsTermOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsCurrentTermOk_WhenTerm_EQ_CurrentTerm_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                Term = 1
            };
            var status = new Status
            {
                CurrentTerm = 1
            };
            LogRequestChecks
                .IsCurrentTermOk(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsCurrentTermOk_WhenTerm_NEQ_CurrentTerm_ReturnError()
        {
            var message = new LogRequestMessage
            {
                Term = 2
            };
            var status = new Status
            {
                CurrentTerm = 1
            };
            LogRequestChecks
                .IsCurrentTermOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsEntriesLogLengthOk_WhenEntries_And_LogLength_GT_MessageLogLength_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                Entries = new LogEntry[]
                {
                    new LogEntry()
                }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry()
                }
            };
            LogRequestChecks
                .IsEntriesLogLengthOk(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsEntriesLogLengthOk_WhenEntries_And_LogLength_LE_MessageLogLength_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 2,
                Entries = new LogEntry[]
                {
                    new LogEntry()
                }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry()
                }
            };
            LogRequestChecks
                .IsEntriesLogLengthOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsEntriesLogLengthOk_WhenNoEntries_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                Entries = new LogEntry[] {}
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry()
                },
            };
            LogRequestChecks
                .IsEntriesLogLengthOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsEntriesTermhNotOk_WhenLogTerm_NEQ_FirstEntryTerm_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                Entries = new LogEntry[]
                {
                    new LogEntry { Term = 5}
                }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry { Term = 1 }
                }
            };
            LogRequestChecks
                .IsEntriesTermNotOk(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsEntriesTermhNotOk_WhenLogTerm_EQ_FirstEntryTerm_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                Entries = new LogEntry[]
                {
                    new LogEntry { Term = 5}
                }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry { Term = 5 }
                }
            };
            LogRequestChecks
                .IsEntriesTermNotOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsEntriesTermhNotOk_WhenNoEntries_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                Entries = new LogEntry[] { }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry { Term = 1 }
                }
            };
            LogRequestChecks
                .IsEntriesTermNotOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void IsEntriesTermhNotOk_WhenLogLengthWrong_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 3,
                Entries = new LogEntry[]
                {
                    new LogEntry { Term = 5 }
                }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry { Term = 1 }
                }
            };
            LogRequestChecks
                .IsEntriesTermNotOk(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void AreThereEntriesToAdd_WhenLogLength_Plus_EntriesLength_GT_LogLength_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                Entries = new LogEntry[]
                {
                    new LogEntry { Term = 5 }
                }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry()
                }
            };
            LogRequestChecks
                .AreThereEntriesToAdd(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void AreThereEntriesToAdd_WhenLogLength_Plus_EntriesLength_LT_LogLength_ReturnError()
        {
            var message = new LogRequestMessage
            {
                LogLength = 1,
                Entries = new LogEntry[]
                {
                    new LogEntry { Term = 5 }
                }
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry(),
                    new LogEntry()                    
                }
            };
            LogRequestChecks
                .AreThereEntriesToAdd(message, status)
                .IsLeft.Should().BeTrue();
        }

        [Test]
        public void AreThereUncommitedMessages_WhenCommitLength_GT_StatusCommitLength_ReturnStatus()
        {
            var message = new LogRequestMessage
            {
                CommitLength = 5
            };
            var status = new Status
            {
                CommitLenght = 4
            };
            LogRequestChecks
                .AreThereUncommitedMessages(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void AreThereUncommitedMessages_WhenCommitLength_LE_StatusCommitLength_ReturnError()
        {
            var message = new LogRequestMessage
            {
                CommitLength = 4
            };
            var status = new Status
            {
                CommitLenght = 5
            };
            LogRequestChecks
                .AreThereUncommitedMessages(message, status)
                .IsLeft.Should().BeTrue();
        }
    }
}
