using FrooxEngine;
using MonkeyLoader.Resonite;

using GenerationEvent = MonkeyLoader.Resonite.UI.Inspectors.InspectorMemberActionsMenuItemsGenerationEvent;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed partial class DynamicVariableMemberActions
    {
        private static ButtonEventHandler GetDriveTypeFieldFromVariable(GenerationEvent eventData, SyncType syncTypeTarget, string variable)
             => (button, args) =>
             {
                 syncTypeTarget.DriveFromVariable(variable);
                 eventData.CloseContextMenu();
             };

        private static ButtonEventHandler GetOfferTypeFieldDriveActions(GenerationEvent eventData, SyncType syncTypeTarget, DynamicVariableSpace space)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    var blankVariableName = string.IsNullOrWhiteSpace(space.CurrentName) ? string.Empty : $"{space.CurrentName}/";

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromVariable", "variable", GetDisplayName(space, "")), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveTypeFieldFromVariable(eventData, syncTypeTarget, blankVariableName);

                    foreach (var variable in space.GetVariableIdentities<Type>().RemoveSharedConfigVariables())
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromVariable", "variable", GetDisplayName(variable)), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetDriveTypeFieldFromVariable(eventData, syncTypeTarget, variable.QualifiedName);
                    }
                });
            };

        private static ButtonEventHandler GetOfferTypeFieldDriveSpaceActions(GenerationEvent eventData, SyncType syncTypeTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromBlank"), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveFieldFromVariable(eventData, syncTypeTarget, string.Empty);

                    foreach (var space in eventData.Slot!.GetAvailableSpaces())
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromSpace", "space", GetDisplayName(space)), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetOfferTypeFieldDriveActions(eventData, syncTypeTarget, space);
                    }
                });
            };

        private static ButtonEventHandler GetOfferTypeFieldReferenceActions(GenerationEvent eventData, SyncType syncTypeTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference.Blank"), ReferenceIcon, ReferenceColor)
                        .Button.LocalPressed += GetReferenceTypeFieldForVariable(eventData, syncTypeTarget, string.Empty);

                    foreach (var space in eventData.Slot!.GetAvailableSpaces(SpaceHasName))
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference.InSpace", "space", space.SpaceName), ReferenceIcon, ReferenceColor)
                            .Button.LocalPressed += GetReferenceTypeFieldForVariable(eventData, syncTypeTarget, $"{space.SpaceName}/");
                    }
                });
            };

        private static ButtonEventHandler GetOfferTypeFieldSourceActions(GenerationEvent eventData, SyncType syncTypeTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source.Blank"), SourceIcon, SourceColor)
                        .Button.LocalPressed += GetSourceTypeFieldForVariable(eventData, syncTypeTarget, string.Empty);

                    foreach (var space in eventData.Slot!.GetAvailableSpaces(SpaceHasName))
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source.InSpace", "space", space.SpaceName), SourceIcon, SourceColor)
                            .Button.LocalPressed += GetSourceTypeFieldForVariable(eventData, syncTypeTarget, $"{space.SpaceName}/");
                    }
                });
            };

        private static ButtonEventHandler GetReferenceTypeFieldForVariable(GenerationEvent eventData, SyncType syncTypeTarget, string variable)
            => (button, args) =>
            {
                var dynamicReference = syncTypeTarget.FindNearestParent<Slot>().AttachComponent<DynamicReferenceVariable<SyncType>>();
                dynamicReference.VariableName.Value = variable;
                dynamicReference.Reference.Target = syncTypeTarget;

                eventData.CloseContextMenu();
            };

        private static ButtonEventHandler GetSourceTypeFieldForVariable(GenerationEvent eventData, SyncType syncTypeTarget, string variable)
            => (button, args) =>
            {
                syncTypeTarget.SyncWithVariable(variable);
                eventData.CloseContextMenu();
            };

        private static void OfferTypeFieldActions(GenerationEvent eventData)
        {
            if (eventData.Target is not SyncType syncTypeTarget)
                return;

            if (!syncTypeTarget.IsLinked)
            {
                eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive"), DriveIcon, DriveColor)
                    .Button.LocalPressed += GetOfferTypeFieldDriveSpaceActions(eventData, syncTypeTarget);
            }

            eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source", "type", "DynamicTypeField"), SourceIcon, SourceColor)
                .Button.LocalPressed += GetOfferTypeFieldSourceActions(eventData, syncTypeTarget);

            eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference"), ReferenceIcon, ReferenceColor)
                .Button.LocalPressed += GetOfferTypeFieldReferenceActions(eventData, syncTypeTarget);
        }
    }
}