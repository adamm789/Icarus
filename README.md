# Icarus (Working Title)
Work-In-Progress project that packs mods for Final Fantasy XIV.

Currently supports:
* Importing .fbx files and .ttmp2 files
* Modding .fbx files onto equipment and accessories
* Changing material assignments
* Exporting to a Simple TexTools modpack or a basic Penumbra mod folder structure
* Creating, editing, and exporting to an Advanced TexTools modpack
* Exporting files as .fbx
* Adding attributes
* Removing shapes  
* Importing dds to colorset and mtrl
* Importing png and bmp to tex

# Usage
Set game path  
"Browse" to your file  
"Copy" item from the search on the left  
Export to TexTools modpack or Penumbra file structure   

If you wish to build your own copy, you will need the "converters" (for importing fbx), "Skeletons" (for scaling), and "Resources" (for exporting fbx) folders from TexTools in the root directory. They are included in the releases.

# Known Issues
* If you import a ttmp2 file, some files will be called "ReadOnly." They will be skipped if exporting to any Penumbra format. However, they should be included in any TexTools export.  
* While you can theoretically assign any destination path, this is a courtesy. Use at your own risk.  
* When exporting vanilla mdls with skins, the resulting png is incorrect.  
* When importing via fbx, attributes are not automatically set. While intentional, down the line, there may be a setting to change that.  
* Models with custom skeletons will not export to fbx correctly. They appear to work fine in-game.  
* Materials get their colorsets edited slightly upon export  

# Comments
UI is very bare bones and honestly, quite ugly.

Currently uses Lumina, so a clean install is highly recommended/required. Unintended behavior may occur if this is not the case.

This is still in an early development state and will likely change dramatically over time. Crashes may occur both here and in-game. Output files have the possibility of being corrupted, but should™ be fine.

# Notes
Requires .NET 6.0 Desktop Runtime

To save settings, this program adds a folder to AppData. So if you want to remove this project from your computer in its entirety, don't forget that folder.


# Acknowledgements
[TexTools team](https://github.com/TexTools)  
[Lumina](https://github.com/NotAdam/Lumina)
