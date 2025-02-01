using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVariablePowerTools
{
    internal sealed class OpenLinkedDynamicVariableSpace
        : ResoniteInspectorMonkey<OpenLinkedDynamicVariableSpace, BuildInspectorHeaderEvent>
    {
        public override bool CanBeDisabled => true;
        public override int Priority => HarmonyLib.Priority.First;

        // Needs to be able to handle interfaces for IDynamicVariable
        public OpenLinkedDynamicVariableSpace() : base(typeof(DynamicVariableBase<>))
        { }

        protected override void Handle(BuildInspectorHeaderEvent eventData)
        {
            if (Traverse.Create(eventData.Worker).Field("handler").Field("_currentSpace").GetValue() is not DynamicVariableSpace space)
                return;

            InspectorUIHelper.BuildHeaderOpenParentButtons(eventData.UI, space);
        }
    }
}