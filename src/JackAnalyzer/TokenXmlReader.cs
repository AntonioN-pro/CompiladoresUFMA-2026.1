using System.Xml.Linq;

namespace JackAnalyzer;

public static class TokenXmlReader
{
    public static List<ParserToken> ReadTokens(string tokenXmlPath)
    {
        if (!File.Exists(tokenXmlPath))
            throw new FileNotFoundException("Arquivo de tokens não encontrado.", tokenXmlPath);

        var doc = XDocument.Load(tokenXmlPath, LoadOptions.PreserveWhitespace);
        if (doc.Root?.Name.LocalName != "tokens")
            throw new InvalidOperationException("O XML de entrada deve possuir a tag raiz <tokens>.");

        var tokens = new List<ParserToken>();

        foreach (var element in doc.Root.Elements())
        {
            var type = element.Name.LocalName switch
            {
                "keyword" => ParserTokenType.Keyword,
                "symbol" => ParserTokenType.Symbol,
                "identifier" => ParserTokenType.Identifier,
                "integerConstant" => ParserTokenType.IntegerConstant,
                "stringConstant" => ParserTokenType.StringConstant,
                _ => throw new InvalidOperationException($"Tag de token desconhecida: <{element.Name.LocalName}>")
            };

            tokens.Add(new ParserToken(type, element.Value.Trim()));
        }

        return tokens;
    }
}
