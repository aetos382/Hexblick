---
paths:
  - '**/*.csproj'
  - Directory.Packages.props
---

# パッケージ管理に関するルール

- プロジェクトにパッケージ参照を追加する際は `dotnet package add` コマンドを使用する。
- プロジェクトからパッケージ参照を削除する際は `dotnet package remove` コマンドを使用する。
- *.csproj の `<PackageReference>` は直接編集しない。
- Directory.Packages.props の `<PackageVersion>` は直接編集しない。
  - ただし、`dotnet package remove` コマンドを使用して削除した場合は例外とする。
  - `dotnet package remove` コマンドでパッケージ参照を削除した場合、そのパッケージを参照しているプロジェクトが他になければ、`Directory.Packages.props` からも削除すること。
