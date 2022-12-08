using PascalCompiler.Enums;
using PascalCompiler.Extensions;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.SyntaxAnalyzer
{
    public partial class Parser
    {
        private static readonly List<Token> AssignOperators = new List<Token>
        {
            Token.ASSIGN, Token.ADD_ASSIGN, Token.SUB_ASSIGN, Token.MUL_ASSIGN,
            Token.DIV_ASSIGN,
        };

        public StmtNode ParseCompoundStmt()
        {
            if (_currentLexeme != Token.BEGIN)
                throw ExpectedException($"{Token.BEGIN}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var statements = ParseStatements();

            if (_currentLexeme != Token.END)
                throw ExpectedException($"{Token.END}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new CompoundStmtNode(statements);
        }

        public List<StmtNode> ParseStatements()
        {
            var statements = new List<StmtNode>();
            var separatorExist = true;

            if (_currentLexeme == Token.END)
            {
                statements.Add(ParseSimpleStmt());
                return statements;
            }

            while (_currentLexeme != Token.END)
            {
                if (!separatorExist)
                    throw ExpectedException(";", _currentLexeme.Source);

                var statement = ParseStatement();
                separatorExist = false;
                statements.Add(statement);

                if (_currentLexeme == Token.SEMICOLOM)
                {
                    separatorExist = true;
                    _currentLexeme = _lexer.GetLexeme();
                }
            }

            return statements;
        }

        public StmtNode ParseStatement()
        {
            var statement = ParseStructStmt();

            if (statement is null)
                statement = ParseSimpleStmt();

            return statement;
        }

        public StmtNode ParseSimpleStmt()
        {
            if (_currentLexeme == Token.END)
                return new EmptyStmtNode();

            return ParseAssignStatement();
        }

        public StmtNode ParseAssignStatement()
        {
            var left = ParseExpression();
            var lexeme = _currentLexeme;

            if (left is FunctionCallNode)
                return new CallStmtNode(left);

            if (!AssignOperators.Contains(lexeme))
                throw FatalException("Illegal expression");
            _currentLexeme = _lexer.GetLexeme();

            return new AssignStmtNode(lexeme, left, ParseExpression());
        }

        public StmtNode? ParseStructStmt()
        {
            return _currentLexeme.Value switch
            {
                Token.IF => ParseIfStmt(),
                Token.WHILE => ParseWhileStmt(),
                Token.REPEAT => ParseRepeatStmt(),
                Token.FOR => ParseForStmt(),
                Token.BEGIN => ParseCompoundStmt(),
                _ => null
            };
        }

        public StmtNode ParseIfStmt()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var condition = ParseExpression();

            if (_currentLexeme != Token.THEN)
                throw ExpectedException($"{Token.THEN}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var ifPart = ParseStatement();
            StmtNode? elsePart = null;

            if (_currentLexeme == Token.ELSE)
                elsePart = ParseStatement();

            return new IfStmtNode(lexeme, condition, ifPart, elsePart);
        }

        public StmtNode ParseWhileStmt()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var condition = ParseExpression();

            if (_currentLexeme != Token.DO)
                throw ExpectedException($"{Token.DO}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            return new WhileStmtNode(lexeme, condition, ParseStatement());
        }

        public StmtNode ParseRepeatStmt()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var statements = ParseStatements();

            if (_currentLexeme != Token.UNTIL)
                throw ExpectedException($"{Token.UNTIL}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var condition = ParseExpression();

            return new RepeatStmtNode(lexeme, condition, statements);
        }

        public StmtNode ParseForStmt()
        {
            var lexeme = _currentLexeme;
            _currentLexeme = _lexer.GetLexeme();

            var ctrlIdent = ParseIdent();

            if (_currentLexeme != Token.ASSIGN)
                throw ExpectedException(":=", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var forRange = ParseForRange();

            if (_currentLexeme != Token.DO)
                throw ExpectedException($"{Token.DO}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var statement = ParseStatement();

            return new ForStmtNode(lexeme, ctrlIdent, forRange, statement);
        }

        private ForRangeNode ParseForRange()
        {
            var startValue = ParseExpression();
            var direction = _currentLexeme;

            if (_currentLexeme != Token.TO && _currentLexeme != Token.DOWNTO)
                throw ExpectedException($"{Token.TO}", _currentLexeme.Source);
            _currentLexeme = _lexer.GetLexeme();

            var finalValue = ParseExpression();

            return new ForRangeNode(direction, startValue, finalValue);
        }
    }
}