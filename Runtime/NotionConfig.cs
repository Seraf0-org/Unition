using UnityEngine;

namespace Union
{
    /// <summary>
    /// Configuration for Notion API connection.
    /// Create via Assets > Create > Union > Notion Config
    /// </summary>
    [CreateAssetMenu(fileName = "NotionConfig", menuName = "Union/Notion Config")]
    public class NotionConfig : ScriptableObject
    {
        [Header("API Settings")]
        [Tooltip("Notion Integration Token (starts with 'ntn_' or 'secret_')")]
        public string apiKey;
        
        [Header("Cache Settings")]
        [Tooltip("Cache duration in seconds (0 = no cache)")]
        public float cacheDuration = 300f;
        
        /// <summary>
        /// Check if the configuration is valid.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(apiKey);
        }
    }
}
