using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalyzer;

namespace SParser
{
    class Program
    {
        class MyToken : Token
        {
            public string Text;
            public MyToken(Token token, string text) : base(token)
            {
                Text = text;
            }
        }
        static void Main(string[] args)
        {
            Fsm lexer = new Fsm();

            FsmState space = new FsmState("space");
            space.Add(@"\s", space);
            FsmState atom = new FsmState("atom");
            atom.Add(@"[^\s()]", atom);
            FsmState first = new FsmState() {
                                 { @"(", new FsmState("opening bracket") },
                                 { @")", new FsmState("closing bracket") },
                                 { @"[^\s()]", atom },
                                 { @"\s", space }
                             };
            lexer.FirstState = first;
            string input = "(asd   zxc(asd ((asd)) (()  )))";
            var output = Fsm.GetTokens(lexer.GetLexemes(input), (lex) =>
            {
                Token res = new MyToken(lex,
                    input.Substring(lex.Start, lex.Length));
                return new List<Token>() { res };
            });
            foreach (var tok in output)
            {
                Console.WriteLine($"{tok.Class ?? "ERROR",-20} = \"{((MyToken)tok).Text}\"");
            }
            Console.ReadLine();
        }
    }
}
