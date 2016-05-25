using LexicalAnalyzer;
using System;
using System.Collections.Generic;
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
        static void Main(string[] args)
        {
            // Load file
            string input = File.ReadAllText(@"..\..\..\input.txt");
            // Create lexer from rules
            var loadedLexer = FsmCreator.CreateFsm(input);
            // Get lexemes and evaluate them
            var tokens = loadedLexer.GetLexemesAndEvaluate(input, (lex) =>
            {
                Lexeme res = null;
                if (lex.Class == "space")
                    res = null;
                else if (lex.Class == "str-atom")
                    res = lex.ToToken<SStrToken>(input);
                else if (lex.Class != null)
                    res = lex.ToToken<SToken>(input);
                else
                    throw new Exception();
                var list = new List<Lexeme>();
                if (res != null) list.Add(res);
                return list;
            });
            // Console output
            foreach (var tok in tokens)
            {
                Console.Write($"{tok.Class ?? "ERROR",-20} = {(tok as Token).Text,-20}");
                if (tok is SStrToken)
                    Console.Write($"string: {(tok as SStrToken).Value,-20}");
                Console.WriteLine();
            }
            Console.ReadLine();
        }
    }
}
