using LexicalAnalyzer.FsmNS.Types;
using LexicalAnalyzer.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.FsmNS.Builders
{
    public class NfaBuilder<TState, TSymbol>
    {
        public delegate TState NewStateDelegate();
        public NewStateDelegate NewState { get; set; }

        public NfaBuilder(NewStateDelegate newState)
        {
            NewState = newState;
        }

        #region The base of construction of the FSM
        public FsmInfo<TState, TSymbol> Empty()
        {
            throw new NotImplementedException();
        }

        public FsmInfo<TState, TSymbol> Terminal(int terminal)
        {
            throw new NotImplementedException();
        }

        public FsmInfo<TState, TSymbol> Alternatives(FsmInfo<TState, TSymbol> a, FsmInfo<TState, TSymbol> b)
        {
            throw new NotImplementedException();
        }

        public FsmInfo<TState, TSymbol> Sequence(FsmInfo<TState, TSymbol> a, FsmInfo<TState, TSymbol> b)
        {
            throw new NotImplementedException();
        }

        public FsmInfo<TState, TSymbol> Iteration(FsmInfo<TState, TSymbol> a)
        {
            throw new NotImplementedException();
        }
        #endregion

        public FsmInfo<TState, TSymbol> Optional(FsmInfo<TState, TSymbol> a) =>
            Alternatives(a, Empty());

        public FsmInfo<TState, TSymbol> PositiveIteration(FsmInfo<TState, TSymbol> a) =>
            Sequence(a, Iteration(a));

        public FsmInfo<TState, TSymbol> Build(RegexTree tree)
        {
            throw new NotSupportedException();
        }
    }
}
