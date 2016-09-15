using LexicalAnalyzer.FsmNS.Types;
using LexicalAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.FsmNS.Builders
{
    public class AlphabetConverter
    {
        private class BoolArrayEqualityComparer : IEqualityComparer<bool[]>
        {
            public bool Equals(bool[] x, bool[] y)
            {
                if (x.Length == y.Length)
                {
                    for (int i = 0; i < x.Length; i++)
                        if (x[i] != y[i])
                            return false;
                    return true;
                }
                else return false;
            }

            public int GetHashCode(bool[] obj)
            {
                int res = 0;
                for (int i = 0; i < obj.Length; i++)
                    if (obj[i]) res++;
                return res;
            }
        }

        private List<int> edges;
        private List<int> values;
        private Dictionary<IntSet, List<int>> composition;
        
        /// <summary>
        /// Id of set that is absolute complement of union of all sets.
        /// </summary>
        public const int ComplementID = 0;

        public int Count { get; private set; }

        public AlphabetConverter(IEnumerable<IntSet> sets)
        {
            Initialize(sets);
        }

        public static AlphabetConverter Create(IEnumerable<IntSet> sets)
        {
            return new AlphabetConverter(sets);
        }

        public static AlphabetConverter Create<TState>(FsmInfo<TState, IntSet> fsm)
        {
            return new AlphabetConverter(from t in fsm.Transitions
                                         where t.HasSymbol
                                         select t.Symbol);
        }

        /// <summary>
        /// Convert value to set id.
        /// </summary>
        /// <param name="value">Value to converts.</param>
        /// <returns>Id of set that contatains value.</returns>
        public int ConvertValue(int value)
        {
            if (value == int.MinValue)
                return ComplementID;
            int res = edges.BinarySearch(value);
            if (res < 0) res = ~res;
            return values[res - 1];
        }

        /// <summary>
        /// Convert set to its subsets.
        /// </summary>
        /// <param name="set">One of sets that were passed to constructor.</param>
        /// <returns>Not intersecting subsets of set.</returns>
        public IEnumerable<int> ConvertSet(IntSet set)
        {
            return composition[set].Select(_ => _);
        }
        
        /// <summary>
        /// Convert labels of all transitions in FSM.
        /// </summary>
        /// <typeparam name="TState">Type of states in FSM.</typeparam>
        /// <param name="fsm">FSM to convert.</param>
        /// <returns>Converted FSM.</returns>
        public FsmInfo<TState, int> ConvertFsm<TState>(FsmInfo<TState, IntSet> fsm)
        {
            var transitions = Enumerable.Empty<FsmTransition<int>>();
            foreach (var t in fsm.Transitions)
            {
                IEnumerable<FsmTransition<int>> newTransitions;
                if (t.HasSymbol)
                    newTransitions = FsmTransition<int>.CreateTransitions(t.From, t.To, composition[t.Symbol]);
                else
                    newTransitions = Enumerable.Repeat(new FsmTransition<int>(t.From, t.To), 1);
                transitions = transitions.Concat(newTransitions);
            }
            // Copy collections
            var states = fsm.States.ToArray();
            var final = new HashSet<int>(fsm.FinalStates);
            return new FsmInfo<TState, int>(states, transitions.ToArray(), final);
        }

        private void Initialize(IEnumerable<IntSet> sets)
        {
            var uniqueSets = new HashSet<IntSet>(sets);
            int setsCount = uniqueSets.Count;

            // Helps to convert Int Set to its index in array.
            var map = new Dictionary<IntSet, int>(setsCount);
            // Composition of each set.
            var composition = new List<int>[setsCount];
            // Edge enumerators.
            var enumerators = new IEnumerator<int>[setsCount];
            bool[] isEnd = new bool[setsCount];
            bool[] state = new bool[setsCount];
            // Fill arrays.
            int i = 0;
            foreach (var set in uniqueSets)
            {
                map.Add(set, i);
                composition[i] = new List<int>();
                var en = set.GetEdges().GetEnumerator();
                enumerators[i] = en;
                isEnd[i] = !en.MoveNext();
                i++;
            }
            // Combinations of intersecting sets.
            var combinations = new Dictionary<bool[], int>(new BoolArrayEqualityComparer());
            List<int> edges = new List<int>();
            List<int> values = new List<int>();
            int idCounter = ComplementID;
            // Reserve complement Id
            RegisterCombination(combinations, composition, state, ref idCounter);
            int currentEdge = int.MinValue;
            bool complete = false;
            do
            {
                Move(enumerators, isEnd, state, currentEdge);
                int id = RegisterCombination(combinations, composition, state, ref idCounter);
                edges.Add(currentEdge);
                values.Add(id);
                int? next = GetNextEdge(enumerators, isEnd);
                if (next.HasValue)
                    currentEdge = next.Value;
                else
                    complete = true;
            } while (!complete);

            this.edges = edges;
            this.values = values;
            this.composition = map.ToDictionary(
                kvp => kvp.Key,
                kvp => composition[kvp.Value]);
            Count = idCounter;
        }

        private static int? GetNextEdge(IEnumerator<int>[] enumerators, bool[] isEnd)
        {
            int min = int.MaxValue;
            bool found = false;
            for (int i = 0; i < enumerators.Length; i++)
            {
                if (!isEnd[i])
                {
                    found = true;
                    int val = enumerators[i].Current;
                    if (val < min)
                    {
                        min = val;
                    }
                }
            }
            if (!found) return null;
            else return min;
        }

        private static void Move(IEnumerator<int>[] enumerators, bool[] isEnd, bool[] state, int value)
        {
            for (int i = 0; i < enumerators.Length; i++)
            {
                if (!isEnd[i])
                {
                    var en = enumerators[i];
                    int val = en.Current;
                    if (val == value)
                    {
                        isEnd[i] = !en.MoveNext();
                        state[i] = !state[i];
                    }
                }
            }
        }

        private static int RegisterCombination(Dictionary<bool[], int> combinations, List<int>[] composition, bool[] state, ref int idCounter)
        {
            int id;
            if (!combinations.TryGetValue(state, out id))
            {
                id = idCounter++;
                bool[] tmp = new bool[state.Length];
                state.CopyTo(tmp, 0);
                combinations.Add(tmp, id);
                // Associate subset with sets.
                for (int i = 0; i < composition.Length; i++)
                    if (state[i])
                        composition[i].Add(id);
            }
            return id;
        }
    }
}
