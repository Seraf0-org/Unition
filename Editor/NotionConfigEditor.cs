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
        private List<NotionPageInfo> fetchedPages = null;
        private bool isFetching = false;
        private string fetchError = null;
        private Vector2 dbScrollPos;
        private Vector2 pageScrollPos;
        private bool showDatabases = true;
        private bool showPages = true;

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
                
                if (GUILayout.Button("Open Notion Integrations"))
                {
                    Application.OpenURL("https://www.notion.so/my-integrations");
                }
            }
            
            // Cache Duration
            config.cacheDuration = EditorGUILayout.FloatField("Cache Duration (s)", config.cacheDuration);
            if (config.cacheDuration < 0) config.cacheDuration = 0;
            
            EditorGUILayout.Space();
            
            // Fetch Button
            EditorGUILayout.LabelField("Content Browser", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(config.apiKey) || isFetching);
            if (GUILayout.Button(isFetching ? "Fetching..." : "Fetch Pages"))
            {
                FetchAll(config.apiKey);
            }
            EditorGUI.EndDisabledGroup();
            
            if (!string.IsNullOrEmpty(fetchError))
            {
                EditorGUILayout.HelpBox(fetchError, MessageType.Error);
            }
            
            // Databases Section
            if (fetchedDatabases != null && fetchedDatabases.Count > 0)
            {
                EditorGUILayout.Space();
                showDatabases = EditorGUILayout.Foldout(showDatabases, $"Databases ({fetchedDatabases.Count})", true);
                
                if (showDatabases)
                {
                    dbScrollPos = EditorGUILayout.BeginScrollView(dbScrollPos, GUILayout.MaxHeight(120));
                    
                    foreach (var db in fetchedDatabases)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField("📊 " + db.title, GUILayout.Width(180));
                        EditorGUILayout.SelectableLabel(db.id, GUILayout.Height(16));
                        
                        if (GUILayout.Button("Copy", GUILayout.Width(45)))
                        {
                            EditorGUIUtility.systemCopyBuffer = db.id;
                            Debug.Log($"Copied: {db.id}");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
            }
            
            // Pages Section
            if (fetchedPages != null && fetchedPages.Count > 0)
            {
                EditorGUILayout.Space();
                showPages = EditorGUILayout.Foldout(showPages, $"Pages ({fetchedPages.Count})", true);
                
                if (showPages)
                {
                    pageScrollPos = EditorGUILayout.BeginScrollView(pageScrollPos, GUILayout.MaxHeight(120));
                    
                    foreach (var page in fetchedPages)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField("📄 " + page.title, GUILayout.Width(180));
                        EditorGUILayout.SelectableLabel(page.id, GUILayout.Height(16));
                        
                        if (GUILayout.Button("Copy", GUILayout.Width(45)))
                        {
                            EditorGUIUtility.systemCopyBuffer = page.id;
                            Debug.Log($"Copied: {page.id}");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
            }
            
            EditorGUILayout.Space();
            
            // Database Mappings Section
            EditorGUILayout.LabelField("Database Mappings", EditorStyles.boldLabel);
            
            if (config.databaseMappings.Count == 0)
            {
                EditorGUILayout.HelpBox("No database mappings defined. Add mappings to resolve database names to IDs at runtime.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < config.databaseMappings.Count; i++)
                {
                    var mapping = config.databaseMappings[i];
                    
                    EditorGUILayout.BeginHorizontal("box");
                    
                    // Key
                    EditorGUILayout.LabelField("Key:", GUILayout.Width(30));
                    mapping.key = EditorGUILayout.TextField(mapping.key, GUILayout.Width(80));
                    
                    // Database Name
                    EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
                    mapping.databaseName = EditorGUILayout.TextField(mapping.databaseName, GUILayout.Width(120));
                    
                    // Status indicator
                    if (mapping.IsResolved)
                    {
                        EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("○", GUILayout.Width(20));
                    }
                    
                    // Remove button
                    if (GUILayout.Button("×", GUILayout.Width(25)))
                    {
                        config.databaseMappings.RemoveAt(i);
                        EditorUtility.SetDirty(config);
                        break;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            // Add mapping button
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Mapping"))
            {
                config.databaseMappings.Add(new DatabaseMapping { key = "new_key", databaseName = "" });
                EditorUtility.SetDirty(config);
            }
            
            // Quick add from fetched databases
            if (fetchedDatabases != null && fetchedDatabases.Count > 0)
            {
                if (GUILayout.Button("+ Add from List..."))
                {
                    var menu = new GenericMenu();
                    foreach (var db in fetchedDatabases)
                    {
                        string dbTitle = db.title;
                        string suggestedKey = db.title.ToLower().Replace(" ", "_");
                        menu.AddItem(new GUIContent(dbTitle), false, () => {
                            config.databaseMappings.Add(new DatabaseMapping { 
                                key = suggestedKey, 
                                databaseName = dbTitle 
                            });
                            EditorUtility.SetDirty(config);
                        });
                    }
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();
            
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
        
        private void FetchAll(string apiKey)
        {
            isFetching = true;
            fetchError = null;
            fetchedDatabases = null;
            fetchedPages = null;
            
            try
            {
                var client = new NotionClient(apiKey, 0);
                
                // Fetch databases
                string dbJson = client.SearchDatabasesSync();
                if (!string.IsNullOrEmpty(dbJson))
                {
                    fetchedDatabases = NotionSearchParser.ParseDatabases(dbJson);
                }
                
                // Fetch pages (sync version needed)
                string pageJson = SearchPagesSync(client, apiKey);
                if (!string.IsNullOrEmpty(pageJson))
                {
                    fetchedPages = NotionSearchParser.ParsePages(pageJson);
                }
                
                int totalCount = (fetchedDatabases?.Count ?? 0) + (fetchedPages?.Count ?? 0);
                if (totalCount == 0)
                {
                    fetchError = "No content found.";
                }
                else
                {
                    Debug.Log($"[Unition] Fetched {fetchedDatabases?.Count ?? 0} databases, {fetchedPages?.Count ?? 0} pages");
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
        
        private string SearchPagesSync(NotionClient client, string apiKey)
        {
            string url = "https://api.notion.com/v1/search";
            string body = "{\"filter\":{\"property\":\"object\",\"value\":\"page\"}}";
            
            var request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Notion-Version", "2022-06-28");
            request.SetRequestHeader("Content-Type", "application/json");
            
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                System.Threading.Thread.Sleep(10);
            }
            
            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Notion Search Error: {request.error}");
                request.Dispose();
                return null;
            }
            
            string result = request.downloadHandler.text;
            request.Dispose();
            return result;
        }
    }
}
