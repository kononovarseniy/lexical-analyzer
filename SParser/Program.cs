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
    class MyToken : Token
    {
        public string Text;
        public MyToken(Token token, string sourceString) : base(token)
        {
            Text = sourceString.Substring(token.Start, token.Length);
        }
        public override string ToString() => $"\"{Text}\"";
    }
    class StrToken : MyToken
    {
        public string Value = "";
        public StrToken(Token token, string sourceString) : base(token, sourceString)
        {
            Value = Text.Substring(1, token.Length - 2).Replace("\"\"", "\"");
        }
        public override string ToString() => $"\"{Text}\" value=\"{Value}\"";
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
                Token res;
                if (lex.Class == null)
                    throw new Exception();
                if (lex.Class == "str-atom")
                    res = new StrToken(lex, input);
                else
                    res = new MyToken(lex, input);
                var list = new List<Token>();
                if (lex.Class != "space")
                    list.Add(res);
                return list;
            });
            foreach (var tok in tokens)
            {
                Console.WriteLine($"{tok.Class ?? "ERROR",-20} = {tok}");
            }
            return new SList(SExpr.Parse(tokens.Select(t => t as MyToken)));
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
