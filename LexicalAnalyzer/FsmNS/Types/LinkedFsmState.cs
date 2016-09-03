using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer.FsmNS.Types
{
    public class LinkedFsmState
    {
        public string LexemeClass { get; set; }
        public bool IsFinal { get; set; } = false;
        public Dictionary<int, int> Transitions { get; set; } = null;

        public LinkedFsmState()
        {
            LexemeClass = null;
        }
        public LinkedFsmState(string lexClass)
        {
            LexemeClass = lexClass;
        }
    }
}
