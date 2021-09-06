namespace EcmaCompiler.Utils {
    public static class CharExtensions {
        public static bool IsAlpha(this char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        public static bool IsDigit(this char c) => c >= '0' && c <= '9';
        public static bool IsAlphaNum(this char c) => c.IsAlpha() || c.IsDigit();
    }
}
