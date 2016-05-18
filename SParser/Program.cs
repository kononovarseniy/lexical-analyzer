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
    class Program
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
            public StrToken(Token token, string sourceString) : base(token, sourceString) {
                Value = Text.Substring(1, token.Length - 2).Replace("\"\"", "\"");
            }
            public override string ToString() => $"\"{Text}\" value=\"{Value}\"";
        }
        static bool TryGetCurrent<T>(IEnumerator<T> en, out T result) where T : class
        {
            result = null;
            try
            {
                result = en.Current;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            return result != null;
        }
        static IEnumerable<SExpr> ParseSExprImpl(IEnumerator<MyToken> input, bool isTopLevel = true)
        {
            while (true)
            {
                if (!input.MoveNext())
                {
                    if (isTopLevel) yield break;
                    else throw new ArgumentException();
                }
                MyToken current = input.Current;
                if (!isTopLevel && current.Class == "closing-bracket")
                    yield break;
                else if (current.Class == "atom")
                    yield return new SAtom(current.Text);
                else if (current.Class == "str-atom")
                    yield return new SString(((StrToken)current).Value);
                else if (current.Class == "opening-bracket")
                    yield return new SList(ParseSExprImpl(input, false));
            }
        }
        static IEnumerable<SExpr> ParseSExpr(IEnumerable<MyToken> input)
        {
            var en = input.GetEnumerator();
            return ParseSExprImpl(en);
        }

        static void Main(string[] args)
        {
            Fsm lexer = new Fsm();

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
            lexer.FirstState = first;
            string input = File.ReadAllText(@"..\..\..\input.txt");
            var output = Fsm.GetTokens(lexer.GetLexemes(input), (lex) =>
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
            /*foreach (var tok in output)
            {
                Console.WriteLine($"{tok.Class ?? "ERROR",-20} = {tok}");
            }*/
            SList sres = new SList(ParseSExpr(output.Select(t => t as MyToken)));
            Console.WriteLine(sres);
            Console.ReadLine();
        }
    }
}
