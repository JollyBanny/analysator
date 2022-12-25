using System.Collections;
using PascalCompiler.Enums;
using PascalCompiler.Exceptions;
using PascalCompiler.Extensions;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Semantics;
using PascalCompiler.SyntaxAnalyzer.Nodes;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private Lexer _lexer;
        private Lexeme _currentLexeme;
        private SymVisitor _symVisitor;
        private SymStack _symStack = new SymStack();

        public Parser()
        {
            _lexer = new Lexer();
            _currentLexeme = _lexer.GetLexeme();
            _symVisitor = new SymVisitor(_symStack);
        }

        public Parser(string path)
        {
            _lexer = new Lexer(path);
            _currentLexeme = _lexer.GetLexeme();
            _symVisitor = new SymVisitor(_symStack);
        }

        public SyntaxNode Parse()
        {
            try
            {
                return ParseProgram();
            }
            catch (SemanticException e)
            {
                throw new SemanticException(_lexer.Cursor, e.Message);
            }
            catch (SyntaxException e)
            {
                throw new SyntaxException(_lexer.Cursor, e.Message);
            }
        }

        private void NextLexeme()
        {
            _currentLexeme = _lexer.GetLexeme();
        }

        private void Require<T>(bool getNext, params T[] tokens)
        {
            foreach (var token in tokens)
                if (_currentLexeme.Equals(token))
                {
                    if (getNext)
                        NextLexeme();
                    return;
                }

            if (tokens[0] is Token)
            {
                Token tok = (Token)(object)tokens[0]!;
                throw ExpectedException(tok.Stringify()!, _currentLexeme.Source);
            }

            throw ExpectedException(tokens[0]!.ToString()!, _currentLexeme.Source);
        }

        private SyntaxException ExpectedException(string expected, string found)
        {
            return new SyntaxException(_lexer.Cursor, $"'{expected}' expected but '{found}' found");
        }

        private SyntaxException FatalException(string msg)
        {
            return new SyntaxException(_lexer.Cursor, msg);
        }
    }
}