[English](README.md) | [日本語](README_JP.md)

# Unition - Notion Unity Bridge

UnityプロジェクトでNotionデータベースのデータを取得・利用するための軽量Notion APIクライアント。

## 機能

- 🔌 **シンプルAPI** - Unityで扱いやすいNotionクライアント
- 📦 **UPM対応** - Unity Package Manager経由でインストール可能
- 🗄️ **データベースクエリ** - フィルター対応のNotionデータベース検索
- 🖼️ **画像ダウンロード** - Notionからの画像ダウンロード機能
- ⚡ **キャッシュ機能** - APIレスポンスのメモリ内キャッシュ機能
- 🔧 **プロパティヘルパー** - JSONレスポンスから簡単に値を抽出

## インストール

### Git URL経由（推奨）

`Packages/manifest.json` に以下を追加してください：

```json
{
  "dependencies": {
    "com.seraf.unition": "https://github.com/Seraf0-org/Unition.git"
  }
}
```

### ローカルパス経由（開発用）

```json
{
  "dependencies": {
    "com.seraf.unition": "file:../Unition"
  }
}
```

## クイックスタート

### 1. Notion Configの作成

**Assets > Create > Unition > Notion Config** から設定アセットを作成し、Notion APIキーを入力してください。
「Refresh Databases」ボタンを押すと、接続されているデータベースの一覧を確認できます。

### 2. データベースのクエリ

```csharp
using Unition;
using Cysharp.Threading.Tasks;

public class MyDataLoader : MonoBehaviour
{
    public NotionConfig config;
    
    async void Start()
    {
        var client = new NotionClient(config.apiKey, config.cacheDuration);
        
        // データベースをクエリ
        string json = await client.QueryDatabase("your-database-id");
        
        // プロパティヘルパーを使ってパース
        // ... ここに独自のパース処理を記述
    }
}
```

### 3. プロパティの抽出

`NotionPropertyHelpers` を使用して、NotionのJSONレスポンスからデータを抽出できます：

```csharp
using Unition;

// 各種プロパティの抽出
string title = NotionPropertyHelpers.ExtractTitleProperty(pageJson, "Name");
string text = NotionPropertyHelpers.ExtractRichTextProperty(pageJson, "Description");
int number = NotionPropertyHelpers.ExtractNumberProperty(pageJson, "Count", defaultValue: 0);
string status = NotionPropertyHelpers.ExtractSelectProperty(pageJson, "Status");
List<string> tags = NotionPropertyHelpers.ExtractMultiSelectProperty(pageJson, "Tags");
List<string> relatedIds = NotionPropertyHelpers.ExtractRelationProperty(pageJson, "RelatedItems");
string imageUrl = NotionPropertyHelpers.ExtractImageUrl(pageJson, "Cover");
```

## 要件

- Unity 2021.3 以降
- .NET Standard 2.1

### オプションの依存関係

- [UniTask](https://github.com/Cysharp/UniTask) - async/awaitのサポート（推奨）

## Notion APIキーの取得方法

1. [Notion Integrations](https://www.notion.so/my-integrations) にアクセス
2. 新しいインテグレーションを作成
3. "Internal Integration Token" をコピー
4. 使用したいデータベースをそのインテグレーションと共有（メニューの「Connect to」から選択）

## ライセンス

MIT License - 詳細は [LICENSE](LICENSE) を参照してください。
