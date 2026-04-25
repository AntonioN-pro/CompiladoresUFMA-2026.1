using System.Xml.Linq;

namespace JackAnalyzer;

public class TokenXmlReader
{
    public List<ParserToken> ReadTokens(string path)
    {
        var tokens = new List<ParserToken>();

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Arquivo de tokens não encontrado: {path}");
        }

        var doc = XDocument.Load(path);

        if (doc.Root == null || doc.Root.Name.LocalName != "tokens")
        {
            throw new InvalidOperationException("Arquivo XML inválido: raiz <tokens> não encontrada.");
        }

        foreach (var element in doc.Root.Elements())
        {
            string typeText = element.Name.LocalName;
            string value = element.Value.Trim();

            ParserTokenType type = MapType(typeText);

            tokens.Add(new ParserToken(type, value));
        }

        return tokens;
    }

    private ParserTokenType MapType(string type)
    {
        return type switch
        {
            "keyword" => ParserTokenType.Keyword,
            "symbol" => ParserTokenType.Symbol,
            "identifier" => ParserTokenType.Identifier,
            "integerConstant" => ParserTokenType.IntegerConstant,
            "stringConstant" => ParserTokenType.StringConstant,
            _ => throw new InvalidOperationException($"Tipo de token desconhecido no XML: {type}")
        };
    }
}