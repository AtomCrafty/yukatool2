# About YukaTool
YukaTool is a tool designed to work with the files used by the "Yuka System" visual novel engine.  
It allows you to extract and repack the game archives as well as convert the various game files to common formats.  

YukaTool comes in two flavors; a command line operated version and a GUI app.  

## YukaTool Gui
The GUI version is meant so serve as a quick way to look through the archives.  
I do not recommend to use it for extraction and insertion of files for translation projects.  
Here are some of the things it can do:
- Open archives and display their directory structure.
- Preview images and text files.
- Show a hex dump of other files.
- Export individual files.
- Export all files in the archive.  
  **Note:** Only raw export is supported, so files will not be converted.
- Import files per drag'n'drop.
- Delete files from an archive.
- Save the modified archive.

<details><summary><em><b>Image:</b> YukaTool Gui</em></summary> 

![YukaTool Gui](https://user-images.githubusercontent.com/4553050/110994200-b91e0a80-8378-11eb-8347-f76994716bbe.png)
</details>

## YukaTool Cli
The CLI variant is completely command line operated. Please make sure you know at least [the basics](https://www.makeuseof.com/tag/a-beginners-guide-to-the-windows-command-line/) before attempting to use it.  
Place `yuka.exe` in your game directory and open a command prompt in the same folder.  
Typing `yuka help` should now display a help page with a list of supported commands.

<details><summary><em><b>Image:</b> YukaTool Cli</em></summary> 
  
![YukaTool Cli](https://user-images.githubusercontent.com/4553050/111015504-5ee96d80-83a9-11eb-9fd2-c3d3b967f802.png)
</details>

# About the "Yuka System" Engine

## The Game Folder
Yuka games are easily identifiable, the game directory usually contains these files:
- A single game executable (`<name>.exe`)
- One to five `dataXX.ykc` files, these contain all game assets.
- An `avi` folder with any video files the game might use.
- A `Save` folder with your settings and save data.

## Archives
The `dataXX.ykc` archives contain the various game files, which include **graphics**, **audio files** and **scripts**.  
When unpacking an archive with default settings, a `.manifest` file will be created in the target folder, listing all extracted files and their original type. This manifest is needed when repacking to assure all files end up in the same format they were in originally.  

## Graphics
The Yuka engine supports a range of common image formats, such as `.bmp` and `.png`.  
There is however also a proprietary image format called `.ykg` (Yuka Graphics), which can store animation frames in addition to the pixel data.  
When unpacking with default settings, these files will automatically be converted to `.png`. If the file also contains frame data, it will be saved in an `.ani` file with the same name.  

## Scripts
Yuka scripts (`.yks` files) are the binary script files controlling everything the game does.  
They are not meant to be read by humans, which is why YukaTool will decompile them back into a human readable format.  
The source files end in `.ykd` and may be accompanied by a `.csv` file of the same name.  
These `.csv` files can be imported into the spreadsheet editor of your choice and contain all the text from their correponding script.  

The binary script format is unfortunately very convoluted and not publically documented, so some of them might fail to decompile properly.  
If that is the case, please [open an issue on GitHub](https://github.com/AtomCrafty/yukatool2/issues/new) and attach the script file in question.  

# Translating a Game
The following instructions should give you an overview over the required steps to translate a Yuka game.

## 1. Inspecting the Archives
> The first thing you probably want to do when starting a translation project is to extract the script files.  
> Which archive contains the scenario scripts varies from game to game, so let's take a look at the file contents.  
> We are looking for a series of `.yks` scripts, possibly grouped by the game route.  
> 
> ### Option A - Using YukaTool Gui
> > Drag and drop one of the `.ykc` archives onto `yuka_gui.exe` to open it in the archive viewer.  
> > You can then search the left panel for any files you're interested in.
> > 
> > <details><summary><em><b>Image:</b> Viewing files in YukaTool Gui</em></summary> 
> > 
> > ![Viewing files in YukaTool Gui](https://user-images.githubusercontent.com/4553050/110998068-8e36b500-837e-11eb-8c39-5f385d8df1ec.png)
> > </details>
> 
> ### Option B - Using YukaTool Cli
> > YukaTool Cli provides the `list` command, which allows you to inspect the files in an archive or folder.  
> > Running `yuka list data01.ykc` will list all files in the `data01.ykc` archive.  
> > Since I already know the files I'm looking for are contained in a folder called `story`, I will restrict the output by passing the filter `*story*` as the second argument to the `list` command.  
> > If your terminal has issues displaying the special line characters used for the tree view, you can instead have the file names printed in a flat list by specifying the `--list` flag.  
> > 
> > <details><summary><em><b>Image:</b> Listing files with YukaTool Cli</em></summary> 
> > 
> > ![Listing files with YukaTool Cli](https://user-images.githubusercontent.com/4553050/111000637-b1636380-8382-11eb-8497-d902f312b950.png)
> > </details>

## 2. Extracting Files
> Now that we know where the files we want are located, the next step is to extract them.  
> In this case we are going to extract everything from the `data01.ykc` archive by using the command `yuka unpack -v data01.ykc`.  
> Notice that I specified the `-v` (or `--verbose`) flag. This will provide more detailed status information and will alert you of any problems that might occur.  
> The `unpack` command will now do its job and extract the entire archive into a folder with the same name.
> 
> <details><summary><em><b>Image:</b> Extracting archives</em></summary> 
> 
> ![Extracting archives](https://user-images.githubusercontent.com/4553050/111007732-d3afae00-838f-11eb-9b89-840c0f69339c.png)
> </details>
> 
> During extraction the proprietary Yuka file formats are converted into common formats for easy editing.  
> If one of these conversions fails, the original file will be copied instead.  
> You can disable the conversion for all files by specifying the `--raw` flag.
> 
> For more information on what the `unpack` command can do, type `yuka help unpack`.

## 3. Importing the Text Files
> As you may have noticed, our `.yks` files from step 1 have now been turned into pairs of `.ykd` and `.csv` files.  
> YukaTool splits the text lines (`.csv`) from the rest (`.ykd`) so you don't have to deal with the code itself.  
> Depending on the game, YukaTool might sometimes not detect all of the text, if some strings are missing from the `.csv` file, have a look at the corresponding `.ykd`.
> 
> <details><summary><em><b>Image:</b> Decompiled <code>.ykd</code> and <code>.csv</code> files</em></summary> 
> 
> ![Decompiled .ykd and .csv files](https://user-images.githubusercontent.com/4553050/111008579-b11e9480-8391-11eb-9cbb-96b3da6c3e48.png)
> </details>
> 
> CSV stands for "comma-separated values" and is one of the simplest and most widely supported file format for tabular data.  
> As such, we can simply import the generated `.csv` files into any spreadsheet application. For this example I will be using [Google Docs](https://drive.google.com/).  
> First I'll create a blank spreadsheet, in which I will then import all of the `.csv` files. Make sure to keep every file in a separate sheet!
> 
> <details><summary><em><b>Image:</b> Creating a Blank Spreadsheet</em></summary> 
> 
> ![Creating a Blank Spreadsheet](https://user-images.githubusercontent.com/4553050/111008988-afa19c00-8392-11eb-913a-41e2b2e5be33.png)
> </details>
> 
> <details><summary><em><b>Image:</b> Importing Translation Sheets</em></summary> 
> 
> ![Importing Translation Sheets](https://user-images.githubusercontent.com/4553050/111010108-cd243500-8395-11eb-88ff-32f1a868c678.png)
> </details>

## 4. The Spreadsheet Format
> Now that you've imported the text into your spreadsheet, let's take a look at how these tables are structured.  
> By default, every sheet has eight columns.
> 
> - The `ID` is a unique identifier that helps YukaTool keep track which line of text goes where when reinserting the text.
>   - It contains a letter denoting the type of string (`N`ame, `L`ine or `S`tring) and a sequential number.
> - The `Speaker` column contains the name of the character speaking that particular line.
>   - As different games might use different mechanisms to set the current speaker, this might not be detected correctly.
> - The `Original` column contains the text as it was present when the files were unpacked.
> - All columns whose name is enclosed by `[` square brackets `]` are treated as translation revisions.
>   - You can add or remove as many of these columns as you like, YukaTool will always choose the rightmost non-empty translation column for each line.
>   - Cells containing just a single period `.` are treated as empty when inserting. This allows you to signal "no change needed" on a revision without leaving the cell empty.
>   - If all translation columns are empty, the original text will be used.
> - The `Comment` column allows you to insert arbitrary comments, it is ignored by the script inserter.
> 
> At the top of every sheet you can find a list of the names of all characters speaking in that file.  
> This only works if the speaker names are detected correctly, otherwise they might be listed as a separate string for every line.
> 
> Underneath are all other lines and strings, in order of occurrence.  
> Entries with an `S` id might refer to choice text, dynamically generated lines or other text.  
> YukaTool does its best to filter out file names and other strings that don't need to be translated.
> 
> <details><summary><em><b>Image:</b> Translation Sheet Structure</em></summary> 
> 
> ![Translation Sheet Structure](https://user-images.githubusercontent.com/4553050/111011129-c64af180-8398-11eb-9205-a73e6735f586.png)
> </details>

## 5. Reinserting the Text
> Now, let's assume you've made some edits to the translation sheet and want to check if the changes look right in-game.  
> First of all you will need to export the modified spreadsheet back to a `.csv` file.
> 
> <details><summary><em><b>Image:</b> Downloading as <code>.csv</code></em></summary> 
> 
> ![Downloading as .csv](https://user-images.githubusercontent.com/4553050/111014176-cb14a300-83a2-11eb-8255-1517beaf42b0.png)
> </details>
> 
> After you've done that, use this new version to replace the original `.csv` file.
> Make sure the name matches that of the corresponding `.ykd` file.  
> In my case I have to replace the `CHAPTER1-1.csv` file in `data01\yks\story\common\`.  
> 
> If you haven't already done so, now would be a good time to create a backup of the original game archive, because next we will repack all the files we previously extracted.  
> For me, the command `yuka pack -v data01` will do the trick. Note that I'm specifying the *folder* name this time, not the archive name.
> 
> For more information on what the `pack` command can do, type `yuka help pack`.
> 
> <details><summary><em><b>Image:</b> Repacking the Archive</em></summary> 
> 
> ![Repacking the Archive](https://user-images.githubusercontent.com/4553050/111014570-96095000-83a4-11eb-87fc-7a84c15283fc.png)
> </details>
> 
> And that's it, your text is now in the game!
> 
> <details><summary><em><b>Image:</b> Text in the Game</em></summary> 
> 
> ![Text in the Game](https://user-images.githubusercontent.com/4553050/111014747-8807ff00-83a5-11eb-8f52-798007b2f78e.png)
> </details>

## 6. Closing Remarks
> Of course there is more to a good localization than just translating the text.  
> Editing menu graphics is pretty straightforward, since YukaTool automatically converts `.ykg` graphics to `.png` and vice versa.  
> If you want to change the font name or size, search for a script containing a call to the function `DefaultFontSet`.  
> The second parameter (most likely an `@S` reference to an entry in the `.csv` file) is the font name, the third one the font size.
> 
> Finally, if you end up using YukaTool I would be glad to hear about your project!  
> Let me know if I you need help with anything, either per GitHub issue or on Discord ([AtomCrafty#4511](https://discord.com/users/289189886609588224)).  
> And good luck :)

# Game Support
This is a list of games utilizing the Yuka engine and how well YukaTool works with their files.  
Let me know if you find any games not listed :)

| Year | Developer   | Title                                                                            | Compatibility                                     | YKC | YKG | YKS |
| ---- | ----------- | -------------------------------------------------------------------------------- | ------------------------------------------------- | :-: | :-: | :-: |
| 2007 | Feng        | [Akane Iro ni Somaru Saka                             ](https://vndb.org/v547)   | ⚠ Partially compatible<sup>[1](#fn-g1)</sup>      | ✔  | ✔  | ✔  |
| 2008 | PeasSoft    | [Tsun na Kanojo Dere na Kanojo                        ](https://vndb.org/v1336)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2010 | PeasSoft    | [Ama Ane                                              ](https://vndb.org/v3034)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2010 | Hooksoft    | [Sakura Bitmap                                        ](https://vndb.org/v3859)  | ⚠ Partially compatible<sup>[2](#fn-g2)</sup>      | ✔  | ⚠  | ⚠  |
| 2010 | Feng        | [Hoshizora e Kakaru Hashi                             ](https://vndb.org/v2968)  | ⚠ Partially compatible<sup>[3](#fn-g3)</sup>      | ✔  | ✔  | ⚠  |
| 2011 | Smee        | [Lover Able                                           ](https://vndb.org/v5734)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2011 | Hooksoft    | [Strawberry Nauts                                     ](https://vndb.org/v7507)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2011 | PeasSoft    | [Iinazuke wa Imouto-sama!                             ](https://vndb.org/v11086) | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2011 | PeasSoft    | [Kimi o Aogi Otome wa Hime ni                         ](https://vndb.org/v5942)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2011 | PeasSoft    | [Amakan Ecchi na "Love Icha" Tsumechaimashita         ](https://vndb.org/v7752)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2012 | Feng        | [Hoshizora e Kakaru Hashi AA                          ](https://vndb.org/v8309)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2012 | Smee        | [Dousei Lover Able                                    ](https://vndb.org/v7774)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2012 | PeasSoft    | [Zutto Tsukushite Ageru no!                           ](https://vndb.org/v8439)  | ✔ Fully compatible                                | ✔  | ✔  | ✔  |
| 2013 | PeasSoft    | [Koisuru Shimai no Sextet                             ](https://vndb.org/v13353) | ⚠ Partially compatible<sup>[4](#fn-g4)</sup>      | ✔  | ✔  | ⚠  |
| 2013 | PeasSoft    | [Shitsuji ga Aruji o Erabu Toki                       ](https://vndb.org/v10932) | ⚠ Partially compatible<sup>[5](#fn-g5)</sup>      | ✔  | ✔  | ⚠  |
| 2013 | PeasSoft    | [Koi x Koi = Infinity \~Koisuru Otome ni Dekiru Koto\~](https://vndb.org/v12377) | ⚠ Partially compatible<sup>[6](#fn-g6)</sup>      | ✔  | ✔  | ⚠  |
| 2014 | Caramel Box | [Semiramis no Tenbin                                  ](https://vndb.org/v14760) | ⛔ Different engine version<sup>[7](#fn-g7)</sup> | ⚠  | ✔  | ⛔ |
| 2015 | Caramel Box | [Semiramis no Tenbin - Fated Dolls                    ](https://vndb.org/v16742) | ⛔ Different engine version<sup>[7](#fn-g7)</sup> | ⚠  | ✔  | ⛔ |

1. <a name="fn-g1" href="#fn-g1"></a>There's a bug when unpacking .bmp files.  
2. <a name="fn-g2" href="#fn-g2"></a>Decompiling `battle.ykc` and `init.yks` results in the error `Assignment target already set`.  
Some graphics in `ykg\coin\` appear to be glitched.  
3. <a name="fn-g3" href="#fn-g3"></a>Decompiling `ui_017.yks` results in an invalid cast (CInt -> Ctrl).  
4. <a name="fn-g4" href="#fn-g4"></a>Decompiling `bgmmode.yks`, `CGMode.yks`, `config.yks`, `Confirmation.yks`, `loadmenu.yks`, `menu.yks`, `omake.yks`, `savemenu.yks`, `SCMode.yks`, `Start.yks` and `TextWindow_SystemSelect.yks` results in the error `Invalid expression element: Func`.  
5. <a name="fn-g5" href="#fn-g5"></a>Decompiling `bgmmode.yks`, `config.yks`, `Confirmation.yks`, `GalleryMode.yks`, `loadmenu.yks`, `menu.yks`, `savemenu.yks`, `start.yks` and `TextWindow_SystemSelect.yks` results in the error `Invalid expression element: Func`.  
Assertion failure in `mayu_00410.yks` due to a malformed if statement (pretty sure this is a bug in the game script).  
6. <a name="fn-g6" href="#fn-g6"></a>Decompiling `bgmmode.yks`, `CGMode.yks`, `config.yks`, `Confirmation.yks`, `loadmenu.yks`, `menu.yks`, `omake.yks`, `savemenu.yks`, `SCMode.yks`, `Start.yks` and `TextWindow_SystemSelect.yks` results in the error `Invalid expression element: Func`.  
Decompiling `all_00791.yks`, `all_00810.yks`, `all_00820.yks`, `all_00830.yks`, `all_00840.yks`, `all_00851.yks`, `all_00861.yks`, `all_00901.yks`, `all_00920.yks`, `all_00930.yks`, `all_00960.yks`, `all_01020.yks` results in an invalid cast (CInt -> Ctrl).  
Assertion failure in `chris_00300.yks` due to a malformed if statement (pretty sure this is a bug in the game script).  
7. <a name="fn-g7" href="#fn-g7"></a>Assertion failure due to new archive signature (`YKC002\0\0`).  
Hard crash while trying to unpack `.csv` files.  
Script files are not recognized as such due to new file signature (`YKS002\0\0`).  
Non-ascii file names get corrupted when unpacking, maybe the encoding changed with the new container format?  

**If you discover any other games using this engine, please contact me so I can add them to the list.**
