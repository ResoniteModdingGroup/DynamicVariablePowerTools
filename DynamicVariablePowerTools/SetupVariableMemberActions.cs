using FrooxEngine;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.Inspectors;
using System.Reflection;

namespace DynamicVariablePowerTools
{
    internal sealed class SetupVariableMemberActions
        : ResoniteAsyncEventHandlerMonkey<SetupVariableMemberActions, InspectorMemberActionsMenuItemsGenerationEvent>
    {
        private static readonly MethodInfo _createFieldItemsMethod = AccessTools.DeclaredMethod(typeof(SetupVariableMemberActions), nameof(CreateFieldItems));
        private static readonly MethodInfo _createSyncRefItemsMethod = AccessTools.DeclaredMethod(typeof(SetupVariableMemberActions), nameof(CreateSyncRefItems));

        private static readonly Dictionary<Type, Action<InspectorMemberActionsMenuItemsGenerationEvent>> _itemCreatorsByType = new()
        {
            { typeof(Type), AccessTools.MethodDelegate<Action<InspectorMemberActionsMenuItemsGenerationEvent>>(AccessTools.DeclaredMethod(typeof(SetupVariableMemberActions), nameof(CreateTypeFieldItems))) }
        };

        public override bool CanBeDisabled => true;
        public override int Priority => HarmonyLib.Priority.Normal;

        protected override bool AppliesTo(InspectorMemberActionsMenuItemsGenerationEvent eventData)
            => base.AppliesTo(eventData) && eventData.Target is IField;

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
            var menuItem = eventData.ContextMenu.AddItem("Set up DynamicField", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);

            menuItem.Button.LocalPressed += (button, args) =>
            {
                // Swap to eventData.Worker when updated
                var slot = eventData.Target.FindNearestParent<Slot>();
                var dynamicField = slot.AttachComponent<DynamicField<T>>();
                dynamicField.TargetField.Target = (IField<T>)eventData.Target;

                button.World.LocalUser.CloseContextMenu(eventData.Summoner);
            };

            menuItem = eventData.ContextMenu.AddItem("Drive from Dynamic Variable", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);

            menuItem.Button.LocalPressed += (button, args) =>
            {
                button.World.LocalUser.CloseContextMenu(eventData.Summoner);

                button.Slot.StartTask(async () =>
                {
                    await button.World.LocalUser.OpenContextMenu(eventData.Summoner, args.source.Slot);

                    // Need to check dynamic variable spaces hiding eachother
                    // Also use full space/varName for drive
                    var slot = eventData.Target.FindNearestParent<Slot>();
                    var options = slot.GetComponentsInParents<DynamicVariableSpace>()
                        .SelectMany(space => space._dynamicValues.Keys.Where(variable => typeof(T).IsAssignableFrom(variable.type)))
                        .ToArray();

                    var menuItem2 = eventData.ContextMenu.AddItem("Blank", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);
                    menuItem2.Button.LocalPressed += (button2, args2) =>
                    {
                        var driver = slot.AttachComponent<DynamicValueVariableDriver<T>>();
                        driver.Target.Target = (IField<T>)eventData.Target;
                        button.World.LocalUser.CloseContextMenu(eventData.Summoner);
                    };

                    foreach (var option in options)
                    {
                        var menuItem3 = eventData.ContextMenu.AddItem(option.name, (Uri)null!, RadiantUI_Constants.Sub.PURPLE);
                        menuItem3.Button.LocalPressed += (button2, args2) =>
                        {
                            ((IField<T>)eventData.Target).DriveFromVariable(option.name);
                            button.World.LocalUser.CloseContextMenu(eventData.Summoner);
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

            var menuItem = eventData.ContextMenu.AddItem("Set up DynamicReference", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);

            menuItem.Button.LocalPressed += (sender, args) =>
            {
                // Swap to eventData.Worker when updated
                var slot = eventData.Target.FindNearestParent<Slot>();
                var dynamicReference = slot.AttachComponent<DynamicReference<T>>();
                dynamicReference.TargetReference.Target = syncRefTarget;
            };
        }

        private static void CreateTypeFieldItems(InspectorMemberActionsMenuItemsGenerationEvent eventData)
        {
            if (eventData.Target is not SyncType syncTypeTarget)
                return;

            var menuItem = eventData.ContextMenu.AddItem("Set up DynamicTypeField", (Uri)null!, RadiantUI_Constants.Sub.PURPLE);

            menuItem.Button.LocalPressed += (sender, args) =>
            {
                // Swap to eventData.Worker when updated
                var slot = eventData.Target.FindNearestParent<Slot>();
                var dynamicReference = slot.AttachComponent<DynamicTypeField>();
                dynamicReference.TargetField.Target = syncTypeTarget;
            };
        }

        private static Action<InspectorMemberActionsMenuItemsGenerationEvent> MakeMethod(MethodInfo method, Type type)
        {
            method = method.MakeGenericMethod(type);
            return AccessTools.MethodDelegate<Action<InspectorMemberActionsMenuItemsGenerationEvent>>(method);
        }
    }
}