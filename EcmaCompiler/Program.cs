using EcmaCompiler.Tokens;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace EcmaCompiler {
    class Program {
        static void Main(string[] args) {
            var tokenManager = new TokenManager();
            var parser = new Parser(tokenManager);
            if (args.Length != 1) {
                Console.WriteLine("Invalid argument supplied to program: the input should be a single string containing the filename");
                return;
            }

            parser.Parse(args[0]);

            tokenManager.PrintTokens();
        }
    }
}
