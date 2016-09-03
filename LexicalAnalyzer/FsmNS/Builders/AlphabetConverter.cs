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
        private AlphabetConverter()
        {

        }

        /// <summary>
        /// Get index of set by value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Convert(int value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts labels of transitions from IntSet to int. And creates back converter.
        /// </summary>
        /// <typeparam name="TState">Type of states in fsm.</typeparam>
        /// <param name="fsm">Fsm to convert.</param>
        /// <param name="converter">Back converter.</param>
        /// <returns></returns>
        public static FsmInfo<TState, int> Convert<TState>(FsmInfo<TState, IntSet> fsm, out AlphabetConverter converter)
        {
            throw new NotImplementedException();
        }
    }
}
