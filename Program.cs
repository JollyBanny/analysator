using PascalCompiler.LexicalAnalyzer;
using PascalCompiler.SimpleSyntaxAnalyzer;
using PascalCompiler.SyntaxAnalyzer;
using PascalCompiler.Enums;

using System.Globalization;

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
                " -l [option] \t\t run lexical analysis\n" +
                " -sp [option] \t\t run simple_parser analysis\n" +
                " -p [option] \t\t run parser analysis\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Options:");
            Console.ResetColor();
            Console.WriteLine(
                " -t, --test \t\t run analyzer tests\n" +
                " -f, --file <path> \t run alanyzer with file");
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
                if (option.StartsWith("--") || option.StartsWith("-"))
                    options.Add(option);
                else
                    break;
            }
            return options.ToArray();
        }

        static bool ValidateArgs(string[] args, string mode, string[] options, string path)
        {
            if (!new string[] { "-l", "-sp", "-p" }.Contains(mode))
            {
                WrongArgs("Invalid mode");
                return false;
            }

            string[] availableOptions = { "--test", "--file", "-t", "-f" };
            var _ = options.Where(o => availableOptions.Contains(o));
            if (_.Count() != options.Count())
            {
                WrongArgs("Invalid option");
                return false;
            }

            if (options.Contains("--file") || options.Contains("-f") || options.Length == 0)
            {
                if (path == string.Empty)
                {
                    WrongArgs("Path argument is missing");
                    return false;
                }
                else if (!File.Exists(path))
                {
                    WrongArgs($"Invalid path {path}");
                    return false;
                }
            }

            return true;
        }

        static public void RunLexer(string path)
        {
            Lexer _lexer = path == string.Empty ? new Lexer() : new Lexer(path);
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

        static public void RunSimpleParser(string path)
        {
            try
            {
                SimpleParser _simpleParser = path == string.Empty ?
                    new SimpleParser() : new SimpleParser(path);
                _simpleParser.ParseExpression().PrintTree();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static public void RunParser(string path)
        {
            try
            {
                Parser _parser = path == string.Empty ? new Parser() : new Parser(path);
                _parser.ParseVarDecl();
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
                case "-sp":
                    if (options.Count() > 0 &&
                        (options.Contains("--test") || options.Contains("-t")))
                        Tester.RunTests(mode);
                    else
                        RunSimpleParser(path);
                    break;
                case "-p":
                    if (options.Count() > 0 &&
                        (options.Contains("--test") || options.Contains("-t")))
                        Tester.RunTests(mode);
                    else
                        RunParser(path);
                    break;
                case "-l":
                    if (options.Count() > 0 &&
                        (options.Contains("--test") || options.Contains("-t")))
                        Tester.RunTests(mode);
                    else
                        RunLexer(path);
                    break;
                default:
                    Console.WriteLine("Unknown argument");
                    break;
            }
        }
    }
}