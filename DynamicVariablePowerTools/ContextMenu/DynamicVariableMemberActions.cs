using FrooxEngine;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.Inspectors;
using System.Reflection;

namespace DynamicVariablePowerTools.ContextMenu
{
    internal sealed class DynamicVariableMemberActions
        : ResoniteAsyncEventHandlerMonkey<DynamicVariableMemberActions, InspectorMemberActionsMenuItemsGenerationEvent>
    {
        private static readonly MethodInfo _createFieldItemsMethod = AccessTools.DeclaredMethod(typeof(DynamicVariableMemberActions), nameof(CreateFieldItems));
        private static readonly MethodInfo _createSyncRefItemsMethod = AccessTools.DeclaredMethod(typeof(DynamicVariableMemberActions), nameof(CreateSyncRefItems));

        private static readonly Dictionary<Type, Action<InspectorMemberActionsMenuItemsGenerationEvent>> _itemCreatorsByType = new()
        {
            { typeof(Type), AccessTools.MethodDelegate<Action<InspectorMemberActionsMenuItemsGenerationEvent>>(AccessTools.DeclaredMethod(typeof(DynamicVariableMemberActions), nameof(CreateTypeFieldItems))) }
        };

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(InspectorMemberActionsMenuItemsGenerationEvent eventData)
            // Check for existence of Slot parent to filter out fields on UserComponents etc.
            => base.AppliesTo(eventData) && eventData.Target is IField && eventData.Target.FindNearestParent<Slot>() is not null;

        protected override Task Handle(InspectorMemberActionsMenuItemsGenerationEvent eventData)
        {
            Action<InspectorMemberActionsMenuItemsGenerationEvent>? createItems = null;

            // Check ISyncRef first because those are IField<RefID>
            if (eventData.Target is ISyncRef syncRef)
            {
                if (!_itemCreatorsByType.TryGetValue(syncRef.TargetType, out createItems))
                {
                    createItems = MakeMethod(_createSyncRefItemsMethod, syncRef.TargetType);
                    _itemCreatorsByType.Add(syncRef.TargetType, createItems);
                }
            }
            // This includes SyncType fields, since they're derived from SyncField<Type> and thus IField<Type>
            else if (eventData.Target is IField field)
            {
                if (!_itemCreatorsByType.TryGetValue(field.ValueType, out createItems))
                {
                    createItems = MakeMethod(_createFieldItemsMethod, field.ValueType);
                    _itemCreatorsByType.Add(field.ValueType, createItems);
                }
            }
            else
            {
                Logger.Warn(() => $"Tried to create inspector member action items for unsupported target: {eventData.Target.GetType().CompactDescription()}");
                return Task.CompletedTask;
            }

            createItems(eventData);

            return Task.CompletedTask;
        }

        private static void CreateFieldItems<T>(InspectorMemberActionsMenuItemsGenerationEvent eventData)
        {
            if (eventData.Target is not IField<T> fieldTarget)
                return;

            var menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicField"), OfficialAssets.Graphics.Icons.ProtoFlux.Source, RadiantUI_Constants.Sub.CYAN);

            menuItem.Button.LocalPressed += (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    var menuItem2 = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.Blank"), (Uri)null!, RadiantUI_Constants.Sub.CYAN);

                    menuItem2.Button.LocalPressed += (button2, args2) =>
                    {
                        fieldTarget.SyncWithVariable("");
                        eventData.CloseContextMenu();
                    };

                    foreach (var space in eventData.Target.FindNearestParent<Slot>().GetAvailableSpaces())
                    {
                        menuItem2 = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source.InSpace", "space", space.SpaceName.Value ?? "null"), (Uri)null!, RadiantUI_Constants.Sub.CYAN);

                        menuItem2.Button.LocalPressed += (button2, args2) =>
                        {
                            var name = space.SpaceName.Value is null ? "" : $"{space.SpaceName}/";
                            fieldTarget.SyncWithVariable(name);
                            eventData.CloseContextMenu();
                        };
                    }
                });
            };

            menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Reference"), OfficialAssets.Graphics.Icons.ProtoFlux.Reference, RadiantUI_Constants.Neutrals.LIGHT);

            menuItem.Button.LocalPressed += (button, args) =>
            {
                var dynamicReference = fieldTarget.FindNearestParent<Slot>().AttachComponent<DynamicReferenceVariable<IField<T>>>();
                dynamicReference.Reference.Target = fieldTarget;

                eventData.CloseContextMenu();
            };

            if (fieldTarget.IsLinked)
                return;

            menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive"), OfficialAssets.Graphics.Icons.ProtoFlux.Drive, RadiantUI_Constants.Sub.PURPLE);

            menuItem.Button.LocalPressed += (button, args) =>
            {
                eventData.CloseContextMenu();

                button.Slot.StartTask(async () =>
                {
                    if (await eventData.OpenContextMenuAsync(args.source.Slot) is null)
                        return;

                    var slot = eventData.Target.FindNearestParent<Slot>();

                    var menuItem2 = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Drive.FromBlank"), (Uri)null!, RadiantUI_Constants.Sub.PURPLE);
                    menuItem2.Button.LocalPressed += (button2, args2) =>
                    {
                        fieldTarget.DriveFromVariable("");
                        eventData.CloseContextMenu();
                    };

                    foreach (var option in slot.GetAvailableVariableIdentities<T>())
                    {
                        var menuItem3 = eventData.ContextMenu.AddItem($"{option.Space.SpaceName}/{option.Name}", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);
                        menuItem3.Button.LocalPressed += (button2, args2) =>
                        {
                            fieldTarget.DriveFromVariable($"{option.Space.SpaceName}/{option.Name}");
                            eventData.CloseContextMenu();
                        };
                    }
                });
            };
        }

        private static void CreateSyncRefItems<T>(InspectorMemberActionsMenuItemsGenerationEvent eventData)
            where T : class, IWorldElement
        {
            if (eventData.Target is not SyncRef<T> syncRefTarget)
                return;

            var menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicReference"), (Uri)null!, RadiantUI_Constants.Sub.PURPLE);

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

        private static void CreateTypeFieldItems(InspectorMemberActionsMenuItemsGenerationEvent eventData)
        {
            if (eventData.Target is not SyncType syncTypeTarget)
                return;

            var menuItem = eventData.ContextMenu.AddItem(Mod.GetLocaleString("Source", "type", "DynamicTypeField"), (Uri)null!, RadiantUI_Constants.Sub.PURPLE);

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

        private static Action<InspectorMemberActionsMenuItemsGenerationEvent> MakeMethod(MethodInfo method, Type type)
        {
            method = method.MakeGenericMethod(type);
            return AccessTools.MethodDelegate<Action<InspectorMemberActionsMenuItemsGenerationEvent>>(method);
        }
    }
}