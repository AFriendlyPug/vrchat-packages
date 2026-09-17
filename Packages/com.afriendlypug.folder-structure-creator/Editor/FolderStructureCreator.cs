using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FolderTools
{
    /// <summary>
    /// Menu items and the editing window.
    ///
    /// The folder list itself is not stored here. It lives in a plain text file,
    /// by default at ProjectSettings/FolderStructure.txt, so it can be edited in
    /// any text editor, committed to git and shared between projects.
    /// </summary>
    public class FolderStructureCreator : EditorWindow
    {
        private const string GitKeepPrefsKey = "FolderTools.FolderStructureCreator.GitKeep";

        private string _buffer = string.Empty;
        private DateTime _loadedStamp;
        private bool _dirty;
        private bool _changedOnDisk;
        private bool _showPreview;
        private Vector2 _scroll;
        private Vector2 _previewScroll;

        // Ctrl+Shift+F on Windows, Cmd+Shift+F on macOS.
        [MenuItem("Tools/Folder Structure/Create Structure %#f", false, 1)]
        private static void CreateStructure()
        {
            bool gitKeep = EditorPrefs.GetBool(GitKeepPrefsKey, false);
            int created = FolderStructureBuilder.CreateFromFile(gitKeep);
            Debug.Log($"[Folder Structure] Created {created} folder(s) from {FolderStructureFile.FilePath}");
        }

        [MenuItem("Tools/Folder Structure/Edit Folder List", false, 2)]
        private static void EditListExternally()
        {
            FolderStructureFile.Load(); // Creates the file with the defaults if it is missing.
            EditorUtility.OpenWithDefaultApp(FolderStructureFile.FilePath);
        }

        [MenuItem("Tools/Folder Structure/Open Window", false, 20)]
        private static void OpenWindow()
        {
            GetWindow<FolderStructureCreator>("Folder Structure").minSize = new Vector2(360f, 360f);
        }

        private void OnEnable()
        {
            ReloadFromDisk();
        }

        private void OnFocus()
        {
            // Pick up edits made in an external text editor while the window was in the background.
            if (FolderStructureFile.LastWriteTime > _loadedStamp)
            {
                if (_dirty)
                {
                    _changedOnDisk = true;
                }
                else
                {
                    ReloadFromDisk();
                }

                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawFileBar();

            if (_changedOnDisk)
            {
                EditorGUILayout.HelpBox("This file was changed outside Unity while you had unsaved edits here.", MessageType.Warning);
                if (GUILayout.Button("Discard My Edits And Reload"))
                {
                    ReloadFromDisk();
                }
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(140f));
            _buffer = EditorGUILayout.TextArea(_buffer, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            if (EditorGUI.EndChangeCheck())
            {
                _dirty = true;
            }

            DrawPreview();

            EditorGUILayout.Space();

            bool gitKeep = EditorPrefs.GetBool(GitKeepPrefsKey, false);
            bool newGitKeep = EditorGUILayout.ToggleLeft(
                new GUIContent("Add .gitkeep files", "Empty folders are not tracked by git. This keeps them in source control."),
                gitKeep);
            if (newGitKeep != gitKeep)
            {
                EditorPrefs.SetBool(GitKeepPrefsKey, newGitKeep);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!_dirty))
                {
                    if (GUILayout.Button("Save List", GUILayout.Height(26f)))
                    {
                        SaveToDisk();
                    }
                }

                if (GUILayout.Button("Reload", GUILayout.Height(26f), GUILayout.Width(80f)))
                {
                    ReloadFromDisk();
                }

                if (GUILayout.Button("Restore Defaults", GUILayout.Height(26f), GUILayout.Width(130f)))
                {
                    _buffer = FolderStructureFile.DefaultContents;
                    _dirty = true;
                    GUI.FocusControl(null);
                }
            }

            if (GUILayout.Button(_dirty ? "Save And Create Folders" : "Create Folders", GUILayout.Height(32f)))
            {
                if (_dirty)
                {
                    SaveToDisk();
                }

                int created = FolderStructureBuilder.Create(_buffer, newGitKeep);
                Debug.Log($"[Folder Structure] Created {created} folder(s).");
            }
        }

        private void DrawFileBar()
        {
            EditorGUILayout.LabelField("Folder List File", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(FolderStructureFile.FilePath);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Edit In Text Editor"))
                {
                    FolderStructureFile.Load();
                    EditorUtility.OpenWithDefaultApp(FolderStructureFile.FilePath);
                }

                if (GUILayout.Button("Show In Folder", GUILayout.Width(110f)))
                {
                    FolderStructureFile.Load();
                    EditorUtility.RevealInFinder(FolderStructureFile.FilePath);
                }

                if (GUILayout.Button("Choose File", GUILayout.Width(100f)))
                {
                    ChooseFile();
                }

                using (new EditorGUI.DisabledScope(FolderStructureFile.UsingDefaultPath))
                {
                    if (GUILayout.Button("Use Default", GUILayout.Width(100f)))
                    {
                        FolderStructureFile.FilePath = null;
                        ReloadFromDisk();
                    }
                }
            }
        }

        private void DrawPreview()
        {
            _showPreview = EditorGUILayout.Foldout(_showPreview, "Preview", true);
            if (!_showPreview)
            {
                return;
            }

            List<string> paths = FolderStructureBuilder.Preview(_buffer);
            if (paths.Count == 0)
            {
                EditorGUILayout.HelpBox("Nothing to create yet.", MessageType.None);
                return;
            }

            _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, GUILayout.Height(100f));
            foreach (string path in paths)
            {
                EditorGUILayout.LabelField(path, AssetDatabase.IsValidFolder(path)
                    ? EditorStyles.miniLabel
                    : EditorStyles.boldLabel);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField("Bold entries do not exist yet.", EditorStyles.miniLabel);
        }

        private void ChooseFile()
        {
            string selected = EditorUtility.OpenFilePanel(
                "Choose a folder list file",
                FolderStructureFile.ProjectRoot,
                "txt");

            if (string.IsNullOrEmpty(selected))
            {
                return;
            }

            FolderStructureFile.FilePath = selected;
            ReloadFromDisk();
        }

        private void ReloadFromDisk()
        {
            _buffer = FolderStructureFile.Load();
            _loadedStamp = FolderStructureFile.LastWriteTime;
            _dirty = false;
            _changedOnDisk = false;
            GUI.FocusControl(null);
        }

        private void SaveToDisk()
        {
            if (!FolderStructureFile.Save(_buffer))
            {
                return;
            }

            _loadedStamp = FolderStructureFile.LastWriteTime;
            _dirty = false;
            _changedOnDisk = false;
        }
    }
}
