using ExileCore.Shared.Attributes;
using ExileCore.Shared.Nodes;

namespace AdvancedTooltip.Settings;

[Submenu(CollapsedByDefault = true)]
public class DebugSettings
{
    public ToggleNode ShowDebug { get; set; } = new ToggleNode(false);
}

