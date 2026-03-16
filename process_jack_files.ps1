# Script para processar todos os arquivos .jack com o JackAnalyzer

$analyzerPath = "c:\Users\anton\OneDrive\Documentos\Compiladores-2026.1\CompiladoresUFMA-2026.1\src\JackAnalyzer"
$projectsBasePath = "c:\Users\anton\OneDrive\Documentos\Compiladores-2026.1\CompiladoresUFMA-2026.1\nand2tetris\nand2tetris\projects"

Push-Location $analyzerPath
dotnet build | Out-Null

# Encontra todos os arquivos .jack
$jackFiles = Get-ChildItem -Path $projectsBasePath -Filter "*.jack" -Recurse

Write-Host "Encontrados $($jackFiles.Count) arquivos .jack" -ForegroundColor Green

foreach ($file in $jackFiles) {
    Write-Host "Processando: $($file.FullName)" -ForegroundColor Cyan
    dotnet run -- "$($file.FullName)"
}

Pop-Location
Write-Host "✓ Pronto!" -ForegroundColor Green
