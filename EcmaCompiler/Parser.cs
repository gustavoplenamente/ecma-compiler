using EcmaCompiler.Tokens;
using EcmaCompiler.Utils;
using System;
using System.IO;
using System.Text;

namespace EcmaCompiler {
    public class Parser {
        private StreamReader _reader;
        private TokenManager _tokenManager;
        private char _nextChar;
        private Boolean _isDone = false;

        public Parser(TokenManager tokenManager) {
            _tokenManager = tokenManager;
        }

        public void Parse(string filename) {
            _reader = new StreamReader(filename);

            while (!_isDone) {
                ParseNextRule();
            }
        }

        private void ParseNextRule() {
            if (_isDone) return;

            while (_nextChar == ' ') {
                if (ParseNextChar() == null) return;
            }

            if (_isDone) {
                return;
            }

            if (_nextChar.IsAlpha()) {
                _tokenManager.SaveToken(ParseId());
                return;
            }
            
            if (_nextChar.IsDigit()) {
                _tokenManager.SaveToken(ParseNumeral());
                return;
            }
            
            if (_nextChar == '\"') {
                _tokenManager.CreateAndSaveSymbolToken("\"");
                _tokenManager.SaveToken(ParseString());
                _tokenManager.CreateAndSaveSymbolToken("\"");
                return;
            }
            
            if (_nextChar == '\'') {
                _tokenManager.CreateAndSaveSymbolToken("\'");
                _tokenManager.SaveToken(ParseChar());
                _tokenManager.CreateAndSaveSymbolToken("\'");
                return;
            }

            _tokenManager.SaveToken(ParseSymbol());
        }

        private char? ParseNextChar() {
            if (_reader.Peek() == 0) {
                _isDone = false;
                return null;
            }
            _nextChar = (char) _reader.Read();
            return _nextChar;
        }

        private (Token, int) ParseId() {
            var builder = new StringBuilder();

            do {
                builder.Append(_nextChar);
                ParseNextChar();
            }
            while (!_isDone && (_nextChar.IsAlphaNum() || _nextChar == '_'));

            var name = builder.ToString();
            var token = _tokenManager.SearchKeyword(name);
            int secondaryToken = 0;

            if (token == Token.ID) {
                secondaryToken = _tokenManager.SearchName(name);
            }

            return (token, secondaryToken);
        }

        private (Token, int) ParseNumeral() {
            var builder = new StringBuilder();

            do {
                builder.Append(_nextChar);
                ParseNextChar();
            }
            while (!_isDone && (_nextChar.IsDigit() || _nextChar == '_'));

            var name = builder.ToString();
            var token = Token.NUMERAL;
            var secondaryToken = _tokenManager.AddIntConst(Int32.Parse(name));

            return (token, secondaryToken);
        }

        private (Token, int) ParseString() {
            var builder = new StringBuilder();
            ParseNextChar();

            do {
                builder.Append(_nextChar);
                ParseNextChar();
            }
            while (!_isDone && _nextChar != '\"');

            var name = builder.ToString();
            var token = Token.STRING;
            var secondaryToken = _tokenManager.AddStringConst(name);

            ParseNextChar(); // skip trailing

            return (token, secondaryToken);
        }

        private (Token, int) ParseChar() {
            ParseNextChar();

            var token = Token.CHARACTER;
            var secondaryToken = _tokenManager.AddCharConst(_nextChar);

            ParseNextChar(); // skip trailing '
            ParseNextChar();

            return (token, secondaryToken);
        }

        private (Token, int) ParseSymbol() {
            if (_nextChar == ':') {
                ParseNextChar();
                return(Token.COLON, 0);
            }

            if (_nextChar == ':') {
                ParseNextChar();
                return(Token.COLON, 0);
            }

            if (_nextChar == ';') {
                ParseNextChar();
                return(Token.SEMI_COLON, 0);
            }

            if (_nextChar == ',') {
                ParseNextChar();
                return(Token.COMMA, 0);
            }

            if (_nextChar == '=') {
                ParseNextChar();
                if (_nextChar == '=') {
                    ParseNextChar();
                    return(Token.EQUAL_EQUAL, 0);
                }
                return(Token.EQUALS, 0);
            }

            if (_nextChar == '[') {
                ParseNextChar();
                return(Token.LEFT_SQUARE, 0);
            }

            if (_nextChar == ']') {
                ParseNextChar();
                return(Token.RIGHT_SQUARE, 0);
            }

            if (_nextChar == '{') {
                ParseNextChar();
                return(Token.LEFT_BRACES, 0);
            }

            if (_nextChar == '}') {
                ParseNextChar();
                return(Token.RIGHT_BRACES, 0);
            }

            if (_nextChar == '(') {
                ParseNextChar();
                return(Token.LEFT_PARENTHESIS, 0);
            }

            if (_nextChar == ')') {
                ParseNextChar();
                return(Token.RIGHT_PARENTHESIS, 0);
            }

            if (_nextChar == '&') {
                ParseNextChar();
                if (_nextChar == '&') {
                    ParseNextChar();
                    return(Token.AND, 0);
                }
                return(Token.UNKNOWN, 0);
            }

            if (_nextChar == '|') {
                ParseNextChar();
                if (_nextChar == '|') {
                    ParseNextChar();
                    return(Token.OR, 0);
                }
                return(Token.UNKNOWN, 0);
            }

            if (_nextChar == '<') {
                ParseNextChar();
                if (_nextChar == '=') {
                    ParseNextChar();
                    return(Token.LESS_OR_EQUAL, 0);
                }
                return(Token.LESS_THAN, 0);
            }

            if (_nextChar == '>') {
                ParseNextChar();
                if (_nextChar == '=') {
                    ParseNextChar();
                    return(Token.GREATER_OR_EQUAL, 0);
                }
                return(Token.GREATER_THAN, 0);
            }

            if (_nextChar == '!') {
                ParseNextChar();
                if (_nextChar == '=') {
                    ParseNextChar();
                    return(Token.NOT_EQUAL, 0);
                }
                return(Token.NOT, 0);
            }

            if (_nextChar == '+') {
                ParseNextChar();
                if (_nextChar == '+') {
                    ParseNextChar();
                    return(Token.PLUS_PLUS, 0);
                }
                return(Token.PLUS, 0);
            }

            if (_nextChar == '-') {
                ParseNextChar();
                if (_nextChar == '-') {
                    ParseNextChar();
                    return(Token.MINUS_MINUS, 0);
                }
                return(Token.MINUS, 0);
            }

            if (_nextChar == '*') {
                ParseNextChar();
                return(Token.TIMES, 0);
            }

            if (_nextChar == '/') {
                ParseNextChar();
                return(Token.DIVIDE, 0);
            }

            if (_nextChar == '.') {
                ParseNextChar();
                return(Token.DOT, 0);
            }

            return (Token.UNKNOWN, 0);
        }
    }
}
