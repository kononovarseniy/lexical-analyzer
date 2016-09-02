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
        public void CompileTest()
        {
            Assert.AreEqual(Regex.WhiteSpaces,
                Regex.Compile(@"\s").Value);

            Assert.AreEqual(Regex.WhiteSpaces,
                Regex.Compile(@"\s\D").Value);

            Assert.AreEqual((Regex.WhiteSpaces | ~Regex.Digits),
                Regex.Compile(@"[\s\D]").Value);

            Assert.AreEqual(~(Regex.WhiteSpaces | ~Regex.Digits),
                Regex.Compile(@"[^\s\D]").Value);

            Assert.AreEqual(~(Regex.WhiteSpaces | new IntSet('a', 'z')),
                Regex.Compile(@"[^\sa-z]").Value);

            Assert.AreEqual(IntSet.Empty,
                Regex.Compile(@"[^\sa-z.]").Value);

            Assert.AreEqual(IntSet.All,
                Regex.Compile(@"[\sa-z.]").Value);

            Assert.AreEqual(new IntSet('\n', '\r'),
                Regex.Compile(@"[\n-\r]").Value);

            Assert.AreEqual(new IntSet('\n', 'z'),
                Regex.Compile(@"[\n-z]").Value);

            Assert.AreEqual(new IntSet('-'),
                Regex.Compile(@"[\--\-]").Value);

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
                @"["
            };
            foreach (var error in errors)
            {
                try
                {
                    Regex.Compile(error);
                    Assert.Fail();
                }
                catch (ArgumentException) { }
            }
        }
    }
}