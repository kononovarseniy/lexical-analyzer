using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public class LexerBlock : IEnumerable<Lexeme>
    {
        private List<Lexeme> Lexemes;
        public Fsm Lexer { get; private set; }
        public IEnumerable<char> Input { get; private set; }
        public FsmStatus EndStatus { get; private set; }
        public Evaluator Evaluator { get; private set; }

        public LexerBlock(Fsm lexer, IEnumerable<char> input, Evaluator evaluator = null)
        {
            if (lexer == null)
                throw new ArgumentNullException(nameof(lexer));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            Lexer = lexer;
            Input = input;
            EndStatus = null;
            Evaluator = evaluator;
        }

        public FsmStatus ExecuteAnalysis() =>
            ExecuteAnalysis(Lexer.InitialStatus);

        public FsmStatus ExecuteAnalysis(FsmStatus startStatus)
        {
            EndStatus = startStatus.Clone();
            var lexemesTemp = Lexer.GetLexemes(Input, EndStatus);
            Lexemes = new List<Lexeme>();
            if (Evaluator != null)
            {
                foreach (var lex in lexemesTemp)
                    foreach (var tok in Evaluator(lex))
                        Lexemes.Add(tok);
            }
            else
            {
                Lexemes = lexemesTemp.ToList();
            }
            return EndStatus;
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

    public delegate IEnumerable<Lexeme> Evaluator(Lexeme lexeme);

    public class FsmStatus
    {
        public FsmNode InitialNode { get; private set; }
        public FsmNode Node { get; set; }
        public Lexeme Lexeme { get; set; }
        public bool ErrorFlag { get; set; }
        public int Position { get; set; }

        private FsmStatus() { }
        public FsmStatus(FsmNode initialNode, int position = 0)
        {
            InitialNode = initialNode;
            Node = initialNode;
            Position = position;
            Lexeme = new Lexeme();
        }
        
        public void Reset()
        {
            Node = InitialNode;
            Lexeme = new Lexeme(Position);
            ErrorFlag = false;
        }

        public FsmStatus Clone() => new FsmStatus()
        {
            InitialNode = InitialNode,
            Node = Node,
            Lexeme = Lexeme.Clone(),
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
        public FsmNode FirstNode;

        public FsmStatus InitialStatus => new FsmStatus(FirstNode);
        
        public IEnumerable<Lexeme> GetLexemes(IEnumerable<char> input, FsmStatus status)
        {
            foreach (var ch in input)
            {
                Lexeme lex = HandleChar(ch, status);
                if (lex != null)
                {
                    yield return lex;
                }
                status.Position++;
            }
            if (status.Lexeme.Length != 0)
            {
                Lexeme lex = status.Lexeme.Clone();
                yield return lex;
            }
        }

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
