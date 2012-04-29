// This is free and unencumbered software released into the public domain.
namespace Bunseki
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BeaEngineCS;

    public class InstructionArgument
    {
        public string Mnemonic { get; private set; }
        public bool AffectsMemory { get; private set; }

        internal InstructionArgument()
        {
            this.Mnemonic = "invalid argument";
            this.AffectsMemory = false;
        }

        internal InstructionArgument(BeaEngine.ARGTYPE arg)
        {
            this.Mnemonic = arg.ArgMnemonic;
            this.AffectsMemory = arg.Details.HasFlag(BeaEngine.ArgumentDetails.MEMORY_TYPE);
        }
    }
}
