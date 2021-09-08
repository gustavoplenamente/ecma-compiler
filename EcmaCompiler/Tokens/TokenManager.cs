using System.Collections.Generic;
using System.Text.RegularExpressions;
using static EcmaCompiler.Tokens.Token;
using System;

namespace EcmaCompiler.Tokens {
    public class TokenManager {
        private string _charPattern = @"^'.'$";
        private string _stringPattern = @"^"".*""$";
        private string _numeralPattern = @"^\d+$";
        private string _idPattern = @"^[a-zA-Z]\w*$";

        private Dictionary<string, Token> _tokensByName = new Dictionary<string, Token> {
            ["array"] = ARRAY,
            ["boolean"] = BOOLEAN,
            ["break"] = BREAK,
            ["char"] = CHAR,
            ["continue"] = CONTINUE,
            ["do"] = DO,
            ["else"] = ELSE,
            ["false"] = FALSE,
            ["function"] = FUNCTION,
            ["if"] = IF,
            ["integer"] = INTEGER,
            ["of"] = OF,
            ["string"] = STRING,
            ["struct"] = STRUCT,
            ["true"] = TRUE,
            ["type"] = TYPE,
            ["var"] = VAR,
            ["while"] = WHILE,

            [":"] = COLON,
            [";"] = SEMI_COLON,
            [","] = COMMA,
            ["="] = EQUALS,
            ["["] = LEFT_SQUARE,
            ["]"] = RIGHT_SQUARE,
            ["{"] = LEFT_BRACES,
            ["}"] = RIGHT_BRACES,
            ["("] = LEFT_PARENTHESIS,
            [")"] = RIGHT_PARENTHESIS,
            ["&&"] = AND,
            ["||"] = OR,
            ["<"] = LESS_THAN,
            [">"] = GREATER_THAN,
            ["<="] = LESS_OR_EQUAL,
            [">="] = GREATER_OR_EQUAL,
            ["!="] = NOT_EQUAL,
            ["=="] = EQUAL_EQUAL,
            ["+"] = PLUS,
            ["++"] = PLUS_PLUS,
            ["-"] = MINUS,
            ["--"] = MINUS_MINUS,
            ["*"] = TIMES,
            ["/"] = DIVIDE,
            ["."] = DOT,
            ["!"] = NOT,
        };
        private Dictionary<string, int> _secondaryTokensByName = new Dictionary<string, int>();

        private Pool<char> _characters = new Pool<char>();
        private Pool<int> _integers = new Pool<int>();
        private Pool<string> _strings = new Pool<string>();

        private List<(Token, int)> _tokens = new List<(Token, int)>();

        public Token SearchKeyword(string name) {
            if (_tokensByName.TryGetValue(name, out var token))
                return token;

            if (Regex.IsMatch(name, _charPattern)) return CHARACTER;
            if (Regex.IsMatch(name, _stringPattern)) return STRINGVAL;
            if (Regex.IsMatch(name, _numeralPattern)) return NUMERAL;
            if (Regex.IsMatch(name, _idPattern)) return ID;

            return UNKNOWN;
        }

        public int SearchName(string name) {
            if (_secondaryTokensByName.TryGetValue(name, out var token))
                return token;

            return SaveIdentifier(name);
        }

        public int AddCharConst(char value) => _characters.Add(value);
        public int AddIntConst(int value) => _integers.Add(value);
        public int AddStringConst(string value) => _strings.Add(value);

        private int SaveIdentifier(string name) {
            var tokensCount = _secondaryTokensByName.Count + 1;
            _secondaryTokensByName.Add(name, tokensCount);

            return tokensCount;
        }

        public void SaveToken((Token, int) token) {
            _tokens.Add(token);
        }

        public void CreateAndSaveSymbolToken(string symbol) {
            Token token = _tokensByName[symbol];
            SaveToken((token, 0));
        }

        public void PrintTokens() {
            foreach ((Token, int) tokenPair in _tokens) {
                Token token;
                int secondaryToken;
                (token, secondaryToken) = tokenPair;
                Console.Write(token);
                if (secondaryToken > 0) {
                    Console.Write("\t{0}", secondaryToken);
                }
                Console.WriteLine("");
            }
        }
    }
}
