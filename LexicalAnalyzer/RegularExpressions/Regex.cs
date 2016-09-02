using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalyzer.Utils;

namespace LexicalAnalyzer.RegularExpressions
{
    public partial class Regex
    {
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

        public static RegexTree Compile(string pattern)
        {
            int pos = 0;
            return RegexTree.CreateValue(ParseCharClass(pattern, ref pos));
        }
    }
}
