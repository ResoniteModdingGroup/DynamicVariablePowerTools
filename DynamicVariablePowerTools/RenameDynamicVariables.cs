using FrooxEngine;
using FrooxEngine.ProtoFlux;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace DynamicVariablePowerTools
{
    internal sealed class RenameDynamicVariables : ResoniteInspectorMonkey<RenameDynamicVariables, BuildInspectorBodyEvent>
    {
        public override int Priority => HarmonyLib.Priority.Low;

        public RenameDynamicVariables() : base(typeof(DynamicVariableBase<>))
        { }

        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var dynVar = (IDynamicVariable)eventData.Worker;
            var nameField = ((Worker)dynVar).TryGetField<string>("VariableName");

            eventData.UI.BuildRenameUI(
                nameField,
                onRename: newName => RenameDynVar(dynVar, newName),
                buttonText: this.GetLocaleString("Button"),
                tooltipText: this.GetLocaleString("Tooltip")
            );
        }

        private static void RenameDynVar(IDynamicVariable dynVar, string newName)
        {
            if (!dynVar.TryGetLinkedSpace(out var linkedSpace))
            {
                var nameField = ((Worker)dynVar).TryGetField<string>("VariableName");
                nameField.Value = newName;
                return;
            }

            var oldIdentity = new DynamicVariableIdentity(linkedSpace, dynVar.GetVariableType(), dynVar.VariableName);
            var oldQualifiedName = oldIdentity.QualifiedName;

            foreach (var linkedVar in linkedSpace.GetAllLinkedVariablesMatching(oldIdentity))
            {
                // TODO: Move to helper method
                var nameField = ((Worker)linkedVar).TryGetField<string>("VariableName") ?? ((Worker)linkedVar).TryGetField<string>("_variableName");

                if (nameField is not null)
                {
                    nameField.Value = newName;
                    continue;
                }

                if (linkedVar is ProtoFluxEngineProxy { Node.Target: IProtoFluxNode dynVarNode }
                  && dynVarNode.TryGetField("VariableName") is SyncRef<IGlobalValueProxy<string>> nameProxyRef
                  && nameProxyRef.Target is GlobalValue<string> nameProxy)
                {
                    nameProxy.Value.Value = newName;
                    continue;
                }
            }

            // Only attempt rename when the new name is directly binding ("space/name")
            if (RenameConfig.Instance.ChangeProtoFluxStringInputs)
            {
                linkedSpace.Slot.ForeachComponentInChildren<IInput<string>>(
                    stringInput => stringInput.Value = stringInput.Value.Replace(oldQualifiedName, newName),
                    includeLocal: true, cacheItems: true);
            }
        }
    }
}