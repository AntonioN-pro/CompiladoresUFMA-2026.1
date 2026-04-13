using JackAnalyzer;
using System.Text;

if (args.Length == 0 || args.Length > 2)
{
    Console.WriteLine("Uso:");
    Console.WriteLine("  dotnet run -- <arquivo.jack> [diretorio_saida]");
    Console.WriteLine("  dotnet run -- <diretorio> [diretorio_saida]");
    return;
}

string inputPath = args[0];
string? outputDir = args.Length > 1 ? args[1] : null;

// Verifica se é um arquivo ou diretório
if (File.Exists(inputPath))
{
    // Processa um único arquivo
    ProcessarArquivo(inputPath, outputDir);
}
else if (Directory.Exists(inputPath))
{
    // Processa todos os arquivos .jack do diretório
    if (outputDir == null)
    {
        outputDir = Path.Combine(inputPath, "tokens");
    }
    
    Directory.CreateDirectory(outputDir);
    
    var arquivos = Directory.GetFiles(inputPath, "*.jack");
    
    if (arquivos.Length == 0)
    {
        Console.WriteLine("Nenhum arquivo .jack encontrado");
        return;
    }
    
    Console.WriteLine($"Processando {arquivos.Length} arquivo(s)...\n");
    
    foreach (var arquivo in arquivos)
    {
        ProcessarArquivo(arquivo, outputDir);
    }
    
    Console.WriteLine($"\n✓ Todos os arquivos foram processados em: {outputDir}");
}
else
{
    Console.WriteLine("Arquivo ou diretório não encontrado.");
}

void ProcessarArquivo(string filePath, string? outputDir)
{
    if (outputDir == null)
    {
        outputDir = Path.GetDirectoryName(filePath) ?? ".";
    }
    
    Directory.CreateDirectory(outputDir);
    
    JackTokenizer tokenizer = new JackTokenizer(filePath);
    
    // Gera o nome do arquivo de saída
    string outputPath = Path.Combine(
        outputDir,
        Path.GetFileNameWithoutExtension(filePath) + "T.xml"
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
        value = value.Replace("&", "&amp;")
                     .Replace("<", "&lt;")
                     .Replace(">", "&gt;");
        
        sb.AppendLine($"<{tagName}> {value} </{tagName}>");
        
        tokenizer.Advance();
    }
    
    sb.AppendLine("</tokens>");
    
    File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    Console.WriteLine($"✓ {Path.GetFileName(filePath)} → {Path.GetFileName(outputPath)}");
}