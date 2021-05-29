using LexicalAnalyzer;
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

    class Lex
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
    }
    class Lexer3
    {
        public IEnumerable<Lex> Do(FsmInfo<string, int> dfa, AlphabetConverter converter, string input)
        {
            int[] inputTerm = input.Select(ch => converter.ConvertValue(ch)).ToArray();

            Lex currentLex = new Lex()
            {
                Start = 0,
                Length = 0,
                Name = null
            };
            int state = 0;
            for (int i = 0; i < inputTerm.Length; i++)
            {
                int term = inputTerm[i];
                int newState = dfa.Transitions.First(t => t.From == state && t.Symbol == term).To;
                if (dfa.States[newState] == "__error__")
                {
                    currentLex.Name = "__error__";
                    yield return currentLex;
                    state = 0;
                    currentLex = new Lex()
                    {
                        Start = i,
                        Length = 0,
                        Name = null
                    };
                }
                else
                {

                }
            }
        }
    }

    class FsmExporter
    {
        public static void ExportCsv<TState, TSymbol>(FsmInfo<TState, TSymbol> fsm, string edgesFile, string separator = ",")
        {
            using (var file = File.CreateText(edgesFile))
            {
                var allTransitions = from t in fsm.Transitions
                                     group t by new { t.From, t.To } into @group
                                     select new
                                     {
                                         From = @group.Key.From,
                                         To = @group.Key.To,
                                         Symbols = from t in @group
                                                   select t.HasSymbol ? t.Symbol.ToString() : "\u03BB" into t
                                                   group t by t into t
                                                   select t.Key
                                     } into t
                                     select new
                                     {
                                         From = t.From,
                                         To = t.To,
                                         Symbol = string.Join(",", t.Symbols)
                                     };

                file.WriteLine("nodedef>name VARCHAR,label VARCHAR,color VARCHAR");
                for (int i = 0; i < fsm.States.Length; i++)
                {
                    int r = 0, g = 0, b = 0;
                    string label = i.ToString();
                    if (i == 0)
                    {
                        g = 255;
                        label += ",FIRST";
                    }
                    if (fsm.FinalStates.Contains(i))
                    {
                        b = 255;
                        label += ",FINAL";
                    }
                    if (fsm.Transitions.Where(t => t.From == i).All(t => t.To == i))
                    {
                        r = 255;
                        label += ",!!!";
                    }
                    if (r == 0 && g == 0 && b == 0)
                    {
                        r = 100; g = 100; b = 100;
                    }
                    file.WriteLine($"{i},'{label}','{r},{g},{b}'");
                }
                file.WriteLine($"edgedef>node1 VARCHAR,node2 VARCHAR,label VARCHAR,directed BOOLEAN,color VARCHAR");
                foreach (var t in allTransitions)
                {
                    file.WriteLine($"{t.From},{t.To},'{t.Symbol}',true,'0,0,0'");
                }
            }
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
        static void PrintFsm<TSymbol>(FsmInfo<int[], TSymbol> fsm)
        {
            Console.WriteLine($"States[{fsm.States.Length}]:");
            for (int i = 0; i < fsm.States.Length; i++)
            {
                Console.WriteLine($"    #{i}\t{{{string.Join(", ", fsm.States[i])}}}\tFinal: {fsm.FinalStates.Contains(i)}");
            }
            Console.WriteLine($"Transitions[{fsm.Transitions.Length}]:");
            foreach (var t in fsm.Transitions)
                Console.WriteLine($"    {t}");
        }
        static FsmInfo<int[], IntSet> BuildNfa(RegexTree tree)
        {
            int counter = 0;
            var builder = new NfaBuilder<int[], IntSet>(() => new[] { counter++ });
            var nfa = tree.ToFsm(builder);
            PrintFsm(nfa);
            return nfa;
        }
        static FsmInfo<int[], int> BuildDfa(FsmInfo<int[], int> nfa, int alphabetLength)
        {
            var builder = new DfaBuilder<int[]>(
                alphabetLength,
                (s) => s.Aggregate(
                    Enumerable.Empty<int>(),
                    (acc, item) => acc.Concat(item),
                    (acc) => acc.ToArray()));

            Console.WriteLine("Building DFA from NFA...");
            var res = builder.Build(nfa);
            PrintFsm(res);
            return res;

            //// alphabetLength = 2;
            //var builder = new DfaBuilder<int[]>(
            //    alphabetLength,
            //    (s) => s.Aggregate(
            //        Enumerable.Empty<int>(),
            //        (acc, item) => acc.Concat(item),
            //        (acc) => acc.ToArray()));

            ////const int a = 0;
            ////const int b = 1;
            ////nfa = new FsmInfo<int[], int>(
            ////    states: Enumerable.Range(0, 4).Select(i => new int[] { i }).ToArray(),
            ////    transitions: new FsmTransition<int>[]
            ////    {
            ////        new FsmTransition<int>(0, 3, b),
            ////        new FsmTransition<int>(0, 1, a),
            ////        new FsmTransition<int>(0, 2),
            ////        new FsmTransition<int>(1, 3),
            ////        new FsmTransition<int>(2, 1, b),
            ////        new FsmTransition<int>(3, 1, a),
            ////        new FsmTransition<int>(3, 2)
            ////    },
            ////    finalStates: new HashSet<int>() { 3 });

            //Console.WriteLine("RemoveEmptyTransitions");
            //var tmp = builder.RemoveEmptyTransitions(nfa);
            //PrintFsm(tmp);
            /////////////////////////////
            ////tmp = new FsmInfo<int[], int>(
            ////    states: new int[][]
            ////    {
            ////        new[] { 0 },
            ////        new[] { 1 },
            ////        new[] { 2 }, // not exists
            ////        new[] { 3 }
            ////    },
            ////    transitions: new FsmTransition<int>[]
            ////    {
            ////        new FsmTransition<int>(0, 3, b),
            ////        new FsmTransition<int>(0, 1, a),
            ////        new FsmTransition<int>(0, 1, b),
            ////        new FsmTransition<int>(1, 1, a),
            ////        new FsmTransition<int>(1, 1, b),
            ////        new FsmTransition<int>(3, 1, a),
            ////        new FsmTransition<int>(3, 1, b)
            ////    },
            ////    finalStates: new HashSet<int>() { 1, 3 });

            //Console.WriteLine("Determinize");
            //var res = builder.Determinize(tmp);
            //PrintFsm(res);
            //return res;
        }
        
        static bool Execute(FsmInfo<int[], int> dfa, AlphabetConverter converter, string inputString)
        {
            int[] terminals = inputString
                .Select(ch => converter.ConvertValue(ch))
                .ToArray();

            int state = 0;
            for (int i = 0; i < terminals.Length; i++)
            {
                int term = terminals[i];
                
                state = dfa.Transitions.First(t => t.From == state && t.Symbol == term).To;
            }
            return dfa.FinalStates.Contains(state);
        }
        
        static void Test1()
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
        }

        static void TestRegexToDfa(string regex, string[] goodInput, string[] badInput, bool complement, string exportFile = null)
        {
            Console.WriteLine($"Regular expression: {regex}");
            // Parse regex
            RegexTree regexTree = RegexParser.Parse(regex);
            // Create NFA builder
            int stateIdCounter = 0;
            NfaBuilder<int[], IntSet> nfaBuilder =
                new NfaBuilder<int[], IntSet>(
                    () => new[] { stateIdCounter++ });
            // Build NFA from regexTree
            FsmInfo<int[], IntSet> nfa = regexTree.ToFsm(nfaBuilder);
            // Create alphabet converter
            AlphabetConverter converter = AlphabetConverter.Create(nfa);
            // Convert NFA alphabet
            FsmInfo<int[], int> convertedNfa = converter.ConvertFsm(nfa);
            // Create DFA builder
            DfaBuilder<int[]> dfaBuilder = new DfaBuilder<int[]>(
                converter.AlphabetLength,
                (s) => s.Aggregate(
                    Enumerable.Empty<int>(),
                    (acc, item) => acc.Concat(item),
                    (acc) => acc.ToArray()));
            // Build DFA from NFA
            FsmInfo<int[], int> dfa = dfaBuilder.Build(convertedNfa);
            if (complement)
            {
                dfa = DfaBuilder<int[]>.Complement(dfa);
            }
            if (exportFile != null)
            {
                FsmExporter.ExportCsv(dfa, exportFile, ",");
            }
            // Execute
            Console.WriteLine("Good input");
            foreach (var inputStr in goodInput)
            {
                Console.Write($@"{inputStr,-20}");
                if (Execute(dfa, converter, inputStr))
                {
                    Console.WriteLine("Sequence accepted!!!");
                }
                else
                {
                    Console.WriteLine("No match");
                }
            }
            Console.WriteLine("=====");
            Console.WriteLine("Bad input");
            foreach (var inputStr in badInput)
            {
                Console.Write($@"{inputStr,-20}");
                if (Execute(dfa, converter, inputStr))
                {
                    Console.WriteLine("Sequence accepted!!!");
                }
                else
                {
                    Console.WriteLine("No match");
                }
            }
            Console.WriteLine("=====");
        }

        static void Main(string[] args)
        {
            string regex;
            string[] goodInput, badInput;
            
            #region regex1
            regex = "[0-9](a|(b*B)|c|b*C)?!+";
            goodInput = new string[]
            {
                "4bB!",
                "4bbB!!",
                "5bB!",
                "4a!",
                "4c!",
                "4c!!",
                "4!!",
                "4!",
                "4bbC!!",
                "5bC!"
            };
            badInput = new string[]
            {
                "4bb!",
                "4bbB",
                "bB!",
                "4b!",
                "4Q!",
                "4c",
                "!!",
                ""
            };
            TestRegexToDfa(regex, goodInput, badInput, false, "regex1.gdf");
            TestRegexToDfa(regex, goodInput, badInput, true);
            #endregion

            #region ip regex
            string byteRegex = @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
            regex = $@"{byteRegex}\.{byteRegex}\.{byteRegex}(\.{byteRegex})";
            goodInput = new string[]
            {
                "1.2.3.4",
                "255.249.199.99",
                "127.0.0.1",
                "192.168.1.255",
                "123.123.123.123",
                "123.123.123.000",
            };
            badInput = new string[]
            {
                "1.2.3.999",
                "256.349.199.99",
                "127.0.0",
                "192.168.1111.111",
                "123.123.123.123.123",
            };
            TestRegexToDfa(regex, goodInput, badInput, false, "ip.gdf");
            TestRegexToDfa(regex, goodInput, badInput, true);
            #endregion

            //var tree = ParseRegex("[0-9](a|(b*B)|c|b*C)?!+");
            //var nfa = BuildNfa(tree);
            //var converter = AlphabetConverter.Create(nfa);
            //var convertedNfa = converter.ConvertFsm(nfa);
            //PrintFsm(convertedNfa);
            //var dfa = BuildDfa(convertedNfa, converter.AlphabetLength);
            //Execute(dfa, converter, "4bB!");

            Console.ReadLine();
        }
    }
}
