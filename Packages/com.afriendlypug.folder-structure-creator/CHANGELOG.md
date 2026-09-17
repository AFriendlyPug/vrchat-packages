# Changelog

All notable changes to this package are documented in this file.

## [1.1.1] - 2026-09-17

### Changed
- Added `Hair` to the default list, under `Extras`. Projects with an existing `FolderStructure.txt` keep their own list, and can pick this up with Restore Defaults in the window.

## [1.1.0] - 2026-09-17

### Changed
- The folder list now lives in its own text file at `ProjectSettings/FolderStructure.txt` instead of inside the script, so it can be edited without touching the package.

### Added
- `Tools > Folder Structure > Edit Folder List` opens that file in your default text editor.
- Point the tool at any other text file, including one shared between projects.
- Preview of the folders the list will create, marking the ones that do not exist yet.
- Blank lines and `#` comments are ignored in the list.

## [1.0.0] - 2026-09-17

### Added
- Menu item and shortcut to create the folder structure inside Assets.
- Editor window for changing the folder list, saved to EditorPrefs.
- Optional `.gitkeep` files so empty folders survive source control.
