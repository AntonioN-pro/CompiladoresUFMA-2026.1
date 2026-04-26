using JackAnalyzer.Lexer;
using JackAnalyzer.Parser;
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

if (File.Exists(inputPath))
{
    ProcessarArquivo(inputPath, outputDir);
}
else if (Directory.Exists(inputPath))
{
    if (outputDir == null)
    {
        outputDir = inputPath;
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

    Console.WriteLine($"\nTodos os arquivos foram processados em: {outputDir}");
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

    string tokenOutputPath = Path.Combine(
        outputDir,
        Path.GetFileNameWithoutExtension(filePath) + "T.xml"
    );

    string parserOutputPath = Path.Combine(
        outputDir,
        Path.GetFileNameWithoutExtension(filePath) + ".xml"
    );

    GerarTokens(filePath, tokenOutputPath);

    var reader = new TokenXmlReader();
    var tokens = reader.ReadTokens(tokenOutputPath);

    var parser = new CompilationEngine(tokens);
    parser.CompileToFile(parserOutputPath);

    Console.WriteLine($"{Path.GetFileName(filePath)} → {Path.GetFileName(tokenOutputPath)} e {Path.GetFileName(parserOutputPath)}");
}

void GerarTokens(string filePath, string outputPath)
{
    JackTokenizer tokenizer = new JackTokenizer(filePath);

    var sb = new StringBuilder();
    sb.AppendLine("<tokens>");

    while (tokenizer.HasMoreTokens())
    {
        string value = tokenizer.GetCurrentValue();
        TokenType type = tokenizer.GetCurrentType();

        string tagName = type switch
        {
            TokenType.KEYWORD => "keyword",
            TokenType.SYMBOL => "symbol",
            TokenType.INT_CONST => "integerConstant",
            TokenType.STRING_CONST => "stringConstant",
            TokenType.IDENTIFIER => "identifier",
            _ => "unknown"
        };

        value = value.Replace("&", "&amp;")
                     .Replace("<", "&lt;")
                     .Replace(">", "&gt;")
                     .Replace("\"", "&quot;");

        sb.AppendLine($"<{tagName}> {value} </{tagName}>");

        tokenizer.Advance();
    }

    sb.AppendLine("</tokens>");

    File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
}