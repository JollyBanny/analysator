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
            Require<Token>(new List<Token> { Token.BEGIN }, true);

            var statements = ParseStatements();

            Require<Token>(new List<Token> { Token.END }, true);

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

            while (!new List<Token> { Token.END, Token.EOF, Token.UNTIL }
                .Contains(_currentLexeme))
            {
                if (!separatorExist)
                    Require<Token>(new List<Token> { Token.SEMICOLOM }, false);
                    // throw ExpectedException(";", _currentLexeme.Source);

                var statement = ParseStatement();
                separatorExist = false;
                statements.Add(statement);

                if (_currentLexeme == Token.SEMICOLOM)
                {
                    separatorExist = true;
                    NextLexeme();

                    if (_currentLexeme == Token.ELSE)
                        throw ExpectedException(";", $"{Token.ELSE}");
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

            return ParseAssignStmt();
        }

        public StmtNode ParseAssignStmt()
        {
            var left = ParseExpression();
            var lexeme = _currentLexeme;

            if (left is FunctionCallNode)
                return new CallStmtNode(left);

            if (!AssignOperators.Contains(lexeme))
            {
                if (left is IdentNode)
                    return new CallStmtNode(left);
                else
                    throw FatalException("Illegal expression");
            }
            NextLexeme();

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
            NextLexeme();

            var condition = ParseExpression();

            Require<Token>(new List<Token> { Token.THEN }, true);

            var ifPart = ParseStatement();
            StmtNode? elsePart = null;

            if (_currentLexeme == Token.ELSE)
            {
                NextLexeme();
                elsePart = ParseStatement();
            }

            return new IfStmtNode(lexeme, condition, ifPart, elsePart);
        }

        public StmtNode ParseWhileStmt()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var condition = ParseExpression();

            Require<Token>(new List<Token> { Token.DO }, true);

            return new WhileStmtNode(lexeme, condition, ParseStatement());
        }

        public StmtNode ParseRepeatStmt()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var statements = ParseStatements();

            if (statements.Count == 0)
                statements.Add(new EmptyStmtNode());

            Require<Token>(new List<Token> { Token.UNTIL }, true);

            var condition = ParseExpression();

            return new RepeatStmtNode(lexeme, condition, statements);
        }

        public StmtNode ParseForStmt()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var ctrlIdent = ParseIdent();

            Require<Token>(new List<Token> { Token.ASSIGN }, true);

            var forRange = ParseForRange();

            Require<Token>(new List<Token> { Token.DO }, true);

            var statement = ParseStatement();

            return new ForStmtNode(lexeme, ctrlIdent, forRange, statement);
        }

        private ForRangeNode ParseForRange()
        {
            var startValue = ParseExpression();
            var direction = _currentLexeme;

            Require<Token>(new List<Token> { Token.TO, Token.DOWNTO }, true);

            var finalValue = ParseExpression();

            return new ForRangeNode(direction, startValue, finalValue);
        }
    }
}