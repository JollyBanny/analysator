using System.Diagnostics;
using PascalCompiler.Semantics;

namespace PascalCompiler.AsmGenerator
{
    public class Generator
    {
        public Generator(SymStack stack)
        {
            HeaderParts = new List<AsmPart>();
            CodeParts = new List<AsmPart>();
            SymStack = stack;
        }

        public List<AsmPart> HeaderParts { get; }
        public List<AsmPart> CodeParts { get; }
        public SymStack SymStack { get; }

        public void GenCommand(Instruction instruction) =>
            CodeParts.Add(new Command(instruction));

        public void GenCommand(Instruction instruction, Operand operand1) =>
            CodeParts.Add(new Command(instruction, operand1));

        public void GenCommand(Instruction instruction, Operand operand1, Operand operand2) =>
            CodeParts.Add(new Command(instruction, operand1, operand2));

        public void GenLibrary(AccessModifier modifier, string libraryName) =>
            HeaderParts.Add(new Library(modifier, libraryName));

        public void GenLabel(string labelName) => CodeParts.Add(new Label(labelName));

        public void PrintProgram()
        {
            // create string writer
            var found = new StringWriter();
            Console.SetOut(found);

            foreach (var command in HeaderParts)
            {
                Console.WriteLine(command);
            }

            Console.WriteLine("SECTION .text");

            foreach (var command in CodeParts)
            {
                Console.WriteLine(command);
            }

            Console.WriteLine("section .data");
            Console.WriteLine("msg : db \"hello, world%f\", 0xA, 0");

            // write to asm file
            var testWritter = new StreamWriter("./tests/asm/program.asm");
            testWritter.AutoFlush = true;
            testWritter.Write(found);
            testWritter.Close();

            // set origin output
            var originOutput = new StreamWriter(Console.OpenStandardOutput());
            originOutput.AutoFlush = true;
            Console.SetOut(originOutput);
        }

        public void RunProgram()
        {
            var process = new Process();
            process.StartInfo.FileName = "./tests/asm/compile.bat";
            process.Start();
            process.WaitForExit();
            process.Close();
        }
    }
}