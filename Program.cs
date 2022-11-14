using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SyntaxAnalyzer;
using PascalCompiler.Enums;

namespace PascalCompiler
{
    class Program
    {
        static void WrongArgs(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            Console.WriteLine(
                "Options:\n--test \t run analyzer tests\n\n" +
                "Mode:\n-l [option | path] \t run lexical analysis\n" +
                "-p [option | path] \t run parser analysis\n"
            );
            Console.WriteLine("Note: If it runs without --test path option is required");
            return;
        }

        static string GetPath(string[] args, int optionsCount)
        {
            var _ = args.Skip(1 + optionsCount);
            if (_.Count() > 0) return _.First();
            else return "";
        }

        static string[] GetOptions(string[] args)
        {
            var options = new List<string>();
            foreach (var option in args.Skip(1))
            {
                if (option.StartsWith("--"))
                    options.Add(option);
                else
                    break;
            }
            return options.ToArray();
        }

        static void ValidateArgs(string[] args, string mode, string[] options, string path)
        {
            if (!new string[] { "-l", "-p" }.Contains(mode))
            {
                WrongArgs("Invalid mode");
                return;
            }

            string[] availableOptions = { "--test" };
            var _ = options.Where(o => availableOptions.Contains(o));
            if (_.Count() != options.Count())
            {
                WrongArgs("Invalid option");
                return;
            }

            if (options.Count() == 0 && !File.Exists(path))
            {
                WrongArgs($"Invalid path {path}");
                return;
            }
        }

        static public void RunLexer(string path)
        {
            Lexer _lexer = new Lexer(path);
            while (true)
            {
                try
                {
                    var lexeme = _lexer.GetLexem();
                    Console.WriteLine(lexeme);
                    if (lexeme.Type == TokenType.EOF) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            _lexer.CloseFile();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                WrongArgs("No arguments");
                return;
            }

            string mode = args[0];
            var options = GetOptions(args);
            string path = GetPath(args, options.Count());
            ValidateArgs(args, mode, options, path);

            switch (mode)
            {
                case "-p":
                    ParserTester.AnalyzeFile(path);
                    break;
                case "-l":
                    if (options.Count() > 0)
                    {
                        if (options.Contains("--test"))
                            LexerTester.RunTests();
                    }
                    else RunLexer(path);
                    break;
                default:
                    Console.WriteLine("Unknown argument");
                    break;
            }
        }
    }
}