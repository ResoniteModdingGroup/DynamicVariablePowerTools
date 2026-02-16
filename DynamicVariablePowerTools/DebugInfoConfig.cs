using MonkeyLoader.Configuration;

namespace DynamicVariablePowerTools
{
    internal sealed class DebugInfoConfig : ConfigSection
    {
        private static readonly DefiningConfigKey<bool> _enableLinkedComponentHierarchy = new("EnableLinkedComponentHierarchy", "Allow generating a hierarchical list of all dynamic variable components linked to a space.", () => true);
        private static readonly DefiningConfigKey<bool> _enableLinkedVariablesList = new("EnableLinkedVariablesList", "Allow generating a list of all dynamic variable definitions linked to a space.", () => true);

        public override string Description => "Contains the options for the available debug info buttons on DynamicVariableSpace Components.";

        public bool EnableLinkedComponentHierarchy => _enableLinkedComponentHierarchy;

        public bool EnableLinkedVariablesList => _enableLinkedVariablesList;

        public override string Id => "DebugInfo";

        public override Version Version { get; } = new Version(1, 0, 0);
    }
}