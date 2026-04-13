# Analisador Léxico para a Linguagem Jack (nand2tetris)

## Descrição

Este projeto tem como objetivo implementar um **analisador léxico (scanner)** para a linguagem **Jack**, utilizando **C#**. O programa deve ler arquivos fonte `.jack`, identificar os tokens da linguagem e gerar um arquivo `.xml` no formato exigido pelo projeto nand2tetris.

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

## Funcionalidades

- Leitura de arquivos `.jack`
- Identificação dos seguintes tokens:
  - `keyword`
  - `symbol`
  - `integerConstant`
  - `stringConstant`
  - `identifier`
- Remoção de ruídos:
  - Espaços em branco
  - Quebras de linha
  - Comentários (`//` e `/* */`)
- Geração de saída `.xml` no padrão nand2tetris:
  - Estrutura `<tokens> ... </tokens>`
  - Escape de caracteres especiais (`&`, `<`, `>`, `"`)
  - Sem inclusão de EOF

---

## Como Executar

### Pré-requisitos

- **.NET 10.0** ou superior instalado
- Sistema operacional: Windows, Linux ou macOS

### Compilação

A partir da raiz do repositório:
```bash
dotnet build src/JackAnalyzer/JackAnalyzer.csproj
```

ou, alternadamente, entre no diretório do projeto:
```bash
cd src/JackAnalyzer
dotnet build
```

### Ambiente de teste

- .NET SDK 10.0.201 instalado
- Projeto compilado com `dotnet build src/JackAnalyzer/JackAnalyzer.csproj`
- Validação realizada em `nand2tetris/nand2tetris/projects/10/Square`

### Execução

#### Arquivo Individual

Para analisar um arquivo `.jack` específico, execute a partir da raiz do repositório:

```bash
dotnet run --project src/JackAnalyzer/JackAnalyzer.csproj -- <caminho_do_arquivo.jack> [diretorio_saida]
```

ou, se estiver dentro do diretório do projeto:

```bash
cd src/JackAnalyzer
dotnet run -- <caminho_do_arquivo.jack> [diretorio_saida]
```

- `caminho_do_arquivo.jack`: caminho completo ou relativo para o arquivo Jack.
- `diretorio_saida` (opcional): pasta onde serão gravados os arquivos de saída `.xml`. Se omitido, usa o mesmo diretório do arquivo `.jack`.

**Exemplos:**

```bash
# Análise básica (arquivo de saída no mesmo diretório do .jack)
dotnet run --project src/JackAnalyzer/JackAnalyzer.csproj -- "C:\caminho\para\Main.jack"

# Com diretório de saída personalizado
dotnet run --project src/JackAnalyzer/JackAnalyzer.csproj -- "C:\caminho\para\Main.jack" "C:\caminho\para\resultado"
```

**Exemplo específico para o projeto Square:**

```powershell
cd "src\JackAnalyzer"
dotnet run -- "..\..\nand2tetris\nand2tetris\projects\10\Square\Main.jack"
```

#### Processamento em Lote (Múltiplos Arquivos)

Para processar **todos os arquivos `.jack` de uma pasta de uma vez** e gerar os tokens em uma **pasta separada**:

```bash
dotnet run -- <caminho_da_pasta> [diretorio_saida]
```

- `caminho_da_pasta`: diretório contendo os arquivos `.jack`
- `diretorio_saida` (opcional): pasta de destino. Se omitido, cria uma subpasta `tokens` automaticamente no mesmo diretório.

**Exemplos:**

```powershell
# Processa todos os .jack do Square em uma pasta "tokens" criada automaticamente
cd "src\JackAnalyzer"
dotnet run -- "..\..\nand2tetris\nand2tetris\projects\10\Square"

# Especificar pasta de saída personalizada
dotnet run -- "..\..\nand2tetris\nand2tetris\projects\10\Square" "C:\resultado\tokens"
```

**Resultado:**
- MainT.xml
- SquareT.xml  
- SquareGameT.xml

Serão gerados em `projects/10/Square/tokens/` ou no local especificado.

### Verificação dos Resultados

Para verificar se os arquivos gerados estão corretos, use o **TextComparer**:

```bash
# Navegue até a pasta tools
cd nand2tetris/nand2tetris/tools

# Compare com arquivo de referência
.\TextComparer.bat "caminho/arquivo_gerado.xml" "caminho/arquivo_referencia.xml"
```

**Resultado esperado:**
```
Comparison ended successfully
```

### Estrutura de Arquivos Gerados

Após processar a pasta Square, serão criados arquivos `.xml` em uma pasta `tokens`:

```
Square/
├── Main.jack
├── Square.jack
├── SquareGame.jack
├── tokens/                    # Pasta criada automaticamente
│   ├── MainT.xml            # Tokens do Main.jack
│   ├── SquareT.xml          # Tokens do Square.jack
│   └── SquareGameT.xml       # Tokens do SquareGame.jack
└── *.xml                     # Arquivos de referência (Main.xml, Square.xml, SquareGame.xml)
```

Exemplo de conteúdo de um arquivo gerado:

```xml
<tokens>
  <keyword> class </keyword>
  <identifier> Main </identifier>
  <symbol> { </symbol>
  <!-- ... outros tokens ... -->
</tokens>
```

---

## Estrutura do Projeto

```
CompiladoresUFMA-2026.1/
├── README.md                          # Este arquivo
├── nand2tetris/                       # Arquivos do projeto nand2tetris
│   └── projects/
│       └── 10/
│           ├── ArrayTest/             # Projeto de exemplo
│           │   ├── Main.jack         # Código fonte
│           │   ├── MainT.xml         # Tokens (saída do analisador)
│           │   └── Main.xml          # Análise sintática (próximo passo)
│           └── Square/                # Projeto Square
│               ├── Main.jack
│               ├── Square.jack
│               ├── SquareGame.jack
│               ├── tokens/            # Pasta com arquivos gerados pelo analisador
│               │   ├── MainT.xml
│               │   ├── SquareT.xml
│               │   └── SquareGameT.xml
│               ├── Main.xml           # Arquivos de referência para análise sintática
│               ├── Square.xml
│               └── SquareGame.xml
└── src/
    └── JackAnalyzer/                  # Projeto C#
        ├── JackAnalyzer.csproj       # Arquivo de projeto
        ├── Program.cs                # Ponto de entrada
        ├── JackTokenizer.cs          # Implementação do analisador léxico
        └── TolkenType.cs             # Enumeração dos tipos de token
```

---

## Exemplos de Uso

### Exemplo 1: Análise Básica

```bash
# Entrada: Main.jack
class Main {
    function void main() {
        var int x;
        let x = 42;
        return;
    }
}

# Comando
dotnet run -- "projects/10/ArrayTest/Main.jack"

# Saída: MainT.xml
<tokens>
  <keyword> class </keyword>
  <identifier> Main </identifier>
  <symbol> { </symbol>
  <keyword> function </keyword>
  <keyword> void </keyword>
  <identifier> main </identifier>
  <symbol> ( </symbol>
  <symbol> ) </symbol>
  <symbol> { </symbol>
  <keyword> var </keyword>
  <keyword> int </keyword>
  <identifier> x </identifier>
  <symbol> ; </symbol>
  <keyword> let </keyword>
  <identifier> x </identifier>
  <symbol> = </symbol>
  <integerConstant> 42 </integerConstant>
  <symbol> ; </symbol>
  <keyword> return </keyword>
  <symbol> ; </symbol>
  <symbol> } </symbol>
  <symbol> } </symbol>
</tokens>
```

### Exemplo 2: Com Strings e Comentários

```bash
# Entrada: arquivo com comentários e strings
// Este é um comentário
class Hello {
    function void main() {
        do Output.printString("Olá, mundo!");
        return;
    }
}

# Saída: comentários removidos, strings preservadas
<tokens>
  <keyword> class </keyword>
  <identifier> Hello </identifier>
  <symbol> { </symbol>
  <keyword> function </keyword>
  <keyword> void </keyword>
  <identifier> main </identifier>
  <symbol> ( </symbol>
  <symbol> ) </symbol>
  <symbol> { </symbol>
  <keyword> do </keyword>
  <identifier> Output </identifier>
  <symbol> . </symbol>
  <identifier> printString </identifier>
  <symbol> ( </symbol>
  <stringConstant> Olá, mundo! </stringConstant>
  <symbol> ) </symbol>
  <symbol> ; </symbol>
  <keyword> return </keyword>
  <symbol> ; </symbol>
  <symbol> } </symbol>
  <symbol> } </symbol>
</tokens>
```

---

## Próximos Passos

Este projeto implementa apenas a **análise léxica**. Os próximos passos incluem:

1. **Análise Sintática (Parser)**: Construir árvore de sintaxe abstrata
2. **Geração de Código**: Traduzir para VM code
3. **Otimização**: Melhorar o código gerado

---

## Referências

- [nand2tetris - The Elements of Computing Systems](https://www.nand2tetris.org/)
- [Documentação da Linguagem Jack](https://www.nand2tetris.org/project10)