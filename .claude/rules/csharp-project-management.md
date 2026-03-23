---
paths:
  - '*.slnx'
  - '**/*.csproj'
---

# .NET のプロジェクト管理に関するルール

- プロジェクトを追加する際は `dotnet new` コマンドを使用する。
- プロジェクトをソリューションに追加する際は `dotnet solution add` コマンドを使用する。
- プロジェクト参照を追加する際は `dotnet reference add` コマンドを使用する。
- これらの目的で `*.slnx` ファイルや `*.csproj` ファイルを直接編集しない。
