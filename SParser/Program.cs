using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalyzer;
using System.IO;

namespace SParser
{
    sealed class SToken : Token { }
    sealed class SStrToken : Token
    {
        public string Value { get; private set; } = "";
        protected override void SetText(string text)
        {
            base.SetText(text);
            Value = text.Substring(1, text.Length - 2).Replace("\"\"", "\"");
        }
    }
    class Program
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
        static SList ParseS(Fsm lexer, string input)
        {
            var lexemes = lexer.GetLexemes(input);
            var tokens = Fsm.GetTokens(lexemes, (lex) =>
            {
                Lexeme res;
                if (lex.Class == null)
                    throw new Exception();
                if (lex.Class == "str-atom")
                    res = lex.ToToken<SStrToken>(input);
                else
                    res = lex.ToToken<SToken>(input);
                var list = new List<Lexeme>();
                if (lex.Class != "space")
                    list.Add(res);
                return list;
            });
            foreach (var tok in tokens)
            {
                Console.WriteLine($"{tok.Class ?? "ERROR",-20} = {tok}");
            }
            return new SList(SExpr.Parse(tokens.Select(t => t as Token)));
        }

        static void Main(string[] args)
        {
            var rulesLexer = GetRulesDescriptionLanguageLexer();

            string input = File.ReadAllText(@"..\..\..\input.txt");
            SList sres = ParseS(rulesLexer, input);
            Console.WriteLine(sres);

            FsmBuilder fsmBuilder = new FsmBuilder();
            var loadedLexer = new Fsm() { FirstState = fsmBuilder.BuildFsm(sres) };

            SList sres2 = ParseS(loadedLexer, input);
            Console.WriteLine(sres2);

            Console.ReadLine();
        }
    }
}
