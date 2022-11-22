# Icarus  
Work-In-Progress project that packs mods for Final Fantasy XIV.

Currently supports:
* Converting .fbx files to models; .dds to material colorset; .png,, .bmp, and .dds to textures  
* Importing .ttmp2 files and editing advanced modpacks  
* Editing models' item path, material assignment, and attributes  
* Editing material's item path, texture paths, colorset, and shader information  
* Editing textures' item path  
* Exporting to .ttmp2 (simple or advanced), or models to .fbx, material colorset to .dds, and textures to .dds (which can then be converted to .png or .bmp with pretty much any image editing software) 

Do note: **Just because Icarus allows something does not necessarily mean it will work in-game. Once you start messing with paths beyond changing variants, the risk of a game crash may increase.**  

As a side note: yes, you should be able to export model, material, and texture mods in .ttmp2 files as described above.  

# Usage
Set game path in settings  
"Browse" to your file  
Assign item paths with "Copy" which takes the item from your search  
Export to desired format

If you wish to build your own copy, you will need the "converters" (for importing fbx), "Skeletons" (for scaling), and "Resources" (for exporting fbx) folders from TexTools in the root directory.

# Known Issues
* If you import a .ttmp2 file, some files will be called "ReadOnly." These cannot be edited and can only be exported in a .ttmp2 file.  
* While you can theoretically assign any destination path, this is a courtesy. **Use at your own risk.**  
* When importing via .fbx, attributes are not automatically set. While intentional, down the line, there may be a setting to change that.  
* Models with custom skeletons will not export to fbx correctly. They appear to work fine in-game.  
* Materials get their colorsets edited slightly upon export. But it shouldn't be an issue overall.  
* Raw imports (i.e. fbx, dds, png) cannot be exported as is back to that format (is there any real reason to actually do this?)  

# Comments
I have created a Discord: https://discord.gg/YFt9Y33hPZ  
You can provide feedback or suggestions, if want.  

UI is very bare bones and honestly, quite ugly.

Currently uses Lumina, so a clean install is highly recommended/required. Unintended behavior may occur if this is not the case.

This is still in an early development state and will likely change dramatically over time. Crashes may occur both here and in-game. Output files have the possibility of being corrupted, but should™ be fine.

# Notes
Requires .NET 6.0 Desktop Runtime

To save settings, this program adds a folder to AppData. So if you want to remove this project from your computer in its entirety, don't forget that folder.


# Acknowledgements
[TexTools team](https://github.com/TexTools)  
[Lumina](https://github.com/NotAdam/Lumina)
