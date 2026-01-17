using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unition
{
    /// <summary>
    /// HTTP client for Notion API requests.
    /// </summary>
    public class NotionClient
    {
        private const string BASE_URL = "https://api.notion.com/v1";
        private const string NOTION_VERSION = "2022-06-28";
        
        private readonly string apiKey;
        
        // Simple cache
        private Dictionary<string, (string data, float expiry)> cache = new Dictionary<string, (string, float)>();
        private float cacheDuration;

        public NotionClient(string apiKey, float cacheDuration = 300f)
        {
            this.apiKey = apiKey;
            this.cacheDuration = cacheDuration;
        }

        /// <summary>
        /// Query a database and return all results.
        /// </summary>
        public async Task<string> QueryDatabase(string databaseId, string filter = null)
        {
            string cacheKey = $"db_{databaseId}_{filter?.GetHashCode()}";
            
            // Check cache
            if (cache.TryGetValue(cacheKey, out var cached) && Time.time < cached.expiry)
            {
                return cached.data;
            }

            string url = $"{BASE_URL}/databases/{databaseId}/query";
            string body = filter ?? "{}";
            
            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            
            SetHeaders(request);
            
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Notion API Error: {request.error}\n{request.downloadHandler.text}");
                request.Dispose();
                return null;
            }
            
            string result = request.downloadHandler.text;
            request.Dispose();
            
            // Cache result
            if (cacheDuration > 0)
            {
                cache[cacheKey] = (result, Time.time + cacheDuration);
            }
            
            return result;
        }

        /// <summary>
        /// Get a single page by ID.
        /// </summary>
        public async Task<string> GetPage(string pageId)
        {
            string url = $"{BASE_URL}/pages/{pageId}";
            
            var request = UnityWebRequest.Get(url);
            SetHeaders(request);
            
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Notion API Error: {request.error}");
                request.Dispose();
                return null;
            }
            
            string result = request.downloadHandler.text;
            request.Dispose();
            return result;
        }

        /// <summary>
        /// Download image from URL.
        /// </summary>
        public async Task<Texture2D> DownloadImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;
            
            var request = UnityWebRequestTexture.GetTexture(imageUrl);
            
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to download image from {imageUrl}: {request.error}");
                request.Dispose();
                return null;
            }
            
            var texture = DownloadHandlerTexture.GetContent(request);
            request.Dispose();
            return texture;
        }

        /// <summary>
        /// Search for databases accessible by this integration.
        /// Works synchronously for Editor use.
        /// </summary>
        public string SearchDatabasesSync()
        {
            string url = $"{BASE_URL}/search";
            string body = "{\"filter\":{\"property\":\"object\",\"value\":\"database\"}}";
            
            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            
            SetHeaders(request);
            
            var operation = request.SendWebRequest();
            
            // Busy wait for Editor (not ideal but works for small requests)
            while (!operation.isDone)
            {
                System.Threading.Thread.Sleep(10);
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Notion Search Error: {request.error}\n{request.downloadHandler.text}");
                request.Dispose();
                return null;
            }
            
            string result = request.downloadHandler.text;
            request.Dispose();
            return result;
        }

        /// <summary>
        /// Search for databases accessible by this integration (async version).
        /// </summary>
        public async Task<string> SearchDatabasesAsync()
        {
            string cacheKey = "search_databases";
            
            // Check cache
            if (cache.TryGetValue(cacheKey, out var cached) && Time.time < cached.expiry)
            {
                return cached.data;
            }

            string url = $"{BASE_URL}/search";
            string body = "{\"filter\":{\"property\":\"object\",\"value\":\"database\"}}";
            
            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            
            SetHeaders(request);
            
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Notion Search Error: {request.error}\n{request.downloadHandler.text}");
                request.Dispose();
                return null;
            }
            
            string result = request.downloadHandler.text;
            request.Dispose();
            
            // Cache result
            if (cacheDuration > 0)
            {
                cache[cacheKey] = (result, Time.time + cacheDuration);
            }
            
            return result;
        }

        /// <summary>
        /// Find a database ID by its name (title).
        /// Returns null if not found.
        /// </summary>
        /// <param name="databaseName">The name of the database to find (case-insensitive partial match).</param>
        public async Task<string> FindDatabaseIdByName(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                Debug.LogWarning("[Unition] FindDatabaseIdByName: databaseName is null or empty.");
                return null;
            }

            string json = await SearchDatabasesAsync();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var databases = NotionSearchParser.ParseDatabases(json);
            string searchName = databaseName.ToLower();

            // First, try exact match
            foreach (var db in databases)
            {
                if (db.title.ToLower() == searchName)
                {
                    return db.id;
                }
            }

            // Then, try contains match
            foreach (var db in databases)
            {
                if (db.title.ToLower().Contains(searchName))
                {
                    return db.id;
                }
            }

            Debug.LogWarning($"[Unition] Database not found: '{databaseName}'");
            return null;
        }

        /// <summary>
        /// Get all accessible databases.
        /// </summary>
        public async Task<List<NotionDatabaseInfo>> GetAllDatabases()
        {
            string json = await SearchDatabasesAsync();
            if (string.IsNullOrEmpty(json))
            {
                return new List<NotionDatabaseInfo>();
            }
            return NotionSearchParser.ParseDatabases(json);
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void ClearCache()
        {
            cache.Clear();
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Notion-Version", NOTION_VERSION);
            request.SetRequestHeader("Content-Type", "application/json");
        }
    }
}
