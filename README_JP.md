[English](README.md) | [日本語](README_JP.md)

# Unition - Notion Unity Bridge

UnityプロジェクトでNotionデータベースのデータを取得・利用するための軽量Notion APIクライアント。

## 機能

- **Database Queries**: フィルターを使用したデータベース検索に対応。
- **Page Search**: データベースやページを名前で検索可能。
- **Image Download**: Notion上の画像をダウンロードする機能。
- **Caching**: APIレスポンスをメモリ上にキャッシュ。
- **Property Helpers**: JSONレスポンスから汎用的に値を抽出するヘルパー。
- **Editor Tools**: Inspectorからデータベース/ページを取得・閲覧可能。

## インストール

### Unity Package Manager (推奨)

1. Unityで **Window > Package Manager** を開く
2. 左上の **+ > Add package from git URL...** をクリック
3. `https://github.com/Seraf0-org/Unition.git` を入力してAdd

### manifest.json を編集する場合

`Packages/manifest.json` に以下を追加してください：

```json
{
  "dependencies": {
    "com.seraf.unition": "https://github.com/Seraf0-org/Unition.git"
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

public class MyDataLoader : MonoBehaviour
{
    public NotionConfig config;
    
    async void Start()
    {
        var client = new NotionClient(config.apiKey, config.cacheDuration);
        
        // IDで直接クエリ
        string json = await client.QueryDatabase("your-database-id");
        
        // 名前で検索 (v1.1.0+)
        string dbId = await client.FindDatabaseIdByName("Cards");
        string cardsJson = await client.QueryDatabase(dbId);
        
        // ページも名前で検索可能
        string pageId = await client.FindPageIdByName("Rules");
        string pageJson = await client.GetPage(pageId);
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

## 応用：Database Mappings (v1.2.0+)

データベースIDをハードコーディングする代わりに、Inspector上で**キーと名前のマッピング**を定義し、実行時に解決できます。

### 1. Inspectorでの設定

`NotionConfig` アセットにマッピングを追加します：

| Key | Database Name |
|-----|---------------|
| `cards` | Cards |
| `items` | Items Database |
| `enemies` | Enemies |

### 2. 実行時の解決

```csharp
using Unition;

public class GameDataManager : MonoBehaviour
{
    public NotionConfig config;
    
    async void Start()
    {
        var client = new NotionClient(config.apiKey, config.cacheDuration);
        
        // 全マッピングを解決
        await config.ResolveAllAsync(client);
        
        // キーで解決済みIDを取得
        string cardsDbId = config.GetDatabaseId("cards");
        string itemsDbId = config.GetDatabaseId("items");
        
        // データベースをクエリ
        string cardsJson = await client.QueryDatabase(cardsDbId);
    }
}
```

### 3. プロジェクト固有の拡張

必要に応じて `NotionConfig` を継承してカスタマイズも可能です：

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Notion Config")]
public class GameNotionConfig : NotionConfig
{
    // カスタムフィールドやメソッドを追加
    public override bool IsValid()
    {
        return base.IsValid() && databaseMappings.Count > 0;
    }
}
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
