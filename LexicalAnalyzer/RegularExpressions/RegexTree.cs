using LexicalAnalyzer.FsmNS.Builders;
using LexicalAnalyzer.FsmNS.Types;
using LexicalAnalyzer.Utils;
using System;

namespace LexicalAnalyzer.RegularExpressions
{
    public enum RegexTreeNodeType
    {
        Value,
        Alternatives,
        Sequence,
        Iteration,
        PositiveIteration,
        Optional
    }

    public class RegexTree
    {
        public IntSet Value { get; private set; }
        public RegexTree Left { get; private set; }
        public RegexTree Right { get; private set; }
        public RegexTreeNodeType NodeType { get; private set; }

        private RegexTree() { }

        public static RegexTree CreateValue(IntSet value) => new RegexTree()
        {
            Value = value,
            NodeType = RegexTreeNodeType.Value
        };

        public static RegexTree CreateAlternatives(RegexTree left, RegexTree right) => new RegexTree()
        {
            Left = left,
            Right = right,
            NodeType = RegexTreeNodeType.Alternatives
        };

        public static RegexTree CreateSequence(RegexTree left, RegexTree right) => new RegexTree()
        {
            Left = left,
            Right = right,
            NodeType = RegexTreeNodeType.Sequence
        };

        public static RegexTree CreateIteration(RegexTree tree) => new RegexTree()
        {
            Left = tree,
            NodeType = RegexTreeNodeType.Iteration
        };
        
        public static RegexTree CreatePositiveIteration(RegexTree tree) => new RegexTree()
        {
            Left = tree,
            NodeType = RegexTreeNodeType.PositiveIteration
        };

        public static RegexTree CreateOptional(RegexTree tree) => new RegexTree()
        {
            Left = tree,
            NodeType = RegexTreeNodeType.Optional
        };

        public FsmInfo<TState, IntSet> ToFsm<TState>(NfaBuilder<TState, IntSet> builder)
        {
            switch (NodeType)
            {
                case RegexTreeNodeType.Value:
                    return builder.Terminal(Value);
                case RegexTreeNodeType.Alternatives:
                    return builder.Alternatives(Left.ToFsm(builder), Right.ToFsm(builder));
                case RegexTreeNodeType.Sequence:
                    return builder.Sequence(Left.ToFsm(builder), Right.ToFsm(builder));
                case RegexTreeNodeType.Iteration:
                    return builder.Iteration(Left.ToFsm(builder));
                case RegexTreeNodeType.PositiveIteration:
                    return builder.PositiveIteration(Left.ToFsm(builder));
                case RegexTreeNodeType.Optional:
                    return builder.Optional(Left.ToFsm(builder));
                default:
                    throw new Exception("Bug.");
            }
        }
    }
}
