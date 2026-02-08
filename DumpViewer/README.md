# DumpViewer - Mini IT-Quant Project

This project is a minimal WPF application demonstrating a typical IT Quant workflow:
loading and analyzing pricing dumps in a testable and maintainable architecture.

## Stack
- .NET 9
- WPF
- MVVM
- async/await
- xUnit

## Architecture
- DumpViewer.Core: domain models, services, and ViewModel (testable, no UI dependency)
- DumpViewer.App: WPF UI and user interactions
- DumpViewer.Tests: unit tests for core logic

## Run
dotnet run --project DumpViewer.App

## Tests
dotnet test
