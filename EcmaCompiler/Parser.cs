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

        public Parser(TokenManager tokenManager) {
            _tokenManager = tokenManager;
        }

        public void Parse(string filename) {
            _reader = new StreamReader(filename);
            var (token, secondaryToken) = NextToken();
        }

        private (Token, int) NextToken() {
            _nextChar = ' ';
            Token token;
            int secondaryToken;

            while (_nextChar == ' ') {
                _nextChar = ReadChar();
            }

            if (_nextChar.IsAlpha()) {
                (token, secondaryToken) = ParseId();
            } else if (_nextChar.IsDigit()) {
                (token, secondaryToken) = ParseNumeral();
            } else if (_nextChar == '\"') {
                (token, secondaryToken) = ParseString();
            } else if (_nextChar == '\'') {
                (token, secondaryToken) = ParseChar();
            } else {
                token = Token.UNKNOWN;
                secondaryToken = 0;
            }

            return (token, secondaryToken);
        }

        private char ReadChar() => (char)_reader.Read();

        private (Token, int) ParseId() {
            var builder = new StringBuilder();

            do {
                builder.Append(_nextChar);
                _nextChar = ReadChar();
            }
            while (_nextChar.IsAlphaNum() || _nextChar == '_');

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
                _nextChar = ReadChar();
            }
            while (_nextChar.IsDigit() || _nextChar == '_');

            var name = builder.ToString();
            var token = Token.NUMERAL;
            var secondaryToken = _tokenManager.AddIntConst(Int32.Parse(name));

            return (token, secondaryToken);
        }

        private (Token, int) ParseString() {
            var builder = new StringBuilder();

            do {
                builder.Append(_nextChar);
                _nextChar = ReadChar();
            }
            while (_nextChar != '\"');

            var name = builder.ToString();
            var token = Token.STRING;
            var secondaryToken = _tokenManager.AddStringConst(name);

            return (token, secondaryToken);
        }

        private (Token, int) ParseChar() {
            _nextChar = ReadChar();

            var token = Token.CHARACTER;
            var secondaryToken = _tokenManager.AddCharConst(_nextChar);

            _nextChar = ReadChar(); // skip trailing '
            _nextChar = ReadChar();

            return (token, secondaryToken);
        }
    }
}
