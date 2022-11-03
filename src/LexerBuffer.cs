namespace LexicalAnalyzer
{
    struct Position
    {
        public int line = 1;
        public int ch = 0;

        public Position() { }
    }

    class LexerBuffer
    {
        private StreamReader fstream;
        private Position cursor = new Position();
        private Position lexemePos;
        private string buffer;
        private char? bufferedChar;

        public LexerBuffer()
        {
            string path = "./tests/" + Path.GetFileName(Directory.GetFiles("./tests", "*.in")[0]);
            this.fstream = new StreamReader(path);
            buffer = "";
        }
        public LexerBuffer(StreamReader fstream)
        {
            this.fstream = fstream;
            buffer = "";
        }

        protected string Buffer { get => buffer; }
        protected char CurrentChar { get => buffer.Last(); }
        protected bool EndOfStream { get => fstream.EndOfStream; }

        protected Position Cursor { get => cursor; set => cursor = value; }
        protected Position LexemePos { get => lexemePos; set => lexemePos = value; }

        /*
        - Next takes the next character from the stream and returns it.
        - TryNext write to buffer and returns 'true' if the next character
        matches with 'ch', otherwise just returns 'false'.
        - WriteToBuffer just write to buffer next character.
        */
        protected int Peek() => fstream.Peek();

        protected char Next()
        {
            var _ = (char)fstream.Read();
            cursor.line = (_ == '\n') ? ++cursor.line : cursor.line;
            cursor.ch = (_ == '\n' || _ == '\r') ? 0 : ++cursor.ch;
            return _;
        }

        protected void Back()
        {
            bufferedChar = CurrentChar;
            buffer = buffer.Substring(0, buffer.Length - 1);
        }

        protected bool TryNext(char ch)
        {
            if (fstream.Peek() != ch)
                return false;
            buffer += Next();
            return true;
        }

        protected void WriteToBuffer(string? str = null, bool resetBuffer = false)
        {
            if (resetBuffer)
                buffer = "";
            if (bufferedChar is not null)
            {
                buffer += bufferedChar;
                bufferedChar = null;
                return;
            }
            if (str == null)
                buffer += Next();
            else buffer += str;
        }

        public void ChangeFile(string path)
        {
            this.fstream = new StreamReader(path);
            lexemePos = cursor = new Position();
        }

        public void CloseFile() => fstream.Close();
    }
}