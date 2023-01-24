using PascalCompiler.Enums;
using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer;
using PascalCompiler.AsmGenerator;
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
        public ProgramConfig(CompilerFlag mode, CompilerFlag generatorMode, CompilerFlag option, string filePath)
        {
            Mode = mode;
            GeneratorMode = generatorMode;
            Option = option;
            FilePath = filePath;
        }

        public CompilerFlag Mode { get; }
        public CompilerFlag GeneratorMode { get; }
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
                " -l | --lexer      \t  <run mode>                   \t  run lexical analysis\n" +
                " -p | --parser     \t  <run mode>                   \t  run parser analysis\n" +
                " -s | --semantics  \t  <run mode>                   \t  run parser analysis with semantics tests\n" +
                " -g | --generator  \t  <generator mode> <run mode>  \t  run code generator\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Generator mode:");
            Console.ResetColor();
            Console.WriteLine(
                " -gen  | --generate    \t  generate NASM code and stop\n" +
                " -comp | --compile     \t  generate NASM code and compile it\n" +
                " -exec | --execute     \t  generate NASM code, compile and run it\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Run mode:");
            Console.ResetColor();

            Console.WriteLine(
                " -t, --test \t\t run analyzer tests\n" +
                " -f, --file <path> \t run alanyzer with file");
        }

        static ProgramConfig ParseArguments(string[] args)
        {
            var argPos = 0;

            CompilerFlag mode = args[argPos] switch
            {
                "-l" or "--lexer" => CompilerFlag.Lexer,
                "-p" or "--parser" => CompilerFlag.Parser,
                "-s" or "--semantics" => CompilerFlag.Semantics,
                "-g" or "--generator" => CompilerFlag.Generator,
                _ => throw new Exception("Invalid mode"),
            };
            ++argPos;

            CompilerFlag generatorMode = args[argPos] switch
            {
                "-gen" or "--generate" when mode == CompilerFlag.Generator => CompilerFlag.Generate,
                "-comp" or "--compile" when mode == CompilerFlag.Generator => CompilerFlag.Compile,
                "-exec" or "--execute" when mode == CompilerFlag.Generator => CompilerFlag.Execute,
                { } when mode == CompilerFlag.Generator => throw new Exception("Invalid generator mode"),
                _ => CompilerFlag.Invalid
            };
            argPos = generatorMode == CompilerFlag.Invalid ? argPos : ++argPos;

            CompilerFlag option = args[argPos] switch
            {
                "-t" or "--test" => CompilerFlag.Test,
                "-f" or "--file" => CompilerFlag.File,
                _ => throw new Exception("Invalid option"),
            };
            ++argPos;

            var path = option == CompilerFlag.File ? args[argPos] : "";
            if (option == CompilerFlag.File && !File.Exists(path))
                throw new Exception($"Invalid path {path}");

            return new ProgramConfig(mode, generatorMode, option, path);
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

        static public void RunGenerator(string path, CompilerFlag generateMode)
        {
            try
            {
                Parser _parser = new Parser(path);

                var syntaxTree = _parser.Parse();
                syntaxTree.Accept(new SymVisitor(_parser._symStack));

                var generator = new Generator(_parser._symStack);
                syntaxTree.Accept(new AsmVisitor(generator), false);

                switch (generateMode)
                {
                    case CompilerFlag.Generate: generator.GenerateProgram(); break;
                    case CompilerFlag.Compile: generator.CompileProgram(); break;
                    case CompilerFlag.Execute:
                        var output = generator.RunProgram();
                        Console.WriteLine(output);
                        break;
                };
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
                        RunGenerator(programConfig.FilePath, programConfig.GeneratorMode);
                        break;
                }
        }
    }
}