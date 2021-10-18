using System;

namespace EcmaCompiler {
    public class SyntaxError : Exception {
        public SyntaxError(string? message) : base(message) { }
    }

    public class LexicalError : Exception {
        public LexicalError(string? message) : base(message) { }
    }

    public class ScopeError : Exception {
        public ScopeError() : base() { }

        public ScopeError(string? message) : base(message) { }
    }

    public enum ScopeErrorType {
        REDECLARATION_ERROR,
        NOT_DECLARED_ERROR
    }

    public class TypeError : Exception {
        public TypeError(string? message) : base(message) { }
    }
}
