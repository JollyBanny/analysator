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
            Console.WriteLine($"[ERROR] {message}\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Mode:");
            Console.ResetColor();
            Console.WriteLine(
                " -l [option | path] \t run lexical analysis\n" +
                " -p [option | path] \t run parser analysis\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Options:");
            Console.ResetColor();
            Console.WriteLine(
                " --test \t run analyzer tests\n\n" +
                "Note: If it runs without --test path option is required");
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

        static bool ValidateArgs(string[] args, string mode, string[] options, string path)
        {
            if (!new string[] { "-l", "-p" }.Contains(mode))
            {
                WrongArgs("Invalid mode");
                return false;
            }

            string[] availableOptions = { "--test" };
            var _ = options.Where(o => availableOptions.Contains(o));
            if (_.Count() != options.Count())
            {
                WrongArgs("Invalid option");
                return false;
            }

            if (options.Count() == 0 && !File.Exists(path))
            {
                WrongArgs($"Invalid path {path}");
                return false;
            }

            return true;
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
        }

        static public void RunParser(string path)
        {
            Parser _parser = new Parser(path);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            _parser.ParseExpression().PrintTree();
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
            if (!ValidateArgs(args, mode, options, path))
                return;

            switch (mode)
            {
                case "-p":
                    if (options.Count() > 0 && options.Contains("--test"))
                        LexerTester.RunTests();
                    else
                        RunParser(path);
                    break;
                case "-l":
                    if (options.Count() > 0 && options.Contains("--test"))
                        LexerTester.RunTests();
                    else RunLexer(path);
                    break;
                default:
                    Console.WriteLine("Unknown argument");
                    break;
            }
        }
    }
}