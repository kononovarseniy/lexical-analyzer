using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SParser
{
    enum SType { List, Atom, Srting }
    abstract class SExpr
    {
        public SType Type { get; protected set; }
        public abstract string ToString(int deep);
        protected SExpr(SType type)
        {
            Type = type;
        }
    }
    class SAtom : SExpr
    {
        public string Name;
        public SAtom(string name) : this(name, SType.Atom) { }
        protected SAtom(string name, SType type) : base(type)
        {
            Name = name;
        }
        public override string ToString(int deep) => new string(' ', deep * 4) + Name;
        public override string ToString() => Name;
    }
    class SString : SAtom
    {
        public SString(string name) : base(name, SType.Srting) { }
    }
    class SList : SExpr, IEnumerable<SExpr>
    {
        public List<SExpr> Children;

        public SList() : base(SType.Atom)
        {
            Children = new List<SExpr>();
        }
        public SList(IEnumerable<SExpr> collection) : base(SType.Atom)
        {
            Children = new List<SExpr>(collection);
        }

        public void Add(SExpr item)
        {
            Children.Add(item);
        }

        public override string ToString(int deep)
        {
            StringBuilder res = new StringBuilder();
            res.Append(new string(' ', deep * 4));
            res.Append("(");
            if (Children.Count > 0)
                res.AppendLine();
            foreach (var item in Children)
                res.AppendLine(item.ToString(deep + 1));
            if (Children.Count > 0)
                res.Append(new string(' ', deep * 4));
            res.Append(")");
            return res.ToString();
        }

        public override string ToString() => ToString(0);

        public IEnumerator<SExpr> GetEnumerator()
        {
            return ((IEnumerable<SExpr>)Children).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<SExpr>)Children).GetEnumerator();
        }
    }
}
