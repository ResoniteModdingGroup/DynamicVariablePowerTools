using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;

namespace DynamicVariablePowerTools
{
    internal sealed class RenameConfig : SingletonConfigSection<RenameConfig>
    {
        private readonly DefiningConfigKey<bool> _changeProtoFluxStringInputsKey = new("ChangeProtoFluxStringInputs", "Search and rename ProtoFlux inputs with the old name in the form OldName/* (Experimental).", () => false);

        private readonly DefiningConfigKey<bool> _showRenameDynamicVariableKey = new("ShowRenameDynamicVariable", "Show rename action on dynamic variable components.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        private readonly DefiningConfigKey<bool> _showRenameDynamicVariableSpaceKey = new("ShowRenameDynamicVariableSpace", "Show rename action on DynamicVariableSpace components.", () => true)
        {
            new ConfigKeySessionShare<bool>(true)
        };

        /// <summary>
        /// Gets whether to search and rename ProtoFlux inputs with the old name in the form OldName/* (Experimental).
        /// </summary>
        public bool ChangeProtoFluxStringInputs => _changeProtoFluxStringInputsKey;

        /// <inheritdoc/>
        public override string Description => "Rename Options";

        /// <inheritdoc/>
        public override string Id => "RenameOptions";

        /// <summary>
        /// Gets whether to show the rename action on dynamic variable components.
        /// </summary>
        public ConfigKeySessionShare<bool> ShowRenameDynamicVariable => _showRenameDynamicVariableKey.Components.Get<ConfigKeySessionShare<bool>>();

        /// <summary>
        /// Gets whether to show the rename action on DynamicVariableSpace components.
        /// </summary>
        public ConfigKeySessionShare<bool> ShowRenameDynamicVariableSpace => _showRenameDynamicVariableSpaceKey.Components.Get<ConfigKeySessionShare<bool>>();

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 0);
    }
}