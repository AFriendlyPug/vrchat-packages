using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FolderTools
{
    /// <summary>
    /// Creates a folder structure inside the project's Assets folder from a simple text list.
    ///
    /// Format rules:
    ///   The folder name is whatever sits inside the "" marks.
    ///   A line with no leading dash is created directly inside Assets.
    ///   A line starting with - is created inside the closest line above it
    ///   that has one fewer dash. Extra dashes go deeper again (--, ---, etc).
    ///
    /// This ships as a package, so the tool itself lives under Packages.
    /// The folders it creates always go into Assets.
    /// </summary>
    public class FolderStructureCreator : EditorWindow
    {
        private const string PrefsKey = "FolderTools.FolderStructureCreator.Structure";
        private const string GitKeepPrefsKey = "FolderTools.FolderStructureCreator.GitKeep";

        private const string DefaultStructure =
@"""!Dependencies""
""Extras""
-""Animations""
-""Accessories""
-""Clothes""
-""Textures""
-""Other""";

        private string _structure = DefaultStructure;
        private bool _addGitKeep;
        private Vector2 _scroll;

        // Ctrl+Shift+F on Windows, Cmd+Shift+F on macOS.
        [MenuItem("Tools/Folder Structure/Create Structure %#f", false, 1)]
        private static void CreateSavedStructure()
        {
            string structure = EditorPrefs.GetString(PrefsKey, DefaultStructure);
            bool gitKeep = EditorPrefs.GetBool(GitKeepPrefsKey, false);

            int created = CreateFolders(structure, gitKeep);
            Debug.Log($"[Folder Structure] Created {created} folder(s).");
        }

        [MenuItem("Tools/Folder Structure/Open Editor", false, 20)]
        private static void OpenWindow()
        {
            GetWindow<FolderStructureCreator>("Folder Structure").minSize = new Vector2(320f, 300f);
        }

        private void OnEnable()
        {
            _structure = EditorPrefs.GetString(PrefsKey, DefaultStructure);
            _addGitKeep = EditorPrefs.GetBool(GitKeepPrefsKey, false);
        }

        private void OnDisable()
        {
            SavePrefs();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "One folder per line. The name goes in quote marks.\n" +
                "A leading - nests the folder inside the line above it.\n" +
                "Use -- to go a level deeper again.\n" +
                "Everything is created inside Assets.",
                MessageType.Info);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _structure = EditorGUILayout.TextArea(_structure, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            _addGitKeep = EditorGUILayout.ToggleLeft(
                new GUIContent("Add .gitkeep files", "Empty folders are not tracked by git. This keeps them in source control."),
                _addGitKeep);

            if (EditorGUI.EndChangeCheck())
            {
                SavePrefs();
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Folders", GUILayout.Height(30f)))
                {
                    int created = CreateFolders(_structure, _addGitKeep);
                    Debug.Log($"[Folder Structure] Created {created} folder(s).");
                }

                if (GUILayout.Button("Reset To Default", GUILayout.Height(30f), GUILayout.Width(130f)))
                {
                    _structure = DefaultStructure;
                    SavePrefs();
                    GUI.FocusControl(null);
                }
            }
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString(PrefsKey, _structure);
            EditorPrefs.SetBool(GitKeepPrefsKey, _addGitKeep);
        }

        /// <summary>
        /// Parses the structure text and creates any folders that do not already exist.
        /// Returns the number of folders actually created.
        /// </summary>
        public static int CreateFolders(string structure, bool addGitKeep)
        {
            if (string.IsNullOrWhiteSpace(structure))
            {
                return 0;
            }

            // parents[depth] holds the asset path a folder at that depth should be created in.
            List<string> parents = new List<string> { "Assets" };
            List<string> resolvedPaths = new List<string>();
            int created = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string rawLine in structure.Split('\n'))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    int depth = 0;
                    while (depth < line.Length && line[depth] == '-')
                    {
                        depth++;
                    }

                    string name = ExtractName(line.Substring(depth));
                    if (string.IsNullOrEmpty(name))
                    {
                        Debug.LogWarning($"[Folder Structure] Skipped a line with no usable folder name: {rawLine}");
                        continue;
                    }

                    // A line indented deeper than its parent allows is pulled back to the deepest valid level.
                    if (depth >= parents.Count)
                    {
                        depth = parents.Count - 1;
                    }

                    string parent = parents[depth];
                    string path = $"{parent}/{name}";

                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        string guid = AssetDatabase.CreateFolder(parent, name);
                        if (string.IsNullOrEmpty(guid))
                        {
                            Debug.LogError($"[Folder Structure] Could not create {path}. Check the name for invalid characters.");
                            continue;
                        }

                        created++;
                    }

                    resolvedPaths.Add(path);

                    // This folder becomes the parent for anything one level deeper.
                    if (parents.Count > depth + 1)
                    {
                        parents[depth + 1] = path;
                        if (parents.Count > depth + 2)
                        {
                            parents.RemoveRange(depth + 2, parents.Count - (depth + 2));
                        }
                    }
                    else
                    {
                        parents.Add(path);
                    }
                }

                if (addGitKeep)
                {
                    foreach (string path in resolvedPaths)
                    {
                        string keep = Path.Combine(path, ".gitkeep");
                        if (!File.Exists(keep))
                        {
                            File.WriteAllText(keep, string.Empty);
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            return created;
        }

        /// <summary>
        /// Pulls the folder name out of the "" marks. Falls back to the raw text if no quotes are present.
        /// </summary>
        private static string ExtractName(string text)
        {
            text = text.Trim();

            int first = text.IndexOf('"');
            int last = text.LastIndexOf('"');
            if (first >= 0 && last > first)
            {
                text = text.Substring(first + 1, last - first - 1);
            }

            return text.Trim();
        }
    }
}
