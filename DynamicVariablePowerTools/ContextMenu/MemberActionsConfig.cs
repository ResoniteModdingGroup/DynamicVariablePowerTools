using MonkeyLoader.Configuration;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed class MemberActionsConfig : ConfigSection
    {
        private readonly DefiningConfigKey<bool> _allowDowncastToDriveKey = new("AllowDowncastToDrive", "Allows downcasting the type of the dynamic variable to the type required by the target reference.", () => true);
        private readonly DefiningConfigKey<bool> _allowUpcastToDriveKey = new("AllowUpcastToDrive", "Allows upcasting the type of the dynamic variable to the type required by the target reference.", () => false);

        /// <summary>
        /// Gets whether to allow downcasting the type of the dynamic variable's value to the type required by the target field.
        /// </summary>
        public bool AllowDowncastToDrive => _allowDowncastToDriveKey;

        /// <summary>
        /// Gets whether to allow upcasting the type of the dynamic variable's value to the type required by the target field.
        /// </summary>
        public bool AllowUpcastToDrive => _allowUpcastToDriveKey;

        /// <inheritdoc/>
        public override string Id => "MemberActions";

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 0, 0);
    }
}