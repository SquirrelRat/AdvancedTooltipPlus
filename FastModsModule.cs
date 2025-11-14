using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdvancedTooltip.Settings;
using ExileCore;
using ExileCore.Shared.Enums;
using SharpDX;
using Vector2 = System.Numerics.Vector2;
using RectangleF = SharpDX.RectangleF;

namespace AdvancedTooltip;

// Shows the suffix/prefix tier directly near mod on hover item
public class FastModsModule
{
    private readonly Graphics _graphics;
    private readonly ItemModsSettings _modsSettings;
    private readonly List<ModTierInfo> _mods = new List<ModTierInfo>();

    public FastModsModule(Graphics graphics, ItemModsSettings modsSettings)
    {
        _graphics = graphics;
        _modsSettings = modsSettings;
    }

    // PoE1-stable path: use parsed ModValue list (no UI Element dependency)
    public void DrawUiHoverFastMods(List<ModValue> mods, RectangleF tooltipRect)
    {
        try
        {
            var modTiers = InitializeElements(mods);
            if (modTiers.Count == 0) return;

            _mods.Clear();
            _mods.AddRange(modTiers);

            var height = _graphics.MeasureText("P1").Y * 1.5f;
            var fastModsHeight = height * _mods.Count;

            var drawPos = new Vector2(tooltipRect.X - 6, tooltipRect.TopLeft.Y);
            if (_modsSettings.FastModsAnchor.Value == "Bottom")
            {
                drawPos.Y = tooltipRect.BottomLeft.Y - fastModsHeight;
            }

            for (var i = 0; i < _mods.Count; i++)
            {
                var modTierInfo = _mods[i];
                var boxHeight = height * modTierInfo.ModLines;

                var textPos = drawPos.Translate(0, boxHeight / 2);

                var textSize = _graphics.DrawText(modTierInfo.DisplayName, textPos, modTierInfo.Color,
                    FontAlign.Right | FontAlign.VerticalCenter);

                textSize.X += 5;
                textPos.X -= textSize.X + 5;

                var rectangleF = new RectangleF(drawPos.X - textSize.X, drawPos.Y, textSize.X + 6,
                    height * modTierInfo.ModLines);
                _graphics.DrawBox(rectangleF, Color.Black);
                _graphics.DrawFrame(rectangleF, Color.Gray, 1);

                drawPos.Y += boxHeight;
                i += modTierInfo.ModLines - 1;
            }
        }
        catch
        {
            //ignored   
        }
    }

    private List<ModTierInfo> InitializeElements(List<ModValue> modValues)
    {
        var modTierInfo = new List<ModTierInfo>();
        foreach (var mod in modValues.OrderBy(m => m.AffixType).ThenBy(m => m.Tier))
        {
            // Skip implicits, uniques, corrupted, and crafted mods for FastMods
            if (mod.AffixType == ModType.Unique || mod.AffixType == ModType.Corrupted || mod.IsImplicit || mod.IsCrafted)
            {
                continue;
            }

            // Skip if not a prefix or suffix
            if (mod.AffixType != ModType.Prefix && mod.AffixType != ModType.Suffix)
            {
                continue;
            }

            string affix = string.Empty;
            Color color = Color.White;

            if (mod.AffixType == ModType.Prefix)
            {
                affix = "P";
                color = _modsSettings.PrefixColor;
            }
            else if (mod.AffixType == ModType.Suffix)
            {
                affix = "S";
                color = _modsSettings.SuffixColor;
            }

            // Color by tier
            color = mod.Tier switch
            {
                1 => _modsSettings.T1Color,
                2 => _modsSettings.T2Color,
                3 => _modsSettings.T3Color,
                _ => color
            };

            // Add tier to display if it exists
            if (mod.Tier > 0)
            {
                affix += mod.Tier;
            }
            else
            {
                affix += "?";
            }

            var currentModTierInfo = new ModTierInfo(affix, color);
            modTierInfo.Add(currentModTierInfo);
        }
        return modTierInfo;
    }

    private class ModTierInfo
    {
        public ModTierInfo(string displayName, Color color)
        {
            DisplayName = displayName;
            Color = color;
        }

        public string DisplayName { get; }
        public Color Color { get; }
        /// <summary>Mean twinned mod</summary>
        public int ModLines { get; set; } = 1;
    }
}