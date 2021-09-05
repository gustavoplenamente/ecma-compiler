using System.Collections.Generic;
using System.Text.RegularExpressions;
using static EcmaCompiler.Tokens.Token;

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

        public Token SearchKeyword(string name) {
            if (_tokensByName.TryGetValue(name, out var token)) return token;

            if (Regex.IsMatch(name, _charPattern)) return CHARACTER;
            if (Regex.IsMatch(name, _stringPattern)) return STRINGVAL;
            if (Regex.IsMatch(name, _numeralPattern)) return NUMERAL;
            if (Regex.IsMatch(name, _idPattern)) {
                _secondaryTokensByName.TryAdd(name, _tokensByName.Count + 1);
                return ID;
            }

            return UNKNOWN;
        }

        public int SearchName(string name) {

        }
    }
}
