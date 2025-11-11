# AdvancedTooltip - Enhanced Edition (PoE1)

A modernized and feature-rich tooltip plugin for Path of Exile 1 using ExileAPI, with enhanced UX features backported from the PoE2 version.

## ✨ Features

### **Enhanced Mod Display**
- 🔢 **Tier Counter** - Visual T1/T2/T3 mod counter in corner with customizable scale
- 🏷️ **Short Names** - Quick mod identification (e.g., "Flat Phys", "Hybrid ES")
- 📊 **Mod Sorting** - Sort by tier quality or alphabetically
- 🎨 **Color-Coded Tags** - Element-specific mod tags (Fire, Cold, Life, etc.)
- 📝 **Human-Readable Format** - Clean stat display with embedded ranges

### **Tooltip Management**
- ⌨️ **Override Mode** - Hold hotkey (default: Left Shift) to overlay advanced tooltip on original
- 🔄 **Inverse Mode** - Flip the override behavior if preferred
- 🎯 **Smart Positioning** - Prevents HUD overlap with flexible placement

### **Weapon DPS Calculator**
- ⚔️ **Labeled Display** - Shows "pDPS", "eDPS", and "Total" clearly
- 🎚️ **Scale-Based Sizing** - Smooth scaling from 0.5x to 2x
- 🏆 **Always Full Quality** - Calculate DPS as if weapon has 20% quality
- 🎨 **Textured Backdrop** - Beautiful visual presentation

### **Item Level Display**
- 📍 **Enhanced Format** - Shows "iLVL: XX" with textured backdrop
- 📏 **Scalable** - Adjustable size with smooth scaling

### **Fast Mods**
- ⚡ **Quick Tier View** - Shows mod tiers directly next to tooltip
- ⬆️⬇️ **Anchor Positioning** - Choose Top or Bottom placement
- 🏷️ **Tag Display** - Optional mod tags in fast view

### **Developer Tools**
- 📋 **Clipboard Export** - Export mod names or stat names for filter creation
- 🐛 **Debug Mode** - Conditional logging for troubleshooting
- 📊 **Stat Names** - Show internal stat names when enabled

## 🎮 Installation

1. Download or clone this repository
2. Place the `AdvancedTooltip` folder in your ExileAPI `Plugins/Compiled/` directory
3. Restart ExileAPI or reload plugins
4. Configure settings via the ExileAPI menu

## ⚙️ Configuration

All settings are accessible through the ExileAPI settings menu:

### Item Mods Settings
- **Enable Tooltip** - Main toggle for enhanced tooltip
- **Sort Mods by Tier** - Best tiers shown first
- **Sort Mods by Name** - Alphabetical sorting
- **Show Short Names** - Display shorthand mod names
- **Show Full Names** - Display human-readable mod names
- **Show Tags** - Display element/type tags
- **Show Stat Names** - Debug: Show internal stat names
- **Enable Mod Count** - T1/T2/T3 counter box
- **Mod Count Size** - Scale factor (0.5-2x)
- **Enable Fast Mods** - Quick mod tier display
- **Fast Mods Anchor** - Top or Bottom placement
- **Override Tooltip Hotkey** - Key to overlay on original tooltip
- **Inverse Override** - Flip override behavior
- **Dump Mod Names** - Hotkey to copy mod names
- **Dump Stat Names** - Hotkey to copy stat names

### Weapon DPS Settings
- **Enable Weapon DPS** - Show DPS calculation
- **Always Full Quality** - Calculate with 20% quality
- **DPS Text Size** - Scale factor (0.5-2x)

### Item Level Settings
- **Enable** - Show item level
- **Text Size** - Scale factor (0.5-2x)

### Debug Settings
- **Show Debug** - Enable debug logging

## 🛠️ Technical Details

- **ExileAPI Version**: Compatible with ExileCore 327.8+
- **Game**: Path of Exile 1
- **Language**: C# (.NET)
- **Dependencies**: ExileCore, SharpDX

## 📋 Mod Groups Supported

The short names system includes 60+ common PoE1 mod groups:
- Physical Damage (Flat, %, Hybrid)
- Elemental Damage (Fire, Cold, Lightning)
- Defenses (Armour, Evasion, ES, Hybrid)
- Resistances (All elements)
- Life, Mana, Attributes
- Critical Strike, Attack Speed, Cast Speed
- And many more...

## 🔄 Changelog

### v2.0.0 - Enhanced Edition (2025-11-11)
- ✅ Backported PoE2 UX features to PoE1
- ✅ Added mod sorting system
- ✅ Added short names display
- ✅ Added tag system with color coding
- ✅ Added tooltip override mode
- ✅ Added scale-based UI sizing
- ✅ Added textured backdrops
- ✅ Enhanced weapon DPS display
- ✅ Added developer clipboard export tools
- ✅ Simplified FastMods implementation
- ✅ Added comprehensive debug system

## 🙏 Credits

This plugin builds upon the original AdvancedTooltip concept with significant enhancements and modernization, incorporating UX improvements inspired by the PoE2 version.

## 📝 License

This project is provided as-is for the Path of Exile community. Use and modify as needed.

## 🐛 Issues & Contributions

If you encounter any issues or have suggestions for improvements, feel free to open an issue or submit a pull request.

## ⚠️ Disclaimer

This is a third-party tool for Path of Exile. Use at your own risk. Always ensure you comply with the game's Terms of Service.
