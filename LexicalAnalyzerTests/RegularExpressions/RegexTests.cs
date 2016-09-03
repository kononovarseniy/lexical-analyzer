using LexicalAnalyzer.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.RegularExpressions.Tests
{
    [TestClass()]
    public class RegexTests
    {
        [TestMethod()]
        public void ParseTest()
        {
            Assert.AreEqual(RegexParser.WhiteSpaces,
                RegexParser.Parse(@"\s").Value);

            Assert.AreEqual((RegexParser.WhiteSpaces | ~RegexParser.Digits),
                RegexParser.Parse(@"[\s\D]").Value);

            Assert.AreEqual(~(RegexParser.WhiteSpaces | ~RegexParser.Digits),
                RegexParser.Parse(@"[^\s\D]").Value);

            Assert.AreEqual(~(RegexParser.WhiteSpaces | new IntSet('a', 'z')),
                RegexParser.Parse(@"[^\sa-z]").Value);

            Assert.AreEqual(IntSet.Empty,
                RegexParser.Parse(@"[^\sa-z.]").Value);

            Assert.AreEqual(IntSet.All,
                RegexParser.Parse(@"[\sa-z.]").Value);

            Assert.AreEqual(new IntSet('\n', '\r'),
                RegexParser.Parse(@"[\n-\r]").Value);

            Assert.AreEqual(new IntSet('\n', 'z'),
                RegexParser.Parse(@"[\n-z]").Value);

            Assert.AreEqual(new IntSet('-'),
                RegexParser.Parse(@"[\--\-]").Value);

            var res = RegexParser.Parse(@"123\s|asd([^123]*|5)+");

            string[] errors =
            {
                @"[a-]",
                @"[-z]",
                @"[\s-z]",
                @"[.-z]",
                @"[.-\d]",
                @"[\r-\n]",
                @"-",
                @"a-",
                @"[abc",
                @"[",
                @"*",
                @"+",
                @"*+?",
                @"\s|",
                @"\s?|",
                @"|\s",
                @"|",
                @"||",
                @")",
                @"(",
                @"\s?)",
                @"(\s?",
            };
            foreach (var error in errors)
            {
                try
                {
                    RegexParser.Parse(error);
                    Assert.Fail();
                }
                catch (ArgumentException) { }
            }
        }
    }
}