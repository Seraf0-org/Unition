using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Unition.Editor
{
    /// <summary>
    /// Unition main menu and editor window.
    /// </summary>
    public class UnitionWindow : EditorWindow
    {
        private string apiKey = "";
        private bool showApiKey = false;
        private List<NotionDatabaseInfo> fetchedDatabases = null;
        private bool isFetching = false;
        private string fetchError = null;
        private Vector2 scrollPos;

        [MenuItem("Unition/Database Browser")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnitionWindow>("Unition");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        [MenuItem("Unition/Open Notion Integrations")]
        public static void OpenNotionIntegrations()
        {
            Application.OpenURL("https://www.notion.so/my-integrations");
        }

        [MenuItem("Unition/Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/Seraf0-org/Unition");
        }

        [MenuItem("Unition/Create Notion Config")]
        public static void CreateNotionConfig()
        {
            var config = ScriptableObject.CreateInstance<NotionConfig>();
            
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Notion Config",
                "NotionConfig",
                "asset",
                "Choose where to save the Notion Config asset"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
                
                Debug.Log($"[Unition] Created NotionConfig at: {path}");
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // Header
            GUILayout.Label("Unition - Notion Unity Bridge", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Connect to Notion and browse your databases.", MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            // API Key input
            EditorGUILayout.LabelField("API Key", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (showApiKey)
            {
                apiKey = EditorGUILayout.TextField(apiKey);
            }
            else
            {
                apiKey = EditorGUILayout.PasswordField(apiKey);
            }
            
            if (GUILayout.Button(showApiKey ? "Hide" : "Show", GUILayout.Width(50)))
            {
                showApiKey = !showApiKey;
            }
            EditorGUILayout.EndHorizontal();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                EditorGUILayout.HelpBox(
                    "Enter your Notion API key to connect.\nGet one from: notion.so/my-integrations",
                    MessageType.Warning
                );
            }
            
            EditorGUILayout.Space(10);
            
            // Fetch button
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(apiKey) || isFetching);
            if (GUILayout.Button(isFetching ? "Fetching..." : "🔄 Fetch Databases", GUILayout.Height(30)))
            {
                FetchDatabases();
            }
            EditorGUI.EndDisabledGroup();
            
            if (!string.IsNullOrEmpty(fetchError))
            {
                EditorGUILayout.HelpBox(fetchError, MessageType.Error);
            }
            
            EditorGUILayout.Space(10);
            
            // Database list
            if (fetchedDatabases != null && fetchedDatabases.Count > 0)
            {
                EditorGUILayout.LabelField($"Found {fetchedDatabases.Count} Databases", EditorStyles.boldLabel);
                
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                
                foreach (var db in fetchedDatabases)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(db.title, EditorStyles.boldLabel, GUILayout.Width(200));
                    EditorGUILayout.SelectableLabel(db.id, GUILayout.Height(18));
                    
                    if (GUILayout.Button("Copy ID", GUILayout.Width(70)))
                    {
                        EditorGUIUtility.systemCopyBuffer = db.id;
                        Debug.Log($"Copied: {db.id}");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            else if (fetchedDatabases != null)
            {
                EditorGUILayout.HelpBox("No databases found. Make sure your integration has access to databases in Notion.", MessageType.Info);
            }
            
            GUILayout.FlexibleSpace();
            
            // Footer links
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Notion Integrations"))
            {
                Application.OpenURL("https://www.notion.so/my-integrations");
            }
            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL("https://github.com/Seraf0-org/Unition");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }

        private void FetchDatabases()
        {
            isFetching = true;
            fetchError = null;
            fetchedDatabases = null;
            
            try
            {
                var client = new NotionClient(apiKey, 0);
                string json = client.SearchDatabasesSync();
                
                if (string.IsNullOrEmpty(json))
                {
                    fetchError = "Failed to fetch databases. Check console for details.";
                }
                else
                {
                    fetchedDatabases = NotionSearchParser.ParseDatabases(json);
                    
                    if (fetchedDatabases.Count == 0)
                    {
                        fetchError = "No databases found.";
                    }
                    else
                    {
                        Debug.Log($"[Unition] Fetched {fetchedDatabases.Count} databases");
                    }
                }
            }
            catch (System.Exception e)
            {
                fetchError = $"Error: {e.Message}";
                Debug.LogException(e);
            }
            finally
            {
                isFetching = false;
                Repaint();
            }
        }
    }

    /// <summary>
    /// Custom editor for NotionConfig ScriptableObject.
    /// </summary>
    [CustomEditor(typeof(NotionConfig))]
    public class NotionConfigEditor : UnityEditor.Editor
    {
        private bool showApiKey = false;
        private List<NotionDatabaseInfo> fetchedDatabases = null;
        private bool isFetching = false;
        private string fetchError = null;
        private Vector2 scrollPos;

        public override void OnInspectorGUI()
        {
            var config = (NotionConfig)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unition - Notion Config", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // API Key
            EditorGUILayout.LabelField("API Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("API Key", GUILayout.Width(EditorGUIUtility.labelWidth));
            
            if (showApiKey)
            {
                config.apiKey = EditorGUILayout.TextField(config.apiKey);
            }
            else
            {
                config.apiKey = EditorGUILayout.PasswordField(config.apiKey);
            }
            
            if (GUILayout.Button(showApiKey ? "Hide" : "Show", GUILayout.Width(50)))
            {
                showApiKey = !showApiKey;
            }
            EditorGUILayout.EndHorizontal();
            
            if (string.IsNullOrEmpty(config.apiKey))
            {
                EditorGUILayout.HelpBox(
                    "API Key is required.\nGet one from: notion.so/my-integrations",
                    MessageType.Warning
                );
            }
            
            // Cache Duration
            config.cacheDuration = EditorGUILayout.FloatField("Cache Duration (s)", config.cacheDuration);
            if (config.cacheDuration < 0) config.cacheDuration = 0;
            
            EditorGUILayout.Space();
            
            // Fetch Databases Button
            EditorGUILayout.LabelField("Database Browser", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(config.apiKey) || isFetching);
            if (GUILayout.Button(isFetching ? "Fetching..." : "Fetch Databases"))
            {
                FetchDatabases(config.apiKey);
            }
            EditorGUI.EndDisabledGroup();
            
            if (!string.IsNullOrEmpty(fetchError))
            {
                EditorGUILayout.HelpBox(fetchError, MessageType.Error);
            }
            
            // Database List
            if (fetchedDatabases != null && fetchedDatabases.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Found {fetchedDatabases.Count} Databases", EditorStyles.miniLabel);
                
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(150));
                
                foreach (var db in fetchedDatabases)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(db.title, GUILayout.Width(150));
                    EditorGUILayout.SelectableLabel(db.id, GUILayout.Height(16));
                    
                    if (GUILayout.Button("Copy", GUILayout.Width(50)))
                    {
                        EditorGUIUtility.systemCopyBuffer = db.id;
                        Debug.Log($"Copied: {db.id}");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.Space();
            
            // Validation
            if (config.IsValid())
            {
                EditorGUILayout.HelpBox("Configuration is valid.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("API Key is required.", MessageType.Warning);
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
            }
        }
        
        private void FetchDatabases(string apiKey)
        {
            isFetching = true;
            fetchError = null;
            fetchedDatabases = null;
            
            try
            {
                var client = new NotionClient(apiKey, 0);
                string json = client.SearchDatabasesSync();
                
                if (string.IsNullOrEmpty(json))
                {
                    fetchError = "Failed to fetch databases.";
                }
                else
                {
                    fetchedDatabases = NotionSearchParser.ParseDatabases(json);
                    
                    if (fetchedDatabases.Count == 0)
                    {
                        fetchError = "No databases found.";
                    }
                    else
                    {
                        Debug.Log($"[Unition] Fetched {fetchedDatabases.Count} databases");
                    }
                }
            }
            catch (System.Exception e)
            {
                fetchError = $"Error: {e.Message}";
                Debug.LogException(e);
            }
            finally
            {
                isFetching = false;
            }
        }
    }
}
