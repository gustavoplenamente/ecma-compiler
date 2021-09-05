using NUnit.Framework;
using static EcmaCompiler.Tokens.Token;

namespace EcmaCompiler.Tokens.Tests {
    [TestFixture()]
    public class TokenManagerTests {
        private TokenManager TokenManager = new TokenManager();

        [TestCase("char", CHAR)]
        [TestCase("if", IF)]
        [TestCase("+", PLUS)]
        [TestCase(">=", GREATER_OR_EQUAL)]
        [TestCase("'Z'", CHARACTER)]
        [TestCase("'9'", CHARACTER)]
        [TestCase("'$'", CHARACTER)]
        [TestCase("\"Teste\"", STRINGVAL)]
        [TestCase("42", NUMERAL)]
        [TestCase("a1", ID)]
        [TestCase("1_var", UNKNOWN)]
        public void SearchKeywordTest(string name, Token expectedToken) {
            var actualToken = TokenManager.SearchKeyword(name);
            Assert.AreEqual(expectedToken, actualToken);
        }
    }
}