# Repository Guidelines

## Project Structure & Module Organization

SkinCreator is a C# Windows Forms application targeting `net9.0-windows7.0`. The entry point is `Program.cs`, and the project file is `SkinCreator.csproj`. Main editor UI code lives in `UI/`, with dialogs under `UI/Dialogs/`. Domain models are in `Models/`, concrete skin elements are in `Models/Elements/`, serialization bridge and POCOs are split between `Models/` and `Json/`, file I/O is in `IO/`, rendering/color helpers are in `Helpers/`, and undoable editor actions are in `Commands/`. Build output directories such as `bin/`, `obj/`, `.vs/`, and `.dotnet-home/` should not be edited by hand.

## Build, Test, and Development Commands

Use RTK for shell commands in this repository.

- `rtk dotnet build SkinCreator.csproj` builds the WinForms application.
- `rtk dotnet run --project SkinCreator.csproj` launches the editor locally.
- `rtk dotnet clean SkinCreator.csproj` removes generated build artifacts.
- `rtk rg --files` lists tracked source paths quickly.

No test project is currently present. If tests are added, prefer `rtk dotnet test` from the repository root.

## Coding Style & Naming Conventions

Use C# with nullable references enabled and implicit usings as configured in `SkinCreator.csproj`. Match the existing tab-indented style in `.cs` files. Use PascalCase for types, methods, properties, and WinForms event handlers such as `DeleteItem_Click`; use camelCase for locals and private fields unless an existing file clearly follows another pattern. Keep UI behavior in `UI/`, model state in `Models/`, and command mutations in `Commands/`.

## Testing Guidelines

There is no established testing framework yet. For new tests, create a separate test project such as `SkinCreator.Tests` and use clear names like `SkinDocumentJsonBridgeTests`. Cover JSON round trips, `.xsk` I/O, command undo/redo behavior, and UI-independent model logic first.

## Commit & Pull Request Guidelines

Recent commits use short, direct subjects, including Japanese descriptions. Keep commit messages concise and action-oriented, for example `picCoverの丸角設定に対応`. Pull requests should summarize behavior changes, list manual verification steps, link related issues, and include screenshots or screen recordings for visible UI changes.

## Agent-Specific Instructions

Keep text file line endings as CRLF. Prefix shell commands with `rtk`. Prefer `code-review-graph` for codebase exploration, impact analysis, and review context before falling back to plain text search. Do not use `get_minimal_context_tool`, because it times out in this repository; use focused methods such as `list_graph_stats_tool`, `get_review_context_tool`, `detect_changes_tool`, `get_impact_radius_tool`, `semantic_search_nodes_tool`, or `query_graph_tool` instead.
