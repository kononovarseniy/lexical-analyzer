using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LexicalAnalyzer.Utils
{
    public class IntSet : IEnumerable<IntSet>
    {
        public static readonly IntSet Empty = new IntSet()
        {
            edges = new List<int>()
        };
        public static readonly IntSet All = new IntSet()
        {
            edges = new List<int>() { int.MinValue, int.MaxValue }
        };

        private List<int> edges;

        private IntSet() { }

        public IntSet(int value) : this(value, value) { }

        public IntSet(int from, int to)
        {
            if (to < from)
                throw new ArgumentException($"Argument '{nameof(to)} must be more or equal then '{nameof(from)}'.");
            edges = new List<int>() { from - 1, to };
        }

        public bool IsEmpty => edges.Count == 0;
        public bool IsRange => edges.Count == 2;
        public int Min => edges[0] + 1;
        public int Max => edges[edges.Count - 1];

        public override string ToString()
        {
            string result;
            Func<IntSet, string> toString = (r) =>
                r.Min == r.Max ? r.Min.ToString() : $"{r.Min}..{r.Max}";
            result = string.Join(", ", this.Select(toString));
            return $"{{{result}}}";
        }

        public bool Contains(int value)
        {
            int ind = edges.BinarySearch(value);
            if (ind < 0) ind = ~ind;
            return ind % 2 != 0;
        }

        private static List<int> BinaryOperation(List<int> a, List<int> b, Func<bool, bool, bool> func)
        {
            var res = new List<int>();

            bool aState = false;
            bool bState = false;
            bool cState = false;

            var enA = a.GetEnumerator();
            var enB = b.GetEnumerator();
            bool endOfA = !enA.MoveNext();
            bool endOfB = !enB.MoveNext();

            //int? prevEdge = null;
            while (!endOfA || !endOfB)
            {
                bool moveA = false;
                bool moveB = false;
                if (endOfA) moveB = true;
                else if (endOfB) moveA = true;
                else
                {
                    int dlt = enA.Current.CompareTo(enB.Current);
                    if (dlt < 0)
                        moveA = true;
                    else if (dlt > 0)
                        moveB = true;
                    else
                    {
                        moveA = true;
                        moveB = true;
                    }
                }
                int edge = int.MinValue;
                if (moveA)
                {
                    edge = enA.Current;
                    aState = !aState;
                    endOfA = !enA.MoveNext();
                }
                if (moveB)
                {
                    edge = enB.Current;
                    bState = !bState;
                    endOfB = !enB.MoveNext();
                }
                if (cState != func(aState, bState))
                {
                    res.Add(edge);
                    cState = !cState;
                }
            }
            return res;
        }

        public static IntSet Union(IntSet a, IntSet b)
        {
            return new IntSet()
            {
                edges = BinaryOperation(a.edges, b.edges, (aState, bState) => aState || bState)
            };
        }

        public static IntSet Intersection(IntSet a, IntSet b)
        {
            return new IntSet()
            {
                edges = BinaryOperation(a.edges, b.edges, (aState, bState) => aState && bState)
            };
        }

        public static IntSet Difference(IntSet a, IntSet b)
        {
            return new IntSet()
            {
                edges = BinaryOperation(a.edges, b.edges, (aState, bState) => aState && !bState)
            };
        }

        public static IntSet Complement(IntSet a) => Difference(All, a);

        public static bool Equal(IntSet a, IntSet b)
        {
            return a.edges.SequenceEqual(b.edges);
        }

        public static IntSet operator |(IntSet a, IntSet b) => Union(a, b);

        public static IntSet operator &(IntSet a, IntSet b) => Intersection(a, b);

        public static IntSet operator -(IntSet a, IntSet b) => Difference(a, b);

        public static IntSet operator ~(IntSet a) => Complement(a);

        public static bool operator ==(IntSet a, IntSet b) => Equal(a, b);
        public static bool operator !=(IntSet a, IntSet b) => !Equal(a, b);
        public override bool Equals(object obj)
        {
            return obj is IntSet && Equal(this, obj as IntSet);
        }
        public override int GetHashCode()
        {
            return edges.Aggregate(0, (acc, item) => acc ^ item);
        }

        public IEnumerable<int> GetEdges() => edges.Select(it => it);

        public IEnumerator<IntSet> GetEnumerator()
        {
            var en = edges.GetEnumerator();
            while (en.MoveNext())
            {
                int edge1 = en.Current;
                en.MoveNext();
                int edge2 = en.Current;
                yield return new IntSet()
                {
                    edges = new List<int>() { edge1, edge2 }
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
