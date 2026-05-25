using JackAnalyzer.Parser;

namespace JackAnalyzer.CodeGen;

public sealed class VmCompilationEngine
{
    private readonly List<ParserToken> _tokens;
    private readonly SymbolTable _symbols = new();
    private readonly VmWriter _writer = new();
    private int _current;
    private int _labelCounter;
    private string _className = string.Empty;

    private static readonly HashSet<string> ClassVarKinds = ["static", "field"];
    private static readonly HashSet<string> SubroutineKinds = ["constructor", "function", "method"];
    private static readonly HashSet<string> StatementKinds = ["let", "if", "while", "do", "return"];
    private static readonly HashSet<string> Operators = ["+", "-", "*", "/", "&", "|", "<", ">", "="];
    private static readonly HashSet<string> UnaryOperators = ["-", "~"];

    public VmCompilationEngine(IEnumerable<ParserToken> tokens)
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
        ConsumeKeyword("class");
        _className = Consume(ParserTokenType.Identifier, "Era esperado o nome da classe.").Value;
        ConsumeSymbol("{");

        while (CheckKeyword(ClassVarKinds))
            CompileClassVarDec();

        while (CheckKeyword(SubroutineKinds))
            CompileSubroutineDec();

        ConsumeSymbol("}");
    }

    private void CompileClassVarDec()
    {
        string kindKeyword = ConsumeKeyword(ClassVarKinds).Value;
        SymbolKind kind = kindKeyword == "static" ? SymbolKind.Static : SymbolKind.Field;
        string type = CompileType();

        string name = Consume(ParserTokenType.Identifier, "Era esperado o nome da variável de classe.").Value;
        _symbols.Define(name, type, kind);

        while (MatchSymbol(","))
        {
            string nextName = Consume(ParserTokenType.Identifier, "Era esperado outro identificador após ','.").Value;
            _symbols.Define(nextName, type, kind);
        }

        ConsumeSymbol(";");
    }

    private void CompileSubroutineDec()
    {
        _symbols.StartSubroutine();

        string subroutineKind = ConsumeKeyword(SubroutineKinds).Value;

        if (CheckKeyword("void"))
            ConsumeKeyword("void");
        else
            CompileType();

        string subroutineName = Consume(ParserTokenType.Identifier, "Era esperado o nome da sub-rotina.").Value;

        if (subroutineKind == "method")
        {
            // Em métodos, argument 0 representa o objeto atual (this).
            _symbols.Define("this", _className, SymbolKind.Argument);
        }

        ConsumeSymbol("(");
        CompileParameterList();
        ConsumeSymbol(")");

        ConsumeSymbol("{");
        while (CheckKeyword("var"))
            CompileVarDec();

        int localCount = _symbols.VarCount(SymbolKind.Var);
        string fullSubroutineName = $"{_className}.{subroutineName}";
        _writer.WriteFunction(fullSubroutineName, localCount);

        if (subroutineKind == "constructor")
        {
            // Construtor precisa alocar memória para todos os campos da classe.
            int fieldCount = _symbols.VarCount(SymbolKind.Field);
            _writer.WritePush(VmSegment.Constant, fieldCount);
            _writer.WriteCall("Memory.alloc", 1);
            _writer.WritePop(VmSegment.Pointer, 0);
        }
        else if (subroutineKind == "method")
        {
            // Em método, this recebe o primeiro argumento real da chamada.
            _writer.WritePush(VmSegment.Argument, 0);
            _writer.WritePop(VmSegment.Pointer, 0);
        }

        CompileStatements();
        ConsumeSymbol("}");
    }

    private void CompileParameterList()
    {
        if (CheckSymbol(")"))
            return;

        string type = CompileType();
        string name = Consume(ParserTokenType.Identifier, "Era esperado o nome do parâmetro.").Value;
        _symbols.Define(name, type, SymbolKind.Argument);

        while (MatchSymbol(","))
        {
            string nextType = CompileType();
            string nextName = Consume(ParserTokenType.Identifier, "Era esperado o nome do parâmetro após ','.").Value;
            _symbols.Define(nextName, nextType, SymbolKind.Argument);
        }
    }

    private void CompileVarDec()
    {
        ConsumeKeyword("var");
        string type = CompileType();

        string name = Consume(ParserTokenType.Identifier, "Era esperado o nome da variável local.").Value;
        _symbols.Define(name, type, SymbolKind.Var);

        while (MatchSymbol(","))
        {
            string nextName = Consume(ParserTokenType.Identifier, "Era esperado outro identificador após ','.").Value;
            _symbols.Define(nextName, type, SymbolKind.Var);
        }

        ConsumeSymbol(";");
    }

    private void CompileStatements()
    {
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
    }

    private void CompileLet()
    {
        ConsumeKeyword("let");
        string name = Consume(ParserTokenType.Identifier, "Era esperado o nome da variável no let.").Value;
        SymbolKind kind = _symbols.KindOf(name);
        int index = _symbols.IndexOf(name);

        if (kind == SymbolKind.None)
            throw Error(Peek(), $"Variável '{name}' não foi declarada.");

        bool isArrayAssignment = MatchSymbol("[");
        if (isArrayAssignment)
        {
            // Base do array + índice avaliado em tempo de execução.
            WritePushVariable(name);
            CompileExpression();
            _writer.WriteArithmetic("add");
            ConsumeSymbol("]");
        }

        ConsumeSymbol("=");
        CompileExpression();
        ConsumeSymbol(";");

        if (isArrayAssignment)
        {
            // Guarda o valor e depois escreve em THAT 0 no endereço calculado.
            _writer.WritePop(VmSegment.Temp, 0);
            _writer.WritePop(VmSegment.Pointer, 1);
            _writer.WritePush(VmSegment.Temp, 0);
            _writer.WritePop(VmSegment.That, 0);
        }
        else
        {
            _writer.WritePop(MapKindToSegment(kind), index);
        }
    }

    private void CompileIf()
    {
        ConsumeKeyword("if");

        string trueLabel = NextLabel("IF_TRUE");
        string falseLabel = NextLabel("IF_FALSE");
        string endLabel = NextLabel("IF_END");

        ConsumeSymbol("(");
        CompileExpression();
        ConsumeSymbol(")");

        _writer.WriteIf(trueLabel);
        _writer.WriteGoto(falseLabel);
        _writer.WriteLabel(trueLabel);

        ConsumeSymbol("{");
        CompileStatements();
        ConsumeSymbol("}");

        if (MatchKeyword("else"))
        {
            _writer.WriteGoto(endLabel);
            _writer.WriteLabel(falseLabel);
            ConsumeSymbol("{");
            CompileStatements();
            ConsumeSymbol("}");
            _writer.WriteLabel(endLabel);
        }
        else
        {
            _writer.WriteLabel(falseLabel);
        }
    }

    private void CompileWhile()
    {
        ConsumeKeyword("while");

        string expLabel = NextLabel("WHILE_EXP");
        string endLabel = NextLabel("WHILE_END");

        _writer.WriteLabel(expLabel);

        ConsumeSymbol("(");
        CompileExpression();
        ConsumeSymbol(")");

        _writer.WriteArithmetic("not");
        _writer.WriteIf(endLabel);

        ConsumeSymbol("{");
        CompileStatements();
        ConsumeSymbol("}");

        _writer.WriteGoto(expLabel);
        _writer.WriteLabel(endLabel);
    }

    private void CompileDo()
    {
        ConsumeKeyword("do");
        CompileSubroutineCall();
        ConsumeSymbol(";");

        // Chamadas em do descartam o valor de retorno.
        _writer.WritePop(VmSegment.Temp, 0);
    }

    private void CompileReturn()
    {
        ConsumeKeyword("return");

        if (!CheckSymbol(";"))
            CompileExpression();
        else
            _writer.WritePush(VmSegment.Constant, 0);

        ConsumeSymbol(";");
        _writer.WriteReturn();
    }

    private void CompileExpression()
    {
        CompileTerm();

        while (CheckSymbol(Operators))
        {
            string op = Advance().Value;
            CompileTerm();
            WriteBinaryOperator(op);
        }
    }

    private void CompileTerm()
    {
        if (Check(ParserTokenType.IntegerConstant))
        {
            int value = int.Parse(Advance().Value);
            _writer.WritePush(VmSegment.Constant, value);
            return;
        }

        if (Check(ParserTokenType.StringConstant))
        {
            string value = Advance().Value;
            WriteStringConstant(value);
            return;
        }

        if (CheckKeyword("true") || CheckKeyword("false") || CheckKeyword("null") || CheckKeyword("this"))
        {
            WriteKeywordConstant(Advance().Value);
            return;
        }

        if (Check(ParserTokenType.Identifier))
        {
            if (CheckNextSymbol("["))
            {
                string varName = Consume(ParserTokenType.Identifier, "Era esperado identificador antes de '['.").Value;
                ConsumeSymbol("[");
                WritePushVariable(varName);
                CompileExpression();
                ConsumeSymbol("]");

                _writer.WriteArithmetic("add");
                _writer.WritePop(VmSegment.Pointer, 1);
                _writer.WritePush(VmSegment.That, 0);
                return;
            }

            if (CheckNextSymbol("(") || CheckNextSymbol("."))
            {
                CompileSubroutineCall();
                return;
            }

            string name = Advance().Value;
            WritePushVariable(name);
            return;
        }

        if (MatchSymbol("("))
        {
            CompileExpression();
            ConsumeSymbol(")");
            return;
        }

        if (CheckSymbol(UnaryOperators))
        {
            string unaryOp = Advance().Value;
            CompileTerm();
            _writer.WriteArithmetic(unaryOp == "-" ? "neg" : "not");
            return;
        }

        throw Error(Peek(), "Termo inválido na expressão.");
    }

    private int CompileExpressionList()
    {
        int argumentCount = 0;

        if (!CheckSymbol(")"))
        {
            CompileExpression();
            argumentCount++;

            while (MatchSymbol(","))
            {
                CompileExpression();
                argumentCount++;
            }
        }

        return argumentCount;
    }

    private void CompileSubroutineCall()
    {
        string caller = Consume(ParserTokenType.Identifier, "Era esperado o nome da sub-rotina, classe ou objeto.").Value;
        int argCount;
        string fullName;

        if (MatchSymbol("."))
        {
            string subroutineName = Consume(ParserTokenType.Identifier, "Era esperado o nome da sub-rotina após '.'.").Value;

            SymbolKind kind = _symbols.KindOf(caller);
            if (kind != SymbolKind.None)
            {
                // Chamada de método via variável-objeto.
                WritePushVariable(caller);
                string typeName = _symbols.TypeOf(caller);
                fullName = $"{typeName}.{subroutineName}";
                argCount = 1;
            }
            else
            {
                // Chamada estática via nome de classe.
                fullName = $"{caller}.{subroutineName}";
                argCount = 0;
            }
        }
        else
        {
            // Chamada sem prefixo é método da classe atual.
            _writer.WritePush(VmSegment.Pointer, 0);
            fullName = $"{_className}.{caller}";
            argCount = 1;
        }

        ConsumeSymbol("(");
        argCount += CompileExpressionList();
        ConsumeSymbol(")");
        _writer.WriteCall(fullName, argCount);
    }

    private string CompileType()
    {
        if (CheckKeyword(["int", "char", "boolean"]))
            return Advance().Value;

        return Consume(ParserTokenType.Identifier, "Era esperado um tipo: int, char, boolean ou nome de classe.").Value;
    }

    private void WriteBinaryOperator(string op)
    {
        switch (op)
        {
            case "+": _writer.WriteArithmetic("add"); break;
            case "-": _writer.WriteArithmetic("sub"); break;
            case "*": _writer.WriteCall("Math.multiply", 2); break;
            case "/": _writer.WriteCall("Math.divide", 2); break;
            case "&": _writer.WriteArithmetic("and"); break;
            case "|": _writer.WriteArithmetic("or"); break;
            case "<": _writer.WriteArithmetic("lt"); break;
            case ">": _writer.WriteArithmetic("gt"); break;
            case "=": _writer.WriteArithmetic("eq"); break;
            default: throw new InvalidOperationException($"Operador binário inválido: {op}");
        }
    }

    private void WriteKeywordConstant(string keyword)
    {
        switch (keyword)
        {
            case "true":
                _writer.WritePush(VmSegment.Constant, 0);
                _writer.WriteArithmetic("not");
                break;
            case "false":
            case "null":
                _writer.WritePush(VmSegment.Constant, 0);
                break;
            case "this":
                _writer.WritePush(VmSegment.Pointer, 0);
                break;
            default:
                throw new InvalidOperationException($"Keyword constant inválida: {keyword}");
        }
    }

    private void WriteStringConstant(string value)
    {
        _writer.WritePush(VmSegment.Constant, value.Length);
        _writer.WriteCall("String.new", 1);

        foreach (char ch in value)
        {
            _writer.WritePush(VmSegment.Constant, ch);
            _writer.WriteCall("String.appendChar", 2);
        }
    }

    private void WritePushVariable(string name)
    {
        SymbolKind kind = _symbols.KindOf(name);
        int index = _symbols.IndexOf(name);

        if (kind == SymbolKind.None || index < 0)
            throw new InvalidOperationException($"Variável não declarada: {name}");

        _writer.WritePush(MapKindToSegment(kind), index);
    }

    private static VmSegment MapKindToSegment(SymbolKind kind) => kind switch
    {
        SymbolKind.Static => VmSegment.Static,
        SymbolKind.Field => VmSegment.This,
        SymbolKind.Argument => VmSegment.Argument,
        SymbolKind.Var => VmSegment.Local,
        _ => throw new InvalidOperationException($"Não existe segmento VM para kind: {kind}")
    };

    private string NextLabel(string prefix) => $"{prefix}_{_labelCounter++}";

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

    private ParserToken ConsumeKeyword(string value)
    {
        if (!CheckKeyword(value))
            throw Error(Peek(), $"Era esperado o keyword '{value}'.");
        return Advance();
    }

    private ParserToken ConsumeKeyword(IEnumerable<string> values)
    {
        if (!CheckKeyword(values))
            throw Error(Peek(), $"Era esperado um destes keywords: {string.Join(", ", values)}.");
        return Advance();
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

        return _tokens[_current++];
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
        return new InvalidOperationException($"Erro de compilação {where}: {message}");
    }
}
