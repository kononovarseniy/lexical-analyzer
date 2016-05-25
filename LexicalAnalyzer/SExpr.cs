using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LexicalAnalyzer
{
    internal enum SType { List, Atom, Srting }

    internal abstract class SExpr
    {
        public SType Type { get; protected set; }
        public abstract string ToString(int deep);
        private static IEnumerable<SExpr> ParseImpl(IEnumerator<Token> input, bool isTopLevel = true)
        {
            while (true)
            {
                if (!input.MoveNext())
                {
                    if (isTopLevel) yield break;
                    else throw new ArgumentException();
                }
                Token current = input.Current;
                if (!isTopLevel && current.Class == "closing-bracket")
                    yield break;
                else if (current.Class == "atom")
                    yield return new SAtom(current.Text);
                else if (current.Class == "str-atom")
                    yield return new SString(((SStrToken)current).Value);
                else if (current.Class == "opening-bracket")
                    yield return new SList(ParseImpl(input, false));
            }
        }

        public static IEnumerable<SExpr> Parse(IEnumerable<Token> input)
        {
            var en = input.GetEnumerator();
            return ParseImpl(en);
        }
    }

    internal class SAtom : SExpr
    {
        public string Name;
        public SAtom(string name)
        {
            Type = SType.Atom;
            Name = name;
        }
        public override string ToString(int deep) => new string(' ', deep * 4) + Name;
        public override string ToString() => Name;
    }

    internal class SString : SAtom
    {
        public SString(string name) : base(name)
        {
            Type = SType.Srting;
        }
    }

    internal class SList : SExpr, IEnumerable<SExpr>
    {
        public List<SExpr> Children;

        public SList()
        {
            Type = SType.List;
            Children = new List<SExpr>();
        }
        public SList(IEnumerable<SExpr> collection)
        {
            Type = SType.List;
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
