using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public class Lexeme
    {
        public int Start;
        public int Length;
        public bool Complited;
        public string Class;

        public Lexeme(int start = 0)
        {
            Start = start;
            Length = 0;
            Class = null;
            Complited = false;
        }
        public void CopyFrom(Lexeme lexeme)
        {
            Start = lexeme.Start;
            Length = lexeme.Length;
            Class = lexeme.Class;
            Complited = lexeme.Complited;
        }
        public Lexeme Clone()
        {
            var lex = new Lexeme();
            lex.CopyFrom(this);
            return lex;
        }
        public TToken ToToken<TToken>(string sourceString) where TToken : Token, new() =>
            Token.FromLexeme<TToken>(this, sourceString);
    }
    public class Token : Lexeme
    {
        public string Text { get; protected set; } = null;
        public Token() { }
        protected virtual void SetText(string text)
        {
            Text = text;
        }
        public static TokenType FromLexeme<TokenType>(Lexeme lexeme, string sourceString) where TokenType : Token, new()
        {
            var token = new TokenType();
            token.CopyFrom(lexeme);
            token.SetText(sourceString.Substring(lexeme.Start, lexeme.Length));
            return token;
        }
    }
}
