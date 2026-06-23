# Karpathy Skills Benchmark

Karpathy Skills Benchmark is a .NET 10 CLI for running repeatable coding benchmarks against agent tools such as OpenCode, with or without a skills file, and storing history in SQLite.

## Skills Source

The benchmark is inspired by and can ingest the skills file from https://github.com/multica-ai/andrej-karpathy-skills.

## Prerequisites

- .NET 10 SDK
- OpenCode CLI available on PATH
- Venice AI account and API access
- Git
- Optional: Node.js and Python for non-.NET fixtures

## Installation

1. Clone this repository.
2. Restore dependencies with dotnet restore.
3. Build with dotnet build.
4. Run tests with dotnet test.

## Venice AI Setup

Set the VENICE_API_KEY environment variable before running the benchmark.
The repository root includes opencode.json, which configures the Venice AI provider for OpenCode using the OpenAI-compatible Venice endpoint.

## How to Add the Skills File

- OpenCode: copy the file to AGENTS.md in the workspace root.
- Cursor: translate the guidance into .cursorrules.
- Aider: include the guidance via its prompt or repo instructions.
- Copilot: place the guidance where your workflow surfaces repository instructions.
- Continue: add the guidance to Continue workspace rules.
- Codex CLI: inject the guidance into the workspace instructions or agent profile.

Or simply run enchmark init-skills to fetch the upstream skills/AGENTS.md placeholder.

## Running a Benchmark

Run dotnet run --project src/KarpathySkillsBenchmark -- run.
If --model is omitted, the tool offers an interactive Spectre.Console model selector using the Venice models declared in opencode.json.
Useful flags include --tasks, --repeats, --with-skills, --output, --judge-model, --timeout, --skip-judge, --only-without, and --only-with.

## Viewing History

Run dotnet run --project src/KarpathySkillsBenchmark -- history to see stored provider, tool, model, token, cost, and timing history from esults/benchmark.db.

## Interpreting Results

Compare pass rates first, then review token usage, estimated cost, wall clock time, and diff metrics.
Use the generated Markdown, HTML, and CSV reports in esults/ for deeper analysis.

## Authoring Tasks

See docs/task-authoring-guide.md for the task schema and authoring expectations.

## Adding Agents

See docs/adding-agents.md for the IAgentRunner extension model.

## Metric Reference

See docs/metric-reference.md for recorded metrics.

## Contributing & License

Contributions are welcome. The project is licensed under the terms in LICENSE.
