using Microsoft.VisualStudio.TestTools.UnitTesting;
using LexicalAnalyzer.FsmNS.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalyzer.FsmNS.Types;

namespace LexicalAnalyzer.FsmNS.Builders.Tests
{
    [TestClass()]
    public class NfaBuilderTests
    {
        private void CompareFsm(FsmInfo<int, int> a, FsmInfo<int, int> b, bool compareStates = false)
        {
            Assert.IsTrue(a.States.Length == b.States.Length);
            if (compareStates)
            {
                Assert.IsTrue(Enumerable.Zip(a.States, b.States, (s1, s2) =>
                    s1 == s2).All(_ => _));
            }

            var hsT = new HashSet<FsmTransition<int>>();
            hsT.UnionWith(a.Transitions);
            hsT.SymmetricExceptWith(b.Transitions);
            Assert.IsTrue(hsT.Count == 0);

            var hsF = new HashSet<int>();
            hsF.UnionWith(a.FinalStates);
            hsF.SymmetricExceptWith(b.FinalStates);
            Assert.IsTrue(hsF.Count == 0);
        }

        private void DoTest(Action<NfaBuilder<int, int>> action)
        {
            int counter = 0;
            action(new NfaBuilder<int, int>(() => counter++));
        }

        [TestMethod()]
        public void NfaBuilderTest()
        {
            Func<int> f = () => 123;
            var b = new NfaBuilder<int, int>(f);
            Assert.AreEqual(b.NewState, f);
        }

        [TestMethod()]
        public void EmptyTest()
        {
            DoTest((builder) =>
            {
                var actual = builder.Empty();
                var expected = new FsmInfo<int, int>(
                    states: new[] { 0 },
                    transitions: new FsmTransition<int>[] { },
                    finalStates: new HashSet<int>() { 0 });
                CompareFsm(expected, actual, true);
            });
        }

        [TestMethod()]
        public void TerminalTest()
        {
            DoTest((builder) =>
            {
                var actual = builder.Terminal(-1);
                var expected = new FsmInfo<int, int>(
                    states: new[] { 0, 1 },
                    transitions: new[] {
                        new FsmTransition<int>(0, 1, -1)},
                    finalStates: new HashSet<int>() { 1 });
                CompareFsm(expected, actual, true);
            });
        }

        [TestMethod()]
        public void AlternativesTest()
        {
            DoTest((builder) =>
            {
                var actual = builder.Alternatives(
                    builder.Terminal(-1),
                    builder.Terminal(-2));
                var expected = new FsmInfo<int, int>(
                    states: new[] { 4, 0, 1, 2, 3 },
                    transitions: new[] {
                        new FsmTransition<int>(0, 1),
                        new FsmTransition<int>(0, 3),
                        new FsmTransition<int>(1, 2, -1),
                        new FsmTransition<int>(3, 4, -2)},
                    finalStates: new HashSet<int>() { 2, 4 });
                CompareFsm(expected, actual, false);
            });
        }

        [TestMethod()]
        public void SequenceTest()
        {
            DoTest((builder) =>
            {
                var actual = builder.Sequence(
                    builder.Alternatives(
                        builder.Terminal(-1),
                        builder.Terminal(-2)),
                    builder.Terminal(-3));
                var expected = new FsmInfo<int, int>(
                    states: new[] { 4, 0, 1, 2, 3, 5, 6 },
                    transitions: new[] {
                        new FsmTransition<int>(2, 5),
                        new FsmTransition<int>(4, 5),
                        new FsmTransition<int>(0, 1),
                        new FsmTransition<int>(0, 3),
                        new FsmTransition<int>(1, 2, -1),
                        new FsmTransition<int>(3, 4, -2),
                        new FsmTransition<int>(5, 6, -3)},
                    finalStates: new HashSet<int>() { 6 });
                CompareFsm(expected, actual, false);
            });
        }

        [TestMethod()]
        public void IterationTest()
        {
            DoTest((builder) =>
            {
                var actual = builder.Iteration(
                    builder.Alternatives(
                        builder.Terminal(-1),
                        builder.Terminal(-2)));
                var expected = new FsmInfo<int, int>(
                    states: new[] {5, 6, 4, 0, 1, 2, 3},
                    transitions: new[] {
                        new FsmTransition<int>(2, 3),
                        new FsmTransition<int>(2, 5),
                        new FsmTransition<int>(3, 4, -1),
                        new FsmTransition<int>(5, 6, -2),
                        new FsmTransition<int>(0, 2),
                        new FsmTransition<int>(0, 1),
                        new FsmTransition<int>(4, 1),
                        new FsmTransition<int>(6, 1),
                        new FsmTransition<int>(4, 2),
                        new FsmTransition<int>(6, 2)},
                    finalStates: new HashSet<int>() { 1 });
                CompareFsm(expected, actual, false);
            });
        }

        [TestMethod()]
        public void OptionalTest()
        {
        }

        [TestMethod()]
        public void PositiveIterationTest()
        {
        }
    }
}