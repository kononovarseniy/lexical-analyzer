﻿using System;
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
            FsmState space = new FsmState("space", true);
            space.Add(@"\s", space);

            FsmState atom = new FsmState("atom", true);
            atom.Add(@"[^\s()""]", atom);

            FsmState str = new FsmState("str-atom");
            str.Add(@"[^""]", str);
            str.Add(@"""", new FsmState(true) { { @"""", str } });
            FsmState first = new FsmState() {
                                 { @"(", new FsmState("opening-bracket", true) },
                                 { @")", new FsmState("closing-bracket", true) },
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
            var tokens = lexer.GetLexemesAndEvaluate(rules, (lex) =>
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
            var sList = new SList(SExpr.Parse(tokens.Select(t => t as Token)));
            return new Fsm() { FirstState = Builder.BuildFsm(sList) };
        }
    }
}
