using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace DynamicVariablePowerTools
{
    internal sealed class DebugInfoConfig : SingletonConfigSection<DebugInfoConfig>
    {
        private readonly DefiningConfigKey<bool> _enableLinkedComponentHierarchy = new("EnableLinkedComponentHierarchy", "Allow generating a hierarchical list of all dynamic variable components linked to a space.", () => true);
        private readonly DefiningConfigKey<bool> _enableLinkedVariablesList = new("EnableLinkedVariablesList", "Allow generating a list of all dynamic variable definitions linked to a space.", () => true);

        private readonly DefiningConfigKey<int> _openLinkedSpaceOffset = new("OpenLinkedSpaceOffset", "The Order Offset of the buttons to open the linked dynamic variable space on Inspector Headers of dynamic variable components. Higher is further right.", () => 0)
        {
            DefaultInspectorHeaderConfig.OffsetRange,
            DefaultInspectorHeaderConfig.MakeOffsetRangeShare(0)
        };

        public override string Description => "Contains the options for the available debug info buttons on DynamicVariableSpace Components.";

        public bool EnableLinkedComponentHierarchy => _enableLinkedComponentHierarchy;

        public bool EnableLinkedVariablesList => _enableLinkedVariablesList;

        public override string Id => "DebugInfo";

        public ConfigKeySessionShare<int, long> OpenLinkedSpaceOffset => _openLinkedSpaceOffset.Components.Get<ConfigKeySessionShare<int, long>>();

        public override Version Version { get; } = new Version(1, 0, 0);
    }
}