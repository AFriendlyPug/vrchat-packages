using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FolderTools
{
    /// <summary>
    /// Turns the text list into folders inside Assets.
    /// </summary>
    public static class FolderStructureBuilder
    {
        /// <summary>
        /// Reads the structure file and creates any folders that do not already exist.
        /// Returns the number of folders actually created.
        /// </summary>
        public static int CreateFromFile(bool addGitKeep)
        {
            return Create(FolderStructureFile.Load(), addGitKeep);
        }

        /// <summary>
        /// Parses the supplied text and creates any folders that do not already exist.
        /// Returns the number of folders actually created.
        /// </summary>
        public static int Create(string structure, bool addGitKeep)
        {
            if (string.IsNullOrWhiteSpace(structure))
            {
                Debug.LogWarning("[Folder Structure] The folder list is empty, so nothing was created.");
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
                    if (line.Length == 0 || line[0] == '#')
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
        /// Builds a preview of the paths the list would create, without touching the project.
        /// </summary>
        public static List<string> Preview(string structure)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(structure))
            {
                return result;
            }

            List<string> parents = new List<string> { "Assets" };

            foreach (string rawLine in structure.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#')
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
                    continue;
                }

                if (depth >= parents.Count)
                {
                    depth = parents.Count - 1;
                }

                string path = $"{parents[depth]}/{name}";
                result.Add(path);

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

            return result;
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
