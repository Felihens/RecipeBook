# Recipe Book

A client-side BepInEx mod for Valheim. A read-only book of every recipe you have unlocked: cost, station, upgrades, newest first, filters by stage, metal, item type and material. Client-side.

Mod para Valheim (BepInEx, client-side).

For players, see [package/README.md](package/README.md). That file is also the Thunderstore page.
Para jogadores, veja [package/README.md](package/README.md).

Download: [Thunderstore](https://thunderstore.io/c/valheim/p/DJAventuras/RecipeBook/) · [Nexus Mods](https://www.nexusmods.com/valheim/mods/4955) · [GitHub Releases](https://github.com/Felihens/RecipeBook/releases)

## Building

`sh build.sh` compiles `src/*.cs` against the game's assemblies and installs the dll into `BepInEx/plugins/RecipeBook/` (`sh build.sh nocopy` only compiles). Then `powershell -File pack.ps1` builds `dist/RecipeBook-<version>.zip` from `package/`.

## License

MIT
