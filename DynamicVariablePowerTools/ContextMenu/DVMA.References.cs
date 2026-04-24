using FrooxEngine;
using MonkeyLoader.Resonite;

using GenerationEvent = MonkeyLoader.Resonite.UI.Inspectors.InspectorMemberActionsMenuItemsGenerationEvent;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed partial class DynamicVariableMemberActions
    {
        private static void CreateSyncRefItems<T>(GenerationEvent eventData)
            where T : class, IWorldElement
        {
            if (eventData.Target is not SyncRef<T> syncRefTarget)
                return;

            var menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicReference"), SourceIcon, SourceColor);

            menuItem.Button.LocalPressed += (sender, args) =>
            {
                var slot = eventData.Target.FindNearestParent<Slot>();
                var dynamicReference = slot.AttachComponent<DynamicReference<T>>();
                dynamicReference.TargetReference.Target = syncRefTarget;

                eventData.CloseContextMenu();
            };

            if (syncRefTarget.IsLinked)
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
                        syncRefTarget.DriveFromVariable("");
                        eventData.CloseContextMenu();
                    };

                    foreach (var option in slot.GetAvailableVariableIdentities<T>())
                    {
                        var menuItem3 = eventData.ContextMenu.AddItem($"{option.Space.SpaceName}/{option.Name}", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);
                        menuItem3.Button.LocalPressed += (button2, args2) =>
                        {
                            syncRefTarget.DriveFromVariable($"{option.Space.SpaceName}/{option.Name}");
                            eventData.CloseContextMenu();
                        };
                    }
                });
            };
        }
    }
}