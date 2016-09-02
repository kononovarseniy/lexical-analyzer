using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.Utils.Tests
{
    [TestClass()]
    public class IntSetTests
    {
        public void CompareSets(IntSet input, IEnumerable<IntSet> expected)
        {
            Assert.IsTrue(input.Count() == expected.Count());
            bool success = Enumerable.Zip(input, expected, (a, b) => a.Min == b.Min && a.Max == b.Max).All(a => a);
            Assert.IsTrue(success);
        }

        [TestMethod()]
        public void ContainsTest()
        {
            var set = new IntSet(5, 7);
            Assert.IsFalse(set.Contains(0));
            Assert.IsFalse(set.Contains(1));
            Assert.IsFalse(set.Contains(2));
            Assert.IsFalse(set.Contains(3));
            Assert.IsFalse(set.Contains(4));
            Assert.IsTrue(set.Contains(5));
            Assert.IsTrue(set.Contains(6));
            Assert.IsTrue(set.Contains(7));
            Assert.IsFalse(set.Contains(8));
            Assert.IsFalse(set.Contains(9));

            set = new IntSet(1, 3) | new IntSet(5, 5) | new IntSet(7, 9);
            Assert.IsFalse(set.Contains(0));
            Assert.IsTrue(set.Contains(1));
            Assert.IsTrue(set.Contains(2));
            Assert.IsTrue(set.Contains(3));
            Assert.IsFalse(set.Contains(4));
            Assert.IsTrue(set.Contains(5));
            Assert.IsFalse(set.Contains(6));
            Assert.IsTrue(set.Contains(7));
            Assert.IsTrue(set.Contains(8));
            Assert.IsTrue(set.Contains(9));
            Assert.IsFalse(set.Contains(10));
        }

        [TestMethod()]
        public void UnionTest()
        {
            CompareSets(
                new IntSet(1, 3) | new IntSet(5, 6) | new IntSet(4, 4),
                new List<IntSet>()
                {
                    new IntSet(1, 6)
                });
            CompareSets(
                new IntSet(1, 3) | new IntSet(6, 7) | new IntSet(4, 4),
                new List<IntSet>()
                {
                    new IntSet(1, 4),
                    new IntSet(6, 7)
                });
            CompareSets(
                new IntSet(1, 3) | new IntSet(6, 7) | new IntSet(4, 4),
                new IntSet(1, 4) | new IntSet(6, 7));

            Assert.AreEqual(IntSet.All, new IntSet(0, 5) | IntSet.All);

            Assert.AreEqual(IntSet.All, IntSet.All | new IntSet(0, 5));
        }

        [TestMethod()]
        public void IntersectionTest()
        {
            CompareSets(
                IntSet.Intersection(
                    new IntSet(1, 3) | new IntSet(5, 6),
                    new IntSet(3, 6)
                ),
                new List<IntSet>()
                {
                    new IntSet(3, 3),
                    new IntSet(5, 6)
                });
            CompareSets(
                IntSet.Intersection(
                    new IntSet(1, 3) | new IntSet(5, 6),
                    new IntSet(3, 6) | new IntSet(10, 12)
                ),
                new List<IntSet>()
                {
                    new IntSet(3, 3),
                    new IntSet(5, 6)
                });
            CompareSets(
                IntSet.Intersection(
                    new IntSet(1, 3) | new IntSet(5, 6),
                    new IntSet(10, 12)
                ),
                IntSet.Empty);
            CompareSets(
                IntSet.Intersection(
                    IntSet.All,
                    IntSet.Empty
                ),
                IntSet.Empty);
            CompareSets(
                IntSet.Intersection(
                    IntSet.All,
                    IntSet.All
                ),
                IntSet.All);
        }

        [TestMethod()]
        public void DifferenceTest()
        {
            CompareSets(
                IntSet.Difference(
                    new IntSet(1, 3) | new IntSet(5, 6),
                    new IntSet(3, 6)
                ),
                new List<IntSet>()
                {
                    new IntSet(1, 2)
                });
            CompareSets(
                IntSet.Difference(
                    new IntSet(1, 3) | new IntSet(5, 10),
                    new IntSet(3, 6)
                ),
                new List<IntSet>()
                {
                    new IntSet(1, 2),
                    new IntSet(7, 10)
                });
        }

        [TestMethod()]
        public void ComplementTest()
        {
            CompareSets(
                IntSet.Complement(new IntSet(1, 3) | new IntSet(5, 6)),
                new List<IntSet>()
                {
                    new IntSet(int.MinValue + 1, 0),
                    new IntSet(4, 4),
                    new IntSet(7, int.MaxValue)
                });
            CompareSets(
                ~IntSet.All,
                IntSet.Empty);
            CompareSets(
                ~IntSet.Empty,
                IntSet.All);
        }
    }
}