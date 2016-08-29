using System;
using System.Collections.Generic;
using System.Linq;

namespace LexicalAnalyzer
{
    internal sealed class SToken : Token { }
    internal sealed class SStrToken : Token
    {
        public string Value { get; private set; } = "";
        protected override void SetText(string text)
        {
            base.SetText(text);
            Value = text.Substring(1, text.Length - 2).Replace("\"\"", "\"");
        }
    }
    
    public static class FsmCreator
    {
        /// <summary>
        /// Ancient dark magic hidden in this method.
        /// Now mortals have no need to do it anymore.
        /// </summary>
        /// <returns></returns>
        private static Fsm GetRulesDescriptionLanguageLexer()
        {
            FsmNode space = new FsmNode("space", true);
            space.Add(@"\s", space);

            FsmNode atom = new FsmNode("atom", true);
            atom.Add(@"[^\s()""]", atom);

            FsmNode str = new FsmNode("str-atom");
            str.Add(@"[^""]", str);
            str.Add(@"""", new FsmNode(true) { { @"""", str } });
            FsmNode first = new FsmNode() {
                                 { @"(", new FsmNode("opening-bracket", true) },
                                 { @")", new FsmNode("closing-bracket", true) },
                                 { @"[^\s()""]", atom },
                                 { @"\s", space },
                                 { @"""", str }
                             };
            return new Fsm() { FirstState = first };
        }
        private static FsmBuilder Builder = new FsmBuilder();
        public static Fsm CreateFsm(string rules)
        {
            Fsm lexer = GetRulesDescriptionLanguageLexer();
            var block = new LexerBlock(lexer, rules, (lex) =>
            {
                Lexeme res = null;
                if (lex.Class == "space")
                    res = null;
                else if (lex.Class == "str-atom")
                    res = lex.ToToken<SStrToken>(rules);
                else if (lex.Class != null)
                    res = lex.ToToken<SToken>(rules);
                else
                    throw new Exception();
                var list = new List<Lexeme>();
                if (res != null) list.Add(res);
                return list;
            });
            block.ExecuteAnalysis();
            var sList = new SList(SExpr.Parse(block.Select(t => t as Token)));
            return new Fsm() { FirstState = Builder.BuildFsm(sList) };
        }
    }
}
