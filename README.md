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

## Funcionalidades

### Analisador Léxico

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

---

### Analisador Sintático

O analisador sintático consome os tokens gerados pelo analisador léxico e verifica se o código segue corretamente a gramática da linguagem Jack.

Foi implementado utilizando a abordagem de **recursive descent parsing**, onde cada regra da gramática é representada por um método.

#### Estruturas suportadas:

- `class`
- `classVarDec`
- `subroutineDec`
- `parameterList`
- `subroutineBody`
- `varDec`
- `statements`
  - `let`
  - `if`
  - `while`
  - `do`
  - `return`
- `expression`
- `term`
- `expressionList`

#### Saída gerada:

- Arquivo `.xml` representando a árvore sintática
- Estrutura compatível com os arquivos `*P.xml` do nand2tetris

---

## Como Executar

### Pré-requisitos

- **.NET 10.0** ou superior instalado
- Sistema operacional: Windows, Linux ou macOS

---

### Compilação

```bash
dotnet build src/JackAnalyzer/JackAnalyzer.csproj