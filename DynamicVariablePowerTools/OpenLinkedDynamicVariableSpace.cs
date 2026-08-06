using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace DynamicVariablePowerTools
{
    internal sealed class OpenLinkedDynamicVariableSpace
        : ResoniteInspectorMonkey<OpenLinkedDynamicVariableSpace, BuildInspectorHeaderEvent>
    {
        private readonly ConfigKeySessionShare<bool> _enabledShare = new(true);

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.First;

        // Can't use generic parameter version because IDynamicVariable isn't a Worker
        public OpenLinkedDynamicVariableSpace() : base(typeof(IDynamicVariable))
        { }

        // Exclude Enabled check to always generate, but use session share for visibility
        protected override bool AppliesTo(BuildInspectorHeaderEvent eventData)
            => eventData.Worker is IDynamicVariable;

        protected override void Handle(BuildInspectorHeaderEvent eventData)
        {
            // Can safely cast since the AppliesTo method will ensure it's an IDynamicVariable
            if (!((IDynamicVariable)eventData.Worker).TryGetLinkedSpace(out var space))
                return;

            var horizontalLayout = eventData.UI.HorizontalLayout(4).Slot;
            eventData.UI.FitContent(SizeFit.MinSize, SizeFit.Disabled);

            _enabledShare.DriveFromVariable(horizontalLayout.ActiveSelf_Field);
            DebugInfoConfig.Instance.OpenLinkedSpaceOffset.DriveFromVariable(horizontalLayout.OrderOffset_Field);

            InspectorUIHelper.BuildHeaderOpenParentButtons(eventData.UI, space);

            eventData.UI.NestOut();
        }

        protected override bool OnEngineReady()
        {
            ((IEntity<IDefiningConfigKey<bool>>)EnabledToggle!).Components.Add(_enabledShare);

            return base.OnEngineReady();
        }
    }
}