iiBrightRespite
=========

iiBrightRespite is a C# library supporting the modification of files relating to Dark Reign, the 1997 RTS game developed by Auran.

| Name   | Read | Write | Comment
|--------|:----:|-------|--------
| ACT    | ✔   |   ✗   |
| AIP    | ✗   |   ✗   | Plain text
| BAK    | ✗   |   ✗   | Plain text
| BMP    | ✗   |   ✗   | Standard bitmap
| BRF    | ✗   |   ✗   | Plain text
| BTM    | ✗   |   ✗   | Plain text
| CFG    | ✗   |   ✗   | Plain text
| CNT    | ✗   |   ✗   | Plain text
| CRS    | 🟢   |   ✗   | Cursors - need to identify palette
| DOC    | ✗   |   ✗   | Plain text
| END    | ✗   |   ✗   | Plain text
| FOG    | ✗   |   ✗   |
| FTG    | ✔   |   ✗   |
| FSM    | ✔   |   ✗   |
| H      | ✗   |   ✗   | Plain text
| MAP    | ✗   |   ✗   |
| MM     | ✗   |   ✗   |
| PAL    | ✗   |   ✗   |
| PCX    | ✗   |   ✗   |
| PSD    | ✗   |   ✗   |
| RLD    | ✗   |   ✗   |
| RLI    | ✗   |   ✗   |
| SAF    | ✗   |   ✗   | Plain text
| SCN    | ✗   |   ✗   | Plain text
| SMK    | ✗   |   ✗   |
| SPR(R) | 🟢   |   ✗   |
| SPR(S) | 🟢   |   ✗   |
| TIL    | ✗   |   ✗   |
| TXT    | ✗   |   ✗   |

## Usage

```csharp
var ftgProcessor = new FtgProcessor();
ftgProcessor.Open(@"D:\Games\Dark Reign\dark\graphics\sprites.ftg");
ftgProcessor.Parse();
for (int i = 0; i < ftgProcessor.Files.Count; i++)
{
    ftgProcessor.Extract(Path.Combine(@"D:\data\darkreign\", ftgProcessor.Files[i].Filename.Split('\0')[0]), ftgProcessor.Files[i]);
}
ftgProcessor.Close();
```

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/iiBrightRespite

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

iiBrightRespite is licenced under the MIT License. Full licence details are available in licence.md