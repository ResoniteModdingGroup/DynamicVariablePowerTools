using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.UI;

namespace DynamicVariablePowerTools
{
    internal static class RenameButtonHelper
    {
        internal static void BuildRenameUI(this UIBuilder builder, IField<string> nameField, Action<string> onRename, LocaleString buttonText, LocaleString tooltipText, ConfigKeySessionShare<bool> enabledShare)
        {
            var layoutSlot = builder.HorizontalLayout(4).Slot;
            enabledShare.DriveFromVariable(layoutSlot.ActiveSelf_Field);
            layoutSlot.DestroyWhenLocalUserLeaves();

            builder.PushStyle();
            var style = builder.Style;

            style.FlexibleWidth = 1;
            var newNameField = builder.TextField(nameField.Value, parseRTF: false);

            void ChangedListener(object _) => newNameField.Text.Content.Value = nameField.Value;
            nameField.Changed += ChangedListener;
            layoutSlot.Destroyed += _ => nameField.Changed -= ChangedListener;

            style.FlexibleWidth = -1;
            style.MinWidth = 256;
            builder.LocalActionButton(buttonText, button => onRename(newNameField.Text.Content.Value))
                .WithTooltip(tooltipText);

            builder.PopStyle();
            builder.NestOut();
        }
    }
}