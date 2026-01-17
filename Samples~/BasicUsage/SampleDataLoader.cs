using UnityEngine;
using System.Collections.Generic;

#if UNION_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Union.Samples
{
    /// <summary>
    /// Sample data loader demonstrating Union usage.
    /// </summary>
    public class SampleDataLoader : MonoBehaviour
    {
        [Header("Configuration")]
        public NotionConfig config;
        
        [Header("Database Settings")]
        [Tooltip("The ID of your Notion database")]
        public string databaseId;
        
        [Header("Debug")]
        public bool loadOnStart = true;
        public List<string> loadedPageNames = new List<string>();
        
        private NotionClient client;

#if UNION_UNITASK
        async void Start()
        {
            if (loadOnStart && config != null && !string.IsNullOrEmpty(databaseId))
            {
                await LoadData();
            }
        }

        public async UniTask LoadData()
        {
            if (!config.IsValid())
            {
                Debug.LogError("NotionConfig is invalid. Please set the API key.");
                return;
            }
            
            client = new NotionClient(config.apiKey, config.cacheDuration);
            loadedPageNames.Clear();
            
            Debug.Log($"Querying database: {databaseId}");
            
            string json = await client.QueryDatabase(databaseId);
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to query database");
                return;
            }
            
            // Parse the response
            foreach (var pageJson in NotionPropertyHelpers.IteratePages(json))
            {
                string pageId = NotionPropertyHelpers.ExtractPageId(pageJson);
                string name = NotionPropertyHelpers.ExtractTitleProperty(pageJson, "Name");
                
                if (string.IsNullOrEmpty(name))
                {
                    // Try common title property names
                    name = NotionPropertyHelpers.ExtractTitleProperty(pageJson, "Title");
                }
                
                if (!string.IsNullOrEmpty(name))
                {
                    loadedPageNames.Add(name);
                    Debug.Log($"Loaded page: {name} (ID: {pageId})");
                }
            }
            
            Debug.Log($"Loaded {loadedPageNames.Count} pages from Notion");
        }
#else
        async void Start()
        {
            if (loadOnStart && config != null && !string.IsNullOrEmpty(databaseId))
            {
                await LoadDataAsync();
            }
        }

        public async System.Threading.Tasks.Task LoadDataAsync()
        {
            if (!config.IsValid())
            {
                Debug.LogError("NotionConfig is invalid. Please set the API key.");
                return;
            }
            
            client = new NotionClient(config.apiKey, config.cacheDuration);
            loadedPageNames.Clear();
            
            Debug.Log($"Querying database: {databaseId}");
            
            string json = await client.QueryDatabase(databaseId);
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to query database");
                return;
            }
            
            // Parse the response
            foreach (var pageJson in NotionPropertyHelpers.IteratePages(json))
            {
                string pageId = NotionPropertyHelpers.ExtractPageId(pageJson);
                string name = NotionPropertyHelpers.ExtractTitleProperty(pageJson, "Name");
                
                if (string.IsNullOrEmpty(name))
                {
                    name = NotionPropertyHelpers.ExtractTitleProperty(pageJson, "Title");
                }
                
                if (!string.IsNullOrEmpty(name))
                {
                    loadedPageNames.Add(name);
                    Debug.Log($"Loaded page: {name} (ID: {pageId})");
                }
            }
            
            Debug.Log($"Loaded {loadedPageNames.Count} pages from Notion");
        }
#endif
    }
}
