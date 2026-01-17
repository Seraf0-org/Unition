using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

#if UNION_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Union
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

#if UNION_UNITASK
        /// <summary>
        /// Query a database and return all results (UniTask version).
        /// </summary>
        public async UniTask<string> QueryDatabase(string databaseId, string filter = null)
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
            
            try
            {
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Notion API Error: {request.error}\n{request.downloadHandler.text}");
                    return null;
                }
                
                string result = request.downloadHandler.text;
                
                // Cache result
                if (cacheDuration > 0)
                {
                    cache[cacheKey] = (result, Time.time + cacheDuration);
                }
                
                return result;
            }
            finally
            {
                request.Dispose();
            }
        }

        /// <summary>
        /// Get a single page by ID (UniTask version).
        /// </summary>
        public async UniTask<string> GetPage(string pageId)
        {
            string url = $"{BASE_URL}/pages/{pageId}";
            
            var request = UnityWebRequest.Get(url);
            SetHeaders(request);
            
            try
            {
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Notion API Error: {request.error}");
                    return null;
                }
                
                return request.downloadHandler.text;
            }
            finally
            {
                request.Dispose();
            }
        }

        /// <summary>
        /// Download image from URL (UniTask version).
        /// </summary>
        public async UniTask<Texture2D> DownloadImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;
            
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequestTexture.GetTexture(imageUrl);
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Failed to download image from {imageUrl}: {request.error}");
                    return null;
                }
                
                return DownloadHandlerTexture.GetContent(request);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Exception downloading image from {imageUrl}: {e.Message}");
                return null;
            }
            finally
            {
                request?.Dispose();
            }
        }
#else
        /// <summary>
        /// Query a database and return all results (Coroutine/Task version).
        /// Use with StartCoroutine or await with Task.
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
        /// Get a single page by ID (Task version).
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
        /// Download image from URL (Task version).
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
#endif

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
