# Torrent Legacy Fix

Torrent Legacy Fix is a Windows utility for Elden Ring that repairs legacy or modded `regulation.bin` files that are missing Torrent-related parameter entries required by newer versions of the game.

It is designed for cases where an older or modded regulation worked previously, but after the Elden Ring 1.17 update using the Spectral Steed Whistle no longer spawns Torrent.

The utility performs a surgical repair rather than upgrading or replacing the entire regulation file.

## What the program does

Torrent Legacy Fix:

1. Uses your installed Elden Ring Game folder as a source for the current Torrent parameter data.
2. Allows you to select the legacy or modded `regulation.bin` you want to repair.
3. Inspects `NpcParam` and `RideParam` for the required Torrent entries.
4. Creates a `.torrentbackup` copy of the original regulation before modifying it.
5. Adds only the missing Torrent-related parameter rows.
6. Re-encrypts the modified regulation.
7. Reopens the resulting regulation and verifies that the required rows were successfully added.
8. Provides an option to restore the original backup.

The `regulation.bin` in your Elden Ring installation is used only as a source for the required current data and is not modified.

## Torrent parameter rows

The utility checks and, when necessary, restores the following entries:

### RideParam

- `80020`
- `80030`
- `80040`
- `80050`

### NpcParam

- `80020000`
- `80030000`
- `80040000`
- `80050000`

No other parameter rows are intentionally added or replaced.

## Usage

1. Extract the entire release archive to a folder.
2. Run `TorrentLegacyFixGUI.exe`.
3. Click **Select Elden Ring Game Folder**.
4. Select the folder containing `eldenring.exe`.

For a standard Steam installation, this will normally be:

    ...\Steam\steamapps\common\ELDEN RING\Game

5. Click **Select Legacy / Modded File**.
6. Select the old or modded `regulation.bin` you want to repair.
7. The utility will inspect the file and report whether the required Torrent rows are missing.
8. If the repair is required, click **Install Torrent Fix**.
9. Confirm the operation.

A backup is created alongside the selected regulation before it is modified:

    regulation.bin.torrentbackup

The **Restore Backup** button can be used to restore that original file.

## Important

Do not select the current vanilla `regulation.bin` from your Elden Ring Game folder as the file to repair.

The file selected in step 2 of the application should be the legacy or modded regulation that is actually experiencing the Torrent issue.

Keep all files and folders from the release archive together. The application is distributed as a self-contained Windows build and requires its included runtime files.

The program does not bundle Elden Ring's `regulation.bin` or `oo2core_6_win64.dll`. These are accessed from the user's existing Elden Ring installation when required.

## Source code

The application source is located in:

    TorrentLegacyFixGUI/

SoulsFormatsNEXT is included as a Git submodule and remains maintained by its respective authors.

SoulsFormatsNEXT:

https://github.com/soulsmods/SoulsFormatsNEXT

## Build requirements

- Windows
- .NET 10 SDK
- Git

## Building from source

Clone this repository including its submodules:

    git clone --recurse-submodules https://github.com/Romulus753/Torrent-Legacy-Save-Patcher-and-Fix.git
    cd Torrent-Legacy-Save-Patcher-and-Fix

Build the application:

    dotnet build TorrentLegacyFixGUI/TorrentLegacyFixGUI.csproj -c Release

To create a self-contained Windows x64 release:

    dotnet publish TorrentLegacyFixGUI/TorrentLegacyFixGUI.csproj -c Release -r win-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false

The resulting application will be located under:

    TorrentLegacyFixGUI/bin/Release/net10.0-windows/win-x64/publish/

The entire contents of the `publish` directory should be kept together when distributing or running the self-contained build.

## Dependencies

Torrent Legacy Fix uses SoulsFormatsNEXT for reading, decrypting, modifying, encrypting, and verifying Elden Ring regulation files.

SoulsFormatsNEXT:

https://github.com/soulsmods/SoulsFormatsNEXT

SoulsFormatsNEXT is licensed under the GNU General Public License v3.0.

The project also includes the required `NpcParam.xml` and `RideParam.xml` parameter definitions.

## Privacy / Networking

Torrent Legacy Fix does not contain telemetry or analytics and does not transmit user data over the network.

The application operates on files selected locally by the user and files from the locally installed Elden Ring Game folder.