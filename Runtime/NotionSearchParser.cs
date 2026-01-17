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
    }
}
