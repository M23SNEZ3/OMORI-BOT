<p>
    <img src="OmoriBot.png">
</p>
<a href="https://github.com/Remora/Remora.Discord"><img src="https://img.shields.io/badge/powered_by-Remora.Discord-blue"></img></a>
<a href="https://github.com/M23SNEZ3/OMORI-BOT/blob/master/LICENSE"><img src="https://img.shields.io/github/license/M23SNEZ3/OMORI-BOT?logo=git"></img></a>
<a href="https://github.com/M23SNEZ3/OMORI-BOT/commit/master"><img src="https://img.shields.io/github/last-commit/M23SNEZ3/OMORI-BOT?logo=github"></img></a>

### Welcome to WHITE SPACE. You have been living here for as long as you can remember.
OMORI-BOT it's general use bot for fun and moderation, written by [M23SNEZ3](https://github.com/M23SNEZ3) in C# using [Remora.Discord](https://github.com/Remora/Remora.Discord)

## What can the OMORI-BOT do?
* Banning, clear messages, kicking, etc.
* Remember member birthday
* Whitelist, with the help of which you can configure permissions to use commands

## Building OMORI-BOT
How building OMORI-BOT? Firtly, you must create the application on [Discord Developer Portal](https://discord.com/developers/applications), click on "New application", open your application and find the Bot tab, then generate and copy its token.
Secondly, you must install [.NET9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

### Linux/MacOS
`using dotnet run`
```bash
git clone https://github.com/M23SNEZ3/OMORI-BOT.git
cd OMORI-BOT
```
After that
```bash
dotnet run BOT_TOKEN='ENTER_TOKEN'
```

`using dotnet build`
```bash
git clone https://github.com/M23SNEZ3/OMORI-BOT.git
cd OMORI-BOT
dotnet build
cd OMORI-BOT/bin/Debug/net9.0
export BOT_TOKEN='ENTER_TOKEN'
./OMORI-BOT
```
### Windows
`using dotnet run`
```bash
git clone https://github.com/M23SNEZ3/OMORI-BOT.git
cd OMORI-BOT
```
After that
```bash
dotnet run BOT_TOKEN='ENTER_TOKEN'
```

`using dotnet build`
```bash
git clone https://github.com/M23SNEZ3/OMORI-BOT.git
cd OMORI-BOT
dotnet build
cd OMORI-BOT\bin\Debug\net9.0
$Env:BOT_TOKEN='ENTER_TOKEN'
.\OMORI-BOT.exe
```

## Special Thanks


![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg)


[JetBrains](https://www.jetbrains.com/), the creators of [ReSharper](https://www.jetbrains.com/resharper) 
and [Rider](https://www.jetbrains.com/rider), support OMORI-BOT 
with one of their [Open Source Licenses](https://jb.gg/OpenSourceSupport). 
Rider is the recommended IDE for working with OMORI-BOT, and the entire team uses it.
