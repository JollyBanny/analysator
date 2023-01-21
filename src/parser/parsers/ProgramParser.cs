using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private SyntaxNode ParseProgram()
        {
            ProgramNode? header = null;

            if (_currentLexeme == Token.PROGRAM)
            {
                NextLexeme();
                header = ParseProgramHeader();
            }

            var block = ParseProgramBlock();

            return new FullProgramNode(header, block);
        }

        private ProgramNode ParseProgramHeader()
        {
            var programName = ParseIdent();

            Require<Token>(true, Token.SEMICOLOM);

            return new ProgramHeaderNode(programName);
        }

        private ProgramNode ParseProgramBlock()
        {
            var decls = ParseDecls();
            var statement = ParseCompoundStmt();

            Require<Token>(false, Token.DOT);

            return new ProgramBlockNode(decls, statement);
        }
    }
}