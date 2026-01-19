[English](README.md) | [日本語](README_JP.md)

# Unition - Notion Unity Bridge

A lightweight Unity client for Notion API. Fetch data from Notion databases and use it in your Unity projects.

## Features

- **Database Queries**: Query Notion databases with filters.
- **Page Search**: Find databases and pages by name at runtime.
- **Image Download**: Helper to download images from Notion URLs.
- **Caching**: Built-in memory cache for API responses.
- **Property Helpers**: Static methods to extract properties from JSON responses.
- **Editor Tools**: Fetch and browse databases/pages directly in Inspector.

## Installation

### Unity Package Manager (Recommended)

1. Open **Window > Package Manager**
2. Click **+ > Add package from git URL...**
3. Enter `https://github.com/Seraf0-org/Unition.git`

### Via manifest.json

Add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.seraf.unition": "https://github.com/Seraf0-org/Unition.git"
  }
}
```

## Quick Start

### 1. Create a Notion Config

Create a config asset via **Assets > Create > Unition > Notion Config** and enter your Notion API key.

### 2. Query a Database

```csharp
using Unition;

public class MyDataLoader : MonoBehaviour
{
    public NotionConfig config;
    
    async void Start()
    {
        var client = new NotionClient(config.apiKey, config.cacheDuration);
        
        // Query by ID
        string json = await client.QueryDatabase("your-database-id");
        
        // Or find by name (v1.1.0+)
        string dbId = await client.FindDatabaseIdByName("Cards");
        string cardsJson = await client.QueryDatabase(dbId);
        
        // Find pages by name
        string pageId = await client.FindPageIdByName("Rules");
        string pageJson = await client.GetPage(pageId);
    }
}
```

### 3. Extract Properties

Use the `NotionPropertyHelpers` to extract data from Notion's JSON response:

```csharp
using Unition;

// Extract various property types
string title = NotionPropertyHelpers.ExtractTitleProperty(pageJson, "Name");
string text = NotionPropertyHelpers.ExtractRichTextProperty(pageJson, "Description");
int number = NotionPropertyHelpers.ExtractNumberProperty(pageJson, "Count", defaultValue: 0);
string status = NotionPropertyHelpers.ExtractSelectProperty(pageJson, "Status");
List<string> tags = NotionPropertyHelpers.ExtractMultiSelectProperty(pageJson, "Tags");
List<string> relatedIds = NotionPropertyHelpers.ExtractRelationProperty(pageJson, "RelatedItems");
string imageUrl = NotionPropertyHelpers.ExtractImageUrl(pageJson, "Cover");
```

## Advanced Usage: Database Mappings (v1.2.0+)

Instead of hardcoding database IDs, you can define **key-name mappings** in the Inspector and resolve them at runtime.

### 1. Setup in Inspector

Add mappings in your `NotionConfig` asset:

| Key | Database Name |
|-----|---------------|
| `cards` | Cards |
| `items` | Items Database |
| `enemies` | Enemies |

### 2. Resolve at Runtime

```csharp
using Unition;

public class GameDataManager : MonoBehaviour
{
    public NotionConfig config;
    
    async void Start()
    {
        var client = new NotionClient(config.apiKey, config.cacheDuration);
        
        // Resolve all mappings
        await config.ResolveAllAsync(client);
        
        // Get resolved IDs by key
        string cardsDbId = config.GetDatabaseId("cards");
        string itemsDbId = config.GetDatabaseId("items");
        
        // Query databases
        string cardsJson = await client.QueryDatabase(cardsDbId);
    }
}
```

### 3. Extending for Custom Projects

You can still extend `NotionConfig` for project-specific needs:

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Notion Config")]
public class GameNotionConfig : NotionConfig
{
    // Add custom fields or methods here
    public override bool IsValid()
    {
        return base.IsValid() && databaseMappings.Count > 0;
    }
}
```

## Requirements

- Unity 2021.3 or later
- .NET Standard 2.1

### Optional Dependencies

- [UniTask](https://github.com/Cysharp/UniTask) - For async/await support (recommended)

## Getting a Notion API Key

1. Go to [Notion Integrations](https://www.notion.so/my-integrations)
2. Create a new integration
3. Copy the "Internal Integration Token"
4. Share your database with the integration

## License

MIT License - see [LICENSE](LICENSE) for details.
