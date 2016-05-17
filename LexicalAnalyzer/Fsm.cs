using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public class FsmState : IEnumerable<KeyValuePair<string, FsmState>>
    {
        public string LexemeClass = null;
        public List<KeyValuePair<string, FsmState>> Transitions = new List<KeyValuePair<string, FsmState>>();

        public FsmState(string lexClass = null)
        {
            LexemeClass = lexClass;
        }

        public bool ContainsState(char ch)
        {
            FsmState unused;
            return TryGetState(ch, out unused);
        }

        public bool TryGetState(char ch, out FsmState state)
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
        
        public void Add(string key, FsmState value)
        {
            Transitions.Add(new KeyValuePair<string, FsmState>(key, value));
        }

        public IEnumerator<KeyValuePair<string, FsmState>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, FsmState>>)Transitions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, FsmState>>)Transitions).GetEnumerator();
        }
    }
    public class Token
    {
        public int Start;
        public int Length;
        public string Class;
        public Token(int start = 0)
        {
            Start = start;
            Length = 0;
            Class = null;
        }
        public Token(Token token)
        {
            CopyFrom(token);
        }
        public void CopyFrom(Token token)
        {
            Start = token.Start;
            Length = token.Length;
            Class = token.Class;
        }
    }
    public class Fsm
    {
        public FsmState FirstState;

        public IEnumerable<Token> GetLexemes(IEnumerable<char> input)
        {
            IEnumerator<char> en = input.GetEnumerator();
            int pos = 0;
            Token lex = new Token();
            FsmState state = FirstState;
            bool doNotMove = false;
            while (doNotMove || en.MoveNext())
            {
                doNotMove = false;
                char ch = en.Current;
                if (state.TryGetState(ch, out state))
                {
                    if (state.LexemeClass != null)
                        lex.Class = state.LexemeClass;
                }
                else
                {
                    state = FirstState;
                    if (lex.Class != null)
                        doNotMove = true;
                    else
                    {
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
                    lex = new Token(pos);
                    continue;
                }
                pos++;
            }
            int len = pos - lex.Start;
            if (len != 0)
            {
                lex.Length = len;
                yield return lex;
            }
        }

        public static IEnumerable<Token> GetTokens(IEnumerable<Token> lexemes, Func<Token, IEnumerable<Token>> evaluator)
        {
            foreach (var lex in lexemes)
                foreach (var tok in evaluator(lex))
                    yield return tok;
        }
    }
}
