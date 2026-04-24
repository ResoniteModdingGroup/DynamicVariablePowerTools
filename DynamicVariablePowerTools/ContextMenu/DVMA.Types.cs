using FrooxEngine;
using MonkeyLoader.Resonite;

using GenerationEvent = MonkeyLoader.Resonite.UI.Inspectors.InspectorMemberActionsMenuItemsGenerationEvent;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed partial class DynamicVariableMemberActions
    {
        private static void CreateTypeFieldItems(GenerationEvent eventData)
        {
            if (eventData.Target is not SyncType syncTypeTarget)
                return;

            var menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicTypeField"), SourceIcon, SourceColor);

            menuItem.Button.LocalPressed += (sender, args) =>
            {
                var slot = eventData.Target.FindNearestParent<Slot>();
                var dynamicReference = slot.AttachComponent<DynamicTypeField>();
                dynamicReference.TargetField.Target = syncTypeTarget;

                eventData.CloseContextMenu();
            };

            if (syncTypeTarget.IsLinked)
                return;

            menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("DriveFrom"), (Uri)null!, RadiantUI_Constants.Sub.PURPLE);

            menuItem.Button.LocalPressed += (button, args) =>
            {
                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    var slot = eventData.Target.FindNearestParent<Slot>();

                    var menuItem2 = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromBlank"), (Uri)null!, RadiantUI_Constants.Sub.PURPLE);
                    menuItem2.Button.LocalPressed += (button2, args2) =>
                    {
                        syncTypeTarget.DriveFromVariable("");
                        eventData.CloseContextMenu();
                    };

                    foreach (var option in slot.GetAvailableVariableIdentities<Type>())
                    {
                        var menuItem3 = eventData.ContextMenu.AddItem($"{option.Space.SpaceName}/{option.Name}", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);
                        menuItem3.Button.LocalPressed += (button2, args2) =>
                        {
                            syncTypeTarget.DriveFromVariable($"{option.Space.SpaceName}/{option.Name}");
                            eventData.CloseContextMenu();
                        };
                    }
                });
            };
        }
    }
}