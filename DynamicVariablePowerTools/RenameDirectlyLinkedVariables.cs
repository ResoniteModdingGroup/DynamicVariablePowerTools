using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace DynamicVariablePowerTools
{
    [HarmonyPatchCategory(nameof(RenameDirectlyLinkedVariables))]
    [HarmonyPatch(typeof(DynamicVariableSpace), nameof(DynamicVariableSpace.UpdateName))]
    internal sealed class RenameDirectlyLinkedVariables : ConfiguredResoniteInspectorMonkey<RenameDirectlyLinkedVariables, RenameConfig, BuildInspectorBodyEvent, DynamicVariableSpace>
    {
        //[AutoRegisterConfigKey]
        //private static ModConfigurationKey<bool> ChangeDynVarNamespaces = new ModConfigurationKey<bool>("ChangeDynVarNamespaces", "Enable searching and renaming directly linked variables and drivers when namespace changes.", () => false);

        //[AutoRegisterConfigKey]
        //private static ModConfigurationKey<bool> ChangeLogixStringInputs = new ModConfigurationKey<bool>("ChangeLogixStringInputs", "Search and rename logix inputs with the old name in the form OldName/.* (Experimental).", () => false);

        public override int Priority => HarmonyLib.Priority.Low;

        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var space = (DynamicVariableSpace)eventData.Worker;

            eventData.UI.BuildRenameUI(
                space.SpaceName,
                onRename: newName => RenameSpace(space, newName),
                buttonText: this.GetLocaleString("Button"),
                tooltipText: this.GetLocaleString("Tooltip")
            );
        }

        private static void RenameSpace(DynamicVariableSpace space, string newName)
        {
            newName = DynamicVariableHelper.ProcessName(newName);
            var currentName = space.SpaceName.Value;

            var prefixName = $"{currentName}/";

            space.Slot.ForeachComponentInChildren<IDynamicVariable>(dynVar =>
            {
                DynamicVariableHelper.ParsePath(dynVar.VariableName, out var spaceName, out var variableName);

                if (spaceName == null || Traverse.Create(dynVar).Field("handler").Field("_currentSpace").GetValue() != space)
                    return;

                // TODO: Move to helper method
                var nameField = ((Worker)dynVar).TryGetField<string>("VariableName") ?? ((Worker)dynVar).TryGetField<string>("_variableName");

                if (nameField is not null && nameField.Value.StartsWith(prefixName))
                {
                    nameField.Value = $"{newName}/{variableName}";
                    return;
                }

                if (dynVar is ProtoFluxEngineProxy { Node.Target: IProtoFluxNode dynVarNode }
                  && dynVarNode.TryGetField("VariableName") is SyncRef<IGlobalValueProxy<string>> nameProxyRef
                  && nameProxyRef.Target is GlobalValue<string> nameProxy
                  && nameProxy.Value.Value.StartsWith(prefixName))
                {
                    nameProxy.Value.Value = $"{newName}/{variableName}";
                    return;
                }
            }, includeLocal: true, cacheItems: true);

            if (ConfigSection.ChangeProtoFluxStringInputs)
            {
                space.Slot.ForeachComponentInChildren<IInput<string>>(stringInput =>
                {
                    DynamicVariableHelper.ParsePath(stringInput.Value, out var spaceName, out var variableName);
                    if (spaceName == null || spaceName != currentName)
                        return;

                    stringInput.Value = $"{newName}/{variableName}";
                }, includeLocal: true, cacheItems: true);
            }

            space.SpaceName.Value = newName;
        }
    }
}