namespace JackAnalyzer;

public enum ParserTokenType
{
    Keyword,
    Symbol,
    Identifier,
    IntegerConstant,
    StringConstant
}

public sealed record ParserToken(ParserTokenType Type, string Value, int Line = 0, int Column = 0)
{
    public string XmlTag => Type switch
    {
        ParserTokenType.Keyword => "keyword",
        ParserTokenType.Symbol => "symbol",
        ParserTokenType.Identifier => "identifier",
        ParserTokenType.IntegerConstant => "integerConstant",
        ParserTokenType.StringConstant => "stringConstant",
        _ => throw new InvalidOperationException($"Tipo de token inválido: {Type}")
    };
}