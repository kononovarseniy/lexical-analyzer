using Microsoft.VisualStudio.TestTools.UnitTesting;
using LexicalAnalyzer.FsmNS.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalyzer.Utils;
using LexicalAnalyzer.FsmNS.Types;

namespace LexicalAnalyzer.FsmNS.Builders.Tests
{
    [TestClass()]
    public class AlphabetConverterTests
    {
        [TestMethod()]
        public void AlphabetConverterTest()
        {
            AlphabetConverter converter;
            converter = new AlphabetConverter(new IntSet[] { });
            converter = new AlphabetConverter(new IntSet[]
            {
                new IntSet(0, 100),
                new IntSet(50, 100),
                new IntSet(75),
                new IntSet(-10)
            });
            converter = new AlphabetConverter(new IntSet[]
            {
                new IntSet(0, 100) | new IntSet(200, 300),
                new IntSet(200, 300) | new IntSet(400, 500)
            });
        }

        private void DoValueTest(IEnumerable<IntSet> sets, Dictionary<int, int> values)
        {
            var converter = new AlphabetConverter(sets);
            foreach (var kvp in values)
                Assert.AreEqual(kvp.Value, converter.ConvertValue(kvp.Key));
        }

        [TestMethod()]
        public void ConvertValueTest()
        {
            DoValueTest(new[]
            {
                new IntSet(int.MinValue + 1),
                new IntSet(0, 100) | new IntSet(200, 300),
                new IntSet(200, 300) | new IntSet(400, 500)
            },
            new Dictionary<int, int>()
            {
                { int.MinValue, 0 },
                { int.MinValue + 1, 1 },
                { int.MinValue + 2, 0 },
                { -1, 0 },
                { 0, 2 },
                { 100, 2 },
                { 150, 0 },
                { 250, 3 },
                { 350, 0 },
                { 450, 4 }
            });

            DoValueTest(new[]
            {
                new IntSet(int.MaxValue)
            },
            new Dictionary<int, int>()
            {
                { int.MinValue, 0 },
                { int.MaxValue, 1 }
            });

            DoValueTest(new[]
            {
                new IntSet(0, 100),
                new IntSet(50, 100)
            },
            new Dictionary<int, int>()
            {
                { -1, 0 },
                { 25, 1 },
                { 50, 2 },
                { 100, 2 },
                { 101, 0 }
            });

            DoValueTest(new[]
            {
                new IntSet(0, 100),
                new IntSet(50, 150)
            },
            new Dictionary<int, int>()
            {
                { -1, 0 },
                { 25, 1 },
                { 50, 2 },
                { 100, 2 },
                { 101, 3 },
                { 151, 0 }
            });
        }
        
        private void DoSetTest(IEnumerable<IntSet> sets, Dictionary<IntSet, IEnumerable<int>> values)
        {
            var converter = new AlphabetConverter(sets);
            foreach (var kvp in values)
                Assert.IsTrue(converter.ConvertSet(kvp.Key).SequenceEqual(kvp.Value));
        }

        [TestMethod()]
        public void ConvertSetTest()
        {
            DoSetTest(new[]
            {
                new IntSet(0, 100),
                new IntSet(50, 100)
            },
            new Dictionary<IntSet, IEnumerable<int>>()
            {
                {new IntSet(0, 100), new [] { 1, 2 } },
                {new IntSet(50, 100), new [] { 2 } }
            });
            DoSetTest(new[]
            {
                new IntSet(0, 100),
                new IntSet(50, 150),
                new IntSet(200)
            },
            new Dictionary<IntSet, IEnumerable<int>>()
            {
                {new IntSet(0, 100), new [] { 1, 2 } },
                {new IntSet(50, 150), new [] { 2, 3 } },
                {new IntSet(200), new [] { 4 } }
            });
            try
            {
                DoSetTest(new[]
                {
                    new IntSet(100, 250)
                },
                new Dictionary<IntSet, IEnumerable<int>>()
                {
                    { new IntSet(100, 200), null }
                });
                Assert.Fail();
            }
            catch (KeyNotFoundException) { }
        }

        [TestMethod()]
        public void ConvertFsmTest()
        {
            AlphabetConverter converter;
            FsmInfo<int, IntSet> fsm = new FsmInfo<int, IntSet>(
                states: new[] { 0, 1 },
                transitions: FsmTransition<IntSet>.CreateTransitions(0, 1, new[]
                {
                    new IntSet(0, 100),
                    new IntSet(50, 100),
                    new IntSet(75),
                    new IntSet(-10)
                }).ToArray(),
                finalStates: new HashSet<int>() { 1 });
            converter = AlphabetConverter.Create(fsm);
            Assert.AreEqual(1, converter.ConvertValue(-10));
            Assert.AreEqual(0, converter.ConvertValue(-1));
            Assert.AreEqual(2, converter.ConvertValue(25));
            Assert.AreEqual(3, converter.ConvertValue(70));
            Assert.AreEqual(4, converter.ConvertValue(75));
        }
    }
}