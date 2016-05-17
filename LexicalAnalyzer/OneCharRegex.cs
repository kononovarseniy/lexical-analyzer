using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public static class OneCharRegex
    {
        private static readonly Dictionary<char, char> EscapeMap = new Dictionary<char, char>()
        {
            {'a', '\a' },
            {'b', '\b' },
            {'t', '\t' },
            {'r', '\r' },
            {'v', '\v' },
            {'f', '\f' },
            {'n', '\n' },
            {'.', '.' },
            {'-', '-' },
            {'^', '^' },
            {'[', '[' },
            {']', ']' },
            {'\\', '\\' },
        };
        private static int IndexOfClosingBracket(string str, int start = 0)
        {
            if (start >= str.Length || str[start] != '[')
                throw new ArgumentException(nameof(start));
            int i = start;
            int deep = 0;
            bool esc = false;
            while (i < str.Length)
            {
                if (esc) esc = false;
                else
                {
                    char strch = str[i];
                    if (strch == '[') deep++;
                    else if (strch == ']') deep--;
                    else if (strch == '\\') esc = true;
                    if (deep == 0) return i;
                }
                i++;
            }
            return -1;
        }
        private static char GetChar(string str, ref int pos)
        {
            if (pos >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(pos));
            char ch = str[pos++];
            if (ch != '\\') return ch;
            if (pos >= str.Length)
                throw new ArgumentException(nameof(str));
            return EscapeMap[str[pos++]];
        }
        
        public static bool IsMatch(char ch, string pattern)
        {
            int i = 0;
            while (i < pattern.Length)
            {
                char p = pattern[i];
                bool isMatch = false;
                if (p == '.') isMatch = true;
                // escape sequence and not part of range
                else if (p == '\\' && !(i + 2 < pattern.Length && pattern[i + 2] == '-'))
                {
                    if (i >= pattern.Length - 1) break;
                    p = pattern[i + 1];
                    switch (p)
                    {
                        case 'l': isMatch = char.IsLetter(ch); break;
                        case 'L': isMatch = !char.IsLetter(ch); break;
                        case 'w': isMatch = char.IsLetterOrDigit(ch); break;
                        case 'W': isMatch = !char.IsLetterOrDigit(ch); break;
                        case 'd': isMatch = char.IsDigit(ch); break;
                        case 'D': isMatch = !char.IsDigit(ch); break;
                        case 's': isMatch = char.IsWhiteSpace(ch); break;
                        case 'S': isMatch = !char.IsWhiteSpace(ch); break;
                        default:
                            if (EscapeMap.ContainsKey(p))
                                isMatch = ch == EscapeMap[p];
                            else
                                throw new ArgumentException(nameof(pattern));
                            break;
                    }
                    i += 2;
                }
                else if (p == '[')
                {
                    int ind = IndexOfClosingBracket(pattern, i);
                    if (ind == -1)
                        throw new ArgumentException(nameof(pattern));
                    if (pattern[i + 1] == '^')
                        isMatch = !IsMatch(ch, pattern.Substring(i + 2, ind - i - 2));
                    else
                        isMatch = IsMatch(ch, pattern.Substring(i + 1, ind - i - 1));
                    i = ind + 1;
                }
                else
                {
                    char ch1 = GetChar(pattern, ref i);
                    if (i < pattern.Length && pattern[i] == '-')
                    {
                        i++;
                        char ch2 = GetChar(pattern, ref i);
                        isMatch = ch1 <= ch && ch <= ch2;
                    }
                    else
                        isMatch = ch == ch1;
                }
                if (isMatch) return true;
            }
            return false;
        }
    }
}
