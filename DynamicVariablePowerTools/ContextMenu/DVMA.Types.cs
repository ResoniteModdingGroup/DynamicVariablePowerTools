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

        private static ButtonEventHandler GetOfferTypeFieldDriveActions(GenerationEvent eventData, SyncType syncTypeTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromBlank"), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveTypeFieldFromVariable(eventData, syncTypeTarget, string.Empty);

                    foreach (var variable in GetAvailableVariableOptions<Type>(eventData.Slot!))
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromVariable", "variable", variable), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetDriveTypeFieldFromVariable(eventData, syncTypeTarget, variable);
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

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference.Blank"), ReferenceIcon, ReferenceColor)
                        .Button.LocalPressed += GetReferenceTypeFieldForVariable(eventData, syncTypeTarget, string.Empty);

                    var spaces = eventData.Slot!
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference.InSpace", "space", space.SpaceName), ReferenceIcon, ReferenceColor)
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

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.Blank"), SourceIcon, SourceColor)
                        .Button.LocalPressed += GetSourceTypeFieldForVariable(eventData, syncTypeTarget, string.Empty);

                    var spaces = eventData.Slot!
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.InSpace", "space", space.SpaceName), SourceIcon, SourceColor)
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
                eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive"), DriveIcon, DriveColor)
                    .Button.LocalPressed += GetOfferTypeFieldDriveActions(eventData, syncTypeTarget);
            }

            eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicTypeField"), SourceIcon, SourceColor)
                .Button.LocalPressed += GetOfferTypeFieldSourceActions(eventData, syncTypeTarget);

            eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference"), ReferenceIcon, ReferenceColor)
                .Button.LocalPressed += GetOfferTypeFieldReferenceActions(eventData, syncTypeTarget);
        }
    }
}