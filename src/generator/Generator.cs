using System.Diagnostics;
using PascalCompiler.Semantics;

namespace PascalCompiler.AsmGenerator
{
    public class Generator
    {
        private int _constantCounter = 0;
        private int _labelCounter = 0;

        public Generator(SymStack stack)
        {
            HeaderParts = new List<AsmPart>();
            BDataParts = new List<AsmPart>();
            CodeParts = new List<AsmPart>();
            DataParts = new List<AsmPart>();
            SymStack = stack;

            GenConstant("double_minus", -1.0);
            GenConstant("integer_template", $"\"%d\", 0xA, 0");
            GenConstant("double_template", $"\"%f\", 0xA, 0");
            GenConstant("char_template", $"\"%c\", 0xA, 0");
        }

        public List<AsmPart> HeaderParts { get; }
        public List<AsmPart> BDataParts { get; }
        public List<AsmPart> CodeParts { get; }
        public List<AsmPart> DataParts { get; }
        public SymStack SymStack { get; }

        public void GenCommand(Instruction instruction) =>
            CodeParts.Add(new Command(instruction));

        public void GenCommand(Instruction instruction, Operand operand1) =>
            CodeParts.Add(new Command(instruction, operand1));

        public void GenCommand(Instruction instruction, Operand operand1, Operand operand2) =>
            CodeParts.Add(new Command(instruction, operand1, operand2));

        public string GenLabel()
        {
            _labelCounter++;
            CodeParts.Add(new Label($"label_${_labelCounter}"));
            return $"label_${_labelCounter}";
        }

        public void GenLabel(string labelName) => CodeParts.Add(new Label(labelName));

        public string GenConstant(object value)
        {
            _constantCounter++;
            var instruction = value is double ? Instruction.DQ :
                              value is int ? Instruction.DD : Instruction.DB;
            DataParts.Add(new Data(instruction, $"const_val_{_constantCounter}", value));
            return $"const_val_{_constantCounter}";
        }

        public string GenConstant(string label, object value)
        {
            var instruction = value is double ? Instruction.DQ :
                              value is int ? Instruction.DD : Instruction.DB;
            DataParts.Add(new Data(instruction, label, value));
            return label;
        }

        public string GenVariable(string label, Instruction instruction, object value)
        {
            BDataParts.Add(new Data(instruction, $"var_{label}", value));
            return $"var_{label}";
        }

        public void AddModule(Instruction instruction, string libraryName) =>
            HeaderParts.Add(new Library(instruction, libraryName));

        public void GenerateProgram()
        {
            // create string writer
            var found = new StringWriter();
            Console.SetOut(found);

            foreach (var command in HeaderParts)
                Console.WriteLine(command);

            Console.WriteLine("SECTION .bss");
            foreach (var command in BDataParts)
                Console.WriteLine(command);

            Console.WriteLine("SECTION .text");

            foreach (var command in CodeParts)
                Console.WriteLine(command);

            Console.WriteLine("SECTION .data");

            foreach (var command in DataParts)
                Console.WriteLine(command);

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

        public void CompileProgram()
        {
            GenerateProgram();

            var process = new Process();
            process.StartInfo.FileName = "./tests/asm/compile.bat";
            process.Start();
            process.WaitForExit();
            process.Close();
        }

        public void RunProgram()
        {
            GenerateProgram();
            CompileProgram();

            var process = new Process();
            process.StartInfo.FileName = "./tests/asm/program.exe";
            process.Start();
            process.WaitForExit();
            process.Close();
        }
    }
}