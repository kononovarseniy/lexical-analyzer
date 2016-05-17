using Microsoft.VisualStudio.TestTools.UnitTesting;
using LexicalAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.Tests
{
    [TestClass()]
    public class OneCharRegexTests
    {
        [TestMethod()]
        [Timeout(1000)]
        public void IsMatchTest()
        {
            Assert.AreEqual(OneCharRegex.IsMatch('a', @"a-z"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('b', @"a-z"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('z', @"a-z"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('1', @"a-z"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('2', @"a-z1-5"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('b', @"a-z1-5"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('6', @"a-z1-5"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('b', @"[a-z1-5]"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('6', @"[a-z1-5]"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('b', @"[a-z]1-5"), true);
            Assert.AreEqual(OneCharRegex.IsMatch(' ', @"\s"), true);
            Assert.AreEqual(OneCharRegex.IsMatch(' ', @"\S"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('1', @"\S"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('1', @"\s"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('b', @"[^\s]"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('b', @"[^\S\n]"), false);
            Assert.AreEqual(OneCharRegex.IsMatch(' ', @"[^\S\n]"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('\n', @"[^\S\n]"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('d', @"[^[a-z]]"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('d', @"[^[^[[a-z]]]]"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('d', @"[^[^[[^a-z]]]]d"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('a', @"a\-z"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('-', @"a\-z"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('z', @"a\-z"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('z', @"."), true);
            Assert.AreEqual(OneCharRegex.IsMatch('z', @"\."), false);
            Assert.AreEqual(OneCharRegex.IsMatch('.', @"\."), true);
            Assert.AreEqual(OneCharRegex.IsMatch('-', @"\.-A"), false);
            Assert.AreEqual(OneCharRegex.IsMatch('@', @"\.-A"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('@', @".-a"), true);
            Assert.AreEqual(OneCharRegex.IsMatch('-', @"a\-b"), true);
        }
    }
}