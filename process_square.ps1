# Script para processar arquivos .jack da pasta Square e gerar T.xml na pasta resultado

$analyzerPath = "c:\Users\anton\OneDrive\Documentos\Compiladores-2026.1\CompiladoresUFMA-2026.1\src\JackAnalyzer"
$squarePath = "c:\Users\anton\OneDrive\Documentos\Compiladores-2026.1\CompiladoresUFMA-2026.1\nand2tetris\nand2tetris\projects\10\Square"
$resultadoPath = "$squarePath\resultado"

# Criar pasta resultado se não existir
if (!(Test-Path $resultadoPath)) {
    New-Item -ItemType Directory -Path $resultadoPath -Force
}

Push-Location $analyzerPath
dotnet build | Out-Null

# Processar cada arquivo .jack
$jackFiles = Get-ChildItem -Path $squarePath -Filter "*.jack"

foreach ($file in $jackFiles) {
    Write-Host "Processando: $($file.Name)" -ForegroundColor Cyan
    dotnet run -- "$($file.FullName)" "$resultadoPath"
}

Pop-Location
Write-Host "✓ Arquivos gerados na pasta resultado!" -ForegroundColor Green