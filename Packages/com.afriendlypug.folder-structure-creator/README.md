# Folder Structure Creator

Creates a preset group of folders inside `Assets` in one click.

## Installing

### With ALCOM or the VRChat Creator Companion

Add the listing to ALCOM once, then install the package into any project:

1. ALCOM sidebar > Packages > Add repository.
2. Paste `https://afriendlypug.github.io/folder-structure-creator/index.json`.
3. Open your project in ALCOM, find Folder Structure Creator in the package list and press the plus button.

### Manually

Copy the `com.afriendlypug.folder-structure-creator` folder into your project's `Packages` folder. Unity picks it up on the next focus of the editor window.

### From disk or git

Window > Package Manager > `+` > Add package from disk, then select `package.json`. Or use Add package from git URL with the repository URL.

## Using it

The folder list is a plain text file. By default it is created at:

```
<your project>/ProjectSettings/FolderStructure.txt
```

That location is outside the package, so package updates never overwrite it, and outside `Assets`, so Unity does not import it as an asset. It is a normal file in your project, so it goes into git alongside everything else and anyone who clones the project gets the same structure.

- `Tools > Folder Structure > Create Structure` builds the folders from the file. Shortcut: Ctrl+Shift+F, or Cmd+Shift+F on macOS.
- `Tools > Folder Structure > Edit Folder List` opens the file in your default text editor. Save it and run the shortcut again, no Unity restart needed.
- `Tools > Folder Structure > Open Window` edits the same file inside Unity, with a preview of what will be created.

The file is created with the default list the first time you run any of those, so there is nothing to set up by hand.

### Using a different file

Open the window and press Choose File to point the tool at any other `.txt` file, including one on a shared drive or in a folder you keep your presets in. Press Use Default to go back to the project's own file.

## List format

One folder per line, with the name inside quote marks. A leading `-` nests the folder inside the line above it, and each extra dash goes one level deeper. Blank lines and lines starting with `#` are ignored.

```
# my default avatar project layout

"!Dependencies"
"Extras"
-"Animations"
-"Accessories"
-"Clothes"
-"Hair"
-"Textures"
-"Other"
```

That produces:

```
Assets/
├── !Dependencies/
└── Extras/
    ├── Animations/
    ├── Accessories/
    ├── Clothes/
    ├── Hair/
    ├── Textures/
    └── Other/
```

## Notes

- Folders that already exist are left alone, so running it twice is safe.
- Folders are created through `AssetDatabase`, so `.meta` files are generated correctly and the Project window updates immediately.
- Git does not track empty folders. Tick "Add .gitkeep files" in the window if you want the structure to survive a clone.
