namespace JackAnalyzer;

public sealed class CompilationEngine
{
    private readonly List<ParserToken> _tokens;
    private readonly ParseXmlWriter _writer = new();
    private int _current;

    private static readonly HashSet<string> ClassVarKinds = ["static", "field"];
    private static readonly HashSet<string> SubroutineKinds = ["constructor", "function", "method"];
    private static readonly HashSet<string> StatementKinds = ["let", "if", "while", "do", "return"];
    private static readonly HashSet<string> Operators = ["+", "-", "*", "/", "&", "|", "<", ">", "="];
    private static readonly HashSet<string> UnaryOperators = ["-", "~"];
    private static readonly HashSet<string> KeywordConstants = ["true", "false", "null", "this"];

    public CompilationEngine(IEnumerable<ParserToken> tokens)
    {
        _tokens = tokens.ToList();
    }

    public void CompileToFile(string outputPath)
    {
        CompileClass();

        if (!IsAtEnd())
            throw Error(Peek(), "Tokens extras encontrados depois do fim da classe.");

        _writer.Save(outputPath);
    }

    private void CompileClass()
    {
        _writer.Open("class");
        ConsumeKeyword("class");
        Consume(ParserTokenType.Identifier, "Era esperado o nome da classe.");
        ConsumeSymbol("{");

        while (CheckKeyword(ClassVarKinds))
            CompileClassVarDec();

        while (CheckKeyword(SubroutineKinds))
            CompileSubroutineDec();

        ConsumeSymbol("}");
        _writer.Close("class");
    }

    private void CompileClassVarDec()
    {
        _writer.Open("classVarDec");
        ConsumeKeyword(ClassVarKinds);
        CompileType();
        Consume(ParserTokenType.Identifier, "Era esperado o nome da variável de classe.");

        while (MatchSymbol(","))
            Consume(ParserTokenType.Identifier, "Era esperado outro identificador após ','.");

        ConsumeSymbol(";");
        _writer.Close("classVarDec");
    }

    private void CompileSubroutineDec()
    {
        _writer.Open("subroutineDec");
        ConsumeKeyword(SubroutineKinds);

        if (CheckKeyword("void"))
            ConsumeKeyword("void");
        else
            CompileType();

        Consume(ParserTokenType.Identifier, "Era esperado o nome da sub-rotina.");
        ConsumeSymbol("(");
        CompileParameterList();
        ConsumeSymbol(")");
        CompileSubroutineBody();
        _writer.Close("subroutineDec");
    }

    private void CompileParameterList()
    {
        _writer.Open("parameterList");

        if (!CheckSymbol(")"))
        {
            CompileType();
            Consume(ParserTokenType.Identifier, "Era esperado o nome do parâmetro.");

            while (MatchSymbol(","))
            {
                CompileType();
                Consume(ParserTokenType.Identifier, "Era esperado o nome do parâmetro após ','.");
            }
        }

        _writer.Close("parameterList");
    }

    private void CompileSubroutineBody()
    {
        _writer.Open("subroutineBody");
        ConsumeSymbol("{");

        while (CheckKeyword("var"))
            CompileVarDec();

        CompileStatements();
        ConsumeSymbol("}");
        _writer.Close("subroutineBody");
    }

    private void CompileVarDec()
    {
        _writer.Open("varDec");
        ConsumeKeyword("var");
        CompileType();
        Consume(ParserTokenType.Identifier, "Era esperado o nome da variável local.");

        while (MatchSymbol(","))
            Consume(ParserTokenType.Identifier, "Era esperado outro identificador após ','.");

        ConsumeSymbol(";");
        _writer.Close("varDec");
    }

    private void CompileStatements()
    {
        _writer.Open("statements");

        while (CheckKeyword(StatementKinds))
        {
            switch (Peek().Value)
            {
                case "let": CompileLet(); break;
                case "if": CompileIf(); break;
                case "while": CompileWhile(); break;
                case "do": CompileDo(); break;
                case "return": CompileReturn(); break;
            }
        }

        _writer.Close("statements");
    }

    private void CompileLet()
    {
        _writer.Open("letStatement");
        ConsumeKeyword("let");
        Consume(ParserTokenType.Identifier, "Era esperado o nome da variável no let.");

        if (MatchSymbol("["))
        {
            CompileExpression();
            ConsumeSymbol("]");
        }

        ConsumeSymbol("=");
        CompileExpression();
        ConsumeSymbol(";");
        _writer.Close("letStatement");
    }

    private void CompileIf()
    {
        _writer.Open("ifStatement");
        ConsumeKeyword("if");
        ConsumeSymbol("(");
        CompileExpression();
        ConsumeSymbol(")");
        ConsumeSymbol("{");
        CompileStatements();
        ConsumeSymbol("}");

        if (MatchKeyword("else"))
        {
            ConsumeSymbol("{");
            CompileStatements();
            ConsumeSymbol("}");
        }

        _writer.Close("ifStatement");
    }

    private void CompileWhile()
    {
        _writer.Open("whileStatement");
        ConsumeKeyword("while");
        ConsumeSymbol("(");
        CompileExpression();
        ConsumeSymbol(")");
        ConsumeSymbol("{");
        CompileStatements();
        ConsumeSymbol("}");
        _writer.Close("whileStatement");
    }

    private void CompileDo()
    {
        _writer.Open("doStatement");
        ConsumeKeyword("do");
        CompileSubroutineCall();
        ConsumeSymbol(";");
        _writer.Close("doStatement");
    }

    private void CompileReturn()
    {
        _writer.Open("returnStatement");
        ConsumeKeyword("return");

        if (!CheckSymbol(";"))
            CompileExpression();

        ConsumeSymbol(";");
        _writer.Close("returnStatement");
    }

    private void CompileExpression()
    {
        _writer.Open("expression");
        CompileTerm();

        while (CheckSymbol(Operators))
        {
            Advance();
            CompileTerm();
        }

        _writer.Close("expression");
    }

    private void CompileTerm()
    {
        _writer.Open("term");

        if (Check(ParserTokenType.IntegerConstant) || Check(ParserTokenType.StringConstant) || CheckKeyword(KeywordConstants))
        {
            Advance();
        }
        else if (Check(ParserTokenType.Identifier))
        {
            if (CheckNextSymbol("["))
            {
                Consume(ParserTokenType.Identifier, "Era esperado identificador antes de '['.");
                ConsumeSymbol("[");
                CompileExpression();
                ConsumeSymbol("]");
            }
            else if (CheckNextSymbol("(") || CheckNextSymbol("."))
            {
                CompileSubroutineCall();
            }
            else
            {
                Consume(ParserTokenType.Identifier, "Era esperado identificador.");
            }
        }
        else if (MatchSymbol("("))
        {
            CompileExpression();
            ConsumeSymbol(")");
        }
        else if (CheckSymbol(UnaryOperators))
        {
            Advance();
            CompileTerm();
        }
        else
        {
            throw Error(Peek(), "Termo inválido na expressão.");
        }

        _writer.Close("term");
    }

    private void CompileExpressionList()
    {
        _writer.Open("expressionList");

        if (!CheckSymbol(")"))
        {
            CompileExpression();
            while (MatchSymbol(","))
                CompileExpression();
        }

        _writer.Close("expressionList");
    }

    private void CompileSubroutineCall()
    {
        Consume(ParserTokenType.Identifier, "Era esperado o nome da sub-rotina, classe ou objeto.");

        if (MatchSymbol("."))
            Consume(ParserTokenType.Identifier, "Era esperado o nome da sub-rotina após '.'.");

        ConsumeSymbol("(");
        CompileExpressionList();
        ConsumeSymbol(")");
    }

    private void CompileType()
    {
        if (CheckKeyword(["int", "char", "boolean"]))
            Advance();
        else
            Consume(ParserTokenType.Identifier, "Era esperado um tipo: int, char, boolean ou nome de classe.");
    }

    private bool MatchSymbol(string value)
    {
        if (!CheckSymbol(value)) return false;
        Advance();
        return true;
    }

    private bool MatchKeyword(string value)
    {
        if (!CheckKeyword(value)) return false;
        Advance();
        return true;
    }

    private ParserToken Consume(ParserTokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private void ConsumeKeyword(string value)
    {
        if (!CheckKeyword(value))
            throw Error(Peek(), $"Era esperado o keyword '{value}'.");
        Advance();
    }

    private void ConsumeKeyword(IEnumerable<string> values)
    {
        if (!CheckKeyword(values))
            throw Error(Peek(), $"Era esperado um destes keywords: {string.Join(", ", values)}.");
        Advance();
    }

    private void ConsumeSymbol(string value)
    {
        if (!CheckSymbol(value))
            throw Error(Peek(), $"Era esperado o símbolo '{value}'.");
        Advance();
    }

    private ParserToken Advance()
    {
        if (IsAtEnd())
            throw new InvalidOperationException("Fim inesperado do arquivo de tokens.");

        var token = _tokens[_current++];
        _writer.Token(token);
        return token;
    }

    private bool Check(ParserTokenType type) => !IsAtEnd() && Peek().Type == type;
    private bool CheckKeyword(string value) => !IsAtEnd() && Peek().Type == ParserTokenType.Keyword && Peek().Value == value;
    private bool CheckKeyword(IEnumerable<string> values) => !IsAtEnd() && Peek().Type == ParserTokenType.Keyword && values.Contains(Peek().Value);
    private bool CheckSymbol(string value) => !IsAtEnd() && Peek().Type == ParserTokenType.Symbol && Peek().Value == value;
    private bool CheckSymbol(IEnumerable<string> values) => !IsAtEnd() && Peek().Type == ParserTokenType.Symbol && values.Contains(Peek().Value);
    private bool CheckNextSymbol(string value) => _current + 1 < _tokens.Count && _tokens[_current + 1].Type == ParserTokenType.Symbol && _tokens[_current + 1].Value == value;
    private bool IsAtEnd() => _current >= _tokens.Count;

    private ParserToken Peek()
    {
        if (IsAtEnd())
            return new ParserToken(ParserTokenType.Symbol, "<EOF>");
        return _tokens[_current];
    }

    private static Exception Error(ParserToken token, string message)
    {
        var where = token.Value == "<EOF>" ? "no fim do arquivo" : $"perto de '{token.Value}'";
        return new InvalidOperationException($"Erro sintático {where}: {message}");
    }
}
