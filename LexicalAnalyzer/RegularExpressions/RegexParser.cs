using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalyzer.Utils;

namespace LexicalAnalyzer.RegularExpressions
{
    public partial class RegexParser
    {
        #region Helpers
        private const string UnrecognizedEscapeSequenseText = "Unrecognized escape sequence.";
        private const string ParsingErrorText = "A regular expression parsing error occurred.";
        private const string UnexpectedEndOfLine = "Unexpected end of line.";

        private static void ThrowUnrecognizedEscapeSequence(int pos)
        {
            throw new ArgumentException($"{UnrecognizedEscapeSequenseText} At char {pos}.");
        }
        private static void ThrowParsingError(int pos)
        {
            throw new ArgumentException($"{ParsingErrorText} At char {pos}.");
        }
        private static void ThrowParsingError()
        {
            throw new ArgumentException(ParsingErrorText);
        }
        private static void ThrowUnexpectedEndOfLine()
        {
            throw new ArgumentException(UnexpectedEndOfLine);
        }

        private static char GetUnescapedChar(string str, ref int pos)
        {
            if (pos >= str.Length)
                ThrowUnexpectedEndOfLine();
            return str[pos++];
        }

        private static char PeekUnescapedChar(string str, int pos) =>
            GetUnescapedChar(str, ref pos);

        private static char GetChar(string str, ref int pos, out bool escaped)
        {
            escaped = false;
            if (pos >= str.Length)
                ThrowUnexpectedEndOfLine();
            char ch = str[pos++];
            if (ch != '\\') return ch;
            escaped = true;
            if (pos >= str.Length)
                ThrowUnexpectedEndOfLine();
            return str[pos++];
        }

        private static char PeekChar(string str, int pos, out bool escaped) =>
            GetChar(str, ref pos, out escaped);
        #endregion

        private static IntSet ParseCharClass(string str, ref int pos)
        {
            bool escaped;
            char ch = GetChar(str, ref pos, out escaped);

            if (escaped && EscapeMap.ContainsKey(ch))
                ch = EscapeMap[ch];
            else if (escaped && BuiltinCharClasses.ContainsKey(ch))
                return BuiltinCharClasses[ch];
            else if (escaped)
                ThrowUnrecognizedEscapeSequence(pos - 1);
            // Further not escaped.
            else if (ch == '.')
                return IntSet.All;
            else if (ch == '-')
                ThrowParsingError(pos - 1);
            else if (ch == '[')
            {
                bool inverse = PeekUnescapedChar(str, pos) == '^';
                if (inverse) GetUnescapedChar(str, ref pos);
                IntSet set = IntSet.Empty;
                while (PeekUnescapedChar(str, pos) != ']')
                {
                    set |= ParseCharClass(str, ref pos);
                }
                GetUnescapedChar(str, ref pos);
                return inverse ? ~set : set;
            }
            // not else
            if (PeekUnescapedChar(str, pos) == '-')
            {
                GetUnescapedChar(str, ref pos);

                bool escaped2;
                char ch2 = GetChar(str, ref pos, out escaped2);

                if (escaped2)
                {
                    if (EscapeMap.ContainsKey(ch2))
                        ch2 = EscapeMap[ch2];
                    else
                        ThrowUnrecognizedEscapeSequence(pos - 1);
                }
                else if (ch2 == '.' || ch2 == '[' || ch2 == ']')
                    ThrowParsingError(pos - 1);
                return new IntSet(ch, ch2);
            }
            return new IntSet(ch);
        }
        
        private static int GetPriority(char op)
        {
            if (op == '*' || op == '+' || op == '?')
                return 3;
            else if (op == ' ')
                return 2;
            else if (op == '|')
                return 1;
            else if (op == '(' || op == ')')
                return 0;
            else
                throw new ArgumentException();
        }
        
        private static void EvalOperator(Stack<RegexTree> values, char op)
        {
            if (op == '*')
            {
                if (values.Count < 1) ThrowParsingError();
                values.Push(RegexTree.CreateIteration(values.Pop()));
            }
            else if (op == '+')
            {
                if (values.Count < 1) ThrowParsingError();
                values.Push(RegexTree.CreatePositiveIteration(values.Pop()));
            }
            else if (op == '?')
            {
                if (values.Count < 1) ThrowParsingError();
                values.Push(RegexTree.CreateOptional(values.Pop()));
            }
            else if (op == ' ')
            {
                if (values.Count < 2) ThrowParsingError();
                var right = values.Pop();
                var left = values.Pop();
                values.Push(RegexTree.CreateSequence(left, right));
            }
            else if (op == '|')
            {
                if (values.Count < 2) ThrowParsingError();
                var right = values.Pop();
                var left = values.Pop();
                values.Push(RegexTree.CreateAlternatives(left, right));
            }
        }

        private static void FlushOperators(Stack<RegexTree> values, Stack<char> operators, int priority)
        {
            while (operators.Count > 0)
            {
                char op2 = operators.Peek();
                if (GetPriority(op2) > priority)
                    EvalOperator(values, operators.Pop());
                else break;
            }
        }

        private static void PushOperator(Stack<RegexTree> values, Stack<char> operators, char op)
        {
            FlushOperators(values, operators, GetPriority(op));
            operators.Push(op);
        }
        
        public static RegexTree Parse(string pattern)
        {
            Stack<RegexTree> values = new Stack<RegexTree>();
            Stack<char> operators = new Stack<char>();

            bool insertBefore = false;
            int pos = 0;
            while (pos < pattern.Length)
            {
                bool insertAfterPossible = false;
                char ch = GetUnescapedChar(pattern, ref pos);
                if (ch == '(')
                {
                    if (insertBefore)
                        PushOperator(values, operators, ' ');
                    operators.Push('(');
                }
                else if (ch == ')')
                {
                    FlushOperators(values, operators, 0);
                    if (operators.Count == 0)
                        ThrowParsingError(pos);
                    operators.Pop();
                    insertAfterPossible = true;
                }
                else if ("|*+?".IndexOf(ch) >= 0)
                {
                    PushOperator(values, operators, ch);
                    insertAfterPossible = ch != '|';
                }
                else
                {
                    pos--;
                    insertAfterPossible = true;
                    IntSet val = ParseCharClass(pattern, ref pos);
                    if (insertBefore)
                        PushOperator(values, operators, ' ');
                    values.Push(RegexTree.CreateValue(val));
                }
                insertBefore = insertAfterPossible;
            }
            FlushOperators(values, operators, 0);

            if (operators.Count > 0)
                ThrowUnexpectedEndOfLine();

            if (values.Count > 1)
                ThrowParsingError();

            return values.Pop();
        }
    }
}
