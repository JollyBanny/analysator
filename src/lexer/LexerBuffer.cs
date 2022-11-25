namespace PascalCompiler.LexicalAnalyzer
{
    struct Position
    {
        public int Line = 1;
        public int Ch = 0;

        public Position() { }
    }

    class LexerBuffer
    {
        private StreamReader _fstream;
        private Position _cursor = new Position();
        private Position _lexemePos;
        private string _buffer;
        private char? _bufferedChar;

        public LexerBuffer()
        {
            var path = Directory.GetFiles("./tests/parser", "*.in")[13];
            _fstream = new StreamReader(path);
            _buffer = "";
        }
        public LexerBuffer(string path)
        {
            _fstream = new StreamReader(path);
            _buffer = "";
        }

        protected string Buffer { get => _buffer; }
        protected char CurrentChar { get => _buffer.Last(); }
        protected bool EndOfStream { get => _fstream.EndOfStream; }
        protected Position LexemePos { get => _lexemePos; set => _lexemePos = value; }
        public Position Cursor { get => _cursor; set => _cursor = value; }

        /*
        - Next takes the next character from the stream and returns it.
        - TryNext write to buffer and returns 'true' if the next character
        matches with 'ch', otherwise just returns 'false'.
        - WriteToBuffer just write to buffer next character.
        */
        protected int Peek() => _fstream.Peek();

        protected char Next()
        {
            var _ = (char)_fstream.Read();
            _cursor.Line = (_ == '\n') ? ++_cursor.Line : _cursor.Line;
            _cursor.Ch = (_ == '\n' || _ == '\r') ? 0 : ++_cursor.Ch;
            return _;
        }

        protected void Back()
        {
            _bufferedChar = CurrentChar;
            _buffer = _buffer[..^1];
        }

        protected bool TryNext(char ch)
        {
            if (_fstream.Peek() != ch)
                return false;
            _buffer += Next();
            return true;
        }

        protected void WriteToBuffer(string? str = null, bool resetBuffer = false)
        {
            if (resetBuffer)
                _buffer = "";
            if (_bufferedChar is not null)
            {
                _buffer += _bufferedChar;
                _bufferedChar = null;
                return;
            }
            if (str == null)
                _buffer += Next();
            else
                _buffer += str;
        }

        public void ChangeFile(string path)
        {
            _fstream = new StreamReader(path);
            _lexemePos = _cursor = new Position();
        }
    }
}