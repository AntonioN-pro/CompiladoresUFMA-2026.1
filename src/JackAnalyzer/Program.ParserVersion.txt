namespace JackAnalyzer;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso:");
                Console.WriteLine("  dotnet run -- <arquivoT.xml> [diretorio_saida]");
                Console.WriteLine("  dotnet run -- <pasta_com_arquivos_T.xml> [diretorio_saida]");
                return 1;
            }

            var input = args[0];
            var outputDir = args.Length >= 2 ? args[1] : null;

            if (File.Exists(input))
            {
                CompileTokenFile(input, outputDir);
                return 0;
            }

            if (Directory.Exists(input))
            {
                var tokenFiles = Directory.GetFiles(input, "*T.xml");

                if (tokenFiles.Length == 0)
                {
                    Console.WriteLine("Nenhum arquivo *T.xml foi encontrado na pasta informada.");
                    return 1;
                }

                foreach (var file in tokenFiles)
                    CompileTokenFile(file, outputDir);

                return 0;
            }

            Console.WriteLine("Entrada não encontrada: " + input);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static void CompileTokenFile(string tokenXmlPath, string? outputDir)
    {
        var tokens = TokenXmlReader.ReadTokens(tokenXmlPath);
        var engine = new CompilationEngine(tokens);

        var fileName = Path.GetFileNameWithoutExtension(tokenXmlPath);
        if (fileName.EndsWith("T", StringComparison.OrdinalIgnoreCase))
            fileName = fileName[..^1];

        var destinationDir = outputDir ?? Path.GetDirectoryName(Path.GetFullPath(tokenXmlPath))!;
        var outputPath = Path.Combine(destinationDir, fileName + ".xml");

        engine.CompileToFile(outputPath);
        Console.WriteLine($"Parser concluído: {outputPath}");
    }
}
