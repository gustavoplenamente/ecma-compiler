using EcmaCompiler.Parsers;
using EcmaCompiler.Tokens;
using EcmaCompiler.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static EcmaCompiler.Tokens.Token;

namespace EcmaCompiler {
    public class SyntaxParser {
        private TokenManager _tokenManager;
        private ScopeManager _scopeManager;

        private (Token, int) _current = (UNKNOWN, 0);
        private Token _currentToken => _current.Item1;
        private int _currentName => _current.Item2;
        private int _currentIndex = -1;

        private StreamWriter _writer;

        private int _funcsNum = 0;
        private int _labelNum = 0;
        private Object _currentFunction;

        public SyntaxParser(TokenManager tokenManager, ScopeManager scopeManager, string outFilename) {
            _tokenManager = tokenManager;
            _scopeManager = scopeManager;

            _writer = new StreamWriter(outFilename);
        }

        public void Parse() {
            NextToken();
            P();
        }

        private void Expect(Token token) {
            if (_currentToken == token) {
                NextToken();
            } else {
                SyntaxError(token);
            }
        }

        private void NextToken() {
            _currentIndex++;
            _current = _tokenManager.GetToken(_currentIndex);
        }

        private int NewLabel() => _labelNum++;

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

        private void NF(ref Object obj) {
            obj.Kind = Kind.FUNCTION_;
            obj.ParamsNum = 0;
            obj.VarsNum = 0;
            obj.IndexNum = _funcsNum++;

            _scopeManager.NewBlock();
        }

        private void MF(ref Object obj, IEnumerable<Object> paramList, int size, Object type) {
            obj.ReturnType = type;
            obj.Params = paramList;
            obj.ParamsNum = size;
            obj.VarsNum = size;

            _currentFunction = obj;

            WriteCode($"BEGIN_FUNC {_funcsNum - 1}, {obj.ParamsNum}, 0");

            _scopeManager.NewBlock();
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

        private void WriteCode(string code) => _writer.WriteLine(code);

        private int MT() {
            var label = NewLabel();
            WriteCode($"TJMP_FW L{label}");

            return label;
        }

        private int ME(int prevLabel) {
            var nextLabel = NewLabel();

            WriteCode($"JMP_FW L{nextLabel}");
            WriteCode($"L{prevLabel}:");

            return nextLabel;
        }

        private int MW() {
            var label = NewLabel();
            WriteCode($"L{label}:");

            return label;
        }

        private TypeAttribute T() {
            switch (_currentToken) {
                case INTEGER: Expect(INTEGER); return new TypeAttribute() { Type = TypeManager.Integer, Size = 1 };
                case CHAR: Expect(CHAR); return new TypeAttribute() { Type = TypeManager.Char, Size = 1 };
                case BOOLEAN: Expect(BOOLEAN); return new TypeAttribute() { Type = TypeManager.Bool, Size = 1 };
                case STRING: Expect(STRING); return new TypeAttribute() { Type = TypeManager.String, Size = 1 };
                case ID:
                    var obj = IDU();
                    if (obj.Kind.IsType() || obj.Kind == Kind.UNIVERSAL_)
                        return new TypeAttribute() { Type = obj, Size = obj.Size };

                    TypeManager.TypeError();
                    return new TypeAttribute() { Type = TypeManager.Universal, Size = 0 };

                default: SyntaxError(); return new TypeAttribute() { Type = TypeManager.Universal, Size = 0 };
            }
        }

        private Object LV() {
            var obj1 = IDU();
            var obj2 = LV_();

            var obj = obj2 ?? obj1;

            WriteCode($"LOAD_REF {obj.Type.Size}");
            return obj;
        }

        private Object LV_() {
            Object obj1;
            Object obj2;

            switch (_currentToken) {
                case DOT:
                    Expect(DOT);
                    obj1 = IDU();
                    if (obj1 != null)
                        WriteCode($"ADD {obj1.IndexNum}");
                    obj2 = LV_();
                    return obj2 ?? obj1;
                case LEFT_SQUARE:
                    Expect(LEFT_SQUARE); obj1 = E(); Expect(RIGHT_SQUARE);
                    WriteCode($"MUL {obj1.ElementType.Size}");
                    obj2 = LV_();
                    return obj2 ?? obj1.ElementType;
            }

            return null;
        }

        private Object LE() {
            E();
            return LE_();
        }

        private Object LE_() {
            Object obj = null;

            while (_currentToken == COMMA) {
                Expect(COMMA);
                obj = E();
            }

            return obj;
        }

        private Object F__() {
            switch (_currentToken) {
                case PLUS_PLUS: Expect(PLUS_PLUS); return TypeManager.Integer;
                case MINUS_MINUS: Expect(MINUS_MINUS); return TypeManager.Integer;
            }

            return null;
        }

        private Object F_(Object obj_) {
            switch (_currentToken) {
                case LEFT_PARENTHESIS:
                    Expect(LEFT_PARENTHESIS); var obj = LE(); Expect(RIGHT_PARENTHESIS);
                    WriteCode($"CALL {obj_.IndexNum}");
                    return obj;
                default: LV_(); obj = F__(); return obj;
            }
        }

        private Object F() {
            switch (_currentToken) {
                case ID: var obj_ = IDU(); var obj = F_(obj_); return obj;
                case PLUS_PLUS: Expect(PLUS_PLUS); LV(); return TypeManager.Integer;
                case MINUS_MINUS: Expect(MINUS_MINUS); LV(); return TypeManager.Integer;
                case LEFT_PARENTHESIS: Expect(LEFT_PARENTHESIS); var scalar = E(); Expect(RIGHT_PARENTHESIS); return scalar.Type;
                case MINUS: Expect(MINUS); F(); return TypeManager.Integer;
                case NOT: Expect(NOT); F(); return TypeManager.Bool;
                case TRUE: TRUE_(); return TypeManager.Bool;
                case FALSE: FALSE_(); return TypeManager.Bool;
                case CHARACTER: CHR(); return TypeManager.Char;
                case STRINGVAL: STR(); return TypeManager.String;
                case NUMERAL: NUM(); return TypeManager.Integer;
                default: SyntaxError(); return null;
            }
        }

        private Object Y_() {
            switch (_currentToken) {
                case TIMES:
                    Expect(TIMES);
                    WriteCode("MUL");
                    F(); Y_(); return TypeManager.Integer;
                case DIVIDE:
                    Expect(DIVIDE);
                    WriteCode("DIV");
                    F(); Y_(); return TypeManager.Integer;
            }

            return null;
        }

        private Object Y() {
            var scalar = F();
            if (scalar != null) return scalar;

            return Y_();
        }

        private Object R_() {
            switch (_currentToken) {
                case PLUS:
                    Expect(PLUS);
                    WriteCode("ADD");
                    Y(); R_(); return TypeManager.Integer;
                case MINUS:
                    Expect(MINUS);
                    WriteCode("SUB");
                    Y(); R_(); return TypeManager.Integer;
            }

            return null;
        }

        private Object R() {
            var scalar = Y();
            if (scalar != null) return scalar;

            return R_();
        }

        private Object L_() {
            switch (_currentToken) {
                case LESS_THAN:
                    Expect(LESS_THAN);
                    WriteCode("LT");
                    R(); L_(); return TypeManager.Bool;
                case GREATER_THAN:
                    Expect(GREATER_THAN);
                    WriteCode("GT");
                    R(); L_(); return TypeManager.Bool;
                case LESS_OR_EQUAL:
                    Expect(LESS_OR_EQUAL);
                    WriteCode("LE");
                    R(); L_(); return TypeManager.Bool;
                case GREATER_OR_EQUAL:
                    Expect(GREATER_OR_EQUAL);
                    WriteCode("GE");
                    R(); L_(); return TypeManager.Bool;
                case EQUAL_EQUAL:
                    Expect(GREATER_THAN);
                    WriteCode("EQ");
                    R(); L_(); return TypeManager.Bool;
                case NOT_EQUAL:
                    Expect(NOT_EQUAL);
                    WriteCode("NE");
                    R(); L_(); return TypeManager.Bool;
            }

            return null;
        }

        private Object L() {
            var scalar = R();
            if (scalar != null) return scalar;

            return L_();
        }

        private Object E_() {
            switch (_currentToken) {
                case AND:
                    Expect(AND);
                    WriteCode("AND");
                    L(); E_(); return TypeManager.Bool;
                case OR:
                    Expect(OR);
                    WriteCode("OR");
                    L(); E_(); return TypeManager.Bool;
            }

            return null;
        }

        private Object E() {
            var scalar = L();
            if (scalar != null) return scalar;

            return E_();
        }

        private void S() {
            switch (_currentToken) {
                case IF:
                    Expect(IF); Expect(LEFT_PARENTHESIS); E(); Expect(RIGHT_PARENTHESIS);
                    var label1 = MT();
                    S();
                    if (_currentToken == ELSE) {
                        Expect(ELSE);
                        var label2 = ME(label1);
                        S();
                        WriteCode($"L{label2}:");
                    }
                    break;
                case WHILE:
                    Expect(WHILE);
                    var prevLabel = MW();
                    Expect(LEFT_PARENTHESIS); E();
                    var nextLabel = MT();
                    Expect(RIGHT_PARENTHESIS); S();

                    WriteCode($"JMP_BW L{prevLabel}");
                    WriteCode($"L{nextLabel}:");
                    break;
                case DO:
                    Expect(DO);
                    var label = MW();
                    S(); Expect(WHILE); Expect(LEFT_PARENTHESIS); E(); Expect(RIGHT_PARENTHESIS); Expect(SEMI_COLON);
                    WriteCode("NOT");
                    WriteCode($"TJMP_BW L{label}");
                    break;
                case BREAK: Expect(BREAK); Expect(SEMI_COLON); break;
                case CONTINUE: Expect(CONTINUE); Expect(SEMI_COLON); break;
                case RETURN: Expect(RETURN); ID_(); Expect(SEMI_COLON); break;
                case LEFT_BRACES: B(); break;
                case ID:
                    LV(); Expect(EQUALS);
                    var t = E();
                    Expect(SEMI_COLON);
                    WriteCode($"STORE_REF {t.Size}");
                    break;
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
            var t = T().Type;
            Expect(SEMI_COLON);
            var n = _currentFunction.VarsNum;

            foreach (var obj in list) {
                obj.Kind = Kind.VARIABLE_;
                obj.Type = t;

                obj.IndexNum = n;
                obj.Size = t.Size;

                n += t.Size;
            }

            _currentFunction.VarsNum = n;
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

            WriteCode("END_FUNC");
        }

        private (List<Object>, int) LP() {
            List<Object> objs = new();

            var obj = IDD();
            Expect(COLON);
            var t = T().Type;
            var n = 0;

            obj.Kind = Kind.PARAMETER_;
            obj.Type = t;
            obj.IndexNum = n;
            obj.Size = t.Size;
            n += t.Size;

            objs.Add(obj);

            while (_currentToken == COMMA) {
                Expect(COMMA);

                obj = IDD();
                Expect(COLON);
                t = T().Type;

                obj.Kind = Kind.PARAMETER_;
                obj.Type = t;
                obj.IndexNum = n;
                obj.Size = t.Size;
                n += t.Size;

                objs.Add(obj);
            }

            var size = objs.Aggregate(0, (acc, x) => acc + x.Size);

            return (objs, size);
        }

        private void DF() {
            Expect(FUNCTION);
            var obj = IDD();
            NF(ref obj);
            Expect(LEFT_PARENTHESIS);
            var (paramList, size) = LP();
            Expect(RIGHT_PARENTHESIS);
            Expect(COLON);
            var t = T().Type;
            MF(ref obj, paramList, size, t);
            B();
            EB();
        }

        private (IEnumerable<Object>, int) DC() {
            Object t;
            int n = 0;

            var fields = LI();
            Expect(COLON);
            t = T().Type;

            foreach (var field in fields) {
                field.Kind = Kind.FIELD_;
                field.Type = t;
                field.IndexNum = n;
                field.Size = t.Size;

                n += t.Size;
            }

            while (_currentToken == SEMI_COLON) {
                Expect(SEMI_COLON);
                var fields_ = LI();
                Expect(COLON);
                t = T().Type;

                foreach (var field_ in fields_) {
                    field_.Kind = Kind.FIELD_;
                    field_.Type = t;
                    field_.IndexNum = n;
                    field_.Size = t.Size;

                    n += t.Size;
                }

                fields.Concat(fields_);
            }

            return (fields, n);
        }

        private Object DT() {
            Expect(TYPE);
            var obj = IDD();
            Expect(EQUALS);
            switch (_currentToken) {
                case ARRAY:
                    Expect(ARRAY); Expect(LEFT_SQUARE);
                    var n = (int)NUM().Val;
                    Expect(RIGHT_SQUARE); Expect(OF);
                    var t = T().Type;

                    obj.Kind = Kind.ARRAY_TYPE_;
                    obj.ElementsNum = n;
                    obj.ElementType = t;
                    obj.Size = n * t.Size;
                    return obj;

                case STRUCT:
                    Expect(STRUCT); NB(); Expect(LEFT_BRACES);
                    var (fields, size) = DC();
                    Expect(RIGHT_BRACES); EB();

                    obj.Kind = Kind.STRUCT_TYPE_;
                    obj.Fields = fields.ToList();
                    obj.Size = size;
                    return obj;

                default:
                    t = T().Type;
                    obj.Kind = Kind.ALIAS_TYPE_;
                    obj.BaseType = t;
                    obj.Size = t.Size;
                    return obj;
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
