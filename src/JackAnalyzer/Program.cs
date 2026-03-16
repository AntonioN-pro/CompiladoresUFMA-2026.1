using JackAnalyzer;
using System.Text;

if (args.Length == 0 || args.Length > 2)
{
    Console.WriteLine("Uso: dotnet run -- <arquivo.jack> [diretorio_saida]");
    return;
}

string inputPath = args[0];
string outputDir = args.Length > 1 ? args[1] : Path.GetDirectoryName(inputPath) ?? ".";

if (File.Exists(inputPath))
{
    JackTokenizer tokenizer = new JackTokenizer(inputPath);
    
    // Gera o nome do arquivo de saída
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
    
    File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    Console.WriteLine($"✓ Arquivo gerado: {outputPath}");
}
else
{
    Console.WriteLine("Arquivo não encontrado.");
}