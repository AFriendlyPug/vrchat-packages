# Scene AA

Patches a function in the Unity editor to add anti aliasing back to the editor's Scene view.
Everything happens in memory, no files on disk are changed.

## Usage

1. Install the package with ALCOM, VCC or another VPM client.
2. Open Edit > Project Settings > Quality and choose a level with Anti Aliasing set to something
   other than Disabled.
3. Enjoy.

## Removal

Remove the package from the project in your VPM client. Nothing is left behind, since the patch only
ever existed in memory.

## Credits

Original tool by BlueAmulet, distributed as a .unitypackage. Thanks to pardeike for the Harmony
library. See THIRD-PARTY-NOTICES.md for the bundled library details.

## About this repackaging

This is the original SceneAA source and its bundled Harmony build, rearranged into VPM package
layout. The C# source is unchanged. The asset GUIDs of SceneAA.cs and 0Harmony.dll are carried over
from the original .unitypackage, and package.json declares Assets\SceneAA as a legacy folder, so
installing this package removes an older Assets folder copy rather than leaving a duplicate behind.

No licence was shipped with the original .unitypackage, so no licence is declared here.
