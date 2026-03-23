# Hexblick

## ビルド

WinUI 3 の MSIX パッケージングはプラットフォーム指定が必須。指定しないと以下のエラーが出る。

```bash
dotnet build -p:Platform=x64
```
