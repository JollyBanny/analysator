using System.Diagnostics;
using PascalCompiler.Semantics;

namespace PascalCompiler.AsmGenerator
{
    public class Generator
    {
        private int _constantCounter = 0;

        public Generator(SymStack stack)
        {
            HeaderParts = new List<AsmPart>();
            CodeParts = new List<AsmPart>();
            DataParts = new List<AsmPart>();
            SymStack = stack;
        }

        public List<AsmPart> HeaderParts { get; }
        public List<AsmPart> CodeParts { get; }
        public List<AsmPart> DataParts { get; }
        public SymStack SymStack { get; }

        public void GenCommand(Instruction instruction) =>
            CodeParts.Add(new Command(instruction));

        public void GenCommand(Instruction instruction, Operand operand1) =>
            CodeParts.Add(new Command(instruction, operand1));

        public void GenCommand(Instruction instruction, Operand operand1, Operand operand2) =>
            CodeParts.Add(new Command(instruction, operand1, operand2));

        public void GenLabel(string labelName) => CodeParts.Add(new Label(labelName));

        public void AddModule(Instruction instruction, string libraryName) =>
            HeaderParts.Add(new Library(instruction, libraryName));

        public string AddConstant(double value)
        {
            _constantCounter++;
            DataParts.Add(new Data(Instruction.DQ, $"const_val_{_constantCounter}", value));
            return $"const_val_{_constantCounter}";
        }

        public void PrintProgram()
        {
            // create string writer
            var found = new StringWriter();
            Console.SetOut(found);

            foreach (var command in HeaderParts)
                Console.WriteLine(command);

            Console.WriteLine("SECTION .text");

            foreach (var command in CodeParts)
                Console.WriteLine(command);

            Console.WriteLine("SECTION .data");

            foreach (var command in DataParts)
                Console.WriteLine(command);

            Console.WriteLine("double_template : db \"%f\", 0xA, 0");
            Console.WriteLine("integer_template : db \"%d\", 0xA, 0");

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