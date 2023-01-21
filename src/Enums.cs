namespace PascalCompiler.Enums
{
    public enum TableLinePos
    {
        Start,
        Center,
        End,
    }

    public enum CompilerFlag
    {
        Lexer,
        Parser,
        Semantics,
        Generator,

        File,
        Test,

        Invalid,
    }
}