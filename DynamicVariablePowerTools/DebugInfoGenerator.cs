using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace DynamicVariablePowerTools
{
    internal sealed class DebugInfoGenerator : ConfiguredResoniteInspectorMonkey<DebugInfoGenerator, DebugInfoConfig, BuildInspectorBodyEvent, DynamicVariableSpace>
    {
        /// <inheritdoc/>
        public override bool CanBeDisabled => true;

        /// <inheritdoc/>
        public override int Priority => -HarmonyLib.Priority.High;

        /// <inheritdoc/>
        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var ui = eventData.UI;
            var space = (DynamicVariableSpace)eventData.Worker;

            var outputField = ui.Current.AttachComponent<ValueField<string>>();

            if (ConfigSection.EnableLinkedVariablesList)
            {
                ui.LocalActionButton(Mod.GetLocaleString("EnableLinkedVariablesList.Button"), _ => OutputLinkedVariables(space, outputField.Value))
                    .WithTooltip(Mod.GetLocaleString("EnableLinkedVariablesList.Tooltip"));
            }

            if (ConfigSection.EnableLinkedComponentHierarchy)
            {
                ui.LocalActionButton(Mod.GetLocaleString("EnableLinkedComponentHierarchy.Button"), _ => OutputComponentHierarchy(space, outputField.Value))
                    .WithTooltip(Mod.GetLocaleString("EnableLinkedComponentHierarchy.Tooltip"));
            }

            SyncMemberEditorBuilder.Build(outputField.Value, "Output", outputField.GetSyncMemberFieldInfo("Value"), ui);
        }

        private static void OutputComponentHierarchy(DynamicVariableSpace space, Sync<string> target)
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

        private static void OutputLinkedVariables(DynamicVariableSpace space, Sync<string> target)
        {
            space.StartTask(async () =>
            {
                await default(ToBackground);

                var names = new StringBuilder($"Variables linked to Namespace [{space.SpaceName}] on {space.Slot.Name}");
                names.Append(space.SpaceName);
                names.AppendLine(":");

                foreach (var identity in space._dynamicValues.Keys)
                {
                    names.Append(identity.name);
                    names.Append(" (");
                    names.Append(identity.type.GetNiceName());
                    names.AppendLine(")");
                }

                names.Remove(names.Length - Environment.NewLine.Length, Environment.NewLine.Length);

                await default(ToWorld);

                target.Value = names.ToString();
            });
        }
    }
}