using UnityEngine;
using UnityEditor;

namespace Unition.Editor
{
    [CustomEditor(typeof(NotionConfig))]
    public class NotionConfigEditor : UnityEditor.Editor
    {
        private bool showApiKey = false;

        public override void OnInspectorGUI()
        {
            var config = (NotionConfig)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Notion Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // API Key with show/hide toggle
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
                    "API Key is required. Get one from:\nhttps://www.notion.so/my-integrations",
                    MessageType.Warning
                );
            }
            else if (!config.apiKey.StartsWith("ntn_") && !config.apiKey.StartsWith("secret_"))
            {
                EditorGUILayout.HelpBox(
                    "API Key should start with 'ntn_' or 'secret_'",
                    MessageType.Warning
                );
            }
            
            EditorGUILayout.Space();
            
            // Cache settings
            EditorGUILayout.LabelField("Cache Settings", EditorStyles.boldLabel);
            config.cacheDuration = EditorGUILayout.FloatField(
                new GUIContent("Cache Duration (s)", "How long to cache API responses. Set to 0 to disable caching."),
                config.cacheDuration
            );
            
            if (config.cacheDuration < 0)
            {
                config.cacheDuration = 0;
            }
            
            EditorGUILayout.Space();
            
            // Validation status
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            if (config.IsValid())
            {
                EditorGUILayout.HelpBox("Configuration is valid.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Configuration is invalid. Please provide an API key.", MessageType.Error);
            }
            
            EditorGUILayout.Space();
            
            // Help link
            if (GUILayout.Button("Open Notion Integrations Page"))
            {
                Application.OpenURL("https://www.notion.so/my-integrations");
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
            }
        }
    }
}
