# pinpoint

Pinpoint is an extensible keystroke launcher and productivity tool written in C# (.NET Core).

## Table of Contents

- [Installation](#installation)
- [Plugins](#plugins)
- [Keyboard Shortcuts](#keyboard-shortcuts)
- [Contributing](#contributing)

## Installation

- Download the latest pre-compiled release [here](https://github.com/dkgv/pinpoint/releases) (pick the standalone version if you do not have .NET 5 installed)
- Extract the `Pinpoint-X.X.X.zip` file
- Launch `Pinpoint.exe`

## Plugins

| Name               | What / Why                                                                                             | Preview                              |
| ------------------ | ------------------------------------------------------------------------------------------------------ | ------------------------------------ |
| App Search         | Search for applications on your computer                                                               | ![](https://i.imgur.com/O3BdrxM.png) |
| Bangs              | Search on websites directly with the 10.000+ [DuckDuckGo !Bang](https://duckduckgo.com/bang) operators | ![](https://i.imgur.com/pbF7sZB.png) |
| Bookmarks          | Search in bookmarks from your default browser (Brave, Chrome, Firefox)                                 | ![](https://i.imgur.com/M2qvYCs.png) |
| Calculator         | Calculate mathematical expressions instantly                                                           | ![](https://i.imgur.com/mtsthTj.png) |
| Clipboard          | Easy-to-use clipboard history manager                                                                  |                                      |
| Clipboard Share    | Share your clipboard via a pastebin                                                                    |                                      |
| Color Converter    | Convert hex colors to RGB and vice versa                                                               | ![](https://i.imgur.com/r1NmnZE.png) |
| Command Line       | Execute CLI queries without breaking your workflow                                                     | ![](https://i.imgur.com/tsPcp1l.png) |
| Control Panel      | Search for control panel items                                                                         | ![](https://i.imgur.com/GClOIaI.png) |
| Currency Converter | Convert between currencies (including cryptocurrencies)                                                | ![](https://i.imgur.com/XJUmMNT.png) |
| Dictionary         | Look up definitions on the fly                                                                         | ![](https://i.imgur.com/eokgopn.png) |
| Encode/Decode      | Encode and decode hex, binary, and base64.                                                             |                                      |
| Everything         | Snappy search across all your files using [Everything](https://www.voidtools.com/)                     | ![](https://i.imgur.com/rhovLIX.png) |
| Finance            | Look up real-time ticker prices via Yahoo Finance                                                      | ![](https://i.imgur.com/dXSv6aQ.png) |
| Hacker News        | Browse recently popular submissions from [Hacker News](https://news.ycombinator.com/)                  | ![](https://i.imgur.com/neQd1nv.png) |
| Metric Converter   | Convert between imperial and metric units                                                              | ![](https://i.imgur.com/OqOwZNY.png) |
| Notes              | Quickly create and save notes                                                                          | ![](https://i.imgur.com/foFfxtv.png) |
| Operating System   | Shut down, restart/reboot, and sleep your computer                                                     | ![](https://i.imgur.com/5GwwQBg.png) |
| Password Generator | Generate variable length passwords on the fly                                                          | ![](https://i.imgur.com/zonNyXo.png) |
| Process Manager    | Find and kill processes by name                                                                        |                                      |
| reddit             | Quick browse reddit posts with standard sorting options                                                | ![](https://i.imgur.com/sViePHZ.png) |
| Spotify            | Control Spotify on any device directly from Pinpoint                                                   | ![](https://i.imgur.com/Ol8dBI4.png) |
| Text               | Easily perform various transformative text actions.                                                    | ![](https://i.imgur.com/FbCQXXX.png) |
| Time Zone          | Convert from/to different time zones                                                                   |                                      |
| Translation        | Translate text between any two languages                                                               |                                      |
| URL Launcher       | Launch URLs in your favorite browser                                                                   | ![](https://i.imgur.com/faRe3zd.png) |
| Weather Forecast   | Look up weather forecasts (weekly + daily)                                                             | ![](https://i.imgur.com/OC4RBgr.png) |

## Keyboard Shortcuts

| Key Combination                                                                                                                        | Action                                       |
| -------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| <kbd>ALT</kbd>+<kbd>SPACE</kbd>                                                                                                        | Open/close query box                         |
| <kbd>ESC</kbd>                                                                                                                         | Close query box if empty                     |
| <kbd>Enter</kbd>                                                                                                                       | Execute selected query result                |
| <kbd>ALT</kbd>+<kbd>ENTER</kbd>                                                                                                        | Open primary option of selected query result |
| <kbd>CTRL</kbd>+[<kbd>↑</kbd>,<kbd>↓</kbd>]                                                                                            | Navigate history older/newer                 |
| <kbd>CTRL</kbd>+<kbd>,</kbd>                                                                                                           | Open settings panel                          |
| <kbd>CTRL</kbd>+<kbd>L</kbd>                                                                                                           | Focus query box and its contents             |
| <kbd>CTRL</kbd>+[<kbd>1</kbd>,<kbd>2</kbd>,<kbd>3</kbd>,<kbd>4</kbd>,<kbd>5</kbd>,<kbd>6</kbd>,<kbd>7</kbd>,<kbd>8</kbd>,<kbd>9</kbd>] | Quick open result `n`                        |
| <kbd>ALT</kbd>+[<kbd>1</kbd>,<kbd>2</kbd>,<kbd>3</kbd>,<kbd>4</kbd>,<kbd>5</kbd>,<kbd>6</kbd>,<kbd>7</kbd>,<kbd>8</kbd>,<kbd>9</kbd>]  | Quick open options of result `n`             |
| <kbd>ALT</kbd>/<kbd>ESC</kbd>                                                                                                          | View result options / go back                |
| <kbd>TAB</kbd>                                                                                                                         | Auto-complete query according to results     |
| <kbd>CTRL</kbd>+<kbd>ALT</kbd>+<kbd>V</kbd>                                                                                            | Paste an entry from your clipboard history   |

## Contributing

All contributions are valued and welcome. You may add new features but naturally also improve existing ones. Feel free to browse current [open issues](https://github.com/dkgv/pinpoint/issues?q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc) if you need inspiration.

Please create one branch + PR per major change you make.

Thanks 🙏
