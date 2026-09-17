using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FolderTools
{
    /// <summary>
    /// Handles the plain text file that holds the folder list.
    ///
    /// The file lives outside the package so it survives package updates, and outside
    /// Assets so Unity does not import it as an asset. By default it sits at
    /// ProjectSettings/FolderStructure.txt, which most teams already commit to git.
    /// Edit it in any text editor and the change is picked up the next time the tool runs.
    /// </summary>
    public static class FolderStructureFile
    {
        public const string DefaultRelativePath = "ProjectSettings/FolderStructure.txt";

        private const string PathPrefsKey = "FolderTools.StructureFilePath";

        public const string DefaultContents =
@"# Folder Structure Creator
#
# One folder per line, with the name inside quote marks.
# A leading - nests the folder inside the line above it.
# Use -- to go a level deeper again.
# Lines starting with # are ignored, and so are blank lines.
# Everything is created inside Assets.

""!Dependencies""
""Extras""
-""Animations""
-""Accessories""
-""Clothes""
-""Hair""
-""Textures""
-""Other""
";

        /// <summary>The folder containing Assets, Packages and ProjectSettings.</summary>
        public static string ProjectRoot
        {
            get { return Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/'); }
        }

        /// <summary>
        /// Absolute path of the structure file. Defaults to ProjectSettings/FolderStructure.txt,
        /// and can be pointed anywhere, including a shared folder outside the project.
        /// </summary>
        public static string FilePath
        {
            get
            {
                string custom = EditorPrefs.GetString(PathPrefsKey, string.Empty);
                return string.IsNullOrEmpty(custom) ? $"{ProjectRoot}/{DefaultRelativePath}" : custom;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    EditorPrefs.DeleteKey(PathPrefsKey);
                    return;
                }

                EditorPrefs.SetString(PathPrefsKey, value.Replace('\\', '/'));
            }
        }

        public static bool UsingDefaultPath
        {
            get { return string.IsNullOrEmpty(EditorPrefs.GetString(PathPrefsKey, string.Empty)); }
        }

        public static bool Exists
        {
            get { return File.Exists(FilePath); }
        }

        public static DateTime LastWriteTime
        {
            get { return File.Exists(FilePath) ? File.GetLastWriteTimeUtc(FilePath) : DateTime.MinValue; }
        }

        /// <summary>
        /// Reads the file. If it is missing it is created with the default list first,
        /// so a fresh project or a fresh clone always has something to edit.
        /// </summary>
        public static string Load()
        {
            string path = FilePath;

            if (!File.Exists(path))
            {
                Save(DefaultContents);
                return DefaultContents;
            }

            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Folder Structure] Could not read {path}. {e.Message}");
                return string.Empty;
            }
        }

        public static bool Save(string contents)
        {
            string path = FilePath;

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Normalise line endings so the file opens cleanly in any editor.
                string normalised = contents.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
                File.WriteAllText(path, normalised);

                // Only refresh if the file happens to live inside Assets.
                if (path.StartsWith(Application.dataPath, StringComparison.OrdinalIgnoreCase))
                {
                    AssetDatabase.Refresh();
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Folder Structure] Could not write {path}. {e.Message}");
                return false;
            }
        }
    }
}
