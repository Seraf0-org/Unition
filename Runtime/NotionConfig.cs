using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unition
{
    /// <summary>
    /// Represents a mapping from a key to a Notion database name.
    /// The ID is resolved at runtime using FindDatabaseIdByName.
    /// </summary>
    [System.Serializable]
    public class DatabaseMapping
    {
        [Tooltip("A unique key to identify this database (e.g., 'cards', 'items')")]
        public string key;
        
        [Tooltip("The name of the database in Notion")]
        public string databaseName;
        
        [HideInInspector]
        public string resolvedId;
        
        public bool IsResolved => !string.IsNullOrEmpty(resolvedId);
    }

    /// <summary>
    /// Configuration for Notion API connection.
    /// Create via Assets > Create > Unition > Notion Config
    /// </summary>
    [CreateAssetMenu(fileName = "NotionConfig", menuName = "Unition/Notion Config")]
    public class NotionConfig : ScriptableObject
    {
        [Header("API Settings")]
        [Tooltip("Notion Integration Token (starts with 'ntn_' or 'secret_')")]
        public string apiKey;
        
        [Header("Cache Settings")]
        [Tooltip("Cache duration in seconds (0 = no cache)")]
        public float cacheDuration = 300f;
        
        [Header("Database Mappings")]
        [Tooltip("Define database mappings that can be resolved by name at runtime")]
        public List<DatabaseMapping> databaseMappings = new List<DatabaseMapping>();
        
        /// <summary>
        /// Check if the configuration is valid.
        /// </summary>
        public virtual bool IsValid()
        {
            return !string.IsNullOrEmpty(apiKey);
        }
        
        /// <summary>
        /// Resolve all database mappings by name.
        /// Call this at startup before querying databases.
        /// </summary>
        public async Task ResolveAllAsync(NotionClient client)
        {
            if (client == null)
            {
                Debug.LogError("[Unition] NotionClient is null. Cannot resolve database mappings.");
                return;
            }
            
            foreach (var mapping in databaseMappings)
            {
                if (string.IsNullOrEmpty(mapping.databaseName))
                {
                    Debug.LogWarning($"[Unition] Database mapping '{mapping.key}' has no database name.");
                    continue;
                }
                
                mapping.resolvedId = await client.FindDatabaseIdByName(mapping.databaseName);
                
                if (string.IsNullOrEmpty(mapping.resolvedId))
                {
                    Debug.LogWarning($"[Unition] Could not resolve database: '{mapping.databaseName}' (key: {mapping.key})");
                }
                else
                {
                    Debug.Log($"[Unition] Resolved '{mapping.key}': {mapping.databaseName} -> {mapping.resolvedId}");
                }
            }
        }
        
        /// <summary>
        /// Get the resolved database ID by key.
        /// Returns null if the key is not found or not resolved.
        /// </summary>
        public string GetDatabaseId(string key)
        {
            var mapping = databaseMappings.Find(m => m.key == key);
            return mapping?.resolvedId;
        }
        
        /// <summary>
        /// Check if a specific database mapping is resolved.
        /// </summary>
        public bool IsMappingResolved(string key)
        {
            var mapping = databaseMappings.Find(m => m.key == key);
            return mapping != null && mapping.IsResolved;
        }
        
        /// <summary>
        /// Check if all database mappings are resolved.
        /// </summary>
        public bool AreAllMappingsResolved()
        {
            foreach (var mapping in databaseMappings)
            {
                if (!mapping.IsResolved) return false;
            }
            return true;
        }
    }
}
