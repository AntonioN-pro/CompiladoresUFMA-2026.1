using JackAnalyzer;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Uso: dotnet run -- <arquivo.jack>");
    return;
}

string inputPath = args[0];

if (File.Exists(inputPath))
{
    JackTokenizer tokenizer = new JackTokenizer(inputPath);
    
    // Gera o nome do arquivo de saída
    string? dirInfo = Path.GetDirectoryName(inputPath);
    string outputDir = dirInfo ?? ".";
    string outputPath = Path.Combine(
        outputDir,
        Path.GetFileNameWithoutExtension(inputPath) + "T.xml"
    );
    
    var sb = new StringBuilder();
    sb.AppendLine("<tokens>");
    
    while (tokenizer.HasMoreTokens())
    {
        string value = tokenizer.GetCurrentValue();
        TokenType type = tokenizer.GetCurrentType();
        
        // Converte enum para tag XML
        string tagName = type switch
        {
            TokenType.KEYWORD => "keyword",
            TokenType.SYMBOL => "symbol",
            TokenType.INT_CONST => "integerConstant",
            TokenType.STRING_CONST => "stringConstant",
            TokenType.IDENTIFIER => "identifier",
            _ => "unknown"
        };
        
        // Escapa caracteres especiais XML
        string escapedValue = System.Xml.XmlConvert.EncodeName(value);
        value = value.Replace("&", "&amp;")
                     .Replace("<", "&lt;")
                     .Replace(">", "&gt;");
        
        sb.AppendLine($"<{tagName}> {value} </{tagName}>");
        
        tokenizer.Advance();
    }
    
    sb.AppendLine("</tokens>");
    
    File.WriteAllText(outputPath, sb.ToString());
    Console.WriteLine($"✓ Arquivo gerado: {outputPath}");
}
else
{
    Console.WriteLine("Arquivo não encontrado.");
}