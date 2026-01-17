using System.Collections.Generic;

namespace Unition
{
    /// <summary>
    /// Represents a Notion database's basic info.
    /// </summary>
    [System.Serializable]
    public class NotionDatabaseInfo
    {
        public string id;
        public string title;
        
        public override string ToString() => $"{title} ({id})";
    }

    /// <summary>
    /// Represents a Notion page's basic info.
    /// </summary>
    [System.Serializable]
    public class NotionPageInfo
    {
        public string id;
        public string title;
        public string parentId;
        public bool isDatabase; // true if parent is a database
        
        public override string ToString() => $"{title} ({id})";
    }

    /// <summary>
    /// Helper for parsing Notion Search API responses.
    /// </summary>
    public static class NotionSearchParser
    {
        /// <summary>
        /// Parse database list from Notion Search API response.
        /// </summary>
        public static List<NotionDatabaseInfo> ParseDatabases(string json)
        {
            var databases = new List<NotionDatabaseInfo>();
            
            if (string.IsNullOrEmpty(json)) return databases;
            
            // Find each database object
            int dbStart = 0;
            while ((dbStart = json.IndexOf("{\"object\":\"database\"", dbStart + 1)) >= 0)
            {
                int dbEnd = NotionPropertyHelpers.FindMatchingBrace(json, dbStart);
                if (dbEnd < 0) break;
                
                string dbJson = json.Substring(dbStart, dbEnd - dbStart + 1);
                
                var info = new NotionDatabaseInfo();
                info.id = NotionPropertyHelpers.ExtractStringValue(dbJson, "\"id\"");
                
                // Extract title from title array
                int titleStart = dbJson.IndexOf("\"title\"");
                if (titleStart >= 0)
                {
                    int plainTextStart = dbJson.IndexOf("\"plain_text\"", titleStart);
                    if (plainTextStart >= 0 && plainTextStart < titleStart + 300)
                    {
                        info.title = NotionPropertyHelpers.ExtractStringValue(
                            dbJson.Substring(plainTextStart), "\"plain_text\"");
                    }
                }
                
                if (!string.IsNullOrEmpty(info.id))
                {
                    // Clean up ID (remove dashes if present)
                    info.id = info.id.Replace("-", "");
                    
                    if (string.IsNullOrEmpty(info.title))
                    {
                        info.title = "Untitled Database";
                    }
                    
                    databases.Add(info);
                }
                
                dbStart = dbEnd;
            }
            
            return databases;
        }

        /// <summary>
        /// Parse page list from Notion Search API response.
        /// </summary>
        public static List<NotionPageInfo> ParsePages(string json)
        {
            var pages = new List<NotionPageInfo>();
            
            if (string.IsNullOrEmpty(json)) return pages;
            
            // Find each page object
            int pageStart = 0;
            while ((pageStart = json.IndexOf("{\"object\":\"page\"", pageStart + 1)) >= 0)
            {
                int pageEnd = NotionPropertyHelpers.FindMatchingBrace(json, pageStart);
                if (pageEnd < 0) break;
                
                string pageJson = json.Substring(pageStart, pageEnd - pageStart + 1);
                
                var info = new NotionPageInfo();
                info.id = NotionPropertyHelpers.ExtractStringValue(pageJson, "\"id\"");
                
                // Check parent type
                info.isDatabase = pageJson.Contains("\"parent\":{\"type\":\"database_id\"") ||
                                  pageJson.Contains("\"parent\": {\"type\": \"database_id\"");
                
                // Extract title from properties.title or Name
                info.title = ExtractPageTitle(pageJson);
                
                if (!string.IsNullOrEmpty(info.id))
                {
                    info.id = info.id.Replace("-", "");
                    
                    if (string.IsNullOrEmpty(info.title))
                    {
                        info.title = "Untitled Page";
                    }
                    
                    pages.Add(info);
                }
                
                pageStart = pageEnd;
            }
            
            return pages;
        }

        private static string ExtractPageTitle(string pageJson)
        {
            // Try to find title in properties
            int propsStart = pageJson.IndexOf("\"properties\"");
            if (propsStart < 0) return null;
            
            // Look for title type property
            int titleTypeStart = pageJson.IndexOf("\"type\":\"title\"", propsStart);
            if (titleTypeStart < 0) return null;
            
            // Look backwards for the property name, then forward for plain_text
            int plainTextStart = pageJson.IndexOf("\"plain_text\"", titleTypeStart);
            if (plainTextStart >= 0 && plainTextStart < titleTypeStart + 200)
            {
                return NotionPropertyHelpers.ExtractStringValue(
                    pageJson.Substring(plainTextStart), "\"plain_text\"");
            }
            
            return null;
        }
    }
}
