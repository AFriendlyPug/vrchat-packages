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

- `Tools > Folder Structure > Create Structure` builds the folders. Shortcut: Ctrl+Shift+F, or Cmd+Shift+F on macOS.
- `Tools > Folder Structure > Open Editor` opens a window where you can change the list. Your list is saved per machine in EditorPrefs and is used by the menu item and shortcut too.

## List format

One folder per line, with the name inside quote marks. A leading `-` nests the folder inside the line above it, and each extra dash goes one level deeper.

```
"!Dependencies"
"Extras"
-"Animations"
-"Accessories"
-"Clothes"
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
    ├── Textures/
    └── Other/
```

## Notes

- Folders that already exist are left alone, so running it twice is safe.
- Folders are created through `AssetDatabase`, so `.meta` files are generated correctly and the Project window updates immediately.
- Git does not track empty folders. Tick "Add .gitkeep files" in the window if you want the structure to survive a clone.
