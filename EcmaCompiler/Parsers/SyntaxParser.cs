using EcmaCompiler.Parsers;
using EcmaCompiler.Tokens;
using EcmaCompiler.Utils;
using System.Collections.Generic;
using static EcmaCompiler.Tokens.Token;

namespace EcmaCompiler {
    public class SyntaxParser {
        private TokenManager _tokenManager;
        private ScopeManager _scopeManager;

        private (Token, int) _current = (UNKNOWN, 0);
        private Token _currentToken => _current.Item1;
        private int _currentName => _current.Item2;
        private int _currentIndex = -1;

        public SyntaxParser(TokenManager tokenManager, ScopeManager scopeManager) {
            _tokenManager = tokenManager;
            _scopeManager = scopeManager;
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

        private TypeAttribute NUM() {
            var pos = _currentName;
            Expect(NUMERAL);

            return new TypeAttribute {
                Type = TypeManager.Integer,
                Pos = pos,
                Val = _tokenManager.GetIntConst(pos)
            };
        }

        private TypeAttribute STR() {
            var pos = _currentName;
            Expect(STRINGVAL);

            return new TypeAttribute {
                Type = TypeManager.String,
                Pos = pos,
                Val = _tokenManager.GetStringConst(pos)
            };
        }

        private TypeAttribute CHR() {
            var pos = _currentName;
            Expect(CHARACTER);

            return new TypeAttribute {
                Type = TypeManager.Char,
                Pos = pos,
                Val = _tokenManager.GetCharConst(pos)
            };
        }

        private TypeAttribute FALSE_() {
            Expect(FALSE);
            return new TypeAttribute {
                Type = TypeManager.Bool,
                Val = false
            };
        }

        private TypeAttribute TRUE_() {
            Expect(TRUE);
            return new TypeAttribute {
                Type = TypeManager.Bool,
                Val = true
            };
        }

        private void ID_() {
            Expect(ID);
        }

        private Object IDD() {
            var name = _currentName;
            ID_();

            var obj = _scopeManager.Search(name);

            if (obj != null)
                ScopeManager.ScopeError(ScopeErrorType.REDECLARATION_ERROR, name);

            return _scopeManager.Define(name);
        }

        private Object IDU() {
            var name = _currentName;
            ID_();

            var obj = _scopeManager.Find(name);
            if (obj is null) {
                ScopeManager.ScopeError(ScopeErrorType.NOT_DECLARED_ERROR, name);
                _scopeManager.Define(name);
            }

            return obj;
        }

        private void NB() {
            _scopeManager.NewBlock();
        }

        private void EB() {
            _scopeManager.EndBlock();
        }

        private void END_() {
            if (_currentToken == END) return;
            SyntaxError(END);
        }

        private TypeAttribute T() {
            switch (_currentToken) {
                case INTEGER: Expect(INTEGER); return new TypeAttribute() { Type = TypeManager.Integer };
                case CHAR: Expect(CHAR); return new TypeAttribute() { Type = TypeManager.Char };
                case BOOLEAN: Expect(BOOLEAN); return new TypeAttribute() { Type = TypeManager.Bool };
                case STRING: Expect(STRING); return new TypeAttribute() { Type = TypeManager.String };
                case ID:
                    var obj = IDU();
                    if (obj.Kind.IsType() || obj.Kind == Kind.UNIVERSAL_)
                        return new TypeAttribute() { Type = obj };

                    TypeManager.TypeError();
                    return new TypeAttribute() { Type = TypeManager.Universal };

                default: SyntaxError(); return new TypeAttribute() { Type = TypeManager.Universal };
            }
        }

        private void LV() {
            IDU();
            LV_();
        }

        private void LV_() {
            switch (_currentToken) {
                case DOT: Expect(DOT); IDU(); LV_(); break;
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
                case ID: IDU(); F_(); break;
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

        private IEnumerable<Object> LI() {
            List<Object> list = new() { IDD() };

            while (_currentToken == COMMA) {
                Expect(COMMA);
                list.Add(IDD());
            }

            return list;
        }
        private void DV() {
            Expect(VAR);
            var list = LI();
            Expect(COLON);
            var typeAttr = T();
            Expect(SEMI_COLON);

            foreach (var obj in list) {
                obj.Kind = Kind.VARIABLE_;
                obj.Type = typeAttr.Type;
            }
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
            IDD();
            Expect(COLON);
            T();

            while (_currentToken == COMMA) {
                Expect(COMMA);
                IDD();
                Expect(COLON);
                T();
            }
        }

        private void DF() {
            Expect(FUNCTION);
            IDD();
            NB();
            Expect(LEFT_PARENTHESIS);
            LP();
            Expect(RIGHT_PARENTHESIS);
            Expect(COLON);
            T();
            B();
            EB();
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
            IDD();
            Expect(EQUALS);
            switch (_currentToken) {
                case ARRAY: Expect(ARRAY); Expect(LEFT_SQUARE); NUM(); Expect(RIGHT_SQUARE); Expect(OF); T(); break;
                case STRUCT: Expect(STRUCT); NB(); Expect(LEFT_BRACES); DC(); Expect(RIGHT_BRACES); EB(); break;
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
