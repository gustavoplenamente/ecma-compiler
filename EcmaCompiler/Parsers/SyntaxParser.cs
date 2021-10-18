using EcmaCompiler.Tokens;
using static EcmaCompiler.Tokens.Token;

namespace EcmaCompiler {
    public class SyntaxParser {
        private TokenManager _tokenManager;

        private (Token, int) _current = (UNKNOWN, 0);
        private Token _currentToken => _current.Item1;
        private int _currentIndex = -1;

        public SyntaxParser(TokenManager tokenManager) {
            _tokenManager = tokenManager;
        }

        public void Parse() {
            nextToken();
            P();
        }

        private void Expect(Token token) {
            if (_currentToken == token) {
                nextToken();
            } else {
                SyntaxError(token);
            }
        }

        private void nextToken() {
            _currentIndex++;
            _current = _tokenManager.GetToken(_currentIndex);
        }

        private void NUM() {
            Expect(NUMERAL);
        }

        private void STR() {
            Expect(STRINGVAL);
        }

        private void CHR() {
            Expect(CHARACTER);
        }

        private void FALSE_() {
            Expect(FALSE);
        }

        private void TRUE_() {
            Expect(TRUE);
        }

        private void ID_() {
            Expect(ID);
        }

        private void END_() {
            if (_currentToken == END) return;
            SyntaxError(END);
        }

        private void T() {
            switch (_currentToken) {
                case INTEGER: Expect(INTEGER); break;
                case CHAR: Expect(CHAR); break;
                case BOOLEAN: Expect(BOOLEAN); break;
                case STRING: Expect(STRING); break;
                case ID: ID_(); break;
                default: SyntaxError(); break;
            }
        }

        private void LV() {
            ID_();
            LV_();
        }

        private void LV_() {
            switch (_currentToken) {
                case DOT: Expect(DOT); ID_(); LV_(); break;
                case LEFT_SQUARE: Expect(LEFT_SQUARE); E(); Expect(RIGHT_SQUARE); LV_(); break;
            }
        }

        private void LE() {
            E();
            LE_();
        }

        private void LE_() {
            while (_currentToken == COMMA) {
                Expect(COMMA);
                E();
            }
        }

        private void F__() {
            switch (_currentToken) {
                case PLUS_PLUS: Expect(PLUS_PLUS); break;
                case MINUS_MINUS: Expect(MINUS_MINUS); break;
            }
        }

        private void F_() {
            switch (_currentToken) {
                case LEFT_PARENTHESIS: Expect(LEFT_PARENTHESIS); LE(); Expect(RIGHT_PARENTHESIS); break;
                default: LV_(); F__(); break;
            }
        }

        private void F() {
            switch (_currentToken) {
                case ID: ID_(); F_(); break;
                case PLUS_PLUS: Expect(PLUS_PLUS); LV(); break;
                case MINUS_MINUS: Expect(MINUS_MINUS); LV(); break;
                case LEFT_PARENTHESIS: Expect(LEFT_PARENTHESIS); E(); Expect(RIGHT_PARENTHESIS); break;
                case MINUS: Expect(MINUS); F(); break;
                case NOT: Expect(NOT); F(); break;
                case TRUE: TRUE_(); break;
                case FALSE: FALSE_(); break;
                case CHARACTER: CHR(); break;
                case STRINGVAL: STR(); break;
                case NUMERAL: NUM(); break;
                default: SyntaxError(); break;
            }
        }

        private void Y_() {
            switch (_currentToken) {
                case TIMES: Expect(TIMES); F(); Y_(); break;
                case DIVIDE: Expect(DIVIDE); F(); Y_(); break;
            }
        }

        private void Y() {
            F();
            Y_();
        }

        private void R_() {
            switch (_currentToken) {
                case PLUS: Expect(PLUS); Y(); R_(); break;
                case MINUS: Expect(MINUS); Y(); R_(); break;
            }
        }

        private void R() {
            Y();
            R_();
        }

        private void L_() {
            switch (_currentToken) {
                case LESS_THAN: Expect(LESS_THAN); R(); L_(); break;
                case GREATER_THAN: Expect(GREATER_THAN); R(); L_(); break;
                case LESS_OR_EQUAL: Expect(LESS_OR_EQUAL); R(); L_(); break;
                case GREATER_OR_EQUAL: Expect(GREATER_OR_EQUAL); R(); L_(); break;
                case EQUAL_EQUAL: Expect(GREATER_THAN); R(); L_(); break;
                case NOT_EQUAL: Expect(NOT_EQUAL); R(); L_(); break;
            }
        }

        private void L() {
            R();
            L_();
        }

        private void E_() {
            switch (_currentToken) {
                case AND: Expect(AND); L(); E_(); break;
                case OR: Expect(OR); L(); E_(); break;
            }
        }

        private void E() {
            L();
            E_();
        }

        private void S() {
            switch (_currentToken) {
                case IF:
                    Expect(IF); Expect(LEFT_PARENTHESIS); E(); Expect(RIGHT_PARENTHESIS); S();
                    if (_currentToken == ELSE) {
                        Expect(ELSE); S();
                    }
                    break;
                case WHILE: Expect(WHILE); Expect(LEFT_PARENTHESIS); E(); Expect(RIGHT_PARENTHESIS); S(); break;
                case DO: Expect(DO); S(); Expect(WHILE); Expect(LEFT_PARENTHESIS); E(); Expect(RIGHT_PARENTHESIS); Expect(SEMI_COLON); break;
                case BREAK: Expect(BREAK); Expect(SEMI_COLON); break;
                case CONTINUE: Expect(CONTINUE); Expect(SEMI_COLON); break;
                case RETURN: Expect(RETURN); ID_(); Expect(SEMI_COLON); break;
                case LEFT_BRACES: B(); break;
                case ID: LV(); Expect(EQUALS); E(); Expect(SEMI_COLON); break;
                default: SyntaxError(); break;
            }
        }

        private void LI() {
            ID_();

            while (_currentToken == COMMA) {
                Expect(COMMA);
                ID_();
            }
        }
        private void DV() {
            Expect(VAR);
            LI();
            Expect(COLON);
            T();
            Expect(SEMI_COLON);
        }

        private void LS() {
            do {
                S();
            }
            while (_currentToken == IF ||
                _currentToken == WHILE ||
                _currentToken == DO ||
                _currentToken == BREAK ||
                _currentToken == CONTINUE ||
                _currentToken == RETURN ||
                _currentToken == ID ||
                _currentToken == LEFT_BRACES
                );
        }

        private void LDV() {
            do {
                DV();
            }
            while (_currentToken == VAR);
        }

        private void B() {
            Expect(LEFT_BRACES);
            LDV();
            LS();
            Expect(RIGHT_BRACES);
        }

        private void LP() {
            ID_();
            Expect(COLON);
            T();

            while (_currentToken == COMMA) {
                Expect(COMMA);
                ID_();
                Expect(COLON);
                T();
            }
        }

        private void DF() {
            Expect(FUNCTION);
            ID_();
            Expect(LEFT_PARENTHESIS);
            LP();
            Expect(RIGHT_PARENTHESIS);
            Expect(COLON);
            T();
            B();
        }

        private void DC() {
            LI();
            Expect(COLON);
            T();

            while (_currentToken == SEMI_COLON) {
                Expect(SEMI_COLON);
                LI();
                Expect(COLON);
                T();
            }
        }

        private void DT() {
            Expect(TYPE);
            ID_();
            Expect(EQUALS);
            switch (_currentToken) {
                case ARRAY: Expect(ARRAY); Expect(LEFT_SQUARE); NUM(); Expect(RIGHT_SQUARE); Expect(OF); T(); break;
                case STRUCT: Expect(STRUCT); Expect(LEFT_BRACES); DC(); Expect(RIGHT_BRACES); break;
                default: T(); break;
            }
        }

        private void DE() {
            switch (_currentToken) {
                case FUNCTION: DF(); break;
                case TYPE: DT(); break;
                case VAR: DV(); break;
                default: SyntaxError(); break;
            }
        }

        private void LDE() {
            do {
                DE();
            } while (
            _currentToken == FUNCTION ||
            _currentToken == TYPE ||
            _currentToken == VAR);
        }

        private void P() {
            LDE();
            END_();
        }

        private void SyntaxError() {
            throw new SyntaxError($"Token {_currentToken} not expected.");
        }

        private void SyntaxError(Token token) {
            throw new SyntaxError($"Token {_currentToken} not expected. Should be {token}.");
        }

    }
}
