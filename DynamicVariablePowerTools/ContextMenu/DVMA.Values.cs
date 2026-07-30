using FrooxEngine;
using MonkeyLoader.Resonite;

using GenerationEvent = MonkeyLoader.Resonite.UI.Inspectors.InspectorMemberActionsMenuItemsGenerationEvent;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed partial class DynamicVariableMemberActions
    {
        // TODO:
        private static ButtonEventHandler GetDriveFieldFromVariable<T>(GenerationEvent eventData, IField<T> fieldTarget, string variable)
            => (button, args) =>
            {
                fieldTarget.DriveFromVariable(variable);
                eventData.CloseContextMenu();
            };

        private static ButtonEventHandler GetOfferFieldDriveActions<T>(GenerationEvent eventData, IField<T> fieldTarget, DynamicVariableSpace space)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    var blankVariableName = string.IsNullOrWhiteSpace(space.CurrentName) ? string.Empty : $"{space.CurrentName}/";

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromVariable", "variable", GetDisplayName(space, "")), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveFieldFromVariable(eventData, fieldTarget, blankVariableName);

                    foreach (var variable in space.GetVariableIdentities<T>().WithoutSharedConfigVariables())
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromVariable", "variable", GetDisplayName(variable)), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetDriveFieldFromVariable(eventData, fieldTarget, variable.QualifiedName);
                    }
                });
            };

        private static ButtonEventHandler GetOfferFieldDriveSpaceActions<T>(GenerationEvent eventData, IField<T> fieldTarget)
            => (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromBlank"), DriveIcon, DriveColor)
                        .Button.LocalPressed += GetDriveFieldFromVariable(eventData, fieldTarget, string.Empty);

                    foreach (var space in eventData.Slot!.GetAvailableSpaces())
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive.FromSpace", "space", GetDisplayName(space)), DriveIcon, DriveColor)
                            .Button.LocalPressed += GetOfferFieldDriveActions(eventData, fieldTarget, space);
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

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference.Blank"), ReferenceIcon, ReferenceColor)
                        .Button.LocalPressed += GetReferenceFieldForVariable(eventData, fieldTarget, string.Empty);

                    foreach (var space in eventData.Slot!.GetAvailableSpaces(SpaceHasName))
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference.InSpace", "space", space.CurrentName), ReferenceIcon, ReferenceColor)
                            .Button.LocalPressed += GetReferenceFieldForVariable(eventData, fieldTarget, $"{space.CurrentName}/");
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

                    eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source.Blank"), SourceIcon, SourceColor)
                        .Button.LocalPressed += GetSourceFieldForVariable(eventData, fieldTarget, string.Empty);

                    foreach (var space in eventData.Slot!.GetAvailableSpaces(SpaceHasName))
                    {
                        eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source.InSpace", "space", space.CurrentName), SourceIcon, SourceColor)
                            .Button.LocalPressed += GetSourceFieldForVariable(eventData, fieldTarget, $"{space.CurrentName}/");
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
                eventData.ContextMenu.AddItem(Instance.GetLocaleString("Drive"), DriveIcon, DriveColor)
                    .Button.LocalPressed += GetOfferFieldDriveSpaceActions(eventData, fieldTarget);
            }

            eventData.ContextMenu.AddItem(Instance.GetLocaleString("Source", "type", "DynamicField"), SourceIcon, SourceColor)
                .Button.LocalPressed += GetOfferFieldSourceActions(eventData, fieldTarget);

            eventData.ContextMenu.AddItem(Instance.GetLocaleString("Reference"), ReferenceIcon, ReferenceColor)
                .Button.LocalPressed += GetOfferFieldReferenceActions(eventData, fieldTarget);
        }
    }
}