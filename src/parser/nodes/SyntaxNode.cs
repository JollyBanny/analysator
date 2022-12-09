using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class SyntaxNode
    {
        protected SyntaxNode(Lexeme? lexeme = null) => Lexeme = lexeme!;

        public Lexeme Lexeme { get; }

        public abstract void PrintTree(int depth = 0, string indent = "");
    }
}