using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
using HarmonyLib;

namespace DynamicVariablePowerTools
{
    internal class SpaceTree
    {
        private readonly Slot _slot;
        private readonly DynamicVariableSpace _space;
        private SpaceTree[] _children;
        private IDynamicVariable[] _dynVars;

        public SpaceTree(DynamicVariableSpace space, Slot? slot = null)
        {
            _space = space;
            _slot = slot ?? space.Slot;
        }

        public bool Process()
        {
            _dynVars = [.. _slot.GetComponents<IDynamicVariable>(IsLinkedToThisSpace)];

            _children = _slot.Children.Select(child => new SpaceTree(_space, child)).Where(tree => tree.Process()).ToArray();

            return _dynVars.Length > 0 || _children.Length > 0;
        }

        public override string ToString()
        {
            var builder = new StringBuilder(_space.Slot.Name).Append(": Namespace ").AppendLine(_space.SpaceName);

            BuildString(builder, "");
            builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            return builder.ToString();
        }

        private void AppendDynVar(StringBuilder builder, string indent, IDynamicVariable dynVar, bool last = false)
        {
            builder.Append(indent);
            builder.Append(last ? "└─" : "├─");
            builder.Append(dynVar.VariableName);
            builder.Append(" (");
            builder.AppendTypeName(dynVar.GetType());
            builder.AppendLine(")");
        }

        private void AppendSlot(StringBuilder builder, string indent, SpaceTree child, bool first, bool last)
        {
            if (!first)
            {
                builder.Append(indent);
                builder.AppendLine("│");
            }

            builder.Append(indent);
            builder.Append(last ? "└─" : "├─");
            builder.AppendLine(child._slot.Name);

            child.BuildString(builder, indent + (last ? "  " : "│ "));
        }

        private void BuildString(StringBuilder builder, string indent)
        {
            if (_dynVars.Any())
            {
                for (var i = 0; i < _dynVars.Length - 1; ++i)
                    AppendDynVar(builder, indent, _dynVars[i]);

                AppendDynVar(builder, indent, _dynVars[^1], !_children.Any());

                if (_children.Any())
                {
                    builder.Append(indent);
                    builder.AppendLine("│");
                }
            }

            if (_children.Any())
            {
                for (var i = 0; i < _children.Length; ++i)
                    AppendSlot(builder, indent, _children[i], i == 0, i == _children.Length - 1);
            }
        }

        private bool IsLinkedToThisSpace(IDynamicVariable dynamicVariable)
            => dynamicVariable.IsLinkedToSpace(_space);
    }
}