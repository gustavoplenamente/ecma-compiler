using EcmaCompiler.Parsers;
using static EcmaCompiler.Parsers.Kind;

namespace EcmaCompiler.Utils {
    public static class KindExtensions {
        public static bool IsType(this Kind kind) =>
            kind == ARRAY_TYPE_ ||
            kind == STRUCT_TYPE_ ||
            kind == ALIAS_TYPE_ ||
            kind == SCALAR_TYPE_;
    }
}
