
# Rebuild Webex v0.3

based on [Rebuild Webex 0.1](https://github.com/skuater/rebuildwebex)
> Recovery tool for WebEx sessions. 

* Presented in Rootedcon 2015 by @sanguinawer
* Forked from Piruzzolo's branch.

## What is new?

New version: **0.3**

- The version 0.3 fixes a critical issue with the indexes. If there was more than one file of the same type (e.g. two wav files), the previous version built the file with only the first file leading to an incomplete file. Now, the tool is able to generate the file correctly regardless of how many files of the same type there are.

- Now there is only one binary that can be used in any OS installing the Mono runtime.

## Binaries

There is only a binary in the *builds* folder that can be used in any OS.

The binary has been compiled for x64 architectures.

- In Linux, the binary needs the Mono runtime installed to execute it. Just run it from CLI with `mono rebuild.exe`.

- In MacOS, the binary must be executed as a 32 bits binary due to Carbon driver has not been ported to 64 bits. It needs Mono runtime installed to execute it. Just run it from CLI with `mono --arch=32 rebuild.exe`.

- In Windows, the binary is executed just double clicking on it as any .exe file.

The *src* has been refactored in order to be cross-platform and compatible with [Mono](https://www.mono-project.com/) compilation. It also can be compiled for x86 architectures.

To compile the code for Linux or MacOS native binaries, use the [mkbundle tool](https://www.mono-project.com/docs/tools+libraries/tools/mkbundle/)

### Binary password

*webex*

## Usage

In order to use this program, you first need to get the folder with the WebEx session data.

The steps to use it are:

1. Play the WebEx recording from the meeting website.
2. It should run the WeBex player. Pause the video and wait until the bottom bar is completely loaded (all blue).
3. Without close the player, locate the folder with the session data. Order by date and it should show up as a folder whose name has several numbers.
	- In Windows, the path is C:\Users\\[user_name]\AppData\Local\Temp\[XXXXXXXX]
	- In Linux, the path is /home/[user_name]/.webex/500/[XXXXXXXX]
	- In MacOS, the path is /Users/[user_name]/Library/Application\ Support/WebEx\ Folder/64_500/[XXXXXXXX]
4. Copy the number folder to a new location.
5. Open the program and load the folder copied before.
6. Click in the 'Go' button and wait until it finishes.
7. The video should be created as ***rebuild.arf*** inside the number folder.
8. The video can be played with the [WebEx ARF Player](https://www.webex.com/play-webex-recording.html)


## Authors

- ***cyrivs89*** (last version, fixed index issue, refactoring, Mono compatible cross-platform and nsections and bytes save fixes)
- ***SkUaTeR*** (original dev)
- ***codersk*** (changes to pick selected folder path)
- ***varunpillai*** (WebEx audio changes) 
- ***Piruzzolo*** (UI refactor, eng translation and fixes)

## TODO

- Build native binary for MacOS once the Carbon driver is completely available in 64 bits.
- Build native binary for Linux without need to install any dependencies (There is a bug in mkbundle that do not embed the necessary libraries properly).
