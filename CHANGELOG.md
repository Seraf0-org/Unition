# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
