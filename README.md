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
- .NET

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

- **.NET 8.0** ou superior instalado
- Sistema operacional: Windows, Linux ou macOS

### Compilação

1. Navegue até o diretório do projeto:
   ```bash
   cd src/JackAnalyzer
   ```

2. Compile o projeto:
   ```bash
   dotnet build
   ```

### Execução

#### Arquivo Individual

Para analisar um arquivo `.jack` específico:

```bash
dotnet run -- <caminho_do_arquivo.jack> [diretorio_saida]
```

- `caminho_do_arquivo.jack`: caminho completo ou relativo para o arquivo Jack.
- `diretorio_saida` (opcional): pasta onde serão gravados os arquivos de saída `.xml`.

**Exemplos:**

```bash
# Análise básica (arquivo de saída no mesmo diretório do .jack)
dotnet run -- "C:\caminho\para\Main.jack"

# Com diretório de saída personalizado
dotnet run -- "C:\caminho\para\Main.jack" "C:\caminho\para\resultado"
```

**Exemplo específico para o projeto Square:**

```powershell
cd "C:\Users\gabri\OneDrive\Documentos\GitHub\CompiladoresUFMA-2026.1\src\JackAnalyzer"
dotnet run -- "C:\Users\gabri\OneDrive\Documentos\GitHub\CompiladoresUFMA-2026.1\nand2tetris\nand2tetris\projects\10\Square\Main.jack"
```

Se quiser rodar a partir da raiz do repositório sem mudar de pasta:

```powershell
dotnet run --project "src/JackAnalyzer/JackAnalyzer.csproj" -- "C:\Users\gabri\OneDrive\Documentos\GitHub\CompiladoresUFMA-2026.1\nand2tetris\nand2tetris\projects\10\Square\Main.jack"
```

> O comando gera um arquivo `.xml` com os tokens no mesmo diretório do `.jack`, normalmente chamado `MainT.xml`.

#### Processamento em Lote

Para processar todos os arquivos `.jack` de uma pasta:

```powershell
# Windows PowerShell
.\process_square.ps1
```

Ou manualmente:

```bash
# Processar Main.jack
dotnet run -- "projects/10/Square/Main.jack" "resultado"

# Processar SquareGame.jack
dotnet run -- "projects/10/Square/SquareGame.jack" "resultado"

# Processar Square.jack
dotnet run -- "projects/10/Square/Square.jack" "resultado"
```

### Verificação dos Resultados

Para verificar se os arquivos gerados estão corretos, use o **TextComparer**:

```bash
# Navegue até a pasta tools
cd nand2tetris/tools

# Compare com arquivo de referência
.\TextComparer.bat "caminho/arquivo_gerado.xml" "caminho/arquivo_referencia.xml"
```

**Resultado esperado:**
```
Comparison ended successfully
```

### Estrutura de Arquivos Gerados

Após a execução, serão criados arquivos `.xml` com a seguinte estrutura:

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
├── CompiladoresUFMA-2026.1.sln        # Solução Visual Studio
├── process_square.ps1                 # Script para processamento em lote
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
│               ├── resultado/         # Pasta para arquivos gerados
│               │   ├── MainT.xml
│               │   ├── SquareT.xml
│               │   └── SquareGameT.xml
│               └── *.xml             # Arquivos de referência
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