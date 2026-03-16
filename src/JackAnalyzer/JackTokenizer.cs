using System.Text.RegularExpressions;

namespace JackAnalyzer;

public class JackTokenizer
{
    private string _content;
    // Lista que guardará os tokens encontrados (Ex: ["class", "Main", "{"])
    private List<(string Value, TokenType Type)> _tokens = new();
    private int _currentIndex = 0;

    // Regex que define o que é cada coisa na linguagem Jack
    private static readonly string TokenPattern = 
        @"(?<keyword>class|constructor|function|method|field|static|var|int|char|boolean|void|true|false|null|this|let|do|if|else|while|return)|" +
        @"(?<symbol>[\{\}\(\)\[\]\.,;+\-\*\/&|<>=~])|" +
        @"(?<integerConstant>\d+)|" +
        @"(?<stringConstant>""[^""\n]*"")|" +
        @"(?<identifier>[a-zA-Z_]\w*)";

    public JackTokenizer(string filePath)
    {
        string rawCode = File.ReadAllText(filePath);
        _content = RemoveComments(rawCode);
        Tokenize();
    }

    private string RemoveComments(string input)
    {
        // Remove // e /* */
        return Regex.Replace(input, @"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(//.*)", "");
    }

    private void Tokenize()
    {
        var matches = Regex.Matches(_content, TokenPattern);
        foreach (Match m in matches)
        {
            if (m.Groups["keyword"].Success) _tokens.Add((m.Value, TokenType.KEYWORD));
            else if (m.Groups["symbol"].Success) _tokens.Add((m.Value, TokenType.SYMBOL));
            else if (m.Groups["integerConstant"].Success) _tokens.Add((m.Value, TokenType.INT_CONST));
            else if (m.Groups["stringConstant"].Success) _tokens.Add((m.Value.Trim('"'), TokenType.STRING_CONST));
            else if (m.Groups["identifier"].Success) _tokens.Add((m.Value, TokenType.IDENTIFIER));
        }
    }

    // Métodos que o Parser (próximo projeto) vai usar
    public bool HasMoreTokens() => _currentIndex < _tokens.Count;
    public void Advance() => _currentIndex++;
    public string GetCurrentValue() => _tokens[_currentIndex].Value;
    public TokenType GetCurrentType() => _tokens[_currentIndex].Type;
}