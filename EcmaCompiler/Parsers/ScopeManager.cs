using System.Collections.Generic;

namespace EcmaCompiler.Parsers {
    public class ScopeManager {
        private static int MAX_COMPLEXITY = 16;
        private int _currentLevel = 0;
        private List<Object> _symbolTable = new(MAX_COMPLEXITY) { null };
        private List<Object> _symbolTableLast = new(MAX_COMPLEXITY) { null };

        public void NewBlock() {
            _currentLevel++;
            _symbolTable.Add(null);
            _symbolTableLast.Add(null);
        }

        public void EndBlock() {
            _currentLevel--;
        }

        public Object Define(int name) {
            var obj = new Object(name, null);

            if (_symbolTable[_currentLevel] is null) {
                _symbolTable[_currentLevel] = obj;
                _symbolTableLast[_currentLevel] = obj;
            } else {
                _symbolTableLast[_currentLevel].Next = obj;
                _symbolTableLast[_currentLevel] = obj;
            }

            return obj;
        }

        public Object Search(int name) {
            var obj = _symbolTable[_currentLevel];

            while (obj != null) {
                if (obj.Name == name) break;
                obj = obj.Next;
            }

            return obj;
        }

        public Object Find(int name) {
            Object obj = null;

            for (int i = _currentLevel; i >= 0; i--) {
                obj = _symbolTable[i];
                while (obj != null) {
                    if (obj.Name == name) break;
                    obj = obj.Next;
                }

                if (obj != null) break;
            }

            return obj;
        }

        public static void ScopeError(ScopeErrorType type, int name) {
            switch (type) {
                case ScopeErrorType.REDECLARATION_ERROR: throw new ScopeError($"Redeclaration of identifier {name}.");
                case ScopeErrorType.NOT_DECLARED_ERROR: throw new ScopeError($"Identifier {name} was not declared.");
                default: throw new ScopeError();
            }
        }

    }
}
;