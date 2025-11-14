using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using AdvancedTooltip.Settings;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;
using Vector2 = System.Numerics.Vector2;
using RectangleF = SharpDX.RectangleF;

namespace AdvancedTooltip;

public class AdvancedTooltip : BaseSettingsPlugin<AdvancedTooltipSettings>
{
    private Dictionary<int, Color> TColors;
    private FastModsModule _fastMods;
    private HoverItemIcon _hoverItemIcon;
    // Perf: cache parsed mods for currently hovered item
    private long _modsCacheItemAddr;
    private List<ModValue> _modsCache;
    // Perf: cache minimal affix label width once per session
    private float _affixTypeMinWidth = -1f;

    public override void OnLoad()
    {
        // No external textures required
    }

    public override bool Initialise()
    {
        _fastMods = new FastModsModule(Graphics, Settings.ItemMods);
        Logger.settings = Settings;
        Settings.ItemMods.FastModsAnchor.SetListValues(new List<string> { "Top", "Bottom" });
        if (Settings.ItemMods.FastModsAnchor.Value == null)
        {
            Settings.ItemMods.FastModsAnchor.Value = "Bottom";
        }
        TColors = new Dictionary<int, Color>
        {
            { 1, Settings.ItemMods.T1Color },
            { 2, Settings.ItemMods.T2Color },
            { 3, Settings.ItemMods.T3Color },
        };

        return true;
    }

    private void DumpStatNamesToClipboard(HoverItemIcon icon)
    {
        if (icon == null)
            return;

        var modsComponent = icon?.Item?.GetComponent<Mods>();
        if (modsComponent == null)
            return;

        string statNames = string.Empty;

        foreach (var mod in modsComponent.ItemMods)
        {
            var statName = mod.ModRecord?.StatNames?.FirstOrDefault()?.ToString() ?? "";
            if (!string.IsNullOrEmpty(statName))
                statNames += statName + "\n";
        }

        if (!string.IsNullOrEmpty(statNames))
        {
            Clipboard.SetText(statNames);
            DebugWindow.LogMsg("Hovered item matching stats copied to clipboard.");
        }
    }

    private void DumpModNamesToClipboard(HoverItemIcon icon)
    {
        if (icon == null)
            return;

        var modsComponent = icon?.Item?.GetComponent<Mods>();
        if (modsComponent == null)
            return;

        string modNames = string.Empty;

        foreach (var mod in modsComponent.ItemMods)
        {
            modNames += mod.RawName + "\n";
        }

        if (!string.IsNullOrEmpty(modNames))
        {
            Clipboard.SetText(modNames);
            DebugWindow.LogMsg("Hovered item mod names copied to clipboard.");
        }
    }

    public override Job Tick()
    {
        if (!Initialized) Initialise();

        var hoverItemIcon = GameController?.Game?.IngameState?.UIHover?.AsObject<HoverItemIcon>();
        if (hoverItemIcon != null && hoverItemIcon.IsValid)
        {
            _hoverItemIcon = hoverItemIcon;
        }
        else
        {
            _hoverItemIcon = null;
        }

        if (Settings.ItemMods.DumpStatNames.PressedOnce())
        {
            var thread = new Thread(new ParameterizedThreadStart(param => { DumpStatNamesToClipboard(_hoverItemIcon); }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        if (Settings.ItemMods.DumpModNames.PressedOnce())
        {
            var thread = new Thread(new ParameterizedThreadStart(param => { DumpModNamesToClipboard(_hoverItemIcon); }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        return null;
    }

    public override void Render()
    {
        if (string.IsNullOrEmpty(_hoverItemIcon?.Item?.Path))
        {
            return;
        }

        if (_hoverItemIcon is not { ToolTipType: not ToolTipType.None, ItemFrame: { } tooltip })
        {
            return;
        }

        var poeEntity = _hoverItemIcon.Item;
        if (poeEntity == null || poeEntity.Address == 0 || !poeEntity.IsValid)
        {
            return;
        }

        var modsComponent = poeEntity?.GetComponent<Mods>();
        var origTooltipRect = tooltip.GetClientRect();

        var itemMods = modsComponent?.ItemMods;

        if (itemMods == null ||
            itemMods.Any(x => string.IsNullOrEmpty(x.RawName) && string.IsNullOrEmpty(x.Name)))
            return;

        var origTooltipHeaderOffset = modsComponent.ItemRarity == ItemRarity.Rare || modsComponent.ItemRarity == ItemRarity.Unique
            ? (modsComponent.Identified ? 80 : 50)
            : 50;

        // Build or reuse ModValue list (PoE1-safe; optional UI parsing)
        var useUiTierParsing = Settings.ItemMods.UseUiTierParsing.Value;
        List<ModValue> mods;
        if (_modsCache != null && _modsCacheItemAddr == poeEntity.Address)
        {
            mods = _modsCache;
        }
        else
        {
            mods = itemMods.Select(item => new ModValue(
                    item,
                    GameController.Files,
                    modsComponent.ItemLevel,
                    GameController.Files.BaseItemTypes.Translate(poeEntity.Path),
                    modsComponent,
                    useUiTierParsing ? tooltip : null)
                ).ToList();
            _modsCacheItemAddr = poeEntity.Address;
            _modsCache = mods;
        }

        // Enhanced Tooltip Display
        if (Settings.ItemMods.EnableTooltip.Value && modsComponent.Identified && modsComponent.ItemRarity != ItemRarity.Normal)
        {
            var tooltipTop = origTooltipRect.Bottom + 5;
            var tooltipLeft = origTooltipRect.Left;
            var modPosition = new Vector2(tooltipLeft + 5, tooltipTop + 4);

            // Sort mods
            mods = mods
                .OrderBy(m => !m.IsImplicit)
                .ThenBy(m =>
                {
                    if (m.AffixType == ModType.Corrupted) return -1;
                    if (m.AffixType == ModType.Unique) return 0;
                    return (int)m.AffixType;
                })
                .ThenBy(m => Settings.ItemMods.SortModsByTier ? m.Tier : 0)
                .ThenBy(m =>
                {
                    if (Settings.ItemMods.SortModsByName)
                    {
                        return string.IsNullOrEmpty(m.ShortName) ? 1 : 0;
                    }
                    else
                    {
                        return 0;
                    }
                })
                .ThenBy(m => Settings.ItemMods.SortModsByName ? m.ShortName : null)
                .ToList();

            var height = mods.Where(x => x.Record.StatNames.Any(y => y != null))
                             .Aggregate(modPosition, (position, item) => DrawMod(item, position)).Y -
                         tooltipTop;

            if (height > 4)
            {
                var modsRect = new RectangleF(tooltipLeft, tooltipTop, origTooltipRect.Width, height);
                Graphics.DrawBox(modsRect, Settings.ItemMods.BackgroundColor);
            }
        }

        // Precompute tier counts for multiple features
        bool EligibleForCount(ModValue mv) =>
            mv.CouldHaveTiers() &&
            (mv.AffixType == ModType.Prefix || mv.AffixType == ModType.Suffix) &&
            !mv.IsImplicit &&
            !mv.IsCrafted;
        var t1 = mods.Count(item => EligibleForCount(item) && item.Tier == 1);
        var t2 = mods.Count(item => EligibleForCount(item) && item.Tier == 2);
        var t3 = mods.Count(item => EligibleForCount(item) && item.Tier == 3);

        // Mod Count Display
        if (Settings.ItemMods.EnableModCount)
        {
            var startPosition = new Vector2(origTooltipRect.TopLeft.X, origTooltipRect.TopLeft.Y);
            if (t1 + t2 + t3 > 0)
            {
                var tierNoteHeight = Graphics.MeasureText("T").Y * (Math.Sign(t1) + Math.Sign(t2) + Math.Sign(t3)) + 5;
                var width = Graphics.MeasureText("T1 x6").X + 10;
                Graphics.DrawBox(startPosition, startPosition + new Vector2(width, tierNoteHeight), Settings.ItemMods.BackgroundColor);
                Graphics.DrawFrame(startPosition, startPosition + new Vector2(width, tierNoteHeight), Color.Gray, 1);
                startPosition.X += 5;
                startPosition.Y += 2;
                
                if (t1 > 0)
                {
                    startPosition.Y += Graphics.DrawText($"T1 x{t1}", startPosition, Settings.ItemMods.T1Color).Y;
                }

                if (t2 > 0)
                {
                    startPosition.Y += Graphics.DrawText($"T2 x{t2}", startPosition, Settings.ItemMods.T2Color).Y;
                }

                if (t3 > 0)
                {
                    startPosition.Y += Graphics.DrawText($"T3 x{t3}", startPosition, Settings.ItemMods.T3Color).Y;
                }
            }
        }

        // Quick-glance visual highlight: colored frame based on highest tier present
        if (Settings.ItemMods.EnableItemHighlight)
        {
            Color frameColor = Color.Gray;
            if (t1 > 0) frameColor = Settings.ItemMods.T1Color;
            else if (t2 > 0) frameColor = Settings.ItemMods.T2Color;
            else if (t3 > 0) frameColor = Settings.ItemMods.T3Color;

            // Slightly thicker frame for visibility
            Graphics.DrawFrame(origTooltipRect, frameColor, 2);
        }

        // Grade badge in the top-right corner (S/A/B) for fast judgment
        if (Settings.ItemMods.EnableGradeBadge && (t1 + t2 + t3) > 0)
        {
            string grade = t1 > 0 ? "S" : t2 > 0 ? "A" : t3 > 0 ? "B" : string.Empty;
            if (!string.IsNullOrEmpty(grade))
            {
                var badgePadding = new Vector2(6, 2);
                var size = Graphics.MeasureText(grade);
                var box = new RectangleF(origTooltipRect.Right - size.X - badgePadding.X * 2 - 4,
                                         origTooltipRect.Top + 4,
                                         size.X + badgePadding.X * 2,
                                         size.Y + badgePadding.Y * 2);
                var gradeColor = t1 > 0 ? Settings.ItemMods.T1Color.Value : t2 > 0 ? Settings.ItemMods.T2Color.Value : Settings.ItemMods.T3Color.Value;
                Graphics.DrawBox(box, new Color(0,0,0,180));
                Graphics.DrawFrame(box, gradeColor, 1);
                var textPos = new Vector2(box.X + badgePadding.X, box.Y + badgePadding.Y);
                Graphics.DrawText(grade, textPos, gradeColor);
            }
        }

        // Item Level Display
        if (Settings.ItemLevel.Enable.Value)
        {
            var itemLevel = "iLVL: " + Convert.ToString(modsComponent?.ItemLevel ?? 0);
            var itemLevelPosition = new Vector2(origTooltipRect.TopLeft.X, origTooltipRect.TopLeft.Y + origTooltipHeaderOffset);
            var textSize = Graphics.MeasureText(itemLevel);
            var padding = new Vector2(6, 4);
            var boxRect = new RectangleF(itemLevelPosition.X, itemLevelPosition.Y,
                textSize.X + padding.X * 2, textSize.Y + padding.Y * 2);
            Graphics.DrawBox(boxRect, Settings.ItemLevel.BackgroundColor);
            Graphics.DrawFrame(boxRect, Color.Gray, 1);
            itemLevelPosition = itemLevelPosition.Translate(padding.X, padding.Y);
            Graphics.DrawText(itemLevel, itemLevelPosition, Settings.ItemLevel.TextColor);
        }

        // Weapon DPS Display removed

        // Fast Mods Display
        if (Settings.ItemMods.EnableFastMods &&
            (modsComponent == null ||
             modsComponent.ItemRarity == ItemRarity.Magic ||
             modsComponent.ItemRarity == ItemRarity.Rare))
        {
            _fastMods.DrawUiHoverFastMods(mods, origTooltipRect);
        }
    }

    private Vector2 DrawMod(ModValue item, Vector2 position)
    {
        const float epsilon = 0.001f;
        const int marginBottom = 4;
        var oldPosition = position;
        var settings = Settings.ItemMods;

        // Check if this is a crafted mod first
        if (item.IsCrafted)
        {
            var craftedColor = new Color(180, 96, 255);
            var craftedText = "[CRAFTED]";
            
            Graphics.DrawText(craftedText, position, craftedColor);
            
            // Display the mod text
            if (!string.IsNullOrEmpty(item.HumanName))
            {
                var craftedLabelWidth = Graphics.MeasureText(craftedText + " ").X;
                var displayText = item.HumanName;
                var txSize = Graphics.DrawText($" {displayText}", position.Translate(craftedLabelWidth, 0), Color.Gainsboro);
                position.Y += txSize.Y;
            }
            
            return Math.Abs(position.Y - oldPosition.Y) > 0.001f
                ? oldPosition with { Y = position.Y + marginBottom }
                : oldPosition;
        }

        var (affixTypeText, color) = item.AffixType switch
        {
            ModType.Prefix => ("[P]", settings.PrefixColor.Value),
            ModType.Suffix => ("[S]", settings.SuffixColor.Value),
            ModType.Corrupted => ("[COR]", new Color(220, 20, 60)),
            ModType.Unique => ("[U]", new Color(255, 140, 0)),
            ModType.Enchantment => ("[E]", new Color(255, 0, 255)),
            ModType.Nemesis => ("[NEM]", new Color(255, 20, 147)),
            ModType.BloodLines => ("[BLD]", new Color(0, 128, 0)),
            ModType.Torment => ("[TOR]", new Color(178, 34, 34)),
            ModType.Tempest => ("[TEM]", new Color(65, 105, 225)),
            ModType.Talisman => ("[TAL]", new Color(218, 165, 32)),
            ModType.EssenceMonster => ("[ESS]", new Color(139, 0, 139)),
            ModType.Bestiary => ("[BES]", new Color(255, 99, 71)),
            ModType.DelveArea => ("[DEL]", new Color(47, 79, 79)),
            ModType.SynthesisA => ("[SYN]", new Color(255, 105, 180)),
            ModType.SynthesisGlobals => ("[SGS]", new Color(186, 85, 211)),
            ModType.SynthesisBonus => ("[SYB]", new Color(100, 149, 237)),
            ModType.Blight => ("[BLI]", new Color(0, 100, 0)),
            ModType.BlightTower => ("[BLT]", new Color(0, 100, 0)),
            ModType.MonsterAffliction => ("[MAF]", new Color(123, 104, 238)),
            ModType.FlaskEnchantmentEnkindling => ("[FEE]", new Color(255, 165, 0)),
            ModType.FlaskEnchantmentInstilling => ("[FEI]", new Color(255, 165, 0)),
            ModType.ExpeditionLogbook => ("[LOG]", new Color(218, 165, 32)),
            ModType.ScourgeUpside => ("[SCU]", new Color(218, 165, 32)),
            ModType.ScourgeDownside => ("[SCD]", new Color(218, 165, 32)),
            ModType.ScourgeMap => ("[SCM]", new Color(218, 165, 32)),
            ModType.ExarchImplicit => ("[EXI]", new Color(255, 69, 0)),
            ModType.EaterImplicit => ("[EAT]", new Color(255, 69, 0)),
            ModType.WeaponTree => ("[CRU]", new Color(254, 114, 53)),
            ModType.WeaponTreeRecombined => ("[CRC]", new Color(254, 114, 53)),
            _ => ("[?]", new Color(211, 211, 211))
        };

        // Override for implicit mods
        if (item.IsImplicit)
        {
            affixTypeText = "[I]";
            color = new Color(218, 219, 193, 156);
        }

        var affixTypeWidth = Graphics.MeasureText(affixTypeText + " ").X;
        if (_affixTypeMinWidth < 0)
            _affixTypeMinWidth = Graphics.MeasureText("[P] ").X;
        affixTypeWidth = Math.Max(_affixTypeMinWidth, affixTypeWidth);

        Graphics.DrawText(affixTypeText, position, color);

        if (item.AffixType != ModType.Unique && item.AffixType != ModType.Corrupted && !item.IsCrafted)
        {
            var totalTiers = item.TotalTiers;
            Color affixTextColor = (item.AffixType, totalTiers > 1) switch
            {
                (ModType.Prefix, true) => TColors.GetValueOrDefault(item.Tier, settings.PrefixColor),
                (ModType.Suffix, true) => TColors.GetValueOrDefault(item.Tier, settings.SuffixColor),
                (ModType.Prefix, false) => settings.PrefixColor,
                (ModType.Suffix, false) => settings.SuffixColor,
                _ => default
            };

            var affixTierText = (totalTiers > 1 ? $"T{item.Tier} " : string.Empty);
            var affixTierSize = item.AffixType switch
            {
                ModType.Prefix => Graphics.DrawText(affixTierText, position.Translate(affixTypeWidth), affixTextColor),
                ModType.Suffix => Graphics.DrawText(affixTierText, position.Translate(affixTypeWidth), affixTextColor),
                _ => default
            };

            // Show short names if enabled
            if (Settings.ItemMods.ShowShortNames && item.ShortName.Length > 0)
            {
                affixTierSize.X += Graphics.DrawText($"{item.ShortName}", position.Translate(affixTypeWidth + affixTierSize.X), affixTextColor).X;
            }

            // Show full mod names if enabled
            var affixTextSize = Settings.ItemMods.ShowModNames
                ? Graphics.DrawText(
                    item.ShortName.Length > 0 || (affixTierSize.X > 0 && !item.CouldHaveTiers())
                    ? $" | \"{item.AffixText}\""
                    : $"\"{item.AffixText}\"",
                    position.Translate(affixTypeWidth + affixTierSize.X), affixTextColor)
                : Vector2.Zero;

            // Show tags if enabled
            var tagSize = Vector2.Zero;
            if (Settings.ItemMods.ShowTags && item.Tags.Count > 0)
            {
                var tagsPosition = Vector2.Zero;
                if (Settings.ItemMods.StartTagsOnSameLine)
                {
                    tagsPosition = new Vector2(position.X + affixTypeWidth + affixTierSize.X + affixTextSize.X, position.Y);
                    tagSize.X += Graphics.DrawText(" ", tagsPosition, affixTextColor).X;
                }
                else
                {
                    tagsPosition = new Vector2(position.X + affixTypeWidth, position.Y + Math.Max(affixTierSize.Y, affixTextSize.Y));
                }

                foreach (var tag in item.Tags)
                {
                    tagSize.X += Graphics.DrawText($"[{tag}] ", tagsPosition + tagSize, GetTagColor(tag)).X;
                }
                tagSize.Y = item.Tags.Count > 0 ? Graphics.MeasureText(item.Tags[0]).Y : 0;
                
                if (!Settings.ItemMods.StartTagsOnSameLine)
                {
                    position.Y += tagSize.Y;
                }
            }

            if (Settings.ItemMods.StartStatsOnSameLine)
            {
                position.X += affixTierSize.X + affixTextSize.X + (Settings.ItemMods.StartTagsOnSameLine ? tagSize.X : 0);
            }
            else
            {
                position.Y += Math.Max(affixTierSize.Y, affixTextSize.Y);
            }
        }

        // Display human-readable mod text with stats (always advance at least one line height to avoid overlaps)
        var lineHeight = Graphics.MeasureText("A").Y; // baseline line height
        if (!string.IsNullOrEmpty(item.HumanName))
        {
            var displayText = item.HumanName;
            var txSize = Graphics.DrawText(Settings.ItemMods.StartStatsOnSameLine ? $" {displayText}" : $"{displayText}",
                position.Translate(affixTypeWidth), Color.Gainsboro);
            position.Y += Math.Max(txSize.Y, lineHeight);
        }
        else if (!Settings.ItemMods.StartStatsOnSameLine)
        {
            // When not on the same line, still bump Y by one line to keep spacing consistent
            position.Y += lineHeight;
        }

        // Show stat names for debugging if enabled
        if (Settings.ItemMods.ShowStatNames)
        {
            var statName = item.Record.StatNames.FirstOrDefault()?.ToString() ?? "";
            if (!string.IsNullOrEmpty(statName))
            {
                var txSize = Graphics.DrawText(statName, position.Translate(affixTypeWidth), Color.Gray);
                position.Y += txSize.Y;
            }
        }

        // Final safety: ensure we always advance by at least one line height
        // This prevents overlaps when, for example, StartStatsOnSameLine is true and there's no HumanName/tags drawn.
        if (Math.Abs(position.Y - oldPosition.Y) < epsilon)
        {
            var minLine = Graphics.MeasureText("A").Y;
            position.Y = oldPosition.Y + minLine;
        }

        return oldPosition with { Y = position.Y + marginBottom };
    }

    private Color GetTagColor(string tag)
    {
        return tag switch
        {
            "Fire" => Color.Red,
            "Cold" => new Color(41, 102, 241),
            "Life" => Color.Magenta,
            "Lightning" => Color.Yellow,
            "Physical" => new Color(225, 170, 20),
            "Critical" => new Color(168, 220, 26),
            "Mana" => new Color(20, 240, 255),
            "Attack" => new Color(240, 100, 30),
            "Speed" => new Color(0, 255, 192),
            "Caster" => new Color(216, 0, 255),
            "Elemental" => Color.White,
            "Gem Level" => new Color(200, 230, 160),
            _ => Color.Gray
        };
    }
}