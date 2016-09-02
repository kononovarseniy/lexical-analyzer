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
            Assert.AreEqual(Regex.WhiteSpaces,
                Regex.Parse(@"\s").Value);

            Assert.AreEqual((Regex.WhiteSpaces | ~Regex.Digits),
                Regex.Parse(@"[\s\D]").Value);

            Assert.AreEqual(~(Regex.WhiteSpaces | ~Regex.Digits),
                Regex.Parse(@"[^\s\D]").Value);

            Assert.AreEqual(~(Regex.WhiteSpaces | new IntSet('a', 'z')),
                Regex.Parse(@"[^\sa-z]").Value);

            Assert.AreEqual(IntSet.Empty,
                Regex.Parse(@"[^\sa-z.]").Value);

            Assert.AreEqual(IntSet.All,
                Regex.Parse(@"[\sa-z.]").Value);

            Assert.AreEqual(new IntSet('\n', '\r'),
                Regex.Parse(@"[\n-\r]").Value);

            Assert.AreEqual(new IntSet('\n', 'z'),
                Regex.Parse(@"[\n-z]").Value);

            Assert.AreEqual(new IntSet('-'),
                Regex.Parse(@"[\--\-]").Value);

            var res = Regex.Parse(@"123\s|asd([^123]*|5)+");

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
                    Regex.Parse(error);
                    Assert.Fail();
                }
                catch (ArgumentException) { }
            }
        }
    }
}