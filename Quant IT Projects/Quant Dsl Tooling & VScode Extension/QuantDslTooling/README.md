# QuantDslTooling - DSL Tooling (ANTLR + Diagnostics)

This repo is a minimal end-to-end prototype of quant DSL tooling:
- a DSL grammar (ANTLR4)
- syntax diagnostics (line/col)
- semantic diagnostics (business rules)
- JSON report output (CLI)
- unit tests (xUnit)

## Architecture
- **QuantDsl.Core**: grammar + parser + analyzer + semantic checks
- **QuantDsl.Cli**: `qdsl analyze <file.dsl> [--out report.json]`
- **QuantDsl.Tests**: tests for analyzer behavior

## Quick start
Build:
```bash
dotnet build
