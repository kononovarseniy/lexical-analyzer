using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.RegularExpressions
{
    class FsmTransition
    {
        public int From { get; private set; }
        public int To { get; private set; }
        public int? Symbol { get; private set; }

        public bool IsEmpty => !Symbol.HasValue;

        public FsmTransition(int from, int to, int? symbol)
        {
            From = from;
            To = to;
            Symbol = symbol;
        }
    }
    struct Fsm<TState>
    {
        public TState[] States; // Состояния
        public HashSet<int> FinalStates; // Заключительные состояния
        public FsmTransition[] Transitions; // Переходы

        public Fsm(TState[] states, HashSet<int> finalStates, FsmTransition[] transitions)
        {
            States = states;
            FinalStates = finalStates;
            Transitions = transitions;
        }
    }
    class RegularGrammarFsmBuilder<TState>
    {
        public int TerminalsCount { get; private set; }

        private Func<TState> newState;
        private Func<TState[], TState> mergeStates;

        private HashSet<Fsm<TState>> children = new HashSet<Fsm<TState>>();

        public RegularGrammarFsmBuilder(
            int terminalsCount,
            Func<TState> newState,
            Func<TState[], TState> mergeStates)
        {
            TerminalsCount = terminalsCount;
            this.newState = newState;
            this.mergeStates = mergeStates;
        }

        #region Helpers
        private static void Copy<T>(IEnumerable<T> source, T[] destination, int offset)
        {
            int i = offset;
            foreach (var item in source) destination[i++] = item;
        }

        private static void CopyStates(IEnumerable<TState> source, TState[] destination, int offset)
        {
            Copy(source, destination, offset);
        }

        private static void CopyTransitions(IEnumerable<FsmTransition> source, int stateShift, FsmTransition[] destination, int offset)
        {
            var en = from t in source
                     select new FsmTransition(
                         t.From + stateShift,
                         t.To + stateShift,
                         t.Symbol);
            Copy(en, destination, offset);
        }

        private void CheckFsm(Fsm<TState> fsm, string argName)
        {
            if (!children.Contains(fsm))
                throw new InvalidOperationException($"'{argName}' belongs to another builder.");
        }

        private Fsm<TState> CreateFsm(TState[] states, HashSet<int> finalStates, FsmTransition[] transitions)
        {
            var fsm = new Fsm<TState>(states, finalStates, transitions);
            children.Add(fsm);
            return fsm;
        }
        #endregion

        #region The base of construction of the FSM
        public Fsm<TState> Empty()
        {
            return CreateFsm(
                states:  new TState[] { newState() },
                finalStates: new HashSet<int>() { 0 },
                transitions: new FsmTransition[] { }
            );
        }

        public Fsm<TState> Terminal(int terminal)
        {
            if (terminal > TerminalsCount)
                throw new ArgumentOutOfRangeException();

            return CreateFsm(
                states: new TState[] { newState(), newState() },
                finalStates: new HashSet<int>() { 1 },
                transitions: new FsmTransition[] { new FsmTransition(0, 1, terminal) }
            );
        }

        public Fsm<TState> Alternatives(Fsm<TState> a, Fsm<TState> b)
        {
            CheckFsm(a, nameof(a));
            CheckFsm(b, nameof(b));

            const int newFirst = 0;
            const int newStatesCount = 1;
            int aFirst = newStatesCount;
            int bFirst = aFirst + a.States.Length;

            TState[] states = new TState[newStatesCount + a.States.Length + b.States.Length];
            states[newFirst] = newState();
            CopyStates(a.States, states, aFirst);
            CopyStates(b.States, states, bFirst);

            const int newTransitionsCount = 2;
            int aTransitionsOffset = newTransitionsCount;
            int bTransitionsOffset = aTransitionsOffset + a.Transitions.Length;

            FsmTransition[] transitions = new FsmTransition[newTransitionsCount + a.Transitions.Length + b.Transitions.Length];
            transitions[0] = new FsmTransition(newFirst, aFirst, null);
            transitions[1] = new FsmTransition(newFirst, bFirst, null);
            CopyTransitions(a.Transitions, aFirst, transitions, aTransitionsOffset);
            CopyTransitions(b.Transitions, bFirst, transitions, bTransitionsOffset);

            HashSet<int> final = new HashSet<int>();
            final.UnionWith(a.FinalStates);
            final.UnionWith(b.FinalStates);

            return CreateFsm(states, final, transitions);
        }

        public Fsm<TState> Sequence(Fsm<TState> a, Fsm<TState> b)
        {
            CheckFsm(a, nameof(a));
            CheckFsm(b, nameof(b));

            const int newStatesCount = 0;
            int aStatesOffset = newStatesCount;
            int bStatesOffset = aStatesOffset + a.States.Length;

            TState[] states = new TState[newStatesCount + a.States.Length + b.States.Length];
            CopyStates(a.States, states, aStatesOffset);
            CopyStates(b.States, states, bStatesOffset);

            int newTransitionsCount = a.FinalStates.Count;
            int aTransitionsOffset = newTransitionsCount;
            int bTransitionsOffset = aTransitionsOffset + a.Transitions.Length;
            var newTransitions = from fin in a.FinalStates
                                 select new FsmTransition(fin, bStatesOffset, null);

            FsmTransition[] transitions = new FsmTransition[newTransitionsCount + a.Transitions.Length + b.Transitions.Length];
            CopyTransitions(newTransitions, 0, transitions, 0);
            CopyTransitions(a.Transitions, aStatesOffset, transitions, aTransitionsOffset);
            CopyTransitions(b.Transitions, bStatesOffset, transitions, bTransitionsOffset);

            HashSet<int> final = new HashSet<int>();
            final.UnionWith(b.FinalStates);

            return CreateFsm(states, final, transitions);
        }

        public Fsm<TState> Iteration(Fsm<TState> a)
        {
            CheckFsm(a, nameof(a));

            const int newFirst = 0;
            const int newFinal = 1;
            const int newStatesCount = 2;
            int aFirst = newStatesCount;

            TState[] states = new TState[newStatesCount + a.States.Length];
            states[newFirst] = newState();
            states[newFinal] = newState();
            CopyStates(a.States, states, aFirst);

            int newTransitionsCount = 2 + a.FinalStates.Count * 2;
            int aTransitionsOffset = newTransitionsCount;
            var newTransitions = Enumerable.Concat(
                from fin in a.FinalStates
                select new FsmTransition(fin, newFinal, null),
                from fin in a.FinalStates
                select new FsmTransition(fin, aFirst, null));

            FsmTransition[] transitions = new FsmTransition[newTransitionsCount + a.Transitions.Length];
            transitions[0] = new FsmTransition(newFirst, aFirst, null);
            transitions[1] = new FsmTransition(newFirst, newFinal, null);
            CopyTransitions(newTransitions, 0, transitions, 2);
            CopyTransitions(a.Transitions, aFirst, transitions, aTransitionsOffset);

            HashSet<int> final = new HashSet<int>() { newFinal };

            return CreateFsm(states, final, transitions);
        }
        #endregion

        public Fsm<TState> Optional(Fsm<TState> a) =>
            Alternatives(a, Empty());

        public Fsm<TState> PositiveIteration(Fsm<TState> a) =>
            Sequence(a, Iteration(a));

        public Fsm<TState> BuildDfsm(Fsm<TState> fsm)
        {
            throw new NotImplementedException();
            //CheckFsm(fsm, nameof(fsm));

            //return CreateFsm(states, final, transitions);
        }

        public Fsm<TState> MinimizeDfsm(Fsm<TState> fsm)
        {
            throw new NotImplementedException();
            //CheckFsm(fsm, nameof(fsm));

            //return CreateFsm(states, final, transitions);
        }
    }
}
