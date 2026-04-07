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

## Estrutura do Projeto
