// This is free and unencumbered software released into the public domain.
namespace Bunseki
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using BeaEngineCS;

    public class Instruction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        private string stringRepresentation = string.Empty;

        /// <summary>
        /// Flow control of execution.
        /// </summary>
        public enum ControlFlow : byte
        {
            /// <summary>
            /// Indicates the instruction is not a flow-control instruction.
            /// </summary>
            None = 0,

            /// <summary>
            /// Indicates the instruction is one of: CALL, CALL FAR.
            /// </summary>
            Call = 1,

            /// <summary>
            /// Indicates the instruction is one of: RET, IRET, RETF.
            /// </summary>
            Return,

            /// <summary>
            /// Indicates the instruction is one of: SYSCALL, SYSRET, SYSENTER, SYSEXIT.
            /// </summary>
            SysX,

            /// <summary>
            /// Indicates the instruction is one of: JMP, JMP FAR.
            /// </summary>
            UnconditionalBranch,

            /// <summary>
            /// Indicates the instruction is one of:
            /// JCXZ, JO, JNO, JB, JAE, JZ, JNZ, JBE, JA, JS, JNS, JP, JNP, JL, JGE, JLE, JG, LOOP, LOOPZ, LOOPNZ.
            /// </summary>
            ConditionalBranch,

            /// <summary>
            /// Indiciates the instruction is one of: INT, INT1, INT 3, INTO, UD2.
            /// </summary>
            Interupt,

            /// <summary>
            /// Indicates the instruction is one of: CMOVxx.
            /// </summary>
            CMOVxx,
        }

        public IntPtr Address { get; set; }
        public string Mnemonic { get; private set; }
        public IntPtr BranchTarget { get; private set; }
        public ControlFlow FlowType { get; private set; }
        public uint NumBytes { get; private set; }
        public InstructionArgument Arg1 { get; private set; }
        public InstructionArgument Arg2 { get; private set; }
        public InstructionArgument Arg3 { get; private set; }

        private Instruction()
        {
        }

        internal Instruction(BeaEngine._Disasm inst)
        {
            this.Address = (IntPtr)inst.VirtualAddr;
            this.Mnemonic = inst.Instruction.Mnemonic;
            this.stringRepresentation = inst.CompleteInstr;
            this.BranchTarget = (IntPtr)inst.Instruction.AddrValue;
            this.FlowType = Instruction.GetFlowControl(this.Mnemonic);
            this.NumBytes = (uint)inst.Length;
            this.Arg1 = new InstructionArgument(inst.Argument1);
            this.Arg2 = new InstructionArgument(inst.Argument2);
            this.Arg3 = new InstructionArgument(inst.Argument3);
        }

        private static ControlFlow GetFlowControl(string mnemonic)
        {
            string mnemonicLowercase = mnemonic.ToLower();
            if (mnemonicLowercase.StartsWith("call"))
            {
                return ControlFlow.Call;
            }
            else if (mnemonicLowercase.StartsWith("jmp"))
            {
                return ControlFlow.UnconditionalBranch;
            }
            else if (mnemonicLowercase.StartsWith("j") || mnemonicLowercase.StartsWith("loop"))
            {
                return ControlFlow.ConditionalBranch;
            }
            else if (mnemonicLowercase.StartsWith("cmov"))
            {
                return ControlFlow.CMOVxx;
            }
            else if (mnemonicLowercase.StartsWith("sys"))
            {
                return ControlFlow.SysX;
            }
            else if (mnemonicLowercase.StartsWith("int") || mnemonicLowercase.Equals("ud2"))
            {
                return ControlFlow.Interupt;
            }
            else if (mnemonicLowercase.Contains("ret"))
            {
                // sysret will be handled by the "sys" check above.
                return ControlFlow.Return;
            }
            else
            {
                return ControlFlow.None;
            }
        }

        public override string ToString()
        {
            return this.stringRepresentation;
        }

        public static Instruction CreateInvalidInstruction()
        {
            Instruction inst = new Instruction();
            inst.Address = IntPtr.Subtract(IntPtr.Zero, 1);
            inst.Mnemonic = "invalid";
            inst.stringRepresentation = "invalid instruction";
            inst.BranchTarget = IntPtr.Zero;
            inst.FlowType = ControlFlow.None;
            inst.NumBytes = 0;
            inst.Arg1 = new InstructionArgument();
            inst.Arg2 = new InstructionArgument();
            inst.Arg3 = new InstructionArgument();
            return inst;
        }

        public static Instruction CreateInvalidInstruction(IntPtr address)
        {
            Instruction inst = new Instruction();
            inst.Address = address;
            inst.Mnemonic = "invalid";
            inst.stringRepresentation = "invalid instruction";
            inst.BranchTarget = IntPtr.Zero;
            inst.FlowType = ControlFlow.None;
            inst.NumBytes = 0;
            inst.Arg1 = new InstructionArgument();
            inst.Arg2 = new InstructionArgument();
            inst.Arg3 = new InstructionArgument();
            return inst;
        }
    }
}
