﻿using FluentAssertions;
using NUnit.Framework;
using Raft.Node;
using TinyFp;

namespace RaftTest.Raft
{
    [TestFixture]
    public class ElectionTest
    {
        [Test]
        public void Start_DoNothing()
            => new Election()
                .Start()
                .Should().Be(Unit.Default);

        [Test]
        public void Cancel_DoNothing()
            => new Election()
                .Cancel()
                .Should().Be(Unit.Default);        
    }
}
