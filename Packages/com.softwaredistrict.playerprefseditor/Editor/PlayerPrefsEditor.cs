using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif

namespace SoftwareDistrict.Framework.PlayerPrefsEditor
{
    public class PlayerPrefsEditor : EditorWindow
    {
        public enum PrefType { Int, Float, String }

        [System.Serializable]
        public class PlayerPrefEntry
        {
            public string key;
            public PrefType type;
            public string valueStr;
        }

        private List<PlayerPrefEntry> entries = new List<PlayerPrefEntry>();
        private string searchFilter = "";
        private Vector2 scrollPosition;

        // Add Key parameters
        private bool showAddSection = false;
        private string newKeyName = "";
        private PrefType newKeyType = PrefType.String;
        private string newKeyValue = "";

        private float lastUpdateTime = 0f;

        [MenuItem("Software District/PlayerPrefs Editor")]
        public static void ShowWindow()
        {
            PlayerPrefsEditor window = GetWindow<PlayerPrefsEditor>("PlayerPrefs Editor");
            window.minSize = new Vector2(480, 600);
            window.RefreshKeysList();
        }

        private void OnEnable()
        {
            RefreshKeysList();
        }

        private void Update()
        {
            // Auto-refresh every 1 second during Play Mode (runtime) to show updated values in real-time
            if (EditorApplication.isPlaying && Time.realtimeSinceStartup - lastUpdateTime > 1f)
            {
                lastUpdateTime = Time.realtimeSinceStartup;
                RefreshKeysList();
                Repaint();
            }
        }

        private void OnGUI()
        {
            // Title Header
            GUILayout.Space(12);
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            var titleStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
            
            var subtitleStyle = new GUIStyle(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            
            GUILayout.Label("PLAYERPREFS EDITOR", titleStyle);
            GUILayout.Label($"Active Project: {PlayerSettings.companyName} > {PlayerSettings.productName}", subtitleStyle);
            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            // Controls Toolbar
            DrawSectionHeader("Controls & Search");
            GUILayout.BeginVertical("box");
            
            // Search Bar
            GUILayout.BeginHorizontal();
            GUILayout.Label("🔍 Search:", GUILayout.Width(60));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                searchFilter = "";
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(8);

            // Operation Buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Refresh List", GUILayout.Height(25)))
            {
                RefreshKeysList();
            }
            if (GUILayout.Button("➕ Add New Key", GUILayout.Height(25)))
            {
                showAddSection = !showAddSection;
            }
            GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑️ Clear All Prefs", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Clear All PlayerPrefs", "Are you sure you want to delete EVERY PlayerPref key for this project? This action cannot be undone.", "Yes", "Cancel"))
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    RefreshKeysList();
                    Debug.Log("All PlayerPrefs cleared successfully.");
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(10);

            // Add Key Section
            if (showAddSection)
            {
                DrawSectionHeader("Add New PlayerPref Key");
                GUILayout.BeginVertical("box");
                GUILayout.Space(5);
                newKeyName = EditorGUILayout.TextField("Key Name:", newKeyName);
                newKeyType = (PrefType)EditorGUILayout.EnumPopup("Value Type:", newKeyType);
                newKeyValue = EditorGUILayout.TextField("Initial Value:", newKeyValue);
                
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create Key", GUILayout.Width(100), GUILayout.Height(25)))
                {
                    CreateNewKey();
                }
                if (GUILayout.Button("Cancel", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    showAddSection = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            // Entries List
            DrawSectionHeader($"Stored Keys ({entries.Count})");
            
#if !UNITY_EDITOR_WIN
            EditorGUILayout.HelpBox("Registry scanning is only supported on Windows. On macOS/Linux, keys cannot be auto-fetched, but you can still add, view, and modify manually created keys.", MessageType.Warning);
#endif

            if (entries.Count == 0)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("No keys found. Add a key or run your game to save PlayerPrefs.", EditorStyles.wordWrappedLabel);
                GUILayout.EndVertical();
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (!string.IsNullOrEmpty(searchFilter) && !entry.key.ToLower().Contains(searchFilter.ToLower()))
                    {
                        continue;
                    }

                    GUILayout.BeginVertical("box");
                    GUILayout.Space(3);

                    // Top Row: Key and Delete button
                    GUILayout.BeginHorizontal();
                    
                    bool exists = PlayerPrefs.HasKey(entry.key);
                    string displayKeyName = entry.key;
                    if (!exists)
                    {
                        displayKeyName += " <color=#888888>[Not Saved]</color>";
                    }
                    
                    var keyStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true };
                    GUILayout.Label(displayKeyName, keyStyle, GUILayout.ExpandWidth(true));
                    
                    if (!exists)
                    {
                        GUI.backgroundColor = new Color(0.2f, 0.7f, 0.2f);
                        if (GUILayout.Button("Initialize", GUILayout.Width(75), GUILayout.Height(18)))
                        {
                            if (entry.type == PrefType.Int)
                            {
                                int.TryParse(entry.valueStr, out int val);
                                PlayerPrefs.SetInt(entry.key, val);
                            }
                            else if (entry.type == PrefType.Float)
                            {
                                float.TryParse(entry.valueStr, out float val);
                                PlayerPrefs.SetFloat(entry.key, val);
                            }
                            else
                            {
                                PlayerPrefs.SetString(entry.key, entry.valueStr);
                            }
                            PlayerPrefs.Save();
                            RefreshKeysList();
                            GUIUtility.ExitGUI();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    
                    GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                    if (GUILayout.Button("❌", GUILayout.Width(25), GUILayout.Height(18)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Key", $"Are you sure you want to delete PlayerPref key '{entry.key}'?", "Yes", "Cancel"))
                        {
                            PlayerPrefs.DeleteKey(entry.key);
                            PlayerPrefs.Save();
                            RefreshKeysList();
                            GUIUtility.ExitGUI();
                        }
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();

                    // Bottom Row: Type and Value Editor
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Type:", GUILayout.Width(40));
                    var oldType = entry.type;
                    entry.type = (PrefType)EditorGUILayout.EnumPopup(entry.type, GUILayout.Width(70));
                    
                    if (entry.type != oldType)
                    {
                        // Save type change immediately by writing back default value
                        SaveTypeChanged(entry);
                    }

                    GUILayout.Label("Value:", GUILayout.Width(45));
                    
                    if (entry.type == PrefType.Int)
                    {
                        int.TryParse(entry.valueStr, out int intVal);
                        int newIntVal = EditorGUILayout.IntField(intVal);
                        if (newIntVal != intVal)
                        {
                            entry.valueStr = newIntVal.ToString();
                            PlayerPrefs.SetInt(entry.key, newIntVal);
                            PlayerPrefs.Save();
                        }
                    }
                    else if (entry.type == PrefType.Float)
                    {
                        float.TryParse(entry.valueStr, out float floatVal);
                        float newFloatVal = EditorGUILayout.FloatField(floatVal);
                        if (!Mathf.Approximately(newFloatVal, floatVal))
                        {
                            entry.valueStr = newFloatVal.ToString();
                            PlayerPrefs.SetFloat(entry.key, newFloatVal);
                            PlayerPrefs.Save();
                        }
                    }
                    else
                    {
                        string newStrVal = EditorGUILayout.TextField(entry.valueStr);
                        if (newStrVal != entry.valueStr)
                        {
                            entry.valueStr = newStrVal;
                            PlayerPrefs.SetString(entry.key, newStrVal);
                            PlayerPrefs.Save();
                        }
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(3);
                    GUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();
            }

            GUILayout.Space(10);
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Space(5);
            GUILayout.Label(title, EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetRect(10, 2, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.5f, 0.8f, 0.8f));
            GUILayout.Space(5);
        }

        private void RefreshKeysList()
        {
            entries.Clear();
            HashSet<string> scannedKeys = new HashSet<string>();
            Dictionary<string, PrefType> keyTypes = new Dictionary<string, PrefType>();
            Dictionary<string, string> constantValues = new Dictionary<string, string>();

            // 1. Scan codebase for keys (literals and constants)
            try
            {
                string assetsPath = Application.dataPath;
                if (Directory.Exists(assetsPath))
                {
                    string[] files = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);

                    // Pass 1: Gather all string constants in files
                    foreach (string file in files)
                    {
                        if (file.Contains("PlayerPrefsEditor")) continue;
                        string content = File.ReadAllText(file);

                        string className = "";
                        var classMatch = System.Text.RegularExpressions.Regex.Match(content, @"class\s+([a-zA-Z0-9_]+)");
                        if (classMatch.Success)
                        {
                            className = classMatch.Groups[1].Value;
                        }

                        var constMatches = System.Text.RegularExpressions.Regex.Matches(content, 
                            @"(?:const|static\s+readonly)\s+string\s+([a-zA-Z0-9_]+)\s*=\s*""([^""]+)""");
                        
                        foreach (System.Text.RegularExpressions.Match match in constMatches)
                        {
                            string varName = match.Groups[1].Value;
                            string val = match.Groups[2].Value;
                            
                            constantValues[varName] = val;
                            if (!string.IsNullOrEmpty(className))
                            {
                                constantValues[$"{className}.{varName}"] = val;
                            }
                        }
                    }

                    // Pass 2: Extract all keys passed to PlayerPrefs methods
                    foreach (string file in files)
                    {
                        if (file.Contains("PlayerPrefsEditor")) continue;
                        string content = File.ReadAllText(file);

                        // String literals match
                        var literalMatches = System.Text.RegularExpressions.Regex.Matches(content, 
                            @"PlayerPrefs\s*\.\s*(GetInt|GetFloat|GetString|SetInt|SetFloat|SetString|HasKey|DeleteKey)\s*\(\s*""([^""]+)""");
                        
                        foreach (System.Text.RegularExpressions.Match match in literalMatches)
                        {
                            string method = match.Groups[1].Value;
                            string key = match.Groups[2].Value;
                            if (!string.IsNullOrEmpty(key))
                            {
                                scannedKeys.Add(key);
                                PrefType inferredType = InferTypeFromMethod(method);
                                if (inferredType != PrefType.String || !keyTypes.ContainsKey(key))
                                {
                                    keyTypes[key] = inferredType;
                                }
                            }
                        }

                        // Variable references match
                        var varMatches = System.Text.RegularExpressions.Regex.Matches(content, 
                            @"PlayerPrefs\s*\.\s*(GetInt|GetFloat|GetString|SetInt|SetFloat|SetString|HasKey|DeleteKey)\s*\(\s*([a-zA-Z0-9_\.]+)");
                        
                        foreach (System.Text.RegularExpressions.Match match in varMatches)
                        {
                            string method = match.Groups[1].Value;
                            string varName = match.Groups[2].Value;
                            
                            if (constantValues.TryGetValue(varName, out string resolvedKey))
                            {
                                if (!string.IsNullOrEmpty(resolvedKey))
                                {
                                    scannedKeys.Add(resolvedKey);
                                    PrefType inferredType = InferTypeFromMethod(method);
                                    if (inferredType != PrefType.String || !keyTypes.ContainsKey(resolvedKey))
                                    {
                                        keyTypes[resolvedKey] = inferredType;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error scanning codebase for PlayerPrefs: {ex.Message}");
            }

#if UNITY_EDITOR_WIN
            HashSet<string> keysInRegistry = new HashSet<string>();
            try
            {
                string company = PlayerSettings.companyName;
                string product = PlayerSettings.productName;
                string registryPath = $"Software\\{company}\\{product}";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        string[] valueNames = key.GetValueNames();
                        foreach (string valueName in valueNames)
                        {
                            string actualKey = valueName;
                            int hashIndex = valueName.LastIndexOf("_h");
                            if (hashIndex > 0)
                            {
                                actualKey = valueName.Substring(0, hashIndex);
                            }

                            if (string.IsNullOrEmpty(actualKey)) continue;
                            keysInRegistry.Add(actualKey);

                            var kind = key.GetValueKind(valueName);
                            PrefType type = PrefType.String;
                            object rawValue = key.GetValue(valueName);

                            if (kind == RegistryValueKind.DWord)
                            {
                                type = PrefType.Int;
                            }
                            else if (kind == RegistryValueKind.Binary)
                            {
                                byte[] bytes = rawValue as byte[];
                                if (bytes != null && bytes.Length == 4)
                                {
                                    type = PrefType.Float;
                                }
                            }

                            string valStr = "";
                            if (type == PrefType.Int)
                            {
                                valStr = PlayerPrefs.GetInt(actualKey, 0).ToString();
                            }
                            else if (type == PrefType.Float)
                            {
                                valStr = PlayerPrefs.GetFloat(actualKey, 0f).ToString();
                            }
                            else
                            {
                                valStr = PlayerPrefs.GetString(actualKey, "");
                            }

                            entries.Add(new PlayerPrefEntry {
                                key = actualKey,
                                type = type,
                                valueStr = valStr
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading PlayerPrefs from registry: {ex.Message}");
            }
#endif

            // Add scanned keys that aren't in the registry yet
            foreach (string key in scannedKeys)
            {
#if UNITY_EDITOR_WIN
                if (keysInRegistry.Contains(key)) continue;
#endif
                PrefType type = keyTypes.ContainsKey(key) ? keyTypes[key] : PrefType.String;
                string valStr = "";
                
                if (PlayerPrefs.HasKey(key))
                {
                    if (type == PrefType.Int)
                        valStr = PlayerPrefs.GetInt(key, 0).ToString();
                    else if (type == PrefType.Float)
                        valStr = PlayerPrefs.GetFloat(key, 0f).ToString();
                    else
                        valStr = PlayerPrefs.GetString(key, "");
                }
                else
                {
                    if (type == PrefType.Int)
                        valStr = "0";
                    else if (type == PrefType.Float)
                        valStr = "0";
                    else
                        valStr = "";
                }

                entries.Add(new PlayerPrefEntry {
                    key = key,
                    type = type,
                    valueStr = valStr
                });
            }
        }

        private PrefType InferTypeFromMethod(string method)
        {
            if (method.Contains("Int")) return PrefType.Int;
            if (method.Contains("Float")) return PrefType.Float;
            return PrefType.String;
        }

        private void CreateNewKey()
        {
            if (string.IsNullOrEmpty(newKeyName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a key name.", "OK");
                return;
            }

            if (PlayerPrefs.HasKey(newKeyName))
            {
                if (!EditorUtility.DisplayDialog("Overwrite Key", $"Key '{newKeyName}' already exists. Overwrite it?", "Yes", "Cancel"))
                {
                    return;
                }
            }

            if (newKeyType == PrefType.Int)
            {
                int.TryParse(newKeyValue, out int val);
                PlayerPrefs.SetInt(newKeyName, val);
            }
            else if (newKeyType == PrefType.Float)
            {
                float.TryParse(newKeyValue, out float val);
                PlayerPrefs.SetFloat(newKeyName, val);
            }
            else
            {
                PlayerPrefs.SetString(newKeyName, newKeyValue);
            }

            PlayerPrefs.Save();
            showAddSection = false;
            newKeyName = "";
            newKeyValue = "";
            RefreshKeysList();
        }

        private void SaveTypeChanged(PlayerPrefEntry entry)
        {
            if (entry.type == PrefType.Int)
            {
                int.TryParse(entry.valueStr, out int val);
                PlayerPrefs.SetInt(entry.key, val);
                entry.valueStr = val.ToString();
            }
            else if (entry.type == PrefType.Float)
            {
                float.TryParse(entry.valueStr, out float val);
                PlayerPrefs.SetFloat(entry.key, val);
                entry.valueStr = val.ToString();
            }
            else
            {
                PlayerPrefs.SetString(entry.key, entry.valueStr);
            }
            PlayerPrefs.Save();
        }
    }
}
