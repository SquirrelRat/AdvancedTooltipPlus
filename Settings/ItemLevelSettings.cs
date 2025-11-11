using ExileCore.Shared.Attributes;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace AdvancedTooltip.Settings;

[Submenu]
public class ItemLevelSettings
{
    public ToggleNode Enable { get; set; } = new(true);

    [Menu("Text Size", "Scale factor for item level text")]
    public RangeNode<float> TextSize { get; set; } = new RangeNode<float>(1, 0.5f, 2f);
    
    public ColorNode TextColor { get; set; } = new ColorBGRA(0, 255, 255, 255);
    public ColorNode BackgroundColor { get; set; } = new ColorBGRA(255, 255, 255, 150);
}