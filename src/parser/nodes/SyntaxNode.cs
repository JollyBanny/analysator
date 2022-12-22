using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.Visitor;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class SyntaxNode
    {
        protected SyntaxNode(Lexeme? lexeme = null) => Lexeme = lexeme!;

        public Lexeme Lexeme { get; }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}