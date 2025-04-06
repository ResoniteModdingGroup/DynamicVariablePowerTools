using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicVariablePowerTools
{
    internal sealed class RenameConfig : SingletonConfigSection<RenameConfig>
    {
        private static readonly DefiningConfigKey<bool> _changeProtoFluxStringInputs = new("ChangeProtoFluxStringInputs", "Search and rename ProtoFlux inputs with the old name in the form OldName/* (Experimental).", () => false);

        public bool ChangeProtoFluxStringInputs => _changeProtoFluxStringInputs;

        public override string Description => "Rename Options";

        public override string Id => "RenameOptions";

        public override Version Version { get; } = new(1, 0);
    }
}