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

        // Pure C# Manual Revert Subsystem
        [System.Serializable]
        public class RenameRecord
        {
            public string oldPath;
            public string newPath;
            public string oldName;
            public string newName;
            public bool isFolder;
        }

        [System.Serializable]
        public class TextChangeRecord
        {
            public string filePath;
            public string oldText;
            public string newText;
        }

        [System.Serializable]
        public class RefactorHistory
        {
            public List<RenameRecord> renames = new List<RenameRecord>();
            public List<TextChangeRecord> textChanges = new List<TextChangeRecord>();
        }

        private RefactorHistory activeHistory = new RefactorHistory();

        // Renaming Parameters
        public enum AffixType { Prefix, Suffix }
        private string affixString = "MySD";
        private AffixType affixType = AffixType.Prefix;

        private DefaultAsset targetFolder;
        private bool renameScripts = true;
        private bool renameFolders = true;
        private bool renameAssets = true;

        private Dictionary<string, string> fileContentCache = new Dictionary<string, string>();

        // Obfuscation & Junk Code parameters
        private bool optJunkInFunctions = true;
        private bool optJunkUncalledFunctions = true;

        // Scroll positions
        private Vector2 scrollPosition;
        private Vector2 dryRunScrollPosition;

        [MenuItem("Software District/Refactoring Tool")]
        public static void ShowWindow()
        {
            ProjectRefactorer window = GetWindow<ProjectRefactorer>("Refactoring Tool");
            window.minSize = new Vector2(480, 680);
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
            
            GUILayout.Label("PROJECT REFACTORER & OBFUSCATOR", titleStyle);
            GUILayout.Label("Safely prefix/suffix target directories, scripts, and assets with automated reference integrity and junk code signatures.", subtitleStyle);
            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // History warning
            string historyPath = Path.Combine(Application.dataPath, "../Library/refactorer_history.json");
            bool hasHistory = File.Exists(historyPath);
            if (hasHistory)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Space(5);
                GUI.contentColor = new Color(1.0f, 0.7f, 0.1f);
                GUILayout.Label("⚠️ Refactoring History Found!", EditorStyles.boldLabel);
                GUI.contentColor = Color.white;
                GUILayout.Label("You can revert the project to its original state using the button at the bottom. Note: Running a new refactor will overwrite this history.", EditorStyles.wordWrappedLabel);
                GUILayout.Space(5);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            // --- Section 1: Target Folder & Renaming Settings ---
            DrawSectionHeader("1. Target Folder & Renaming Settings");
            
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder:", targetFolder, typeof(DefaultAsset), false);
            GUILayout.Space(5);
            
            if (targetFolder == null)
            {
                var warnStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                warnStyle.normal.textColor = new Color(0.9f, 0.4f, 0.4f);
                GUILayout.Label("❌ No Target Folder Selected\nDrag and drop a folder (e.g. Assets/MyFeature) from the Project window. The refactoring scope will be strictly restricted to this folder to protect third-party libraries.", warnStyle);
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(targetFolder);
                var infoStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                infoStyle.normal.textColor = new Color(0.4f, 0.8f, 0.5f);
                GUILayout.Label($"✅ Target Folder: {path}\nOnly folders, scripts, and assets inside this path will be renamed. C# class reference updates will scan the entire project to ensure compilation.", infoStyle);
            }
            GUILayout.Space(5);
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Renaming Parameters
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            affixString = EditorGUILayout.TextField("String to Add (e.g. MySD):", affixString);
            affixType = (AffixType)EditorGUILayout.EnumPopup("Add Position:", affixType);
            
            GUILayout.Space(8);
            GUILayout.Label("What to Rename:", EditorStyles.miniBoldLabel);
            renameScripts = DrawLeftToggle("Rename C# Scripts & Classes (uses '_')", renameScripts);
            renameFolders = DrawLeftToggle("Rename Folders (uses '-')", renameFolders);
            renameAssets = DrawLeftToggle("Rename Art, Music & Assets (uses '-')", renameAssets);
            GUILayout.Space(5);
            GUILayout.EndVertical();
            
            GUILayout.Space(8);
            if (GUILayout.Button("Run Batch Refactor", GUILayout.Height(30)))
            {
                if (targetFolder == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please select a target folder first.", "OK");
                    return;
                }
                if (string.IsNullOrEmpty(affixString))
                {
                    EditorUtility.DisplayDialog("Error", "Please enter a string to add.", "OK");
                    return;
                }

                string msg = isDryRun 
                    ? "This will calculate a dry run of the batch rename. Proceed?" 
                    : "WARNING: This will permanently rename scripts, classes, folders, and assets on disk inside the target folder. Proceed?";
                
                if (EditorUtility.DisplayDialog("Run Batch Refactor", msg, "Yes", "Cancel"))
                {
                    RunGlobalBatchRefactor(!isDryRun);
                }
            }
            GUILayout.Space(15);

            // --- Section 2: Smart Junk Code ---
            DrawSectionHeader("2. Smart Junk Code & Obfuscation");
            
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            GUILayout.Label("💡 Smart Obfuscation Details:", EditorStyles.miniBoldLabel);
            GUILayout.Label("Injects randomized, performance-neutral code inside methods and uncalled private helper functions. This changes the binary signature of the build, which is useful for App Store checks. Restricts changes to scripts in the target folder.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(8);
            optJunkInFunctions = DrawLeftToggle("Inject inside methods (Start/End of void functions)", optJunkInFunctions);
            optJunkUncalledFunctions = DrawLeftToggle("Inject small uncalled methods inside classes", optJunkUncalledFunctions);
            GUILayout.Space(5);
            GUILayout.EndVertical();
            
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Inject Junk Code", GUILayout.Height(30)))
            {
                if (targetFolder == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please select a target folder first.", "OK");
                }
                else
                {
                    string msg = isDryRun
                        ? "This will calculate a dry run of junk code injection. Proceed?"
                        : "This will inject junk code into scripts inside the target folder. Proceed?";
                    if (EditorUtility.DisplayDialog("Inject Junk Code", msg, "Yes", "Cancel"))
                    {
                        InjectJunkCodeToAllScripts(!isDryRun);
                    }
                }
            }
            if (GUILayout.Button("Remove Junk Code", GUILayout.Height(30)))
            {
                if (targetFolder == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please select a target folder first.", "OK");
                }
                else
                {
                    string msg = isDryRun
                        ? "This will calculate a dry run of junk code removal. Proceed?"
                        : "This will remove junk code from scripts inside the target folder. Proceed?";
                    if (EditorUtility.DisplayDialog("Remove Junk Code", msg, "Yes", "Cancel"))
                    {
                        RemoveJunkCodeFromAllScripts(!isDryRun);
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            // --- Section 3: Execution & Dry Run Settings ---
            DrawSectionHeader("3. Execution & Safety Settings");
            
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            isDryRun = DrawLeftToggle("Dry Run Mode (Preview changes without writing to disk)", isDryRun);
            GUILayout.Space(8);
            
            if (pendingChanges.Count > 0)
            {
                var pendStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                pendStyle.normal.textColor = new Color(1.0f, 0.7f, 0.1f);
                GUILayout.Label($"⚠️ Planned Changes Pending: {pendingChanges.Count} modifications calculated.\nSelect 'Execute Planned Changes' below to write these changes to disk.", pendStyle);
                GUILayout.Space(8);
                
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
                GUILayout.Label("ℹ️ No pending changes.\nRun 'Batch Refactor' or 'Inject Junk Code' with Dry Run enabled to preview changes here.", EditorStyles.wordWrappedLabel);
            }
            GUILayout.Space(5);
            GUILayout.EndVertical();
            
            GUILayout.Space(8);
            if (hasHistory)
            {
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                if (GUILayout.Button("Revert Renames & Content", GUILayout.Height(30)))
                {
                    RevertProjectManual();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.Space(15);

            // --- Section 4: Dry Run Logs ---
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
            GUILayout.Space(10);
            GUILayout.Label(title, EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetRect(10, 2, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.5f, 0.8f, 0.8f));
            GUILayout.Space(5);
        }

        private bool DrawLeftToggle(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            bool newValue = GUILayout.Toggle(value, "", GUILayout.Width(20));
            GUILayout.Label(label, EditorStyles.wordWrappedLabel);
            GUILayout.EndHorizontal();
            return newValue;
        }

        // --- Cache Helpers ---
        private string GetFileContent(string filePath)
        {
            if (fileContentCache.TryGetValue(filePath, out string content))
            {
                return content;
            }
            if (File.Exists(filePath))
            {
                content = File.ReadAllText(filePath);
                fileContentCache[filePath] = content;
                return content;
            }
            return null;
        }

        private void SetFileContent(string filePath, string newContent)
        {
            fileContentCache[filePath] = newContent;
        }

        private void FlushCacheToPendingChanges()
        {
            foreach (var kvp in fileContentCache)
            {
                string filePath = kvp.Key;
                string newContent = kvp.Value;
                if (File.Exists(filePath))
                {
                    string originalContent = File.ReadAllText(filePath);
                    if (newContent != originalContent)
                    {
                        pendingChanges.Add(new PendingFileChange {
                            filePath = filePath,
                            changeDescription = "Refactored C# code structure / class references",
                            newContent = newContent
                        });
                    }
                }
            }
            fileContentCache.Clear();
        }

        // --- Naming Helpers ---
        private string GetRenamedName(string originalName, bool isCSharp)
        {
            string connector = isCSharp ? "_" : "-";
            if (affixType == AffixType.Prefix)
            {
                return affixString + connector + originalName;
            }
            else
            {
                return originalName + connector + affixString;
            }
        }

        // --- Helper for Dry Run / Live Execution ---
        private void RecordOrApplyFileChange(string filePath, string description, string newContent)
        {
            if (isDryRun)
            {
                pendingChanges.Add(new PendingFileChange { filePath = filePath, changeDescription = description, newContent = newContent });
            }
            else
            {
                string relativePath = "Assets" + filePath.Substring(Application.dataPath.Length).Replace('\\', '/');
                
                // Record history (save original text) if not already done
                if (File.Exists(filePath))
                {
                    bool alreadyRecorded = false;
                    foreach (var tc in activeHistory.textChanges)
                    {
                        if (tc.filePath == relativePath)
                        {
                            alreadyRecorded = true;
                            break;
                        }
                    }

                    if (!alreadyRecorded)
                    {
                        string originalContent = File.ReadAllText(filePath);
                        activeHistory.textChanges.Add(new TextChangeRecord {
                            filePath = relativePath,
                            oldText = originalContent,
                            newText = newContent
                        });
                    }
                }

                File.WriteAllText(filePath, newContent);
            }
        }

        private bool RecordOrApplyRename(string assetPath, string description, string newName)
        {
            string oldName = Path.GetFileName(assetPath);
            bool isFolder = AssetDatabase.IsValidFolder(assetPath);
            
            // Calculate new path relative to Assets/
            string parentDir = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string newPath = parentDir + "/" + newName;
            if (!isFolder)
            {
                string ext = Path.GetExtension(assetPath);
                newPath += ext;
            }

            if (isDryRun)
            {
                pendingChanges.Add(new PendingFileChange { filePath = assetPath, changeDescription = description, renameNewName = newName });
                return true;
            }
            else
            {
                // Record history
                activeHistory.renames.Add(new RenameRecord {
                    oldPath = assetPath,
                    newPath = newPath,
                    oldName = Path.GetFileNameWithoutExtension(assetPath),
                    newName = newName,
                    isFolder = isFolder
                });

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

            activeHistory = new RefactorHistory();

            AssetDatabase.StartAssetEditing();
            try
            {
                // Temporarily disable Dry Run to allow RecordOrApply to write history
                isDryRun = false;

                // 1. Write text changes first
                for (int i = 0; i < pendingChanges.Count; i++)
                {
                    var change = pendingChanges[i];
                    EditorUtility.DisplayProgressBar("Applying Changes", $"Writing file edits... ({i + 1}/{pendingChanges.Count})", (float)i / pendingChanges.Count);
                    if (change.newContent != null && File.Exists(change.filePath))
                    {
                        RecordOrApplyFileChange(change.filePath, change.changeDescription, change.newContent);
                    }
                }

                // 2. Rename files (scripts and assets) second
                for (int i = 0; i < pendingChanges.Count; i++)
                {
                    var change = pendingChanges[i];
                    EditorUtility.DisplayProgressBar("Applying Changes", $"Renaming assets... ({i + 1}/{pendingChanges.Count})", (float)i / pendingChanges.Count);
                    if (change.renameNewName != null && !AssetDatabase.IsValidFolder(change.filePath) && File.Exists(change.filePath))
                    {
                        RecordOrApplyRename(change.filePath, change.changeDescription, change.renameNewName);
                    }
                }

                // 3. Rename folders last
                for (int i = 0; i < pendingChanges.Count; i++)
                {
                    var change = pendingChanges[i];
                    EditorUtility.DisplayProgressBar("Applying Changes", $"Renaming folders... ({i + 1}/{pendingChanges.Count})", (float)i / pendingChanges.Count);
                    if (change.renameNewName != null && AssetDatabase.IsValidFolder(change.filePath) && Directory.Exists(Path.GetFullPath(change.filePath)))
                    {
                        RecordOrApplyRename(change.filePath, change.changeDescription, change.renameNewName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception applying planned changes: {ex.Message}");
            }
            finally
            {
                isDryRun = true; // Restore Dry Run state
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }

            // Save history JSON
            string historyPath = Path.Combine(Application.dataPath, "../Library/refactorer_history.json");
            try
            {
                string json = JsonUtility.ToJson(activeHistory, true);
                File.WriteAllText(historyPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save refactoring history: {ex.Message}");
            }

            pendingChanges.Clear();
            AssetDatabase.Refresh();
            Debug.Log("Successfully applied all planned changes to disk.");
        }

        // --- Core Refactoring Implementations ---

        private void ExecuteClassRefactor(string search, string replace, bool exact, string targetFolderPath)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(replace)) return;

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");

            AssetDatabase.StartAssetEditing();
            try
            {
                var renames = new List<(string oldPath, string oldName, string newName)>();

                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/")) 
                        continue;

                    // SCOPE CHECK: Only rename if script is inside targetFolderPath
                    if (!assetPath.StartsWith(targetFolderPath + "/"))
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

                // Gather all user scripts across the whole project (excluding Packages) to update references globally
                var allUserScripts = new List<string>();
                foreach (string guid in scriptGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.StartsWith("Packages/")) 
                        continue;
                    allUserScripts.Add(Path.GetFullPath(assetPath));
                }

                for (int i = 0; i < renames.Count; i++)
                {
                    var rename = renames[i];
                    EditorUtility.DisplayProgressBar("Class Refactoring", $"Updating references for '{rename.oldName}' -> '{rename.newName}' ({i + 1}/{renames.Count})", (float)i / renames.Count);

                    string oldClassRegex = @"\b" + Regex.Escape(rename.oldName) + @"\b";

                    foreach (string scriptPath in allUserScripts)
                    {
                        string fileContent = GetFileContent(scriptPath);
                        if (fileContent != null && Regex.IsMatch(fileContent, oldClassRegex))
                        {
                            fileContent = Regex.Replace(fileContent, oldClassRegex, rename.newName);
                            SetFileContent(scriptPath, fileContent);
                        }
                    }

                    if (File.Exists(Path.GetFullPath(rename.oldPath)))
                    {
                        // Add rename pending change
                        pendingChanges.Add(new PendingFileChange {
                            filePath = rename.oldPath,
                            changeDescription = $"Rename C# script from '{rename.oldName}' to '{rename.newName}'",
                            renameNewName = rename.newName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during class refactor: {ex.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }
        }

        // --- One-Click Batch Refactorer ---
        private void RunGlobalBatchRefactor(bool runLive)
        {
            pendingChanges.Clear();
            fileContentCache.Clear();

            string targetFolderPath = "";
            if (targetFolder != null)
            {
                targetFolderPath = AssetDatabase.GetAssetPath(targetFolder);
                if (!AssetDatabase.IsValidFolder(targetFolderPath))
                {
                    targetFolderPath = "";
                }
            }

            if (string.IsNullOrEmpty(targetFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid target folder.", "OK");
                return;
            }

            // 1. Prefix/Suffix scripts and classes (using Underscore '_')
            if (renameScripts)
            {
                string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
                AssetDatabase.StartAssetEditing();
                try
                {
                    for (int i = 0; i < scriptGUIDs.Length; i++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(scriptGUIDs[i]);
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        EditorUtility.DisplayProgressBar("Batch Script Renaming", $"Preparing '{fileName}' ({i + 1}/{scriptGUIDs.Length})", (float)i / scriptGUIDs.Length);

                        if (assetPath.StartsWith("Packages/")) 
                            continue;

                        // SCOPE CHECK: Only rename if script is inside targetFolderPath
                        if (!assetPath.StartsWith(targetFolderPath + "/"))
                            continue;

                        string newName = GetRenamedName(fileName, true);

                        if (newName != fileName)
                        {
                            ExecuteClassRefactor(fileName, newName, true, targetFolderPath);
                        }
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                }
            }

            // 2. Prefix/Suffix Assets next (art, music, prefabs, materials, models, animations using Hyphen '-')
            if (renameAssets)
            {
                string[] allAssetGUIDs = AssetDatabase.FindAssets("");
                AssetDatabase.StartAssetEditing();
                try
                {
                    for (int i = 0; i < allAssetGUIDs.Length; i++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(allAssetGUIDs[i]);
                        string fileName = Path.GetFileNameWithoutExtension(assetPath);
                        EditorUtility.DisplayProgressBar("Batch Asset Renaming", $"Renaming '{fileName}' ({i + 1}/{allAssetGUIDs.Length})", (float)i / allAssetGUIDs.Length);

                        if (assetPath.StartsWith("Packages/")) 
                            continue;

                        // SCOPE CHECK: Only rename if asset is inside targetFolderPath
                        if (!assetPath.StartsWith(targetFolderPath + "/"))
                            continue;

                        string ext = Path.GetExtension(assetPath).ToLower();
                        bool isAssetToRename = ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".psd" ||
                                               ext == ".wav" || ext == ".mp3" || ext == ".ogg" ||
                                               ext == ".prefab" || ext == ".mat" || ext == ".fbx" ||
                                               ext == ".anim" || ext == ".controller";

                        if (isAssetToRename)
                        {
                            if (File.Exists(Path.GetFullPath(assetPath)))
                            {
                                string newName = GetRenamedName(fileName, false);

                                if (newName != fileName)
                                {
                                    pendingChanges.Add(new PendingFileChange {
                                        filePath = assetPath,
                                        changeDescription = $"Batch prefix asset file '{fileName}' to '{newName}'",
                                        renameNewName = newName
                                    });
                                }
                            }
                        }
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                }
            }

            // 3. Prefix/Suffix folders last (using bottom-up path ordering and Hyphen '-')
            if (renameFolders)
            {
                string fullTargetFolderPath = Path.GetFullPath(targetFolderPath);
                if (Directory.Exists(fullTargetFolderPath))
                {
                    string[] directories = Directory.GetDirectories(fullTargetFolderPath, "*", SearchOption.AllDirectories);
                    var sortedDirs = new List<string>(directories);
                    sortedDirs.Sort((a, b) => b.Length.CompareTo(a.Length));

                    AssetDatabase.StartAssetEditing();
                    try
                    {
                        for (int i = 0; i < sortedDirs.Count; i++)
                        {
                            string dirPath = sortedDirs[i];
                            string dirName = Path.GetFileName(dirPath);
                            EditorUtility.DisplayProgressBar("Batch Folder Renaming", $"Renaming '{dirName}' ({i + 1}/{sortedDirs.Count})", (float)i / sortedDirs.Count);

                            string relativePath = "Assets" + dirPath.Substring(Application.dataPath.Length).Replace('\\', '/');

                            // Safeguard standard folders and specific third-party structures
                            string lowerDirName = dirName.ToLower();
                            if (lowerDirName == "editor" || 
                                lowerDirName == "plugins" || 
                                lowerDirName == "runtime" || 
                                lowerDirName == "resources" || 
                                lowerDirName == "streamingassets" || 
                                lowerDirName == "webgltemplates" ||
                                lowerDirName == "textmesh pro" ||
                                lowerDirName == "textmeshpro" ||
                                lowerDirName == "googlemobileads" ||
                                lowerDirName == "spine" ||
                                lowerDirName == "spine-unity" ||
                                lowerDirName == "cgincludes" ||
                                lowerDirName == "junkcode")
                            {
                                continue;
                            }

                            if (relativePath.Contains("/Editor/") || relativePath.EndsWith("/Editor") ||
                                relativePath.Contains("/Plugins/") || relativePath.EndsWith("/Plugins") ||
                                relativePath.Contains("/TextMesh Pro/") || relativePath.EndsWith("/TextMesh Pro") ||
                                relativePath.Contains("/Packages/") || relativePath.EndsWith("/Packages") ||
                                relativePath.Contains("/JunkCode/") || relativePath.EndsWith("/JunkCode"))
                            {
                                continue;
                            }

                            if (Directory.Exists(dirPath))
                            {
                                string newDirName = GetRenamedName(dirName, false);
                                if (newDirName != dirName)
                                {
                                    pendingChanges.Add(new PendingFileChange {
                                        filePath = relativePath,
                                        changeDescription = $"Batch prefix folder '{dirName}' to '{newDirName}'",
                                        renameNewName = newDirName
                                    });
                                }
                            }
                        }
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                        AssetDatabase.StopAssetEditing();
                    }
                }
            }

            // Flush in-memory script cache to pendingChanges
            FlushCacheToPendingChanges();

            if (runLive)
            {
                ApplyPendingDryRunChanges();
            }
            else
            {
                Debug.Log($"Dry run refactor calculation completed. {pendingChanges.Count} changes planned.");
            }
        }

        // --- C# Manual Revert ---
        private void RevertProjectManual()
        {
            string historyPath = Path.Combine(Application.dataPath, "../Library/refactorer_history.json");
            if (!File.Exists(historyPath))
            {
                EditorUtility.DisplayDialog("Revert Changes", "No refactoring history found. Cannot revert automatically.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Revert All Changes", "This will restore your scripts, folders, and assets to their original names and contents before the last refactor. Proceed?", "Yes", "Cancel"))
            {
                string json = File.ReadAllText(historyPath);
                RefactorHistory history = JsonUtility.FromJson<RefactorHistory>(json);

                if (history == null)
                {
                    EditorUtility.DisplayDialog("Error", "History file is corrupted.", "OK");
                    return;
                }

                AssetDatabase.StartAssetEditing();
                try
                {
                    int totalSteps = history.textChanges.Count + history.renames.Count;
                    int step = 0;

                    // 1. Restore original text contents
                    for (int i = 0; i < history.textChanges.Count; i++)
                    {
                        var change = history.textChanges[i];
                        step++;
                        EditorUtility.DisplayProgressBar("Reverting Changes", $"Restoring file contents ({step}/{totalSteps})", (float)step / totalSteps);

                        string currentPath = GetCurrentDiskPath(change.filePath, history.renames, -1);
                        string fullPath = Path.GetFullPath(currentPath);

                        if (File.Exists(fullPath))
                        {
                            File.WriteAllText(fullPath, change.oldText);
                        }
                    }

                    // 2. Rename files back (reverse order of renames)
                    for (int i = history.renames.Count - 1; i >= 0; i--)
                    {
                        var rename = history.renames[i];
                        step++;
                        EditorUtility.DisplayProgressBar("Reverting Changes", $"Restoring names ({step}/{totalSteps})", (float)step / totalSteps);

                        if (!rename.isFolder)
                        {
                            string currentAssetPath = GetCurrentDiskPath(rename.oldPath, history.renames, i);
                            string fullPath = Path.GetFullPath(currentAssetPath);
                            if (File.Exists(fullPath))
                            {
                                string error = AssetDatabase.RenameAsset(currentAssetPath, rename.oldName);
                                if (!string.IsNullOrEmpty(error))
                                {
                                    Debug.LogError($"Error reverting file rename: {error}");
                                }
                            }
                        }
                    }

                    // 3. Rename folders back (reverse order of renames)
                    for (int i = history.renames.Count - 1; i >= 0; i--)
                    {
                        var rename = history.renames[i];
                        if (rename.isFolder)
                        {
                            string currentAssetPath = GetCurrentDiskPath(rename.oldPath, history.renames, i);
                            string fullPath = Path.GetFullPath(currentAssetPath);
                            if (Directory.Exists(fullPath))
                            {
                                string error = AssetDatabase.RenameAsset(currentAssetPath, rename.oldName);
                                if (!string.IsNullOrEmpty(error))
                                {
                                    Debug.LogError($"Error reverting folder rename: {error}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception during manual revert: {ex.Message}");
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                }

                try
                {
                    File.Delete(historyPath);
                }
                catch { }

                AssetDatabase.Refresh();
                Debug.Log("Project successfully reverted to its pre-refactored state.");
            }
        }

        private string GetCurrentDiskPath(string path, List<RenameRecord> renames, int currentIndex)
        {
            string currentPath = path;
            for (int j = currentIndex + 1; j < renames.Count; j++)
            {
                if (renames[j].isFolder && currentPath.StartsWith(renames[j].oldPath + "/"))
                {
                    currentPath = renames[j].newPath + currentPath.Substring(renames[j].oldPath.Length);
                }
                else if (!renames[j].isFolder && currentPath.Equals(renames[j].oldPath))
                {
                    currentPath = renames[j].newPath;
                }
            }
            return currentPath;
        }

        // --- Obfuscation & Junk Code ---

        private void InjectJunkCodeToAllScripts(bool runLive)
        {
            pendingChanges.Clear();
            fileContentCache.Clear();

            string targetFolderPath = "";
            if (targetFolder != null)
            {
                targetFolderPath = AssetDatabase.GetAssetPath(targetFolder);
                if (!AssetDatabase.IsValidFolder(targetFolderPath))
                {
                    targetFolderPath = "";
                }
            }

            if (string.IsNullOrEmpty(targetFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid target folder.", "OK");
                return;
            }

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < scriptGUIDs.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(scriptGUIDs[i]);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    EditorUtility.DisplayProgressBar("Junk Code Injection", $"Processing script '{fileName}' ({i + 1}/{scriptGUIDs.Length})", (float)i / scriptGUIDs.Length);

                    if (assetPath.StartsWith("Packages/")) 
                        continue;

                    // SCOPE CHECK: Only target files in the target folder
                    if (!assetPath.StartsWith(targetFolderPath + "/"))
                        continue;

                    string fullPath = Path.GetFullPath(assetPath);
                    string fileContent = File.ReadAllText(fullPath);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
                    bool isModified = false;

                    // A. Inject inside existing functions (Start / End)
                    if (optJunkInFunctions)
                    {
                        if (!fileContent.Contains("// <RefactorerFuncStart_Junk>") && !fileContent.Contains("// <RefactorerFuncEnd_Junk>"))
                        {
                            var matches = Regex.Matches(fileContent, @"\b(void|int|float|string|bool)\s+([a-zA-Z0-9_]+)\s*\([^)]*\)\s*\{");
                            for (int mIndex = matches.Count - 1; mIndex >= 0; mIndex--)
                            {
                                var match = matches[mIndex];
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

                    // B. Inject uncalled functions
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
                        SetFileContent(fullPath, fileContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during junk code injection: {ex.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }

            // Flush cache
            FlushCacheToPendingChanges();

            if (runLive)
            {
                ApplyPendingDryRunChanges();
            }
            else
            {
                Debug.Log($"Dry run junk code injection completed. {pendingChanges.Count} files will be modified.");
            }
        }

        private void RemoveJunkCodeFromAllScripts(bool runLive)
        {
            pendingChanges.Clear();
            fileContentCache.Clear();

            string targetFolderPath = "";
            if (targetFolder != null)
            {
                targetFolderPath = AssetDatabase.GetAssetPath(targetFolder);
                if (!AssetDatabase.IsValidFolder(targetFolderPath))
                {
                    targetFolderPath = "";
                }
            }

            if (string.IsNullOrEmpty(targetFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid target folder.", "OK");
                return;
            }

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");

            AssetDatabase.StartAssetEditing();
            try
            {
                string funcStartPattern = @"\r?\n?// <RefactorerFuncStart_Junk>.*?// </RefactorerFuncStart_Junk>\r?\n?";
                Regex funcStartRegex = new Regex(funcStartPattern, RegexOptions.Singleline);

                string funcEndPattern = @"\r?\n?// <RefactorerFuncEnd_Junk>.*?// </RefactorerFuncEnd_Junk>\r?\n?";
                Regex funcEndRegex = new Regex(funcEndPattern, RegexOptions.Singleline);

                string uncalledPattern = @"\r?\n?// <RefactorerUncalledFunc_Junk>.*?// </RefactorerUncalledFunc_Junk>\r?\n?";
                Regex uncalledRegex = new Regex(uncalledPattern, RegexOptions.Singleline);

                for (int i = 0; i < scriptGUIDs.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(scriptGUIDs[i]);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    EditorUtility.DisplayProgressBar("Junk Code Removal", $"Cleaning script '{fileName}' ({i + 1}/{scriptGUIDs.Length})", (float)i / scriptGUIDs.Length);

                    if (assetPath.StartsWith("Packages/")) 
                        continue;

                    // SCOPE CHECK: Only target files in the target folder
                    if (!assetPath.StartsWith(targetFolderPath + "/"))
                        continue;

                    string fullPath = Path.GetFullPath(assetPath);
                    string fileContent = File.ReadAllText(fullPath);
                    bool wasModified = false;

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
                        SetFileContent(fullPath, fileContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during junk code removal: {ex.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }

            // Flush cache
            FlushCacheToPendingChanges();

            if (runLive)
            {
                ApplyPendingDryRunChanges();
            }
            else
            {
                Debug.Log($"Dry run junk code removal completed. {pendingChanges.Count} files will be cleaned.");
            }
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
    }
}
