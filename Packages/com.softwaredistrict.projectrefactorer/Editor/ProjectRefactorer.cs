using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftwareDistrict.Framework.Refactoring
{
    public class ProjectRefactorer : EditorWindow
    {
        // Dry-Run Subsystem
        [System.Serializable]
        public struct PendingFileChange
        {
            public string filePath;
            public string changeDescription;
            public string newContent; // Null if it's a rename
            public string renameNewName; // Null if it's a text change
        }

        private bool isDryRun = true;
        private List<PendingFileChange> pendingChanges = new List<PendingFileChange>();

        // One-Click Batch Refactorer parameters
        private string globalScriptPrefix = "";
        private string globalScriptSuffix = "";
        private string globalFolderPrefix = "";
        private string globalFolderSuffix = "";
        private bool globalRenameAssets = false;

        // Individual refactor parameters (foldouts)
        private bool showIndividualRefactors = false;
        private string scriptSearchPrefix = "OldPrefix";
        private string scriptReplacePrefix = "NewPrefix";
        private bool scriptMatchExact = false;

        private string namespaceSearchName = "OldNamespace";
        private string namespaceReplaceName = "NewNamespace";

        private string functionSearchName = "OldFunctionName";
        private string functionReplaceName = "NewFunctionName";
        private bool functionRenameStrict = true;

        private string identifierSearchName = "oldVariable";
        private string identifierReplaceName = "newVariable";

        private string assetSearchPrefix = "Placeholder";
        private string assetReplacePrefix = "Final";

        // Obfuscation & Junk Code injection parameters
        private bool showObfuscation = false;
        private bool optJunkInFunctions = true;
        private bool optJunkUncalledFunctions = true;
        private bool optJunkAppendToEnd = false;
        private bool optJunkInsideClassFields = false;
        private bool optJunkGenerateFiles = false;
        private int junkFileCount = 10;

        private const string JunkStartMarker = "// <RefactorerJunkCode_Start>";
        private const string JunkEndMarker = "// <RefactorerJunkCode_End>";
        private const string ClassJunkStartMarker = "// <RefactorerClassJunk_Start>";
        private const string ClassJunkEndMarker = "// <RefactorerClassJunk_End>";

        // Scroll positions
        private Vector2 scrollPosition;
        private Vector2 dryRunScrollPosition;

        [MenuItem("Tools/Project Refactorer")]
        public static void ShowWindow()
        {
            ProjectRefactorer window = GetWindow<ProjectRefactorer>("Project Refactorer");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            // Title Header
            GUILayout.Space(10);
            GUILayout.Label("Project-Wide Mass Refactorer", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter });
            GUILayout.Label("Safely refactor assets, code identifiers, namespaces, and inject signatures.", EditorStyles.miniLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // --- Section 1: Dry Run Settings ---
            DrawSectionHeader("1. Execution Settings");
            isDryRun = EditorGUILayout.Toggle(new GUIContent("Dry Run Mode", "If enabled, changes are calculated and displayed below, but NOT written to disk until you click Apply."), isDryRun);
            
            if (pendingChanges.Count > 0)
            {
                EditorGUILayout.HelpBox($"{pendingChanges.Count} changes are currently planned and pending.", MessageType.Warning);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Execute Planned Changes", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Apply Changes", $"Are you sure you want to write {pendingChanges.Count} planned changes to disk?", "Yes", "Cancel"))
                    {
                        ApplyPendingDryRunChanges();
                    }
                }
                if (GUILayout.Button("Clear Planned Changes", GUILayout.Height(30)))
                {
                    pendingChanges.Clear();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No pending changes. Select a tool below to run refactoring.", MessageType.Info);
            }
            GUILayout.Space(15);

            // --- Section 2: One-Click Batch Refactorer (Prefix/Suffix) ---
            DrawSectionHeader("2. One-Click Batch Refactorer (Prefix/Postfix)");
            EditorGUILayout.LabelField("Add prefix or postfix globally to all scripts/classes, folders, and assets.", EditorStyles.miniLabel);
            globalScriptPrefix = EditorGUILayout.TextField("Script Prefix:", globalScriptPrefix);
            globalScriptSuffix = EditorGUILayout.TextField("Script Suffix:", globalScriptSuffix);
            globalFolderPrefix = EditorGUILayout.TextField("Folder Prefix:", globalFolderPrefix);
            globalFolderSuffix = EditorGUILayout.TextField("Folder Suffix:", globalFolderSuffix);
            globalRenameAssets = EditorGUILayout.Toggle(new GUIContent("Batch Rename Assets", "Include game assets (art, music, prefabs, materials, etc.) in renaming. (Uses GUID-safe renaming)"), globalRenameAssets);

            GUILayout.Space(5);
            if (GUILayout.Button("Run Global Batch Refactor", GUILayout.Height(30)))
            {
                string msg = isDryRun ? "This will simulate a global rename on all selected elements. Proceed?" : "WARNING: This will rename script classes, files, directories, and assets. Proceed?";
                if (EditorUtility.DisplayDialog("Global Batch Refactor", msg, "Yes", "Cancel"))
                {
                    RunGlobalBatchRefactor();
                }
            }
            GUILayout.Space(15);

            // --- Section 3: Individual Refactors (Foldout) ---
            showIndividualRefactors = EditorGUILayout.BeginFoldoutHeaderGroup(showIndividualRefactors, "3. Individual Refactor Tools");
            if (showIndividualRefactors)
            {
                GUILayout.Space(5);
                // Script Renaming
                GUILayout.Label("Script & Class Renaming (With Cross-Script References)", EditorStyles.miniBoldLabel);
                scriptSearchPrefix = EditorGUILayout.TextField("Search Prefix:", scriptSearchPrefix);
                scriptReplacePrefix = EditorGUILayout.TextField("Replace Prefix:", scriptReplacePrefix);
                scriptMatchExact = EditorGUILayout.Toggle("Match Prefix Exactly", scriptMatchExact);
                if (GUILayout.Button("Refactor Scripts", GUILayout.Height(25)))
                {
                    ExecuteClassRefactor(scriptSearchPrefix, scriptReplacePrefix, scriptMatchExact);
                }
                GUILayout.Space(10);

                // Namespace Renaming
                GUILayout.Label("Namespace Renaming", EditorStyles.miniBoldLabel);
                namespaceSearchName = EditorGUILayout.TextField("Old Namespace:", namespaceSearchName);
                namespaceReplaceName = EditorGUILayout.TextField("New Namespace:", namespaceReplaceName);
                if (GUILayout.Button("Refactor Namespaces", GUILayout.Height(25)))
                {
                    ExecuteNamespaceRefactor(namespaceSearchName, namespaceReplaceName);
                }
                GUILayout.Space(10);

                // Function Renaming
                GUILayout.Label("Function & Method Renaming (Updates Prefab/Scene Listeners)", EditorStyles.miniBoldLabel);
                functionSearchName = EditorGUILayout.TextField("Old Function:", functionSearchName);
                functionReplaceName = EditorGUILayout.TextField("New Function:", functionReplaceName);
                functionRenameStrict = EditorGUILayout.Toggle("Strict Match", functionRenameStrict);
                if (GUILayout.Button("Refactor Functions", GUILayout.Height(25)))
                {
                    ExecuteFunctionRefactor(functionSearchName, functionReplaceName, functionRenameStrict);
                }
                GUILayout.Space(10);

                // Variable Renaming
                GUILayout.Label("Variable Renaming (Includes [FormerlySerializedAs])", EditorStyles.miniBoldLabel);
                identifierSearchName = EditorGUILayout.TextField("Old Variable:", identifierSearchName);
                identifierReplaceName = EditorGUILayout.TextField("New Variable:", identifierReplaceName);
                if (GUILayout.Button("Refactor Variables", GUILayout.Height(25)))
                {
                    ExecuteVariableRefactor(identifierSearchName, identifierReplaceName);
                }
                GUILayout.Space(10);

                // Asset Renaming
                GUILayout.Label("Asset & Folder Renaming", EditorStyles.miniBoldLabel);
                assetSearchPrefix = EditorGUILayout.TextField("Asset Search Prefix:", assetSearchPrefix);
                assetReplacePrefix = EditorGUILayout.TextField("Asset Replace Prefix:", assetReplacePrefix);
                if (GUILayout.Button("Refactor Assets", GUILayout.Height(25)))
                {
                    ExecuteFolderRefactor(assetSearchPrefix, assetReplacePrefix);
                }
                GUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(15);

            // --- Section 4: Obfuscation & Junk Code ---
            showObfuscation = EditorGUILayout.BeginFoldoutHeaderGroup(showObfuscation, "4. Obfuscation & Junk Code Injection");
            if (showObfuscation)
            {
                GUILayout.Label("Injection Options:", EditorStyles.miniBoldLabel);
                optJunkInFunctions = EditorGUILayout.Toggle("Inject inside methods (Start/End)", optJunkInFunctions);
                optJunkUncalledFunctions = EditorGUILayout.Toggle("Inject small uncalled methods", optJunkUncalledFunctions);
                optJunkAppendToEnd = EditorGUILayout.Toggle("Append class to end of file", optJunkAppendToEnd);
                optJunkInsideClassFields = EditorGUILayout.Toggle("Inject class body fields", optJunkInsideClassFields);
                optJunkGenerateFiles = EditorGUILayout.Toggle("Generate separate junk scripts", optJunkGenerateFiles);

                if (optJunkGenerateFiles)
                {
                    junkFileCount = EditorGUILayout.IntSlider("Files to Generate:", junkFileCount, 1, 100);
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Inject Junk Code", GUILayout.Height(25)))
                {
                    InjectJunkCodeToAllScripts();
                }
                if (GUILayout.Button("Remove Junk Code", GUILayout.Height(25)))
                {
                    RemoveJunkCodeFromAllScripts();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(15);

            // --- Section 5: Dry Run Logs (Visible if changes are recorded) ---
            if (isDryRun && pendingChanges.Count > 0)
            {
                DrawSectionHeader("Planned Dry-Run Modifications");
                dryRunScrollPosition = EditorGUILayout.BeginScrollView(dryRunScrollPosition, GUILayout.Height(200));
                foreach (var change in pendingChanges)
                {
                    if (change.newContent != null)
                    {
                        GUILayout.Label($"[EDIT] {Path.GetFileName(change.filePath)} - {change.changeDescription}", EditorStyles.miniLabel);
                    }
                    else if (change.renameNewName != null)
                    {
                        GUILayout.Label($"[RENAME] {Path.GetFileName(change.filePath)} -> {change.renameNewName} - {change.changeDescription}", EditorStyles.miniLabel);
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetRect(10, 2, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
            GUILayout.Space(5);
        }

        // --- Helper for Dry Run ---
        private void RecordOrApplyFileChange(string filePath, string description, string newContent)
        {
            if (isDryRun)
            {
                pendingChanges.Add(new PendingFileChange { filePath = filePath, changeDescription = description, newContent = newContent });
                Debug.Log($"[Dry Run Log] Planned edit in {Path.GetFileName(filePath)}: {description}");
            }
            else
            {
                File.WriteAllText(filePath, newContent);
            }
        }

        private bool RecordOrApplyRename(string assetPath, string description, string newName)
        {
            if (isDryRun)
            {
                pendingChanges.Add(new PendingFileChange { filePath = assetPath, changeDescription = description, renameNewName = newName });
                Debug.Log($"[Dry Run Log] Planned rename: '{Path.GetFileName(assetPath)}' -> '{newName}': {description}");
                return true;
            }
            else
            {
                string error = AssetDatabase.RenameAsset(assetPath, newName);
                if (string.IsNullOrEmpty(error))
                {
                    return true;
                }
                else
                {
                    Debug.LogError($"Error renaming asset '{assetPath}' to '{newName}': {error}");
                    return false;
                }
            }
        }

        private void ApplyPendingDryRunChanges()
        {
            if (pendingChanges.Count == 0) return;

            AssetDatabase.StartAssetEditing();
            try
            {
                // Write all text replacements first to avoid path invalidation during renames
                foreach (var change in pendingChanges)
                {
                    if (change.newContent != null && File.Exists(change.filePath))
                    {
                        File.WriteAllText(change.filePath, change.newContent);
                    }
                }

                // Perform file/asset renaming
                foreach (var change in pendingChanges)
                {
                    if (change.renameNewName != null)
                    {
                        string error = AssetDatabase.RenameAsset(change.filePath, change.renameNewName);
                        if (!string.IsNullOrEmpty(error))
                        {
                            Debug.LogError($"Error applying rename of '{change.filePath}' to '{change.renameNewName}': {error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception applying planned changes: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            pendingChanges.Clear();
            AssetDatabase.Refresh();
            Debug.Log("Successfully applied all planned changes to disk.");
        }

        // --- Core Refactoring Implementations ---

        private void ExecuteClassRefactor(string search, string replace, bool exact)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(replace))
            {
                Debug.LogError("Class search/replace parameters cannot be empty.");
                return;
            }

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
            int changedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                var renames = new List<(string oldPath, string oldName, string newName)>();

                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                        continue;

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
                    bool isMatch = exact 
                        ? fileNameWithoutExtension.Equals(search) 
                        : fileNameWithoutExtension.StartsWith(search);

                    if (isMatch)
                    {
                        string newName = exact
                            ? replace
                            : replace + fileNameWithoutExtension.Substring(search.Length);

                        renames.Add((assetPath, fileNameWithoutExtension, newName));
                    }
                }

                if (renames.Count == 0) return;

                // Gather all C# scripts
                var allUserScripts = new List<string>();
                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                        continue;
                    allUserScripts.Add(Path.GetFullPath(assetPath));
                }

                // Apply text updates globally in scripts
                foreach (var rename in renames)
                {
                    string oldClassRegex = @"\b" + Regex.Escape(rename.oldName) + @"\b";

                    foreach (string scriptPath in allUserScripts)
                    {
                        if (!File.Exists(scriptPath)) continue;

                        string fileContent = File.ReadAllText(scriptPath);
                        if (Regex.IsMatch(fileContent, oldClassRegex))
                        {
                            fileContent = Regex.Replace(fileContent, oldClassRegex, rename.newName);
                            RecordOrApplyFileChange(scriptPath, $"Rename class references from '{rename.oldName}' to '{rename.newName}'", fileContent);
                        }
                    }

                    if (RecordOrApplyRename(rename.oldPath, $"Rename script file '{rename.oldName}' to '{rename.newName}'", rename.newName))
                    {
                        changedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during class refactor: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        private void ExecuteNamespaceRefactor(string search, string replace)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(replace)) return;

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
            int filesChangedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                string pattern = @"\b" + Regex.Escape(search) + @"\b";
                Regex regex = new Regex(pattern);

                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                        continue;

                    string fullPath = Path.GetFullPath(assetPath);
                    string fileContent = File.ReadAllText(fullPath);

                    if (regex.IsMatch(fileContent))
                    {
                        fileContent = regex.Replace(fileContent, replace);
                        RecordOrApplyFileChange(fullPath, $"Refactor namespace '{search}' to '{replace}'", fileContent);
                        filesChangedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during namespace refactor: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        private void ExecuteFunctionRefactor(string search, string replace, bool strict)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(replace)) return;

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
            int filesChangedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                string pattern = strict 
                    ? @"\b" + Regex.Escape(search) + @"\b(?=\s*[\(<])" 
                    : @"\b" + Regex.Escape(search) + @"\b";

                Regex regex = new Regex(pattern);

                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                        continue;

                    string fullPath = Path.GetFullPath(assetPath);
                    string fileContent = File.ReadAllText(fullPath);

                    if (regex.IsMatch(fileContent))
                    {
                        fileContent = regex.Replace(fileContent, replace);
                        RecordOrApplyFileChange(fullPath, $"Refactor function '{search}' to '{replace}'", fileContent);
                        filesChangedCount++;
                    }
                }

                // Also update method name bindings in scenes and prefabs
                UpdateYAMLEvents(search, replace);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during function refactor: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        private void ExecuteVariableRefactor(string search, string replace)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(replace)) return;

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
            int filesChangedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                string refPattern = @"\b" + Regex.Escape(search) + @"\b";
                Regex refRegex = new Regex(refPattern);

                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                        continue;

                    string fullPath = Path.GetFullPath(assetPath);
                    string fileContent = File.ReadAllText(fullPath);
                    bool isModified = false;

                    // 1. Inject [FormerlySerializedAs] on declaration line
                    string declarationPattern = @"(?<attributes>(?:\[[^\]]+\]\s*)*)(?<modifiers>\b(?:public|private|protected|internal|serialized)\s+)+(?<type>[a-zA-Z0-9_<>\[\]]+)\s+\b" + Regex.Escape(search) + @"\b\s*(?<initializer>=[^;]+)?\s*;";
                    
                    if (Regex.IsMatch(fileContent, declarationPattern))
                    {
                        fileContent = Regex.Replace(fileContent, declarationPattern, m =>
                        {
                            string attrs = m.Groups["attributes"].Value;
                            if (attrs.Contains("FormerlySerializedAs"))
                            {
                                return m.Value.Replace(search, replace);
                            }
                            
                            string newAttr = $"[UnityEngine.Serialization.FormerlySerializedAs(\"{search}\")] ";
                            return newAttr + m.Value.Replace(search, replace);
                        });
                        isModified = true;
                    }

                    // 2. Replace all remaining variable references globally in script
                    if (refRegex.IsMatch(fileContent))
                    {
                        fileContent = refRegex.Replace(fileContent, replace);
                        isModified = true;
                    }

                    if (isModified)
                    {
                        RecordOrApplyFileChange(fullPath, $"Rename variable '{search}' to '{replace}' (added FormerlySerializedAs)", fileContent);
                        filesChangedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during variable refactor: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        private void ExecuteFolderRefactor(string search, string replace)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(replace)) return;

            string[] directories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
            var sortedDirs = new List<string>(directories);
            sortedDirs.Sort((a, b) => b.Length.CompareTo(a.Length));

            int renamedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string dirPath in sortedDirs)
                {
                    string dirName = Path.GetFileName(dirPath);
                    if (dirName.Equals(search))
                    {
                        string relativePath = "Assets" + dirPath.Substring(Application.dataPath.Length).Replace('\\', '/');

                        if (relativePath.Contains("/Editor") || relativePath.Contains("/Plugins") || relativePath.Contains("/TextMesh Pro") || relativePath.Contains("/Packages"))
                            continue;

                        if (RecordOrApplyRename(relativePath, $"Rename folder '{dirName}' to '{replace}'", replace))
                        {
                            renamedCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during folder refactor: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        private void UpdateYAMLEvents(string oldFuncName, string newFuncName)
        {
            string[] yamlFiles = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);
            foreach (string file in yamlFiles)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (ext == ".unity" || ext == ".prefab")
                {
                    string content = File.ReadAllText(file);
                    string pattern = @"\bm_MethodName:\s*" + Regex.Escape(oldFuncName) + @"\b";
                    if (Regex.IsMatch(content, pattern))
                    {
                        string newContent = Regex.Replace(content, pattern, "m_MethodName: " + newFuncName);
                        RecordOrApplyFileChange(file, $"Update YAML event binding from '{oldFuncName}' to '{newFuncName}'", newContent);
                    }
                }
            }
        }

        // --- One-Click Batch Refactorer ---
        private void RunGlobalBatchRefactor()
        {
            // 1. Prefix scripts and classes
            if (!string.IsNullOrEmpty(globalScriptPrefix) || !string.IsNullOrEmpty(globalScriptSuffix))
            {
                string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (string guid in scriptGUIDs)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                            continue;

                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        string newName = globalScriptPrefix + fileName + globalScriptSuffix;

                        if (newName != fileName)
                        {
                            ExecuteClassRefactor(fileName, newName, true);
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            // 2. Prefix folders (using bottom-up path ordering)
            if (!string.IsNullOrEmpty(globalFolderPrefix) || !string.IsNullOrEmpty(globalFolderSuffix))
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
                var sortedDirs = new List<string>(directories);
                sortedDirs.Sort((a, b) => b.Length.CompareTo(a.Length));

                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (string dirPath in sortedDirs)
                    {
                        string dirName = Path.GetFileName(dirPath);
                        string relativePath = "Assets" + dirPath.Substring(Application.dataPath.Length).Replace('\\', '/');

                        if (relativePath.Contains("/Editor") || relativePath.Contains("/Plugins") || relativePath.Contains("/TextMesh Pro") || relativePath.Contains("/Packages") || relativePath.Contains("/JunkCode"))
                            continue;

                        string newDirName = globalFolderPrefix + dirName + globalFolderSuffix;
                        if (newDirName != dirName)
                        {
                            RecordOrApplyRename(relativePath, $"Batch prefix folder '{dirName}' to '{newDirName}'", newDirName);
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            // 3. Prefix Assets (art, music, prefabs, materials, models, animations)
            if (globalRenameAssets && (!string.IsNullOrEmpty(globalScriptPrefix) || !string.IsNullOrEmpty(globalScriptSuffix)))
            {
                string[] allAssetGUIDs = AssetDatabase.FindAssets("");
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (string guid in allAssetGUIDs)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (assetPath.StartsWith("Packages/") || assetPath.StartsWith("ProjectSettings/") || assetPath.StartsWith("UserSettings/")) 
                            continue;

                        string ext = Path.GetExtension(assetPath).ToLower();
                        bool isAssetToRename = ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".psd" ||
                                               ext == ".wav" || ext == ".mp3" || ext == ".ogg" ||
                                               ext == ".prefab" || ext == ".mat" || ext == ".fbx" ||
                                               ext == ".anim" || ext == ".controller";

                        if (isAssetToRename)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(assetPath);
                            string newName = globalScriptPrefix + fileName + globalScriptSuffix;

                            if (newName != fileName)
                            {
                                RecordOrApplyRename(assetPath, $"Batch prefix asset file '{fileName}' to '{newName}'", newName);
                            }
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Global Batch Refactor calculated successfully.");
        }

        // --- Obfuscation & Junk Code ---

        private void InjectJunkCodeToAllScripts()
        {
            if (optJunkGenerateFiles)
            {
                GenerateJunkFiles();
            }

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
            int injectedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                        continue;

                    string fullPath = Path.GetFullPath(assetPath);
                    string fileContent = File.ReadAllText(fullPath);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
                    bool isModified = false;

                    // A. Append class to end of file
                    if (optJunkAppendToEnd)
                    {
                        if (!fileContent.Contains(JunkStartMarker))
                        {
                            string junkCode = GenerateRandomJunkCodeBlock();
                            StringBuilder sb = new StringBuilder(fileContent);
                            sb.AppendLine();
                            sb.AppendLine(JunkStartMarker);
                            sb.Append(junkCode);
                            sb.AppendLine(JunkEndMarker);
                            fileContent = sb.ToString();
                            isModified = true;
                        }
                    }

                    // B. Inject fields inside class body
                    if (optJunkInsideClassFields)
                    {
                        if (!fileContent.Contains(ClassJunkStartMarker))
                        {
                            int closingBraceIndex = FindClassClosingBraceIndex(fileContent, fileNameWithoutExtension);
                            if (closingBraceIndex != -1)
                            {
                                string junkCode = GenerateRandomJunkMethods();
                                string block = "\n" + ClassJunkStartMarker + "\n" + junkCode + ClassJunkEndMarker + "\n";
                                fileContent = fileContent.Insert(closingBraceIndex, block);
                                isModified = true;
                            }
                        }
                    }

                    // C. Inject inside existing functions (Start / End)
                    if (optJunkInFunctions)
                    {
                        if (!fileContent.Contains("// <RefactorerFuncStart_Junk>") && !fileContent.Contains("// <RefactorerFuncEnd_Junk>"))
                        {
                            var matches = Regex.Matches(fileContent, @"\b(void|int|float|string|bool)\s+([a-zA-Z0-9_]+)\s*\([^)]*\)\s*\{");
                            for (int i = matches.Count - 1; i >= 0; i--)
                            {
                                var match = matches[i];
                                string retType = match.Groups[1].Value;
                                int openBraceIndex = match.Index + match.Length - 1;

                                if (UnityEngine.Random.value > 0.3f)
                                {
                                    string suffix = UnityEngine.Random.Range(1000, 9999).ToString();
                                    
                                    if (retType == "void" && UnityEngine.Random.value > 0.5f)
                                    {
                                        int closeBraceIndex = FindMatchingBrace(fileContent, openBraceIndex);
                                        if (closeBraceIndex != -1)
                                        {
                                            string junk = $"\n// <RefactorerFuncEnd_Junk>\nint junkVal_{suffix} = 99; if (junkVal_{suffix} == 0) {{ junkVal_{suffix}++; }}\n// </RefactorerFuncEnd_Junk>\n";
                                            fileContent = fileContent.Insert(closeBraceIndex, junk);
                                            isModified = true;
                                        }
                                    }
                                    else
                                    {
                                        string junk = $"\n// <RefactorerFuncStart_Junk>\nint junkVal_{suffix} = 99; if (junkVal_{suffix} == 0) {{ junkVal_{suffix}++; }}\n// </RefactorerFuncStart_Junk>\n";
                                        fileContent = fileContent.Insert(openBraceIndex + 1, junk);
                                        isModified = true;
                                    }
                                }
                            }
                        }
                    }

                    // D. Inject uncalled functions
                    if (optJunkUncalledFunctions)
                    {
                        if (!fileContent.Contains("// <RefactorerUncalledFunc_Junk>"))
                        {
                            int closingBraceIndex = FindClassClosingBraceIndex(fileContent, fileNameWithoutExtension);
                            if (closingBraceIndex != -1)
                            {
                                string suffix = UnityEngine.Random.Range(1000, 9999).ToString();
                                string junk = $"\n// <RefactorerUncalledFunc_Junk>\n        private void ResetJunkVal_{suffix}()\n        {{\n            int dummy = UnityEngine.Random.Range(0, 10);\n            if (dummy < 0) {{ dummy = 0; }}\n        }}\n// </RefactorerUncalledFunc_Junk>\n";
                                fileContent = fileContent.Insert(closingBraceIndex, junk);
                                isModified = true;
                            }
                        }
                    }

                    if (isModified)
                    {
                        RecordOrApplyFileChange(fullPath, "Inject requested junk code patterns", fileContent);
                        injectedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during junk code injection: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        private void RemoveJunkCodeFromAllScripts()
        {
            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
            int cleanedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                string appendPattern = @"\r?\n?" + Regex.Escape(JunkStartMarker) + @".*?" + Regex.Escape(JunkEndMarker) + @"\r?\n?";
                Regex appendRegex = new Regex(appendPattern, RegexOptions.Singleline);

                string classPattern = @"\r?\n?" + Regex.Escape(ClassJunkStartMarker) + @".*?" + Regex.Escape(ClassJunkEndMarker) + @"\r?\n?";
                Regex classRegex = new Regex(classPattern, RegexOptions.Singleline);

                string funcStartPattern = @"\r?\n?// <RefactorerFuncStart_Junk>.*?// </RefactorerFuncStart_Junk>\r?\n?";
                Regex funcStartRegex = new Regex(funcStartPattern, RegexOptions.Singleline);

                string funcEndPattern = @"\r?\n?// <RefactorerFuncEnd_Junk>.*?// </RefactorerFuncEnd_Junk>\r?\n?";
                Regex funcEndRegex = new Regex(funcEndPattern, RegexOptions.Singleline);

                string uncalledPattern = @"\r?\n?// <RefactorerUncalledFunc_Junk>.*?// </RefactorerUncalledFunc_Junk>\r?\n?";
                Regex uncalledRegex = new Regex(uncalledPattern, RegexOptions.Singleline);

                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/") || assetPath.Contains("/Editor/") || assetPath.Contains("/Plugins/")) 
                        continue;

                    string fullPath = Path.GetFullPath(assetPath);
                    string fileContent = File.ReadAllText(fullPath);
                    bool wasModified = false;

                    if (fileContent.Contains(JunkStartMarker))
                    {
                        fileContent = appendRegex.Replace(fileContent, string.Empty);
                        wasModified = true;
                    }

                    if (fileContent.Contains(ClassJunkStartMarker))
                    {
                        fileContent = classRegex.Replace(fileContent, string.Empty);
                        wasModified = true;
                    }

                    if (fileContent.Contains("// <RefactorerFuncStart_Junk>"))
                    {
                        fileContent = funcStartRegex.Replace(fileContent, string.Empty);
                        wasModified = true;
                    }

                    if (fileContent.Contains("// <RefactorerFuncEnd_Junk>"))
                    {
                        fileContent = funcEndRegex.Replace(fileContent, string.Empty);
                        wasModified = true;
                    }

                    if (fileContent.Contains("// <RefactorerUncalledFunc_Junk>"))
                    {
                        fileContent = uncalledRegex.Replace(fileContent, string.Empty);
                        wasModified = true;
                    }

                    if (wasModified)
                    {
                        RecordOrApplyFileChange(fullPath, "Remove all junk code patterns", fileContent);
                        cleanedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during junk code removal: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // Clean generated junk folder if it exists
            string junkFolder = Path.Combine(Application.dataPath, "JunkCode");
            if (Directory.Exists(junkFolder))
            {
                if (isDryRun)
                {
                    pendingChanges.Add(new PendingFileChange { filePath = junkFolder, changeDescription = "Delete Assets/JunkCode directory" });
                }
                else
                {
                    Directory.Delete(junkFolder, true);
                    string metaFile = junkFolder + ".meta";
                    if (File.Exists(metaFile)) File.Delete(metaFile);
                }
            }

            AssetDatabase.Refresh();
        }

        private int FindClassClosingBraceIndex(string content, string className)
        {
            string pattern = @"\b(class|struct)\s+" + Regex.Escape(className) + @"\b";
            Match match = Regex.Match(content, pattern);
            if (!match.Success) return -1;

            int startIndex = match.Index + match.Length;
            int openBraceIndex = content.IndexOf('{', startIndex);
            if (openBraceIndex == -1) return -1;

            int depth = 1;
            for (int i = openBraceIndex + 1; i < content.Length; i++)
            {
                if (content[i] == '{') depth++;
                else if (content[i] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private int FindMatchingBrace(string content, int openBraceIndex)
        {
            int depth = 1;
            for (int j = openBraceIndex + 1; j < content.Length; j++)
            {
                if (content[j] == '{') depth++;
                else if (content[j] == '}')
                {
                    depth--;
                    if (depth == 0) return j;
                }
            }
            return -1;
        }

        private string GenerateRandomJunkCodeBlock()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string GetRandStr(int len)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    builder.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
                }
                return builder.ToString();
            }

            string randomSuffix = GetRandStr(6);
            string nsName = "RefactorJunkNS_" + randomSuffix;
            string className = "JunkClass_" + randomSuffix;
            string intFieldName = "value_" + GetRandStr(4);
            string strFieldName = "string_" + GetRandStr(4);
            string methodName = "PerformAction_" + GetRandStr(5);

            int val = UnityEngine.Random.Range(100, 999);
            int loopMax = UnityEngine.Random.Range(2, 6);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {nsName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            sb.AppendLine($"        public int {intFieldName} = {val};");
            sb.AppendLine($"        public string {strFieldName} = \"{randomSuffix}\";");
            sb.AppendLine();
            sb.AppendLine($"        public void {methodName}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            int counter = {intFieldName};");
            sb.AppendLine($"            for (int i = 0; i < {loopMax}; i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                counter += i;");
            sb.AppendLine("            }");
            sb.AppendLine($"            if (counter > {val + 5})");
            sb.AppendLine("            {");
            sb.AppendLine($"                {strFieldName} = \"{randomSuffix}_altered\";");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateRandomJunkMethods()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string GetRandStr(int len)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    builder.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
                }
                return builder.ToString();
            }

            string valName1 = "m_JunkVal_" + GetRandStr(4);
            string valName2 = "m_JunkStr_" + GetRandStr(4);
            string methodName1 = "CalculateJunk_" + GetRandStr(5);
            string methodName2 = "FormatJunk_" + GetRandStr(5);

            int val1 = UnityEngine.Random.Range(10, 100);
            int val2 = UnityEngine.Random.Range(100, 500);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"        // Junk fields");
            sb.AppendLine($"        private int {valName1} = {val1};");
            sb.AppendLine($"        private string {valName2} = \"{GetRandStr(6)}\";");
            sb.AppendLine();
            sb.AppendLine($"        // Junk method 1");
            sb.AppendLine($"        public float {methodName1}(float inputVal)");
            sb.AppendLine("        {");
            sb.AppendLine($"            float result = inputVal * {valName1};");
            sb.AppendLine($"            if (result > {val2})");
            sb.AppendLine("            {");
            sb.AppendLine($"                result /= 2f;");
            sb.AppendLine("            }");
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        // Junk method 2");
            sb.AppendLine($"        public string {methodName2}(string inputStr)");
            sb.AppendLine("        {");
            sb.AppendLine($"            string result = inputStr + {valName2};");
            sb.AppendLine($"            if (result.Length > 20)");
            sb.AppendLine("            {");
            sb.AppendLine($"                result = result.Substring(0, 10);");
            sb.AppendLine("            }");
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");

            return sb.ToString();
        }

        private void GenerateJunkFiles()
        {
            string folderPath = Path.Combine(Application.dataPath, "JunkCode");
            
            if (isDryRun)
            {
                pendingChanges.Add(new PendingFileChange { filePath = folderPath, changeDescription = $"Generate {junkFileCount} random junk script files inside Assets/JunkCode/" });
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string GetRandName(int len)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    builder.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
                }
                return builder.ToString();
            }

            for (int i = 0; i < junkFileCount; i++)
            {
                string className = "JunkUtility_" + GetRandName(8);
                string fileName = className + ".cs";
                string fullPath = Path.Combine(folderPath, fileName);

                string content = GenerateRandomJunkFileContent(className);
                File.WriteAllText(fullPath, content);
            }

            AssetDatabase.Refresh();
            Debug.Log($"Successfully generated {junkFileCount} separate junk files in Assets/JunkCode.");
        }

        private string GenerateRandomJunkFileContent(string className)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string GetRandStr(int len)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    builder.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
                }
                return builder.ToString();
            }

            string nsName = "JunkNS_" + GetRandStr(6);
            string varName = "junkVal_" + GetRandStr(4);
            string funcName = "ComputeJunk_" + GetRandStr(5);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nsName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static int {varName} = {UnityEngine.Random.Range(10, 100)};");
            sb.AppendLine();
            sb.AppendLine($"        public static float {funcName}(float input)");
            sb.AppendLine("        {");
            sb.AppendLine($"            float result = input + {varName};");
            sb.AppendLine("            for (int i = 0; i < 5; i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                result += i;");
            sb.AppendLine("            }");
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
