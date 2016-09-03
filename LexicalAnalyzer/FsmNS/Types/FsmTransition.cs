using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.FsmNS.Types
{
    public class FsmTransition<TSymbol>
    {
        public int From { get; private set; }
        public int To { get; private set; }
        public bool HasSymbol { get; private set; }
        public TSymbol Symbol { get; private set; }

        private FsmTransition(int from, int to, TSymbol symbol, bool hasSymbol)
        {
            From = from;
            To = to;
            Symbol = symbol;
            HasSymbol = hasSymbol;
        }
        public FsmTransition(int from, int to, TSymbol symbol)
            : this(from, to, symbol, true) { }
        public FsmTransition(int from, int to)
            : this(from, to, default(TSymbol), false) { }

        private static IEnumerable<FsmTransition<TSymbol>> CreateTransitionsImpl(int from, IEnumerable<int> to, TSymbol symbol, bool hasSymbol)
        {
            return to.Select(it => new FsmTransition<TSymbol>(from, it, symbol, hasSymbol));
        }
        private static IEnumerable<FsmTransition<TSymbol>> CreateTransitionsImpl(IEnumerable<int> from, int to, TSymbol symbol, bool hasSymbol)
        {
            return from.Select(it => new FsmTransition<TSymbol>(it, to, symbol, hasSymbol));
        }

        public static IEnumerable<FsmTransition<TSymbol>> CreateTransitions(int from, IEnumerable<int> to, TSymbol symbol) =>
            CreateTransitionsImpl(from, to, symbol, true);
        public static IEnumerable<FsmTransition<TSymbol>> CreateTransitions(IEnumerable<int> from, int to, TSymbol symbol) =>
            CreateTransitionsImpl(from, to, symbol, true);

        public static IEnumerable<FsmTransition<TSymbol>> CreateTransitions(int from, IEnumerable<int> to) =>
            CreateTransitionsImpl(from, to, default(TSymbol), false);
        public static IEnumerable<FsmTransition<TSymbol>> CreateTransitions(IEnumerable<int> from, int to) =>
            CreateTransitionsImpl(from, to, default(TSymbol), false);
    }
}
