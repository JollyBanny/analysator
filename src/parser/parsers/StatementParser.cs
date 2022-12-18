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

        private StmtNode ParseCompoundStmt()
        {
            Require<Token>(true, Token.BEGIN);

            var statements = ParseStatements();

            Require<Token>(true, Token.END);

            return new CompoundStmtNode(statements);
        }

        private List<StmtNode> ParseStatements()
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
                    Require<Token>(false, Token.SEMICOLOM);

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

        private StmtNode ParseStatement()
        {
            var statement = ParseStructStmt();

            if (statement is null)
                statement = ParseSimpleStmt();

            return statement;
        }

        private StmtNode ParseSimpleStmt()
        {
            if (_currentLexeme == Token.END)
                return new EmptyStmtNode();

            return ParseAssignStmt();
        }

        private StmtNode ParseAssignStmt()
        {
            var left = ParseExpression();
            var lexeme = _currentLexeme;

            if (left is CallNode)
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

        private StmtNode? ParseStructStmt()
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

        private StmtNode ParseIfStmt()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var condition = ParseExpression();

            Require<Token>(true, Token.THEN);

            var ifPart = ParseStatement();
            StmtNode? elsePart = null;

            if (_currentLexeme == Token.ELSE)
            {
                NextLexeme();
                elsePart = ParseStatement();
            }

            return new IfStmtNode(lexeme, condition, ifPart, elsePart);
        }

        private StmtNode ParseWhileStmt()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var condition = ParseExpression();

            Require<Token>(true, Token.DO);

            return new WhileStmtNode(lexeme, condition, ParseStatement());
        }

        private StmtNode ParseRepeatStmt()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var statements = ParseStatements();

            if (statements.Count == 0)
                statements.Add(new EmptyStmtNode());

            Require<Token>(true, Token.UNTIL);

            var condition = ParseExpression();

            return new RepeatStmtNode(lexeme, condition, statements);
        }

        private StmtNode ParseForStmt()
        {
            var lexeme = _currentLexeme;
            NextLexeme();

            var ctrlIdent = ParseIdent();

            Require<Token>(true, Token.ASSIGN);

            var forRange = ParseForRange();

            Require<Token>(true, Token.DO);

            var statement = ParseStatement();

            return new ForStmtNode(lexeme, ctrlIdent, forRange, statement);
        }

        private ForRangeNode ParseForRange()
        {
            var startValue = ParseExpression();
            var direction = _currentLexeme;

            Require<Token>(true, Token.TO, Token.DOWNTO);

            var finalValue = ParseExpression();

            return new ForRangeNode(direction, startValue, finalValue);
        }
    }
}