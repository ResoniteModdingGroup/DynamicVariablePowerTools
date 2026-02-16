using System.Text;
using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Resonite;

namespace DynamicVariablePowerTools
{
    internal sealed class SpaceTree
    {
        private readonly Slot _slot;
        private readonly DynamicVariableSpace _space;
        private SpaceTree[] _children = [];
        private IDynamicVariable[] _dynVars = [];

        public SpaceTree(DynamicVariableSpace space, Slot? slot = null)
        {
            _space = space;
            _slot = slot ?? space.Slot;
        }

        public bool Process()
        {
            _dynVars = [.. _slot.GetComponents<IDynamicVariable>(dynvar => dynvar.IsLinkedToSpace(_space))];

            _children = [.. _slot.Children.Select(child => new SpaceTree(_space, child)).Where(tree => tree.Process())];

            return _dynVars.Length > 0 || _children.Length > 0;
        }

        public override string ToString()
        {
            var builder = new StringBuilder("Hierarchy of linked dynamic variable components of Namespace [")
                .Append(_space.SpaceName)
                .Append("] on ")
                .AppendLine(_space.Slot.Name);

            BuildString(builder, "");
            builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            return builder.ToString();
        }

        private static void AppendDynVar(StringBuilder builder, string indent, IDynamicVariable dynVar, bool last = false)
        {
            builder.Append(indent);
            builder.Append(last ? "└─" : "├─");
            builder.Append(dynVar.VariableName);
            builder.Append(" (");
            builder.Append(dynVar.GetType().GetNiceName());
            builder.AppendLine(")");
        }

        private static void AppendSlot(StringBuilder builder, string indent, SpaceTree child, bool first, bool last)
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
            if (_dynVars.Length is not 0)
            {
                for (var i = 0; i < _dynVars.Length - 1; ++i)
                    AppendDynVar(builder, indent, _dynVars[i]);

                var isLast = _children.Length is 0;

                AppendDynVar(builder, indent, _dynVars[^1], isLast);

                if (!isLast)
                {
                    builder.Append(indent);
                    builder.AppendLine("│");
                }
            }

            if (_children.Length is not 0)
            {
                for (var i = 0; i < _children.Length; ++i)
                    AppendSlot(builder, indent, _children[i], i == 0, i == _children.Length - 1);
            }
        }
    }
}