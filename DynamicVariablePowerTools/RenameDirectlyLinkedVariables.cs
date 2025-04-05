using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using MonkeyLoader.Resonite;

namespace DynamicVariablePowerTools
{
    [HarmonyPatchCategory(nameof(RenameDirectlyLinkedVariables))]
    [HarmonyPatch(typeof(DynamicVariableSpace), nameof(DynamicVariableSpace.UpdateName))]
    internal sealed class RenameDirectlyLinkedVariables : ResoniteMonkey<RenameDirectlyLinkedVariables>
    {
        //[AutoRegisterConfigKey]
        //private static ModConfigurationKey<bool> ChangeDynVarNamespaces = new ModConfigurationKey<bool>("ChangeDynVarNamespaces", "Enable searching and renaming directly linked variables and drivers when namespace changes.", () => false);

        //[AutoRegisterConfigKey]
        //private static ModConfigurationKey<bool> ChangeLogixStringInputs = new ModConfigurationKey<bool>("ChangeLogixStringInputs", "Search and rename logix inputs with the old name in the form OldName/.* (Experimental).", () => false);

        private static void Prefix(DynamicVariableSpace __instance, string ____lastName, bool ____lastNameSet)
        {
            var newName = DynamicVariableHelper.ProcessName(__instance.SpaceName.Value);

            if (/*!Config.GetValue(ChangeDynVarNamespaces) || */newName == ____lastName && ____lastNameSet)
                return;

            __instance.Slot.ForeachComponentInChildren<IDynamicVariable>(dynVar =>
            {
                DynamicVariableHelper.ParsePath(dynVar.VariableName, out var spaceName, out var variableName);

                if (spaceName == null || Traverse.Create(dynVar).Field("handler").Field("_currentSpace").GetValue() != __instance)
                    return;

                var newVariableName = $"{newName}/{variableName}";

                // TODO: Move to helper method
                var nameField = ((Worker)dynVar).TryGetField<string>("VariableName") ?? ((Worker)dynVar).TryGetField<string>("_variableName");

                if (nameField is not null)
                {
                    nameField.Value = newVariableName;
                    return;
                }

                if (dynVar is ProtoFluxEngineProxy { Node.Target: IProtoFluxNode dynVarNode }
                  && dynVarNode.TryGetField("VariableName") is SyncRef<IGlobalValueProxy<string>> nameProxyRef
                  && nameProxyRef.Target is GlobalValue<string> nameProxy)
                {
                    nameProxy.Value.Value = newVariableName;
                    return;
                }
            }, true, true);

            //if (!Config.GetValue(ChangeLogixStringInputs))
            //    return;

            __instance.Slot.ForeachComponentInChildren<IInput<string>>(stringInput =>
            {
                DynamicVariableHelper.ParsePath(stringInput.Value, out var spaceName, out var variableName);
                if (spaceName == null || spaceName != ____lastName)
                    return;

                stringInput.Value = $"{newName}/{variableName}";
            }, true, true);
        }
    }
}