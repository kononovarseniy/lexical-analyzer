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
        public Func<TState> NewState { get; set; }

        public NfaBuilder(Func<TState> newState)
        {
            NewState = newState;
        }

        #region The base of construction of the FSM
        public FsmInfo<TState, TSymbol> Empty()
        {
            return new FsmInfo<TState, TSymbol>(
                states: new[] { NewState() },
                transitions: new FsmTransition<TSymbol>[] { },
                finalStates: new HashSet<int>() { 0 });
        }

        public FsmInfo<TState, TSymbol> Terminal(TSymbol terminal)
        {
            return new FsmInfo<TState, TSymbol>(
                states: new[] { NewState(), NewState() },
                transitions: new[] {
                    new FsmTransition<TSymbol>(0, 1, terminal)},
                finalStates: new HashSet<int>() { 1 });
        }

        public FsmInfo<TState, TSymbol> Alternatives(FsmInfo<TState, TSymbol> a, FsmInfo<TState, TSymbol> b)
        {
            const int newFirst = 0;
            int aFirst = 1;
            int bFirst = aFirst + a.States.Length;
            var states = new[] { NewState() }.Concat(a.States).Concat(b.States);

            var aFinalStates = a.FinalStates.Select(s => s + aFirst);
            var bFinalStates = b.FinalStates.Select(s => s + bFirst);

            var transitions =
                new[]
                {
                    new FsmTransition<TSymbol>(newFirst, aFirst),
                    new FsmTransition<TSymbol>(newFirst, bFirst)
                }
                .Concat(FsmTransition<TSymbol>.ShiftTransitions(a.Transitions, aFirst))
                .Concat(FsmTransition<TSymbol>.ShiftTransitions(b.Transitions, bFirst));

            var final = new HashSet<int>();
            final.UnionWith(aFinalStates);
            final.UnionWith(bFinalStates);

            return new FsmInfo<TState, TSymbol>(states.ToArray(), transitions.ToArray(), final);
        }

        public FsmInfo<TState, TSymbol> Sequence(FsmInfo<TState, TSymbol> a, FsmInfo<TState, TSymbol> b)
        {
            int aFirst = 0;
            int bFirst = aFirst + a.States.Length;
            var states = a.States.Concat(b.States);

            var aFinalStates = a.FinalStates.Select(s => s + aFirst);
            var bFinalStates = b.FinalStates.Select(s => s + bFirst);

            var transitions =
                FsmTransition<TSymbol>.CreateTransitions(aFinalStates, bFirst)
                .Concat(FsmTransition<TSymbol>.ShiftTransitions(a.Transitions, aFirst))
                .Concat(FsmTransition<TSymbol>.ShiftTransitions(b.Transitions, bFirst));
            
            var final = new HashSet<int>();
            final.UnionWith(bFinalStates);

            return new FsmInfo<TState, TSymbol>(states.ToArray(), transitions.ToArray(), final);
        }

        public FsmInfo<TState, TSymbol> Iteration(FsmInfo<TState, TSymbol> a)
        {
            int newFirst = 0;
            int newFinal = 1;
            int aFirst = 2;
            var states = new[] { NewState(), NewState() }.Concat(a.States);

            var aFinalStates = a.FinalStates.Select(s => s + aFirst);

            var transitions =
                new[]
                {
                    new FsmTransition<TSymbol>(newFirst, aFirst),
                    new FsmTransition<TSymbol>(newFirst, newFinal)
                }
                .Concat(FsmTransition<TSymbol>.CreateTransitions(aFinalStates, aFirst))
                .Concat(FsmTransition<TSymbol>.CreateTransitions(aFinalStates, newFinal))
                .Concat(FsmTransition<TSymbol>.ShiftTransitions(a.Transitions, aFirst));

            var final = new HashSet<int>() { newFinal };

            return new FsmInfo<TState, TSymbol>(states.ToArray(), transitions.ToArray(), final);
        }
        #endregion

        public FsmInfo<TState, TSymbol> Optional(FsmInfo<TState, TSymbol> a) =>
            Alternatives(a, Empty());

        public FsmInfo<TState, TSymbol> PositiveIteration(FsmInfo<TState, TSymbol> a) =>
            Sequence(a, Iteration(a));
    }
}
