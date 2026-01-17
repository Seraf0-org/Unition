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
}
