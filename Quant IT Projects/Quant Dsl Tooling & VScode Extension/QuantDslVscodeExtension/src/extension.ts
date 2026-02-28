import * as vscode from "vscode";
import * as path from "path";
import * as fs from "fs";
import { execSync } from "child_process";

const DIAG_COLLECTION = vscode.languages.createDiagnosticCollection("quantdsl");

function runAnalyzer(dslFilePath: string) {

  // We assume: QuantDslTooling is sibling of QuantDslVscodeExtension.
  const repoRoot = path.resolve(__dirname, "..", "..", ".."); // extension/out -> extension -> repo root
  const toolingRoot = path.join(repoRoot, "QuantDslTooling");

  const outJson = path.join(toolingRoot, "tmp_report.json");

  // Call dotnet CLI analyzer
  const cmd = `dotnet run --project QuantDsl.Cli -- analyze "${dslFilePath}" --out "${outJson}"`;

  execSync(cmd, { cwd: toolingRoot, stdio: "ignore" });

  const raw = fs.readFileSync(outJson, "utf-8");
  return JSON.parse(raw);
}

function toVsRange(line: number, col: number) {
  // diagnostics use 1-based line; VS Code uses 0-based.
  const l = Math.max(0, line - 1);
  const c = Math.max(0, col);
  return new vscode.Range(new vscode.Position(l, c), new vscode.Position(l, c + 1));
}

function refreshDiagnostics(doc: vscode.TextDocument) {
  if (doc.languageId !== "quantdsl") return;
  const filePath = doc.uri.fsPath;

  try {
    const report = runAnalyzer(filePath);

    const diagnostics: vscode.Diagnostic[] = [];

    // Syntax errors
    for (const e of report.syntaxErrors ?? []) {
      const range = toVsRange(e.line, e.column);
      diagnostics.push(
        new vscode.Diagnostic(range, e.message, vscode.DiagnosticSeverity.Error)
      );
    }

    // Semantic errors
    for (const d of report.semanticDiagnostics ?? []) {
      const range = toVsRange(d.line, d.column);
      const msg = `${d.code}: ${d.message}`;
      diagnostics.push(
        new vscode.Diagnostic(range, msg, vscode.DiagnosticSeverity.Error)
      );
    }

    DIAG_COLLECTION.set(doc.uri, diagnostics);
  } catch (err: any) {
    // If analyzer crashes, clear diagnostics but show a single warning.
    const range = new vscode.Range(new vscode.Position(0,0), new vscode.Position(0,1));
    const diag = new vscode.Diagnostic(range, `Analyzer failed: ${err?.message ?? err}`, vscode.DiagnosticSeverity.Warning);
    DIAG_COLLECTION.set(doc.uri, [diag]);
  }
}

export function activate(context: vscode.ExtensionContext) {
  context.subscriptions.push(DIAG_COLLECTION);

  // initial open
  if (vscode.window.activeTextEditor) {
    refreshDiagnostics(vscode.window.activeTextEditor.document);
  }

  context.subscriptions.push(
    vscode.workspace.onDidOpenTextDocument(refreshDiagnostics),
    vscode.workspace.onDidSaveTextDocument(refreshDiagnostics),
    vscode.workspace.onDidChangeTextDocument((e) => refreshDiagnostics(e.document))
  );
}

export function deactivate() {
  DIAG_COLLECTION.clear();
}
