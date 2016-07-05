﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public delegate IEnumerable<Lexeme> Evaluator(Lexeme lexeme);
    public class Block : IEnumerable<Lexeme>
    {
        private readonly List<Lexeme> Lexemes;

        public FsmStatus StartStatus { get; private set; }
        public FsmStatus EndStatus { get; private set; }
        public IEnumerable<char> Input { get; private set; }
        public int Position { get; private set; }
        public int BlockLength { get; private set; }
        public Evaluator Evaluator { get; private set; }
        public bool Analysed { get; private set; }

        public Block(FsmStatus status, IEnumerable<char> input, int blockLength, Evaluator evaluator = null)
        {
            if (status == null)
                throw new ArgumentNullException(nameof(status));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            StartStatus = status.Clone();
            EndStatus = null;
            Input = input;
            Position = status.Position;
            BlockLength = blockLength;
            Evaluator = evaluator;
            Analysed = false;

            Lexemes = new List<Lexeme>();
        }

        public void ExecuteAnalysis()
        {
            EndStatus = StartStatus.Clone();

            int countdown = BlockLength;
            foreach (var ch in Input)
            {
                if (countdown-- == 0) break;
                Lexeme lex = Fsm.HandleChar(ch, EndStatus);
                if (lex != null)
                {
                    if (Evaluator != null)
                        Lexemes.AddRange(Evaluator(lex));
                    else
                        Lexemes.Add(lex);
                }
                EndStatus.Position++;
            }
            if (EndStatus.Lexeme.Length != 0)
            {
                Lexeme lex = EndStatus.Lexeme.Clone();

                if (Evaluator != null)
                    Lexemes.AddRange(Evaluator(lex));
                else
                    Lexemes.Add(lex);
            }
            Analysed = true;
        }

        public Lexeme this[int index]
        {
            get
            {
                return Lexemes[index];
            }
        }
        
        public int Count
        {
            get
            {
                return Lexemes.Count;
            }
        }
        
        public IEnumerator<Lexeme> GetEnumerator()
        {
            return Lexemes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Lexemes.GetEnumerator();
        }
    }

    public class FsmStatus
    {
        public Fsm Machine { get; private set; }
        public FsmNode Node { get; set; }
        public Lexeme Lexeme { get; set; }
        public bool ErrorFlag { get; set; }
        public int Position { get; set; }

        private FsmStatus() { }
        public FsmStatus(Fsm machine, int position = 0)
        {
            Machine = machine;
            Node = machine.FirstState;
            Position = position;
            Lexeme = new Lexeme();
        }
        
        public void Reset()
        {
            Node = Machine.FirstState;
            Lexeme = new Lexeme(Position);
            ErrorFlag = false;
        }

        public FsmStatus Clone() => new FsmStatus()
        {
            Machine = Machine,
            Node = Node,
            Lexeme = Lexeme,
            ErrorFlag = ErrorFlag,
            Position = Position
        };
    }

    public class FsmNode : IEnumerable<KeyValuePair<string, FsmNode>>
    {
        public string LexemeClass = null;
        public bool Final = false;
        public List<KeyValuePair<string, FsmNode>> Transitions = new List<KeyValuePair<string, FsmNode>>();

        public FsmNode(string lexClass, bool final)
        {
            LexemeClass = lexClass;
            Final = final;
        }
        public FsmNode(string lexClass) : this(lexClass, false) { }
        public FsmNode(bool final) : this(null, final) { }
        public FsmNode() : this(null, false) { }

        public bool ContainsState(char ch)
        {
            FsmNode unused;
            return TryGetState(ch, out unused);
        }

        public bool TryGetState(char ch, out FsmNode state)
        {
            state = null;
            foreach (var tr in Transitions)
                if (OneCharRegex.IsMatch(ch, tr.Key))
                {
                    state = tr.Value;
                    return true;
                }
            return false;
        }
        
        public void Add(string key, FsmNode value)
        {
            Transitions.Add(new KeyValuePair<string, FsmNode>(key, value));
        }

        public IEnumerator<KeyValuePair<string, FsmNode>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, FsmNode>>)Transitions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, FsmNode>>)Transitions).GetEnumerator();
        }
    }
    public class Fsm
    {
        public FsmNode FirstState;

        [Obsolete]
        public IEnumerable<Lexeme> GetLexemes(IEnumerable<char> input)
        {
            IEnumerator<char> en = input.GetEnumerator();
            int pos = 0;
            Lexeme lex = new Lexeme();
            bool complited = false;
            FsmNode state = FirstState;
            bool doNotMove = false;
            while (doNotMove || en.MoveNext())
            {
                doNotMove = false;
                char ch = en.Current;
                if (state.TryGetState(ch, out state))
                {
                    if (state.LexemeClass != null)
                        lex.Class = state.LexemeClass;
                    complited = state.Final;
                }
                else
                {
                    state = FirstState;
                    if (lex.Class != null && complited)
                        doNotMove = true;
                    else
                    {
                        lex.Class = null;
                        do
                        {
                            if (state.ContainsState(en.Current))
                            {
                                doNotMove = true;
                                break;
                            }
                            pos++;
                        } while (en.MoveNext());
                    }
                    lex.Length = pos - lex.Start;
                    yield return lex;
                    lex = new Lexeme(pos);
                    complited = false;
                    continue;
                }
                pos++;
            }
            int len = pos - lex.Start;
            if (len != 0)
            {
                lex.Class = complited ? lex.Class : null;
                lex.Length = len;
                yield return lex;
            }
        }

        [Obsolete]
        // TODO: Evaluate method.
        public static IEnumerable<Lexeme> Evaluate(IEnumerable<Lexeme> lexemes, Evaluator evaluator)
        {
            foreach (var lex in lexemes)
                foreach (var tok in evaluator(lex))
                    yield return tok;
        }

        [Obsolete]
        // TODO: GetLexemesAndEvaluate method.
        public IEnumerable<Lexeme> GetLexemesAndEvaluate(IEnumerable<char> input, Evaluator evaluator) =>
            Evaluate(GetLexemes(input), evaluator);

        public static Lexeme HandleChar(char input, FsmStatus status)
        {
            Lexeme res = HandleCharImpl(input, status);
            if (res != null)
            {
                status.Reset();
                HandleCharImpl(input, status);
            }
            return res;
        }
        // При возвращении результата отличного от null
        // вызывающй обязан сбросить параметры: node, lex, errorFlag
        // и повторить вызов HandleCharImpl при этом вернет null.
        private static Lexeme HandleCharImpl(char input, FsmStatus status)
        {
            FsmNode nextNode;
            // Если существует переход из текущего состояния.
            if (status.Node.TryGetState(input, out nextNode))
            {
                // Если до этого накапливалась ошибка сбросить накопленное.
                // (Вместе с установкой errorFlag сбрасывается состояние)
                if (status.ErrorFlag)
                {
                    return status.Lexeme;
                }
                // Иначе продолжить накопление лексемы.
                // И перейти в следующее состойние.
                else
                {
                    status.Lexeme.Length++;
                    if (nextNode.LexemeClass != null)
                        status.Lexeme.Class = nextNode.LexemeClass;
                    status.Lexeme.Complited = nextNode.Final;
                    status.Node = nextNode;
                }
            }
            // Если перехода не существует.
            else
            {
                // Если лексема новая
                // или ошибка уже накапливается.
                if (status.Lexeme.Length == 0 || status.ErrorFlag)
                {
                    status.ErrorFlag = true;
                    status.Lexeme.Length++;
                }
                // Если начато накопление лексемы
                // и лексема не "ошибка".
                else
                {
                    return status.Lexeme;
                }
            }
            return null;
        }
    }
}
