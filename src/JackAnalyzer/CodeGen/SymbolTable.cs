namespace JackAnalyzer.CodeGen;

public sealed class SymbolTable
{
    private readonly Dictionary<string, SymbolInfo> _classScope = new();
    private readonly Dictionary<string, SymbolInfo> _subroutineScope = new();
    private readonly Dictionary<SymbolKind, int> _kindCounters = new()
    {
        [SymbolKind.Static] = 0,
        [SymbolKind.Field] = 0,
        [SymbolKind.Argument] = 0,
        [SymbolKind.Var] = 0
    };

    public void StartSubroutine()
    {
        _subroutineScope.Clear();
        _kindCounters[SymbolKind.Argument] = 0;
        _kindCounters[SymbolKind.Var] = 0;
    }

    public void Define(string name, string type, SymbolKind kind)
    {
        if (kind is SymbolKind.None)
            throw new InvalidOperationException("Tipo de símbolo inválido para definição.");

        int index = _kindCounters[kind];
        _kindCounters[kind] = index + 1;

        var symbol = new SymbolInfo(type, kind, index);

        if (kind is SymbolKind.Static or SymbolKind.Field)
            _classScope[name] = symbol;
        else
            _subroutineScope[name] = symbol;
    }

    public int VarCount(SymbolKind kind)
    {
        return _kindCounters.TryGetValue(kind, out int value) ? value : 0;
    }

    public SymbolKind KindOf(string name)
    {
        if (_subroutineScope.TryGetValue(name, out SymbolInfo? subSymbol))
            return subSymbol.Kind;

        if (_classScope.TryGetValue(name, out SymbolInfo? classSymbol))
            return classSymbol.Kind;

        return SymbolKind.None;
    }

    public string TypeOf(string name)
    {
        if (_subroutineScope.TryGetValue(name, out SymbolInfo? subSymbol))
            return subSymbol.Type;

        if (_classScope.TryGetValue(name, out SymbolInfo? classSymbol))
            return classSymbol.Type;

        return string.Empty;
    }

    public int IndexOf(string name)
    {
        if (_subroutineScope.TryGetValue(name, out SymbolInfo? subSymbol))
            return subSymbol.Index;

        if (_classScope.TryGetValue(name, out SymbolInfo? classSymbol))
            return classSymbol.Index;

        return -1;
    }

    private sealed record SymbolInfo(string Type, SymbolKind Kind, int Index);
}
