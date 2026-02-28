# QuantDSL Tooling & VS Code Extension

This repository contains a small end-to-end tooling project around a domain-specific language (DSL) for quantitative finance products.

It is composed of two independent but complementary parts:
- a .NET DSL engine and CLI
- a VS Code extension that integrates the DSL into the editor

## Repository Structure

```
QuantDslTooling&VscodeExtension/
├── QuantDslTooling/
│   ├── QuantDsl.Core
│   ├── QuantDsl.Cli
│   └── QuantDsl.Tests
├── QuantDslVscodeExtension/
    ├── src/
    ├── out/
    └── package.json
```

## QuantDslTooling (C# / .NET)

Purpose:

Implements a small financial DSL with:
- Formal grammar (ANTLR)
- Syntax parsing
- Semantic analysis
- Structured diagnostics
- CLI interface

Example

```bash
dotnet run --project QuantDsl.Cli -- analyze sample.dsl
```

