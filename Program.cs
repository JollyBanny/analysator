using PascalCompiler.Enums;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer;
using PascalCompiler.Visitor;

using System.Globalization;

namespace PascalCompiler
{
    public struct Position
    {
        public Position() { }

        public int Line = 1;
        public int Ch = 0;
    }

    public struct Pair<T>
    {
        public Pair(T first, T second)
        {
            First = first;
            Second = second;
        }

        public T First;
        public T Second;
    }

    public struct ProgramConfig
    {
        public ProgramConfig(CompilerFlag mode, CompilerFlag option, string filePath)
        {
            Mode = mode;
            Option = option;
            FilePath = filePath;
        }

        public CompilerFlag Mode { get; }
        public CompilerFlag Option { get; }
        public string FilePath { get; }
    }

    class Program
    {
        static void WrongArgs(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {message}\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Mode:");
            Console.ResetColor();
            Console.WriteLine(
                " -l [option] \t\t run lexical analysis\n" +
                " -p [option] \t\t run parser analysis\n" +
                " -s [option] \t\t run parser analysis with semantics tests\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Options:");
            Console.ResetColor();
            Console.WriteLine(
                " -t, --test \t\t run analyzer tests\n" +
                " -f, --file <path> \t run alanyzer with file");
        }

        static ProgramConfig ParseArguments(string[] args)
        {
            CompilerFlag mode = args[0] switch
            {
                "-l" or "--lexer" => CompilerFlag.Lexer,
                "-p" or "--parser" => CompilerFlag.Parser,
                "-s" or "--semantics" => CompilerFlag.Semantics,
                "-g" or "--generator" => CompilerFlag.Generator,
                _ => CompilerFlag.Invalid,
            };

            if (mode == CompilerFlag.Invalid)
                throw new Exception("Invalid mode");

            CompilerFlag option = args[1] switch
            {
                "-t" or "--test" => CompilerFlag.Test,
                "-f" or "--file" => CompilerFlag.File,
                _ => CompilerFlag.Invalid,
            };

            if (option == CompilerFlag.Invalid)
                throw new Exception("Invalid option");

            var path = "";
            if (option == CompilerFlag.File)
            {
                path = args[2];
                if (!File.Exists(path))
                    throw new Exception($"Invalid path {path}");
            }

            return new ProgramConfig(mode, option, path);
        }

        static public void RunLexer(string path)
        {
            Lexer _lexer = new Lexer(path);
            while (true)
            {
                try
                {
                    var lexeme = _lexer.GetLexeme();
                    Console.WriteLine(lexeme);
                    if (lexeme.Type == TokenType.EOF) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }

        static public void RunParser(string path, bool withSemantics)
        {
            try
            {
                Parser _parser = new Parser(path);
                var syntaxTree = _parser.Parse();
                syntaxTree.Accept(new SymVisitor(_parser._symStack));
                syntaxTree.Accept(new PrintVisitor()).PrintTree();

                if (withSemantics)
                    _parser.PrintTables();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static public void RunGenerator(string path)
        {
            try
            {
                Parser _parser = new Parser(path);

                var syntaxTree = _parser.Parse();
                syntaxTree.Accept(new SymVisitor(_parser._symStack));

                var generator = syntaxTree.Accept(new AsmVisitor(_parser._symStack));
                generator.PrintProgram();
                generator.RunProgram();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ProgramConfig programConfig = new ProgramConfig();

            try
            {
                if (args.Length == 0)
                    throw new Exception("No arguments");

                programConfig = ParseArguments(args);
            }
            catch (Exception e)
            {
                WrongArgs(e.Message);
            }

            if (programConfig.Option == CompilerFlag.Test)
                Tester.RunTests(programConfig.Mode);
            else
                switch (programConfig.Mode)
                {
                    case CompilerFlag.Lexer:
                        RunParser(programConfig.FilePath, programConfig.Mode == CompilerFlag.Semantics);
                        break;
                    case CompilerFlag.Parser:
                    case CompilerFlag.Semantics:
                        RunParser(programConfig.FilePath, programConfig.Mode == CompilerFlag.Semantics);
                        break;
                    case CompilerFlag.Generator:
                        RunGenerator(programConfig.FilePath);
                        break;
                }
        }
    }
}