using LexicalAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SParser
{
    class FsmBuilder
    {
        private class FsmPreState : FsmState
        {
            public List<KeyValuePair<string, int>> UnresolvedTransitions = new List<KeyValuePair<string, int>>();
            public FsmPreState(string lexClass, bool final) : base(lexClass, final) { }
            public FsmState ToFsmState()
            {
                return new FsmState(LexemeClass, Final);
            }
        }
        private const string FinAtomName = "FIN";
        private const string EmptyLabelName = ":";
        private const string RootLabelName = "ROOT:";

        private int IdCounter = 0;
        private Dictionary<string, int> LabelIdMap = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<int, FsmPreState> IdPreStateMap = new Dictionary<int, FsmPreState>();
        private Dictionary<int, FsmState> IdStateMap = new Dictionary<int, FsmState>();

        private void Reset()
        {
            IdCounter = 0;
            LabelIdMap.Clear();
            IdPreStateMap.Clear();
            IdStateMap.Clear();
        }

        private int GetNewId() => IdCounter++;
        private int GetId(string label, bool addIfnotExists = true)
        {
            int id;
            if (label == EmptyLabelName)
            {
                id = GetNewId();
            }
            else if (!LabelIdMap.TryGetValue(label, out id))
            {
                if (!addIfnotExists)
                    throw new KeyNotFoundException($"Label '{id}' not found.");
                id = GetNewId();
                LabelIdMap.Add(label, id);
            }
            return id;
        }
        
        private void CreateTransition(FsmPreState state, SExpr expr)
        {
            if (expr.Type != SType.List)
                throw new ArgumentException();
            var list = ((SList)expr).Children;
            if (list.Count != 2)
                throw new ArgumentException();
            var key = list[0];
            var value = list[1];
            if (key.Type != SType.Srting)
                throw new ArgumentException();
            state.UnresolvedTransitions.Add(
                new KeyValuePair<string, int>(((SString)key).Name, CreateState(value)));
        }
        private int CreateState(SExpr expr)
        {
            if (expr.Type == SType.Atom)
                return GetId(((SAtom)expr).Name, false);
            else if (expr.Type == SType.List)
            {
                var list = ((SList)expr).Children;
                int cnt = list.Count;
                string label = EmptyLabelName, lexClass = null;
                bool fin = false;
                var en = list.GetEnumerator();
                bool end = !en.MoveNext();

                #region read arguments
                if (!end && en.Current.Type == SType.Atom)
                    label = ((SAtom)en.Current).Name;
                else throw new ArgumentException();
                end = !en.MoveNext();

                // lexeme final
                if (!end && en.Current.Type == SType.Atom)
                {
                    fin = ((SAtom)en.Current).Name.ToUpper() == FinAtomName;
                    end = !en.MoveNext();
                }

                // lexeme class
                if (!end && en.Current.Type == SType.Atom)
                {
                    lexClass = ((SAtom)en.Current).Name;
                    end = !en.MoveNext();
                }
                #endregion region

                FsmPreState state = new FsmPreState(lexClass, fin);
                int id = GetId(label);
                // if state with this label already defined
                if (IdPreStateMap.ContainsKey(id))
                {
                    id = GetNewId();
                    LabelIdMap[label] = id;
                }
                IdPreStateMap.Add(id, state);
                while (!end)
                {
                    CreateTransition(state, en.Current);
                    end = !en.MoveNext();
                }
                return id;
            }
            else
                throw new ArgumentException();
        }

        private FsmState ResolveState(int id)
        {
            FsmState st;
            if (!IdStateMap.TryGetValue(id, out st))
            {
                FsmPreState preSt = IdPreStateMap[id];
                st = preSt.ToFsmState();
                IdStateMap.Add(id, st);
                foreach (var tr in preSt.UnresolvedTransitions)
                {
                    FsmState st2 = ResolveState(tr.Value);
                    st.Add(tr.Key, st2);
                }
            }
            return st;
        }

        public FsmState BuildFsm(SList rules)
        {
            Reset();
            foreach (var list in rules)
            {
                // top level state cannot be atom
                if (list.Type != SType.List)
                    throw new ArgumentException();
                CreateState(list);
            }
            int rootId;
            if (!LabelIdMap.TryGetValue(RootLabelName, out rootId))
                throw new KeyNotFoundException($"Expression must contain label '{RootLabelName}'");
            return ResolveState(rootId);
        }
    }
}
