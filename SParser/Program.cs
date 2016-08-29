﻿using LexicalAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;

namespace SParser
{
    sealed class SToken : Token { }
    sealed class SError : Token { }
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
        static void Output(IEnumerable<Lexeme> tokens)
        {
            foreach (var tok in tokens)
            {
                Console.Write($"{tok.Class ?? "ERROR",-20} = {(tok as Token).Text,-20}");
                if (tok is SStrToken)
                    Console.Write($"string: {(tok as SStrToken).Value,-20}");
                Console.WriteLine();
            }
        }
        static string[] GetLines(string str)
        {

            List<string> lines = new List<string>();
            string nl = Environment.NewLine;

            int prev = 0;
            int pos = str.IndexOf(nl);
            while (pos != -1)
            {
                lines.Add(str.Substring(prev, pos - prev + nl.Length));
                prev = pos + nl.Length;
                pos = str.IndexOf(nl, prev);
            }
            if (prev < str.Length - 1)
            {
                lines.Add(str.Substring(prev));
            }
            return lines.ToArray();
        }
        static void Main(string[] args)
        {
            // Load file
            string input = File.ReadAllText(@"..\..\..\input2.txt");
            // Create lexer from rules
            var lexer = FsmCreator.CreateFsm(input);
            Evaluator evaluator = (lex) =>
            {
                Lexeme res = null;
                if (lex.Class == "space")
                    res = null;
                else if (lex.Class == "str-atom")
                    res = lex.ToToken<SStrToken>(input);
                else if (lex.Class != null)
                    res = lex.ToToken<SToken>(input);
                else
                    res = lex.ToToken<SError>(input);
                var list = new List<Lexeme>();
                if (res != null) list.Add(res);
                return list;
            };
            // And analyze it
            var block = new LexerBlock(lexer, input, evaluator);
            block.ExecuteAnalysis();
            Output(block);
            Console.WriteLine("========");
            Console.WriteLine("========");
            Console.WriteLine("========");

            input =
@"(list
    aaa
    2
    3
    ""asd
    zxc""
) aaa";

            string[] lines = GetLines(input);
            // Create blocks
            LexerBlock[] lexLines = new LexerBlock[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                lexLines[i] = new LexerBlock(lexer, lines[i], evaluator);
            }
            // Execute analisis
            FsmStatus status = new FsmStatus(lexer);
            for (int i = 0; i < lines.Length; i++)
            {
                status = lexLines[i].ExecuteAnalysis(status);
                Output(lexLines[i]);
                Console.WriteLine("---");
            }
            Console.ReadLine();
        }
    }
}
