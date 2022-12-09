using PascalCompiler.LexicalAnalyzer;

namespace PascalCompiler.SyntaxAnalyzer.Nodes
{
    public abstract class SyntaxNode
    {
        public SyntaxNode(Lexeme? lexeme = null) => Lexeme = lexeme!;

        public Lexeme Lexeme { get; }

        abstract public void PrintTree(int depth = 0, string indent = "");
    }
}