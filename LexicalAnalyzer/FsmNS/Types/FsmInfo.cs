using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.FsmNS.Types
{
    public class FsmInfo<TState, TSymbol>
    {
        public TState[] States;
        public HashSet<int> FinalStates;
        public FsmTransition<TSymbol>[] Transitions;

        public FsmInfo(TState[] states, FsmTransition<TSymbol>[] transitions, HashSet<int> finalStates)
        {
            States = states;
            FinalStates = finalStates;
            Transitions = transitions;
        }
    }
}
