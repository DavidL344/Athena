<img alt="Athena banner" src="./banner.png" width="425"  height="128" />

Athena is a cross-platform utility for file extension handling across multiple applications with various parameters.

![GitHub Release](https://img.shields.io/github/v/release/DavidL344/Athena?style=for-the-badge&logo=github&label=Latest%20release)
![GitHub Release](https://img.shields.io/github/v/release/DavidL344/Athena?include_prereleases&style=for-the-badge&logo=github&label=Latest%20pre-release&color=%23ffa500)

## Features
- **Cross-platform with portability in mind**
  - The commands are as equivalent as possible on all supported platforms
  - Configuration is saved as plain text and can be version-controlled across multiple systems
  - Easily turned portable by creating a `user` directory in the application's root
- **Seamless**
  - Double-clicking a file opens an app picker, letting the user choose how to open the file
    - The entries aren't limited to just applications like it is with most system pickers
  - Easily add multiple entries of the same application with different parameters
    - `Media player (Play)`, `Media player (Add to queue)`
    - `Web browser (Open)`, `Web Browser (Open in private window)`
  - Multiple app picker frontends are available based on the user's preference
    - TUI (platform-agnostic)
    - WPF (Windows)
    - GTK (Linux)
- **Online streaming support**
  - For applications that support it, add a protocol prefix to the front of a URL to parse it as a file
    - `athena run <URL>/<file>.mp4` (opens in a web browser)
    - `athena run athena:<URL>/<file>.mp4` (opens in a video player)
- **QoL enhancements**
  - Easily edit configuration* and generate templates using built-in commands

| Command                | Opened/Generated file       | Detection method                                    |
|------------------------|-----------------------------|-----------------------------------------------------|
| `athena edit`          | the main configuration file | no additional arguments after the `edit` subcommand |
| `athena edit mpv.play` | the `mpv player` app entry  | an argument after the `edit` subcommand             |
| `athena edit .mp4`     | the `.mp4` file extension   | the `.` at the start of the file extension          |
| `athena edit https://` | the `https` protocol        | the `://` part at the end of the protocol           |

&ast; due to CliWrap's [current limitation](https://github.com/Tyrrrz/CliWrap/issues/225), TUI applications opened through Athena don't support interactivity

## Libraries used

| Library                                                              | License                                                                       |
|----------------------------------------------------------------------|-------------------------------------------------------------------------------|
| [Cocona](https://github.com/mayuki/Cocona)                           | [MIT](https://github.com/mayuki/Cocona/blob/master/LICENSE)                   |
| [Spectre.Console](https://github.com/spectreconsole/spectre.console) | [MIT](https://github.com/spectreconsole/spectre.console/blob/main/LICENSE.md) |
| [CliWrap](https://github.com/Tyrrrz/CliWrap)                         | [MIT](https://github.com/Tyrrrz/CliWrap/blob/master/License.txt)              |
| [GitInfo](https://github.com/devlooped/GitInfo)                      | [MIT](https://github.com/devlooped/GitInfo/blob/main/license.txt)             |
| [ini-parser-new](https://github.com/sandrock/ini-parser-dotnet)      | [MIT](https://github.com/sandrock/ini-parser-dotnet/blob/develop/LICENSE)     |
| [GtkSharp](https://github.com/GtkSharp/GtkSharp)                     | [GNU LGPLv2](https://github.com/GtkSharp/GtkSharp/blob/develop/LICENSE)       |
| [ModernWpfUI](https://github.com/Kinnara/ModernWpf)                  | [MIT](https://github.com/Kinnara/ModernWpf/blob/master/LICENSE)               |

Special thanks to @cinnamonbear055 for designing the logo and banner!
