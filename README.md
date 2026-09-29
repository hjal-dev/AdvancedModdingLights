# SPT Advanced Modding Lights

A BepInEx client plugin for SPT (Single Player Tarkov) that improves the lighting in the weapon modding and build screens, so it's easier and nicer to see what you're working on.

**Authors:** Hj & MoxoPixel

## Features

- **Enhanced Lighting**: Improves lighting in weapon modding and build screens
- **Configurable Settings**: Adjust directional light brightness and rotation
- **Real-time Updates**: Settings apply immediately without restart
- **Performance Optimized**: Efficient light caching and management

## Compatibility

| Mod version | SPT version |
| --- | --- |
| 2.0.0 | 4.1.x |
| 1.0.0 | 3.x |

Client-only. No server mod is needed.

## Installation

1. Download the latest release
2. Put `MoxoPixel-AdvancedModdingLights.dll` in `BepInEx/plugins/` in your SPT folder
3. Launch the game and configure settings via F12 (Configuration Manager)

## Configuration

Access settings through the Configuration Manager (F12 in-game):

- **Enable Custom Lighting**: Toggle enhanced lighting on/off
- **Brightness**: Control directional light intensity (0.1 - 5.0)
- **Rotation X/Y**: Adjust light direction (-180° to 180°)

## Building

The project is an SDK-style `netstandard2.1` library that references the game, BepInEx and SPT assemblies straight from an SPT 4.1 install. It defaults to `E:\SPT 4.1`. Point it at your own install with:

```
dotnet build -c Release -p:SPTPath="C:\path\to\SPT"
```

The DLL ends up in `bin/Release/`.

## Requirements

- SPT 4.1.x
- BepInEx 5.x (ships with SPT)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

- **MoxoPixel** - original mod (© 2025)
- **Hj** - SPT 4.1 update

Feel free to modify and distribute, just give credit where it's due! 🎯
