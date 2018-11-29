
# Rebuild Webex v0.2

based on [Rebuild Webex 0.1](https://github.com/skuater/rebuildwebex)
> Recovery tool for WebEx sessions. 

* Presented in Rootedcon 2015 by @sanguinawer
* Forked from Piruzzolo's branch.

## Binaries

~~There are three binaries in the *builds* folder: a Linux binary, a MacOS binary and a Windows binary.~~

Right now there is only one binary in the *builds* folder: a Windows binary.

There will be Linux and MacOS binaries ***as soon as possible***.

Until then, you can use the Windows version in Linux and MacOS using the Mono runtime.

All the binaries have been compiled for x64 architecture.

~~- The Linux binary has been compiled against a Debian 9 x64 target, and it contains all the libraries and the Mono runtime necessary to execute it. You do not need to install anything, just execute it from CLI.~~

~~- The MacOS binary has been compiled against a OSX 10.7 x64 target, and it contains all the libraries and the Mono runtime necessary to execute it. You do not need to install anything, just execute it from CLI.~~

- The Windows binary can be also executed in Linux or MacOS using the Mono runtime.

The *src* has been refactored in order to be cross-platform and compatible with [Mono](https://www.mono-project.com/) compilation. It also can be compiled for x86 architectures.

To compile the code for Linux or MacOS, use the [mkbundle tool](https://www.mono-project.com/docs/tools+libraries/tools/mkbundle/)

### Binary password

*webex*

## Usage

In order to use this program, you first need to get the folder with the WebEx session data.

The steps to use it are:

1. Play the WebEx recording from the meeting website.
2. It should run the WeBex player. Pause the video and wait until the bottom bar is completely loaded (all blue).
3. Without close the player, locate the folder with the session data. Order by date and it should show up as a folder whose name has several numbers.
	- In Windows, the path is C:\Users\[user_name]\AppData\Local\Temp\[XXXXXXXX]
	- In Linux, the path is /home/[user_name]/.webex/500/[XXXXXXXX]
4. Copy the number folder to a new location.
5. Open the program and load the folder copied before.
6. Click in the 'Go' button and wait until it finishes.
7. The video should be created as ***rebuild.arf*** inside the number folder.
8. The video can be played with the [WebEx ARF Player](https://www.webex.com/play-webex-recording.html)


## Authors

- ***cyrivs89*** (last version, refactoring, Mono compatible cross-platform and nsections and bytes save fixes)
- ***SkUaTeR*** (original dev)
- ***codersk*** (changes to pick selected folder path)
- ***varunpillai*** (WebEx audio changes) 
- ***Piruzzolo*** (UI refactor, eng translation and fixes)