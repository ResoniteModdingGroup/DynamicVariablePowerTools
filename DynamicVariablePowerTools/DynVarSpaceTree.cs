using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Resonite.UI;

namespace DynamicVariablePowerTools
{
    internal sealed class DynVarSpaceTree : ResoniteInspectorMonkey<DynVarSpaceTree, BuildInspectorBodyEvent, DynamicVariableSpace>
    {
        //[AutoRegisterConfigKey]
        //private static readonly ModConfigurationKey<bool> EnableLinkedVariablesList = new("EnableLinkedVariablesList", "Allow generating a list of dynamic variable definitions for a space.", () => true);

        //[AutoRegisterConfigKey]
        //private static readonly ModConfigurationKey<bool> EnableVariableHierarchy = new("EnableVariableHierarchy", "Allow generating a hierarchy of dynamic variable components for a space.", () => true);

        public override int Priority => -HarmonyLib.Priority.High;

        /// <inheritdoc/>
        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var ui = eventData.UI;
            var space = (DynamicVariableSpace)eventData.Worker;

            var outputField = ui.Current.AttachComponent<ValueField<string>>();

            //if (Config.GetValue(EnableLinkedVariablesList))
            ui.LocalActionButton("Output names of linked Variables", _ => OutputVariableNames(space, outputField.Value));

            //if (Config.GetValue(EnableVariableHierarchy))
            ui.LocalActionButton("Output tree of linked Variable Hierarchy", _ => OutputVariableHierarchy(space, outputField.Value));

            SyncMemberEditorBuilder.Build(outputField.Value, "Output", outputField.GetSyncMemberFieldInfo("Value"), ui);
        }

        private static void OutputVariableHierarchy(DynamicVariableSpace space, Sync<string> target)
        {
            space.StartTask(async () =>
            {
                await default(ToBackground);

                var hierarchy = new SpaceTree(space);
                var output = hierarchy.Process() ? hierarchy.ToString() : "";

                await default(ToWorld);

                target.Value = output;
            });
        }

        private static void OutputVariableNames(DynamicVariableSpace space, Sync<string> target)
        {
            var names = new StringBuilder("Variables linked to Namespace ");
            names.Append(space.SpaceName);
            names.AppendLine(":");

            foreach (var identity in space._dynamicValues.Keys)
            {
                names.Append(identity.name);
                names.Append(" (");
                names.AppendTypeName(identity.type);
                names.AppendLine(")");
            }

            names.Remove(names.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            target.Value = names.ToString();
        }
    }
}