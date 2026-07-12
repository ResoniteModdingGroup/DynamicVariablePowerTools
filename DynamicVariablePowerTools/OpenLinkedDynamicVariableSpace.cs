using FrooxEngine;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace DynamicVariablePowerTools
{
    internal sealed class OpenLinkedDynamicVariableSpace
        : ResoniteInspectorMonkey<OpenLinkedDynamicVariableSpace, BuildInspectorHeaderEvent>
    {
        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.First;

        // Can't use generic parameter version because IDynamicVariable isn't a Worker
        public OpenLinkedDynamicVariableSpace() : base(typeof(IDynamicVariable))
        { }

        protected override void Handle(BuildInspectorHeaderEvent eventData)
        {
            // Can safely cast since the base.AppliesTo method will ensure it's an IDynamicVariable
            if (!((IDynamicVariable)eventData.Worker).TryGetLinkedSpace(out var space))
                return;

            InspectorUIHelper.BuildHeaderOpenParentButtons(eventData.UI, space);
        }
    }
}