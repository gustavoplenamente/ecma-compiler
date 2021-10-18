using EcmaCompiler.Tokens;

namespace EcmaCompiler {
    class Program {
        static void Main(string[] args) {
            var tokenManager = new TokenManager();
            var lexicalParser = new LexicalParser(tokenManager);
            var syntaxParser = new SyntaxParser(tokenManager);

            //if (args.Length != 1) {
            //    Console.WriteLine("Invalid argument supplied to program: the input should be a single string containing the filename");
            //    return;
            //}

            //parser.Parse(args[0]);
            lexicalParser.Parse("../../../Examples/syntax-success.ssl");
            syntaxParser.Parse();


            tokenManager.PrintTokens();
        }
    }
}
