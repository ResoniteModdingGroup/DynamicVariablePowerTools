using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace DynamicVariablePowerTools
{
    internal sealed class RenameDirectlyLinkedVariables
        : ConfiguredResoniteInspectorMonkey<RenameDirectlyLinkedVariables, RenameConfig, BuildInspectorBodyEvent, DynamicVariableSpace>
    {
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
            var currentName = space.CurrentName;
            newName = DynamicVariableHelper.ProcessName(newName);

            Logger.Info(() => $"Renaming DynamicVariableSpace from {currentName} to {newName}!{Environment.NewLine}{space.ParentHierarchyToString()}");

            var currentNamePrefix = $"{currentName}/";
            var newNamePrefix = $"{newName}/";

            foreach (var dynVar in space.GetAllLinkedVariables())
            {
                try
                {
                    DynamicVariableHelper.ParsePath(dynVar.VariableName, out var spaceName, out var variableName);

                    if (dynVar is ProtoFluxEngineProxy { Node.Target: IProtoFluxNode dynVarNode }
                      && dynVarNode.TryGetField("VariableName") is SyncRef<IGlobalValueProxy<string>> nameProxyRef
                      && nameProxyRef.Target is GlobalValue<string> nameProxy)
                    {
                        var newVariableName = nameProxy.Value.Value.Replace(currentNamePrefix, newNamePrefix);
                        nameProxy.Value.Value = newVariableName;

                        if (nameProxy.Value.Value != newVariableName)
                            Logger.Warn(() => $"Failed to rename dynamic variable!{Environment.NewLine}{((Worker)nameProxy).ParentHierarchyToString()}");

                        continue;
                    }

                    // TODO: Move to helper method
                    var nameField = ((Worker)dynVar).TryGetField<string>("VariableName") ?? ((Worker)dynVar).TryGetField<string>("_variableName");

                    if (nameField is not null)
                    {
                        var newVariableName = $"{newName}/{variableName}";
                        nameField.Value = newVariableName;

                        if (nameField.Value != newVariableName)
                            Logger.Warn(() => $"Failed to rename dynamic variable!{Environment.NewLine}{((Worker)dynVar).ParentHierarchyToString()}");

                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.LogFormat("Exception while trying to rename dynamic variable!"));
                }
            }

            if (ConfigSection.ChangeProtoFluxStringInputs)
            {
                space.Slot.ForeachComponentInChildren<IInput<string>>(stringInput =>
                {
                    // this eats variables like: Space/{0} - so can't construct new name from {newName}/{variableName}
                    //DynamicVariableHelper.ParsePath(stringInput.Value, out var spaceName, out var variableName);
                    if (stringInput.Value is null || !stringInput.Value.StartsWith(currentNamePrefix))
                        return;

                    stringInput.Value = stringInput.Value.Replace(currentNamePrefix, newNamePrefix);
                }, includeLocal: true, cacheItems: true);
            }

            space.SpaceName.Value = newName;
        }
    }
}