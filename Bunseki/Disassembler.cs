// This is free and unencumbered software released into the public domain.
namespace Bunseki
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BeaEngineCS;

    public class Disassembler
    {
        public enum Architecture
        {
            x86_32 = 1,
            x86_64,
        }

        public enum InternalDisassembler
        {
            BeaEngine = 1,
        }

        public Architecture TargetArchitecture { get; set; }

        public InternalDisassembler Engine { get; set; }

        public IEnumerable<Instruction> DisassembleInstructions(byte[] data)
        {
            return this.DisassembleInstructions(data, IntPtr.Zero);
        }

        public IEnumerable<Instruction> DisassembleInstructions(byte[] data, IntPtr virtualAddress)
        {
            if (this.Engine == InternalDisassembler.BeaEngine)
            {
                BeaEngine.Architecture architecture;
                if (this.TargetArchitecture == Architecture.x86_32)
                {
                    architecture = BeaEngine.Architecture.x86_32;
                }
                else if (this.TargetArchitecture == Architecture.x86_64)
                {
                    architecture = BeaEngine.Architecture.x86_64;
                }
                else
                {
                    architecture = BeaEngine.Architecture.x86_32;
                }

                foreach (BeaEngine._Disasm inst in BeaEngine.Disassemble(data, virtualAddress, architecture))
                {
                    yield return new Instruction(inst);
                }
            }
            else
            {
                yield break;
            }
        }
    }
}
