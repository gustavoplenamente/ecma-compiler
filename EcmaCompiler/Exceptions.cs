using System;

namespace EcmaCompiler {
    public class SyntaxError : Exception {
        public SyntaxError(string? message) : base(message) { }
    }
}
