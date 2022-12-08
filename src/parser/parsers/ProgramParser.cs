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
                _currentLexeme = _lexer.GetLexeme();
                header = ParseProgramHeader();
            }

            var block = ParseProgramBlock();

            return new FullProgramNode(header, block);
        }

        public ProgramNode ParseProgramHeader()
        {
            var programName = ParseIdent();

            if (_currentLexeme != Token.SEMICOLOM)
                throw ExpectedException(";", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new ProgramHeaderNode(programName);
        }

        public ProgramNode ParseProgramBlock()
        {
            var decls = ParseDecls();
            var statement = ParseCompoundStmt();
            return new ProgramBlockNode(decls, statement);
        }
    }
}