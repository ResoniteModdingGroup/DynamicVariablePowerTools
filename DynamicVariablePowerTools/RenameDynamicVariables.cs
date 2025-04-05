using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicVariablePowerTools
{
    internal class RenameDynamicVariables : ResoniteInspectorMonkey<RenameDynamicVariables, BuildInspectorBodyEvent>
    {
        public override int Priority => HarmonyLib.Priority.Low;

        public RenameDynamicVariables() : base(typeof(DynamicVariableBase<>))
        { }

        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var dynVar = (IDynamicVariable)eventData.Worker;
            var nameField = ((Worker)dynVar).TryGetField<string>("VariableName");

            var builder = eventData.UI;
            builder.HorizontalLayout(4).Slot.DestroyWhenLocalUserLeaves();
            builder.PushStyle();
            var style = builder.Style;

            style.FlexibleWidth = 1;
            var newNameField = builder.TextField(dynVar.VariableName, parseRTF: false);
            nameField.Changed += _ => newNameField.Text.Content.Value = dynVar.VariableName;

            style.FlexibleWidth = -1;
            style.MinWidth = 256;
            builder.LocalActionButton(this.GetLocaleString("Button"), button => RenameDynVar(dynVar, newNameField.Text.Content.Value))
                .WithTooltip(this.GetLocaleString("Tooltip"));

            builder.PopStyle();
            builder.NestOut();
        }

        private static Type GetDynVarType(IDynamicVariable dynVar)
            => dynVar.GetType().GetGenericArgumentsFromInterface(typeof(IDynamicVariable<>))[0];

        private static bool IsDynVarOfType(IDynamicVariable dynVar, Type innerType)
            => GetDynVarType(dynVar) == innerType;

        private static void RenameDynVar(IDynamicVariable dynVar, string newName)
        {
            if (!dynVar.TryGetLinkedSpace(out var linkedSpace))
            {
                var nameField = ((Worker)dynVar).TryGetField<string>("VariableName");
                nameField.Value = newName;
                return;
            }

            var dynVarType = GetDynVarType(dynVar);
            var currentFullName = dynVar.VariableName;
            DynamicVariableHelper.ParsePath(currentFullName, out var currentSpaceName, out var currentVariableName);

            Predicate<IDynamicVariable> predicate = linkedSpace.OnlyDirectBinding
                ? (it => it.VariableName == currentFullName && IsDynVarOfType(it, dynVarType))
                : (it => (it.VariableName == currentFullName || it.VariableName == currentVariableName) && IsDynVarOfType(it, dynVarType));

            foreach (var linkedVar in linkedSpace.GetLinkedVariables(predicate, true))
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
        }
    }
}