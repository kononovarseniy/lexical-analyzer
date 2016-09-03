using LexicalAnalyzer.FsmNS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.FsmNS.Builders
{
    public class DfaBuilder<TState>
    {
        public delegate TState MergeStatesDelegate(TState[] states);
        public MergeStatesDelegate MergeStates { get; set; }

        public int TerminalAlphabetLength { get; private set; }

        public DfaBuilder(int length, MergeStatesDelegate mergeStates)
        {
            TerminalAlphabetLength = length;
            MergeStates = mergeStates;
        }

        public FsmInfo<TState, int> Build(FsmInfo<TState, int> fsm)
        {
            throw new NotImplementedException();
        }

        public FsmInfo<TState, int> Minimize(FsmInfo<TState, int> fsm)
        {
            throw new NotImplementedException();
        }
    }
}
