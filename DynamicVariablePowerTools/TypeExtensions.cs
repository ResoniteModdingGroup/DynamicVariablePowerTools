using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVariablePowerTools
{
    internal static class TypeExtensions
    {
        public static void AppendTypeName(this StringBuilder builder, Type type)
        {
            if (!type.IsGenericType)
            {
                builder.Append(type.Name);
                return;
            }

            builder.Append(type.Name[..type.Name.IndexOf('`')]);
            builder.Append('<');

            var appendComma = false;
            foreach (var arg in type.GetGenericArguments())
            {
                if (appendComma)
                    builder.Append(", ");

                builder.AppendTypeName(arg);
                appendComma = true;
            }

            builder.Append('>');
        }

        public static DynamicVariableSpace GetLinkedSpace(this IDynamicVariable dynamicVariable)
        {
            if (dynamicVariable.TryGetLinkedSpace(out var space))
                return space;

            throw new NullReferenceException("Dynamic variable is not linked against a space!");
        }

        public static bool IsLinkedToSpace(this IDynamicVariable dynamicVariable)
            => dynamicVariable.TryGetLinkedSpace(out _);

        public static bool IsLinkedToSpace(this IDynamicVariable dynamicVariable, DynamicVariableSpace space)
            => dynamicVariable.TryGetLinkedSpace(out var linkedSpace) && linkedSpace == space;

        public static bool TryGetLinkedSpace(this IDynamicVariable dynamicVariable, [NotNullWhen(true)] out DynamicVariableSpace? linkedSpace)
        {
            linkedSpace = Traverse.Create(dynamicVariable)
                .Field(nameof(DynamicVariableBase<dummy>.handler))
                .Field(nameof(DynamicVariableHandler<dummy>._currentSpace))
                .GetValue<DynamicVariableSpace>();

            return linkedSpace is not null;
        }
    }
}