﻿using LexicalAnalyzer;
using LexicalAnalyzer.FsmNS.Builders;
using LexicalAnalyzer.FsmNS.Types;
using LexicalAnalyzer.RegularExpressions;
using LexicalAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        static void PrintRegexTree(RegexTree tree, int ident = 0)
        {
            string space = new string(' ', ident);
            Console.Write(space);
            if (tree.NodeType == RegexTreeNodeType.Value)
                Console.WriteLine(tree.Value);
            else
            {
                Console.WriteLine(tree.NodeType);
                PrintRegexTree(tree.Left, ident + 4);
                if (tree.NodeType == RegexTreeNodeType.Alternatives ||
                    tree.NodeType == RegexTreeNodeType.Sequence)
                {
                    PrintRegexTree(tree.Right, ident + 4);
                }
            }
        }
        static RegexTree ParseRegex(string pattern)
        {
            Console.WriteLine(pattern);
            var tree = RegexParser.Parse(pattern);
            PrintRegexTree(tree);
            return tree;
        }
        static void PrintFsm<TState, TSymbol>(FsmInfo<TState, TSymbol> fsm)
        {
            Console.WriteLine($"States[{fsm.States.Length}]:");
            for (int i = 0; i < fsm.States.Length; i++)
                Console.WriteLine($"    #{i}\t{fsm.States[i]}\tFinal: {fsm.FinalStates.Contains(i)}");
            Console.WriteLine($"Transitions[{fsm.Transitions.Length}]:");
            foreach (var t in fsm.Transitions)
                Console.WriteLine($"    {t}");
        }
        static FsmInfo<int, IntSet> BuildNfa(RegexTree tree)
        {
            int counter = 0;
            var builder = new NfaBuilder<int, IntSet>(() => counter++);
            var nfa = tree.ToFsm(builder);
            PrintFsm(nfa);
            return nfa;
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
            FsmStatus status = lexer.InitialStatus;
            for (int i = 0; i < lines.Length; i++)
            {
                status = lexLines[i].ExecuteAnalysis(status);
                Output(lexLines[i]);
                Console.WriteLine("---");
            }

            Console.WriteLine("Press enter to build nfa.");
            Console.ReadLine();

            var tree = ParseRegex("[0-9](a|(b*B)|c)?!+");
            var nfa = BuildNfa(tree);
            Console.ReadLine();
        }
    }
}
