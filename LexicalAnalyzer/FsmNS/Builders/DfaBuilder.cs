using LexicalAnalyzer.FsmNS.Types;
using LexicalAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.FsmNS.Builders
{
    /// <summary>
    /// Class for building deterministic finite automaton from nondeterministic.
    /// </summary>
    public class DfaBuilder<TState>
    {
        public delegate TState MergeStatesDelegate(TState[] states);
        public MergeStatesDelegate MergeStates { get; set; }

        public int TerminalAlphabetLength { get; private set; }

        public DfaBuilder(int alphabetLength, MergeStatesDelegate mergeStates)
        {
            TerminalAlphabetLength = alphabetLength;
            MergeStates = mergeStates;
        }

        private FsmInfo<TState, int> RemoveEmptyTransitions(FsmInfo<TState, int> fsm)
        {
            /*
             * first state always available
             * foreach transition
             *    mark transition.To as available
             * foreach fsm.FinalStates
             *    mark state as finsl
             *    
             * foreach transition
             *    if transition has symbol
             *        add transition to new fsm (convert states)
             *    else
             *        merge states:
             *            if transition leads to final state
             *                mark state transition.From as final
             *            add all transitions that leads from trnasiton.To
             *            call merge states
             * 
             * remove unavailable states (create state conversion map)
             * */

            bool[] isAvailable = new bool[fsm.States.Length];
            bool[] isFinal = new bool[fsm.States.Length];
            TState[] states = new TState[fsm.States.Length];
            fsm.States.CopyTo(states, 0);

            Queue<FsmTransition<int>> transitions = new Queue<FsmTransition<int>>(fsm.Transitions);
            List<FsmTransition<int>> ignoredTransitions = new List<FsmTransition<int>>();
            List<FsmTransition<int>> resultTransitions = new List<FsmTransition<int>>();

            // Mark availablestates
            isAvailable[0] = true;
            foreach (var t in transitions)
            {
                if (t.HasSymbol)
                {
                    isAvailable[t.To] = true;
                }
                else
                {
                    // Merge states to save additional information
                    states[t.From] = MergeStates(new[] { states[t.From], states[t.To] });
                }
            }
            // Mark final states
            foreach (var s in fsm.FinalStates)
            {
                isFinal[s] = true;
            }
            
            while (transitions.Count > 0)
            {
                var t = transitions.Dequeue();
                if (t.HasSymbol)
                {
                    if (isAvailable[t.From])
                    {
                        // Save transition if it has symbol and leads from available state
                        resultTransitions.Add(t);
                    }
                    else
                    {
                        ignoredTransitions.Add(t);
                    }
                }
                else
                {
                    isFinal[t.From] |= isFinal[t.To];
                    // Copy all transitions from state 't.To' to state 't.From'.
                    // Copy means add them to queue to handle later.
                    transitions.Concat(ignoredTransitions)
                        .Where(t2 => t2.From == t.To)
                        .Select(t2 => t2.SetFrom(t.From))
                        .ToList()
                        .ForEach(t2 => transitions.Enqueue(t2));
                }
            }

            // Remove unavailable states
            int[] oldToNew;
            int availableCount = CreateStateConversionMap(isAvailable, out oldToNew);

            TState[] newStates = new TState[availableCount];
            HashSet<int> newFinal = new HashSet<int>();
            FsmTransition<int>[] newTransitions = new FsmTransition<int>[resultTransitions.Count];
            for (int i = 0; i < states.Length; i++)
            {
                int newIndex = oldToNew[i];
                if (newIndex != -1)
                {
                    newStates[newIndex] = states[i];
                    if (isFinal[i])
                    {
                        newFinal.Add(oldToNew[i]);
                    }
                }
            }
            for (int i = 0; i < resultTransitions.Count; i++)
            {
                var t = resultTransitions[i];
                newTransitions[i] = t.SetFromAndTo(oldToNew[t.From], oldToNew[t.To]);
            }

            return new FsmInfo<TState, int>(
                states: newStates,
                transitions: newTransitions,
                finalStates: newFinal);
        }
        
        private static int CreateStateConversionMap(bool[] isAvailable, out int[] oldToNew)
        {
            int availableCount = isAvailable.Count(a => a);
            oldToNew = new int[isAvailable.Length];
            int newIndex = 0;
            for (int oldIndex = 0; oldIndex < oldToNew.Length; oldIndex++)
            {
                if (isAvailable[oldIndex])
                {
                    oldToNew[oldIndex] = newIndex;
                    newIndex++;
                }
                else
                {
                    oldToNew[oldIndex] = -1;
                }
            }
            return availableCount;
        }

        private FsmInfo<TState, int> Determinize(FsmInfo<TState, int> fsm)
        {
            /*
             * HashSet handled multiStates
             * Queue multiStatesToHandle
             * 
             * while queue is not empty take next multiState
             *     construct new State from multiState (call merge states)
             *     add constructed state to result
             *     foreach symbol in alphabet
             *         get set of states that are acessible from current state by current symbol (newMultiState)
             *         if newMultiState stil not handled
             *             add newMultiState to queue
             *         add transition by symbol from current multiState to newMultiState
             * ===
             * multiState = HashSet<int>
             * */
            
            Dictionary<HashSet<int>, int> stateIndecies =
                new Dictionary<HashSet<int>, int>(
                    new HashSetEqualityComparer<int>());
            Queue<HashSet<int>> multiStatesToHandle =
                new Queue<HashSet<int>>();

            int indexCounter = 0;
            // Add first state
            var firstState = new HashSet<int>() { 0 };
            multiStatesToHandle.Enqueue(firstState);
            stateIndecies.Add(firstState, indexCounter++);

            List<TState> resultStates = new List<TState>();
            HashSet<int> resultFinal = new HashSet<int>();
            List<FsmTransition<int>> resultTransitions = new List<FsmTransition<int>>();

            while (multiStatesToHandle.Count() > 0)
            {
                var multiState = multiStatesToHandle.Dequeue();
                int multiStateIndex = stateIndecies[multiState];

                // merge multiState
                var mergedState = MergeStates(multiState.Select(s => fsm.States[s]).ToArray());
                bool isFinal = multiState.Any(s => fsm.FinalStates.Contains(s));
                // Item will be placed at multiStateIndex
                resultStates.Add(mergedState);
                if (isFinal)
                {
                    resultFinal.Add(multiStateIndex);
                }
                // create transitions
                for (int symbol = 0; symbol < TerminalAlphabetLength; symbol++)
                {
                    var newMultiState = new HashSet<int>(multiState.Aggregate(
                        Enumerable.Empty<int>(),
                        (accu, state) =>
                        {
                            return accu.Concat(from t in fsm.Transitions
                                               where t.From == state && t.Symbol == symbol
                                               select t.To);
                        }));
                    int newIndex;
                    if (!stateIndecies.TryGetValue(newMultiState, out newIndex))
                    {
                        newIndex = indexCounter++;
                        stateIndecies.Add(newMultiState, newIndex);
                        multiStatesToHandle.Enqueue(newMultiState);
                    }
                    resultTransitions.Add(new FsmTransition<int>(multiStateIndex, newIndex, symbol));
                }
            }

            return new FsmInfo<TState, int>(
                resultStates.ToArray(),
                resultTransitions.ToArray(),
                resultFinal);
        }
        
        public FsmInfo<TState, int> Build(FsmInfo<TState, int> fsm)
        {
            return Determinize(RemoveEmptyTransitions(fsm));
        }

        public FsmInfo<TState, int> Minimize(FsmInfo<TState, int> fsm)
        {
            throw new NotImplementedException();
        }

        public static FsmInfo<TState, int> Complement(FsmInfo<TState, int> dfa)
        {
            var newStates = new TState[dfa.States.Length];
            dfa.States.CopyTo(newStates, 0);

            var newTransitions = new FsmTransition<int>[dfa.Transitions.Length];
            dfa.Transitions.CopyTo(newTransitions, 0);

            var newFinal = new HashSet<int>(Enumerable.Range(0, dfa.States.Length));
            newFinal.ExceptWith(dfa.FinalStates);

            return new FsmInfo<TState, int>(
                states: newStates,
                transitions: newTransitions,
                finalStates: newFinal);
        }
    }
}
