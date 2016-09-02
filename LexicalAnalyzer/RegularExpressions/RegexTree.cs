using LexicalAnalyzer.Utils;

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
    }
}
