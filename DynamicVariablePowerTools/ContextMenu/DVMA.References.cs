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

        private static ButtonEventHandler GetOfferSyncRefDriveActions<T>(GenerationEvent eventData, SyncRef<T> syncRefTarget, DynamicVariableSpace space)
            where T : class, IWorldElement
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    var blankVariableName = string.IsNullOrWhiteSpace(space.CurrentName) ? string.Empty : $"{space.CurrentName}/";

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromVariable", "variable", GetDisplayName(space, "")), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveSyncRefFromVariable(eventData, syncRefTarget, blankVariableName);

                    foreach (var variable in space.GetVariableIdentities<T>().RemoveSharedConfigVariables())
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromVariable", "variable", GetDisplayName(variable)), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetDriveSyncRefFromVariable(eventData, syncRefTarget, variable.QualifiedName);
                    }
                });
            };

        private static ButtonEventHandler GetOfferSyncRefDriveSpaceActions<T>(GenerationEvent eventData, SyncRef<T> syncRefTarget)
            where T : class, IWorldElement
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromBlank"), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveSyncRefFromVariable(eventData, syncRefTarget, string.Empty);

                    foreach (var space in eventData.Slot!.GetAvailableSpaces())
                    {
                        var spaceName = string.IsNullOrWhiteSpace(space.CurrentName) ? "<color=neutrals.midlight><i>null</i></color>" : space.CurrentName;

                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromSpace", "space", spaceName), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetOfferSyncRefDriveActions(eventData, syncRefTarget, space);
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

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference.Blank"), ReferenceIcon, ReferenceColor)
                        .Button.LocalPressed += GetReferenceSyncRefForVariable(eventData, syncRefTarget, string.Empty);

                    var spaces = eventData.Slot!
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference.InSpace", "space", space.SpaceName), ReferenceIcon, ReferenceColor)
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

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source.Blank"), SourceIcon, SourceColor)
                        .Button.LocalPressed += GetSourceSyncRefForVariable(eventData, syncRefTarget, string.Empty);

                    var spaces = eventData.Slot!
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source.InSpace", "space", space.SpaceName), SourceIcon, SourceColor)
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
                eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive"), DriveIcon, DriveColor)
                    .Button.LocalPressed += GetOfferSyncRefDriveSpaceActions(eventData, syncRefTarget);
            }

            eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source", "type", "DynamicReference"), SourceIcon, SourceColor)
                .Button.LocalPressed += GetOfferSyncRefSourceActions(eventData, syncRefTarget);

            eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference"), ReferenceIcon, ReferenceColor)
                .Button.LocalPressed += GetOfferSyncRefReferenceActions(eventData, syncRefTarget);
        }
    }
}