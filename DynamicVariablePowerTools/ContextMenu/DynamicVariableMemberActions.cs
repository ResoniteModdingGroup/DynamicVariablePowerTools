using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.Configuration;
using System.Reflection;

using GenerationEvent = MonkeyLoader.Resonite.UI.Inspectors.InspectorMemberActionsMenuItemsGenerationEvent;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed partial class DynamicVariableMemberActions
        : ResoniteAsyncEventHandlerMonkey<DynamicVariableMemberActions, GenerationEvent>
    {
        private static readonly Dictionary<Type, Action<GenerationEvent>> _actionOfferersByType = new()
        {
            { typeof(Type), AccessTools.MethodDelegate<Action<GenerationEvent>>(AccessTools.DeclaredMethod(typeof(DynamicVariableMemberActions), nameof(CreateTypeFieldItems))) }
        };

        private static readonly MethodInfo _offerFieldActionsMethod = AccessTools.DeclaredMethod(typeof(DynamicVariableMemberActions), nameof(OfferFieldActions));
        private static readonly MethodInfo _offerSyncRefActionsMethod = AccessTools.DeclaredMethod(typeof(DynamicVariableMemberActions), nameof(CreateSyncRefItems));

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Normal;

        private static colorX DriveColor => RadiantUI_Constants.Sub.PURPLE;
        private static Uri DriveIcon => OfficialAssets.Graphics.Icons.ProtoFlux.Drive;

        private static colorX ReferenceColor => RadiantUI_Constants.Neutrals.LIGHT;
        private static Uri ReferenceIcon => OfficialAssets.Graphics.Icons.ProtoFlux.Reference;

        private static colorX SourceColor => RadiantUI_Constants.Sub.CYAN;
        private static Uri SourceIcon => OfficialAssets.Graphics.Icons.ProtoFlux.Source;

        protected override bool AppliesTo(GenerationEvent eventData)
            // Check for existence of Slot to filter out fields on UserComponents etc.
            => base.AppliesTo(eventData) && eventData.Slot is not null && eventData.Target is IField;

        protected override Task Handle(GenerationEvent eventData)
        {
            Action<GenerationEvent>? offerActions;

            // Check ISyncRef first because those are IField<RefID>
            if (eventData.Target is ISyncRef syncRef)
            {
                if (!_actionOfferersByType.TryGetValue(syncRef.TargetType, out offerActions))
                {
                    offerActions = MakeMethod(_offerSyncRefActionsMethod, syncRef.TargetType);
                    _actionOfferersByType.Add(syncRef.TargetType, offerActions);
                }
            }
            // This includes SyncType fields, since they're derived from SyncField<Type> and thus IField<Type>
            else if (eventData.Target is IField field)
            {
                if (!_actionOfferersByType.TryGetValue(field.ValueType, out offerActions))
                {
                    offerActions = MakeMethod(_offerFieldActionsMethod, field.ValueType);
                    _actionOfferersByType.Add(field.ValueType, offerActions);
                }
            }
            else
            {
                Logger.Warn(() => $"Tried to create inspector member action items for unsupported target: {eventData.Target.GetType().CompactDescription()}");
                return Task.CompletedTask;
            }

            offerActions(eventData);

            return Task.CompletedTask;
        }

        private static IEnumerable<string> GetAvailableVariableOptions<T>(Slot slot)
        {
            foreach (var identity in slot.GetAvailableVariableIdentities<T>())
            {
                if (identity.Name.StartsWith(SharedConfig.Identifier))
                    continue;

                yield return !string.IsNullOrWhiteSpace(identity.Space.CurrentName)
                    ? $"{identity.Space.CurrentName}/{identity.Name}"
                    : identity.Name;
            }
        }

        private static Action<GenerationEvent> MakeMethod(MethodInfo method, Type type)
        {
            method = method.MakeGenericMethod(type);
            return AccessTools.MethodDelegate<Action<GenerationEvent>>(method);
        }

        private static bool SpaceHasName(DynamicVariableSpace space)
            => !string.IsNullOrEmpty(space.SpaceName.Value);
    }
}