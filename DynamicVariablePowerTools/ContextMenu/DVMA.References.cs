using FrooxEngine;
using MonkeyLoader.Resonite;

using GenerationEvent = MonkeyLoader.Resonite.UI.Inspectors.InspectorMemberActionsMenuItemsGenerationEvent;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed partial class DynamicVariableMemberActions
    {
        private static ButtonEventHandler GetDriveSyncRefFromVariable<T>(GenerationEvent eventData, SyncRef<T> syncRefTarget, string variable)
            where T : class, IWorldElement
            => (button, args) =>
            {
                syncRefTarget.DriveFromVariable(variable);
                eventData.CloseContextMenu();
            };

        private static ButtonEventHandler GetOfferSyncRefDriveActions<T>(GenerationEvent eventData, SyncRef<T> syncRefTarget)
            where T : class, IWorldElement
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromBlank"), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveSyncRefFromVariable(eventData, syncRefTarget, string.Empty);

                    foreach (var variable in GetAvailableVariableOptions<T>(eventData.Slot!))
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromVariable", "variable", variable), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetDriveSyncRefFromVariable(eventData, syncRefTarget, variable);
                    }
                });
            };

        private static ButtonEventHandler GetOfferSyncRefReferenceActions<T>(GenerationEvent eventData, SyncRef<T> syncRefTarget)
            where T : class, IWorldElement
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference.Blank"), ReferenceIcon, ReferenceColor)
                        .Button.LocalPressed += GetReferenceSyncRefForVariable(eventData, syncRefTarget, string.Empty);

                    var spaces = eventData.Slot!
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference.InSpace", "space", space.SpaceName), ReferenceIcon, ReferenceColor)
                            .Button.LocalPressed += GetReferenceSyncRefForVariable(eventData, syncRefTarget, $"{space.SpaceName}/");
                    }
                });
            };

        private static ButtonEventHandler GetOfferSyncRefSourceActions<T>(GenerationEvent eventData, SyncRef<T> syncRefTarget)
            where T : class, IWorldElement
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.Blank"), SourceIcon, SourceColor)
                        .Button.LocalPressed += GetSourceSyncRefForVariable(eventData, syncRefTarget, string.Empty);

                    var spaces = eventData.Slot!
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.InSpace", "space", space.SpaceName), SourceIcon, SourceColor)
                            .Button.LocalPressed += GetSourceSyncRefForVariable(eventData, syncRefTarget, $"{space.SpaceName}/");
                    }
                });
            };

        private static ButtonEventHandler GetReferenceSyncRefForVariable<T>(GenerationEvent eventData, ISyncRef<T> syncRefTarget, string variable)
            where T : class, IWorldElement
            => (button, args) =>
            {
                var dynamicReference = syncRefTarget.FindNearestParent<Slot>().AttachComponent<DynamicReferenceVariable<ISyncRef<T>>>();
                dynamicReference.VariableName.Value = variable;
                dynamicReference.Reference.Target = syncRefTarget;

                eventData.CloseContextMenu();
            };

        private static ButtonEventHandler GetSourceSyncRefForVariable<T>(GenerationEvent eventData, SyncRef<T> syncRefTarget, string variable)
            where T : class, IWorldElement
            => (button, args) =>
            {
                syncRefTarget.SyncWithVariable(variable);
                eventData.CloseContextMenu();
            };

        private static void OfferSyncRefActions<T>(GenerationEvent eventData)
            where T : class, IWorldElement
        {
            if (eventData.Target is not SyncRef<T> syncRefTarget)
                return;

            if (!syncRefTarget.IsLinked)
            {
                eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive"), DriveIcon, DriveColor)
                    .Button.LocalPressed += GetOfferSyncRefDriveActions(eventData, syncRefTarget);
            }

            eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicReference"), SourceIcon, SourceColor)
                .Button.LocalPressed += GetOfferSyncRefSourceActions(eventData, syncRefTarget);

            eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference"), ReferenceIcon, ReferenceColor)
                .Button.LocalPressed += GetOfferSyncRefReferenceActions(eventData, syncRefTarget);
        }
    }
}