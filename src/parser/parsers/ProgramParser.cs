using PascalCompiler.Enums;
using PascalCompiler.Extensions;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        public SyntaxNode ParseProgram()
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

        public ProgramNode ParseProgramHeader()
        {
            var programName = ParseIdent();

            Require<Token>(new List<Token> { Token.SEMICOLOM }, true);

            return new ProgramHeaderNode(programName);
        }

        public ProgramNode ParseProgramBlock()
        {
            var decls = ParseDecls();
            var statement = ParseCompoundStmt();

            Require<Token>(new List<Token> { Token.DOT }, false);

            return new ProgramBlockNode(decls, statement);
        }
    }
}