# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.1] - 2026-01-19

### Added
- **Generate Keys Class**: Button in Inspector to generate a C# constants class from database mappings for IDE autocomplete and type safety.

---

## [1.2.0] - 2026-01-19

### Added
- **Database Mappings**: Define key-name pairs in Inspector, resolve all at runtime with `ResolveAllAsync()`.
- **GetDatabaseId(key)**: Retrieve resolved database ID by key.
- **Editor UI**: Add/remove mappings, quick-add from fetched database list.

### Changed
- `NotionConfig.IsValid()` is now `virtual` for easier extension.

---

## [1.1.0] - 2026-01-18

### Added
- **Page Search**: `FindPageIdByName()`, `GetAllPages()`, `SearchPagesAsync()` for searching pages by name.
- **Database Name Resolution**: `FindDatabaseIdByName()` for searching databases by name at runtime.
- **Fetch Pages Button**: Editor now fetches both databases and pages, displayed in separate foldout sections.
- **Open Notion Integrations**: Quick button to open Notion integrations page when API key is missing.

### Changed
- Renamed "Fetch Databases" to "Fetch Pages" in Config editor (fetches both databases and pages).
- Improved editor UI with foldable sections and icons.

---

## [1.0.0] - 2026-01-18

### Added
- Initial release (初回リリース)
- `NotionClient`: Core API client for querying databases and retrieving pages.
- `NotionConfig`: ScriptableObject for managing API keys and settings.
- `NotionPropertyHelpers`: Utilities for extracting simplified data from Notion JSON (Title, RichText, Number, Select, Relation, etc).
- **Editor Extensions**:
  - `Database Browser`: Window to fetch and view accessible databases (`Unition > Database Browser`).
  - `Create Notion Config`: Menu item to easily create config assets (`Unition > Create Notion Config`).
  - Database Auto-Fetch: Button in Inspector to fetch database IDs automatically.
- **Samples**: Basic data loading example included.
