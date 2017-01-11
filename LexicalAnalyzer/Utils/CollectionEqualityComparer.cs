using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.Utils
{
    public class BoolArrayEqualityComparer : IEqualityComparer<bool[]>
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

    public class IntArrayEqualityComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
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

        public int GetHashCode(int[] obj)
        {
            // Sum is better than XOR on small values
            // Use Aggreagete instread of Sum to prevent OverflowException
            return obj.Aggregate(0, (acc, item) => acc + item);
        }
    }

    public class HashSetEqualityComparer<T> : IEqualityComparer<HashSet<T>>
    {
        public bool Equals(HashSet<T> x, HashSet<T> y)
        {
            var xCopy = new HashSet<T>(x);
            bool res = x.Count() == y.Count();
            xCopy.ExceptWith(y);
            return res && xCopy.Count() == 0;
        }

        public int GetHashCode(HashSet<T> obj)
        {
            return obj.Aggregate(0, (acc, item) => acc ^ obj.Comparer.GetHashCode(item));
        }
    }
}
