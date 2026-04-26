# Analisador Léxico e Sintático para a Linguagem Jack (nand2tetris)

## Descrição

Este projeto tem como objetivo implementar um **analisador léxico (scanner)** e um **analisador sintático (parser)** para a linguagem **Jack**, utilizando **C#**.

O programa é capaz de:

1. Ler arquivos fonte `.jack`
2. Identificar os tokens da linguagem (análise léxica)
3. Validar a estrutura do programa conforme a gramática Jack (análise sintática)
4. Gerar arquivos `.xml` compatíveis com o padrão do projeto nand2tetris

A implementação foi desenvolvida com base conceitual fornecida em aula, sendo o código totalmente autoral.

---

## Integrantes

- Gabriel Mesquita Torres - 2022020390
- Antônio Neto de Moura Melo - 20250071160

---

## Linguagem e Tecnologias

- C#
- .NET 10.0

---

## Estrutura do Projeto

```
src/JackAnalyzer/
  Lexer/
    JackTokenizer.cs      # Leitura e tokenização dos arquivos .jack
    TokenType.cs          # Enum com os tipos de token (KEYWORD, SYMBOL, etc.)
  Parser/
    CompilationEngine.cs  # Parser recursivo descendente — gera a árvore sintática
    ParserToken.cs        # Tipo que representa um token para o parser
    ParseXmlWriter.cs     # Escrita do XML de saída com indentação e escape
    TokenXmlReader.cs     # Leitura do XML de tokens gerado pelo Lexer
  Program.cs              # Ponto de entrada — orquestra Lexer e Parser
```

---

## Funcionalidades

### Analisador Léxico (`Lexer/`)

- Leitura de arquivos `.jack`
- Remoção de comentários (`//` e `/* */`) antes da tokenização
- Identificação dos seguintes tokens via expressão regular:
  - `keyword` — palavras reservadas (`class`, `if`, `while`, `return`, etc.)
  - `symbol` — símbolos da linguagem (`{`, `}`, `(`, `)`, `+`, `-`, etc.)
  - `integerConstant` — literais inteiros
  - `stringConstant` — literais de string entre aspas duplas
  - `identifier` — nomes de variáveis, classes e sub-rotinas
- Geração de saída `<Nome>T.xml` no padrão nand2tetris:
  - Estrutura `<tokens> ... </tokens>`
  - Escape de caracteres especiais (`&`, `<`, `>`, `"`)

---

### Analisador Sintático (`Parser/`)

O analisador sintático consome os tokens gerados pelo analisador léxico e verifica se o código segue corretamente a gramática da linguagem Jack.

Foi implementado utilizando a abordagem de **recursive descent parsing**, onde cada regra da gramática é representada por um método dedicado em `CompilationEngine`.

O fluxo interno é:
1. `TokenXmlReader` lê o `<Nome>T.xml` e reconstrói a lista de tokens
2. `CompilationEngine` percorre os tokens e constrói a árvore sintática
3. `ParseXmlWriter` grava o XML final com indentação hierárquica e escape correto

#### Estruturas suportadas:

- `class`
- `classVarDec`
- `subroutineDec`
- `parameterList`
- `subroutineBody`
- `varDec`
- `statements`
  - `letStatement`
  - `ifStatement` (com `else` opcional)
  - `whileStatement`
  - `doStatement`
  - `returnStatement`
- `expression`
- `term` (com lookahead de 2 tokens para distinguir array, chamada de método e variável)
- `expressionList`

#### Saída gerada:

- Arquivo `<Nome>.xml` representando a árvore sintática
- Estrutura compatível com os arquivos de referência do nand2tetris (capítulo 10)

---

## Como Executar

### Pré-requisitos

- **.NET 10.0** ou superior instalado
- Sistema operacional: Windows, Linux ou macOS

---

### Compilação

```bash
dotnet build src/JackAnalyzer/JackAnalyzer.csproj
```

### Execução

**Processar um único arquivo:**
```bash
dotnet run --project src/JackAnalyzer -- caminho/para/Arquivo.jack
```

**Processar um diretório inteiro:**
```bash
dotnet run --project src/JackAnalyzer -- caminho/para/diretorio/
```

**Especificar diretório de saída:**
```bash
dotnet run --project src/JackAnalyzer -- caminho/para/diretorio/ caminho/saida/
```

Para cada arquivo `Nome.jack` processado são gerados:
- `NomeT.xml` — lista de tokens (saída do Lexer)
- `Nome.xml` — árvore sintática (saída do Parser)

---

## Testes

Os testes foram realizados com os **arquivos de referência oficiais** do nand2tetris (capítulo 10), comparando a saída gerada com o gabarito usando `Compare-Object` do PowerShell.

### Conjuntos testados

| Conjunto | Arquivos |
|---|---|
| `nand2tetris/projects/10/Square` | `Main.jack`, `Square.jack`, `SquareGame.jack` |
| `nand2tetris/projects/10/ExpressionLessSquare` | `Main.jack`, `Square.jack`, `SquareGame.jack` |

### Como reproduzir

```powershell
# 1. Gerar os XMLs
dotnet run --project src/JackAnalyzer -- nand2tetris/nand2tetris/projects/10/Square output_test

# 2. Comparar com as referências
$ref = "nand2tetris/nand2tetris/projects/10/Square"
foreach ($name in @("Main", "Square", "SquareGame")) {
    $diff = Compare-Object (Get-Content "$ref\$name.xml") (Get-Content "output_test\$name.xml")
    if ($diff) { Write-Host "$name.xml: DIFERENTE"; $diff }
    else        { Write-Host "$name.xml: IGUAL" }
}

# 3. Limpar arquivos temporários
Remove-Item output_test -Recurse -Force
```

### Resultado

Todos os 6 arquivos XML gerados são **idênticos** aos arquivos de referência oficiais:

| Arquivo | Square | ExpressionLessSquare |
|---|---|---|
| `Main.xml` | ✅ IGUAL | ✅ IGUAL |
| `Square.xml` | ✅ IGUAL | ✅ IGUAL |
| `SquareGame.xml` | ✅ IGUAL | ✅ IGUAL |