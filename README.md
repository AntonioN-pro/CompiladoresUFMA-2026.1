# Compilador Jack para XML e VM (nand2tetris)

## Descrição

Este projeto implementa, em C#, as etapas principais do compilador da linguagem Jack propostas no nand2tetris:

- análise léxica (scanner)
- análise sintática (parser)
- geração de código intermediário para a máquina virtual Hack (arquivos .vm)

O programa é capaz de:

1. Ler arquivos fonte `.jack`
2. Identificar os tokens da linguagem (análise léxica)
3. Validar a estrutura do programa conforme a gramática Jack (análise sintática)
4. Gerar arquivos `.xml` compatíveis com o padrão do projeto nand2tetris
5. Gerar arquivos `.vm` compatíveis com o VM Emulator oficial do curso

A implementação foi desenvolvida com base conceitual fornecida em aula, sendo o código totalmente autoral.

---

## Status da Implementação

Etapa concluída:

- Capítulo 10: geração de XML de tokens e árvore sintática
- Capítulo 11: geração de código VM a partir de Jack

Observação: a geração de XML foi mantida para apoio de depuração e comparação com os arquivos de referência.

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
  CodeGen/
    SymbolKind.cs         # Categorias de símbolo (static, field, argument, var)
    SymbolTable.cs        # Tabela de símbolos por escopo (classe e sub-rotina)
    VmSegment.cs          # Segmentos da VM Hack
    VmWriter.cs           # Escrita dos comandos VM
    VmCompilationEngine.cs # Compilação Jack -> VM
  Lexer/
    JackTokenizer.cs      # Leitura e tokenização dos arquivos .jack
    TokenType.cs          # Enum com os tipos de token (KEYWORD, SYMBOL, etc.)
  Parser/
    CompilationEngine.cs  # Parser recursivo descendente — gera a árvore sintática
    ParserToken.cs        # Tipo que representa um token para o parser
    ParseXmlWriter.cs     # Escrita do XML de saída com indentação e escape
    TokenXmlReader.cs     # Leitura do XML de tokens gerado pelo Lexer
  Program.cs              # Ponto de entrada — orquestra Lexer, Parser e CodeGen
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

### Gerador de Código VM (`CodeGen/`)

O gerador consome a lista de tokens já validada pelo parser e produz código da máquina virtual Hack.

Recursos implementados:

- Escopos e símbolos:
  - `static`, `field`, `argument`, `var`
- Sub-rotinas:
  - `constructor`, `function`, `method`
- Statements:
  - `let`, `if`, `while`, `do`, `return`
- Expressões e termos:
  - operadores aritméticos e lógicos
  - constantes inteiras, strings e keywords (`true`, `false`, `null`, `this`)
  - chamadas de função/método (incluindo dispatch por objeto)
  - acesso e atribuição em arrays

Saída gerada:

- Arquivo `<Nome>.vm` compatível com o VM Emulator (capítulo 11)

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
- `Nome.vm` — código intermediário para a VM (saída do CodeGen)

---

## Como Executar no VM Emulator

### 1. Gerar arquivos VM de um conjunto oficial

Exemplo com Square (capítulo 11):

```bash
dotnet run --project src/JackAnalyzer -- nand2tetris/nand2tetris/projects/11/Square output_vm/Square
```

### 2. Abrir o VM Emulator oficial

No Windows:

```powershell
.\nand2tetris\nand2tetris\tools\VMEmulator.bat
```

No VM Emulator:

1. Clique em Load Program
2. Selecione a pasta gerada (exemplo: `output_vm/Square`)
3. Carregue o script de teste correspondente do nand2tetris (quando aplicável)
4. Execute com Run

---

## Roteiro de Apresentação

Sugestão de sequência para apresentação em sala:

1. Contexto do trabalho
  - objetivo do compilador Jack no nand2tetris
  - etapas implementadas (lexer, parser e codegen)
2. Arquitetura do projeto
  - pasta `Lexer`: tokenização
  - pasta `Parser`: validação sintática e XML
  - pasta `CodeGen`: geração VM com SymbolTable e VmWriter
3. Fluxo de compilação
  - entrada `.jack`
  - saídas `.xml` e `.vm`
4. Demonstração ao vivo
  - compilar `projects/11/Seven` e `projects/11/Square`
  - abrir no VM Emulator e executar
5. Validação
  - mostrar que os conjuntos principais do capítulo 11 compilam sem erro

---

## Testes

Os testes foram realizados com os arquivos oficiais do nand2tetris.

- Capítulo 10: comparação de XML com os arquivos de referência
- Capítulo 11: compilação completa dos programas Jack para VM

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

### Validação do Capítulo 11

Conjuntos compilados com sucesso para `.vm`:

- `Seven`
- `Average`
- `ConvertToBin`
- `Square`
- `Pong`
- `ComplexArrays`