using FrooxEngine;
using MonkeyLoader.Resonite;

using GenerationEvent = MonkeyLoader.Resonite.UI.Inspectors.InspectorMemberActionsMenuItemsGenerationEvent;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed partial class DynamicVariableMemberActions
    {
        private static ButtonEventHandler GetDriveFieldFromVariable<T>(GenerationEvent eventData, IField<T> fieldTarget, string variable)
            => (button, args) =>
            {
                fieldTarget.DriveFromVariable(variable);
                eventData.CloseContextMenu();
            };

        private static ButtonEventHandler GetOfferFieldDriveActions<T>(GenerationEvent eventData, IField<T> fieldTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromBlank"), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveFieldFromVariable(eventData, fieldTarget, string.Empty);

                    foreach (var variable in GetAvailableVariableOptions<T>(eventData.Slot!))
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromVariable", "variable", variable), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetDriveFieldFromVariable(eventData, fieldTarget, variable);
                    }
                });
            };

        private static ButtonEventHandler GetOfferFieldReferenceActions<T>(GenerationEvent eventData, IField<T> fieldTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference.Blank"), ReferenceIcon, ReferenceColor)
                        .Button.LocalPressed += GetReferenceFieldForVariable(eventData, fieldTarget, string.Empty);

                    var spaces = eventData.Target.FindNearestParent<Slot>()
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference.InSpace", "space", space.SpaceName), ReferenceIcon, ReferenceColor)
                            .Button.LocalPressed += GetReferenceFieldForVariable(eventData, fieldTarget, $"{space.SpaceName}/");
                    }
                });
            };

        private static ButtonEventHandler GetOfferFieldSourceActions<T>(GenerationEvent eventData, IField<T> fieldTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.Blank"), SourceIcon, SourceColor)
                        .Button.LocalPressed += GetSourceFieldForVariable(eventData, fieldTarget, string.Empty);

                    var spaces = eventData.Target.FindNearestParent<Slot>()
                        .GetAvailableSpaces(SpaceHasName);

                    foreach (var space in spaces)
                    {
                        eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.InSpace", "space", space.SpaceName), SourceIcon, SourceColor)
                            .Button.LocalPressed += GetSourceFieldForVariable(eventData, fieldTarget, $"{space.SpaceName}/");
                    }
                });
            };

        private static ButtonEventHandler GetReferenceFieldForVariable<T>(GenerationEvent eventData, IField<T> fieldTarget, string variable)
            => (button, args) =>
            {
                var dynamicReference = fieldTarget.FindNearestParent<Slot>().AttachComponent<DynamicReferenceVariable<IField<T>>>();
                dynamicReference.VariableName.Value = variable;
                dynamicReference.Reference.Target = fieldTarget;

                eventData.CloseContextMenu();
            };

        private static ButtonEventHandler GetSourceFieldForVariable<T>(GenerationEvent eventData, IField<T> fieldTarget, string variable)
            => (button, args) =>
            {
                fieldTarget.SyncWithVariable(variable);
                eventData.CloseContextMenu();
            };

        private static void OfferFieldActions<T>(GenerationEvent eventData)
        {
            if (eventData.Target is not IField<T> fieldTarget)
                return;

            if (!fieldTarget.IsLinked)
            {
                eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive"), DriveIcon, DriveColor)
                    .Button.LocalPressed += GetOfferFieldDriveActions(eventData, fieldTarget);
            }

            eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicField"), SourceIcon, SourceColor)
                .Button.LocalPressed += GetOfferFieldSourceActions(eventData, fieldTarget);

            eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference"), ReferenceIcon, ReferenceColor)
                .Button.LocalPressed += GetOfferFieldReferenceActions(eventData, fieldTarget);
        }
    }
}