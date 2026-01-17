[English](README.md) | [日本語](README_JP.md)

# Unition - Notion Unity Bridge

A lightweight Unity client for Notion API. Fetch data from Notion databases and use it in your Unity projects.

## Features

- **Simple API**: Easy to use Notion client for Unity.
- **UPM Ready**: Install via Unity Package Manager.
- **Database Queries**: Query Notion databases with filters.
- **Image Download**: Helper to download images from Notion URLs.
- **Caching**: Built-in memory cache for API responses.
- **Property Helpers**: Static methods to extract properties from generic JSON responses.

## Installation

### Via Git URL (Recommended)

Add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.seraf.unition": "https://github.com/Seraf0-org/Unition.git"
  }
}
```

### Via Local Path (Development)

```json
{
  "dependencies": {
    "com.seraf.unition": "file:../Unition"
  }
}
```

## Quick Start

### 1. Create a Notion Config

Create a config asset via **Assets > Create > Unition > Notion Config** and enter your Notion API key.

### 2. Query a Database

```csharp
using Unition;
using Cysharp.Threading.Tasks;

public class MyDataLoader : MonoBehaviour
{
    public NotionConfig config;
    
    async void Start()
    {
        var client = new NotionClient(config.apiKey, config.cacheDuration);
        
        // Query a database
        string json = await client.QueryDatabase("your-database-id");
        
        // Parse the response using property helpers
        // ... your custom parsing logic here
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
