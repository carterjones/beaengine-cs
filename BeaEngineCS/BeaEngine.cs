﻿// This is free and unencumbered software released into the public domain.
namespace BeaEngineCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices;

    using UInt8 = System.Byte;

    public class BeaEngine
    {
#if WIN64
        const string DllName = "BeaEngine-64.dll";
#else
        const string DllName = "BeaEngine-32.dll";
#endif
        [Flags]
        public enum InstructionSet
        {
            GENERAL_PURPOSE_INSTRUCTION = 0x10000,
            FPU_INSTRUCTION = 0x20000,
            MMX_INSTRUCTION = 0x40000,
            SSE_INSTRUCTION = 0x80000,
            SSE2_INSTRUCTION = 0x100000,
            SSE3_INSTRUCTION = 0x200000,
            SSSE3_INSTRUCTION = 0x400000,
            SSE41_INSTRUCTION = 0x800000,
            SSE42_INSTRUCTION = 0x1000000,
            SYSTEM_INSTRUCTION = 0x2000000,
            VM_INSTRUCTION = 0x4000000,
            UNDOCUMENTED_INSTRUCTION = 0x8000000,
            AMD_INSTRUCTION = 0x10000000,
            ILLEGAL_INSTRUCTION = 0x20000000,
            AES_INSTRUCTION = 0x40000000,
            CLMUL_INSTRUCTION = unchecked ((int)0x80000000),
        }

        public enum InstructionType
        {
            DATA_TRANSFER = 0x1,
            ARITHMETIC_INSTRUCTION,
            LOGICAL_INSTRUCTION,
            SHIFT_ROTATE,
            BIT_UInt8,
            CONTROL_TRANSFER,
            STRING_INSTRUCTION,
            InOutINSTRUCTION,
            ENTER_LEAVE_INSTRUCTION,
            FLAG_CONTROL_INSTRUCTION,
            SEGMENT_REGISTER,
            MISCELLANEOUS_INSTRUCTION,
            COMPARISON_INSTRUCTION,
            LOGARITHMIC_INSTRUCTION,
            TRIGONOMETRIC_INSTRUCTION,
            UNSUPPORTED_INSTRUCTION,
            LOAD_CONSTANTS,
            FPUCONTROL,
            STATE_MANAGEMENT,
            CONVERSION_INSTRUCTION,
            SHUFFLE_UNPACK,
            PACKED_SINGLE_PRECISION,
            SIMD128bits,
            SIMD64bits,
            CACHEABILITY_CONTROL,
            FP_INTEGER_CONVERSION,
            SPECIALIZED_128bits,
            SIMD_FP_PACKED,
            SIMD_FP_HORIZONTAL,
            AGENT_SYNCHRONISATION,
            PACKED_ALIGN_RIGHT,
            PACKED_SIGN,
            PACKED_BLENDING_INSTRUCTION,
            PACKED_TEST,
            PACKED_MINMAX,
            HORIZONTAL_SEARCH,
            PACKED_EQUALITY,
            STREAMING_LOAD,
            INSERTION_EXTRACTION,
            DOT_PRODUCT,
            SAD_INSTRUCTION,
            ACCELERATOR_INSTRUCTION,    /* crc32, popcnt (sse4.2) */
            ROUND_INSTRUCTION
        }

        [Flags]
        public enum EFlagsStates
        {
            TE_ = 1,
            MO_ = 2,
            RE_ = 4,
            SE_ = 8,
            UN_ = 0x10,
            PR_ = 0x20
        }

        public enum BranchType
        {
            JO = 1,
            JC,
            JE,
            JA,
            JS,
            JP,
            JL,
            JG,
            JB,
            JECXZ,
            JmpType,
            CallType,
            RetType,
            JNO = -1,
            JNC = -2,
            JNE = -3,
            JNA = -4,
            JNS = -5,
            JNP = -6,
            JNL = -7,
            JNG = -8,
            JNB = -9
        }

        [Flags]
        public enum ArgumentDetails : int
        {
            NO_ARGUMENT = 0x10000000,
            REGISTER_TYPE = 0x20000000,
            MEMORY_TYPE = 0x40000000,
            CONSTANT_TYPE = unchecked((int)0x80000000),

            MMX_REG = 0x10000,
            GENERAL_REG = 0x20000,
            FPU_REG = 0x40000,
            SSE_REG = 0x80000,
            CR_REG = 0x100000,
            DR_REG = 0x200000,
            SPECIAL_REG = 0x400000,
            MEMORY_MANAGEMENT_REG = 0x800000,
            SEGMENT_REG = 0x1000000,

            RELATIVE_ = 0x4000000,
            ABSOLUTE_ = 0x8000000,
        }

        [Flags]
        public enum RegisterId : short
        {
            REG0 = 0x1,
            REG1 = 0x2,
            REG2 = 0x4,
            REG3 = 0x8,
            REG4 = 0x10,
            REG5 = 0x20,
            REG6 = 0x40,
            REG7 = 0x80,
            REG8 = 0x100,
            REG9 = 0x200,
            REG10 = 0x400,
            REG11 = 0x800,
            REG12 = 0x1000,
            REG13 = 0x2000,
            REG14 = 0x4000,
            REG15 = unchecked((short)0x8000)
        }

        public enum AccessMode
        {
            READ = 0x1,
            WRITE = 0x2,
        }

        [Flags]
        public enum SpecialInfo : ulong
        {
            /* === mask = 0xff */
            NoTabulation = 0x00000000,
            Tabulation = 0x00000001,

            /* === mask = 0xff00 */
            MasmSyntax = 0x00000000,
            GoAsmSyntax = 0x00000100,
            NasmSyntax = 0x00000200,
            ATSyntax = 0x00000400,

            /* === mask = 0xff0000 */
            PrefixedNumeral = 0x00010000,
            SuffixedNumeral = 0x00000000,

            /* === mask = 0xff000000 */
            ShowSegmentRegs = 0x01000000
        }

        public const byte ESReg = 1;
        public const byte DSReg = 2;
        public const byte FSReg = 3;
        public const byte GSReg = 4;
        public const byte CSReg = 5;
        public const byte SSReg = 6;

        public const byte InvalidPrefix = 4;
        public const byte SuperfluousPrefix = 2;
        public const byte NotUsedPrefix = 0;
        public const byte MandatoryPrefix = 8;
        public const byte InUsePrefix = 1;

        public const byte LowPosition = 0;
        public const byte HighPosition = 1;

        public const int UnknownOpcode = -1;
        public const int OutOfRange = 0;
        public const int InstructionLength = 64;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct _Disasm
        {
            public UIntPtr EIP;
            public UInt64 VirtualAddr;
            public UInt32 SecurityBlock;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = InstructionLength)]
            public char[] CompleteInstr;
            public UInt32 Archi;
            public SpecialInfo Options;
            public INSTRTYPE Instruction;
            public ARGTYPE Argument1;
            public ARGTYPE Argument2;
            public ARGTYPE Argument3;
            public PREFIXINFO Prefix;
            public InternalDatas Reserved_;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct INSTRTYPE
        {
            public Int32 Category;
            public Int32 Opcode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] Mnemonic;
            public Int32 BranchType;
            public EFLStruct Flags;
            public UInt64 AddrValue;
            public Int64 Immediat;
            public UInt32 ImplicitModifiedRegs;
            public InstructionSet InstructionSet
            {
                get { return (InstructionSet)(0xffff0000 & this.Category); }
            }
            public InstructionType InstructionType
            {
                get { return (InstructionType)(0x0000ffff & this.Category); }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EFLStruct
        {
            public UInt8 OF_;
            public UInt8 SF_;
            public UInt8 ZF_;
            public UInt8 AF_;
            public UInt8 PF_;
            public UInt8 CF_;
            public UInt8 TF_;
            public UInt8 IF_;
            public UInt8 DF_;
            public UInt8 NT_;
            public UInt8 RF_;
            public UInt8 alignment;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PREFIXINFO
        {
            public int Number;
            public int NbUndefined;
            public UInt8 LockPrefix;
            public UInt8 OperandSize;
            public UInt8 AddressSize;
            public UInt8 RepnePrefix;
            public UInt8 RepPrefix;
            public UInt8 FSPrefix;
            public UInt8 SSPrefix;
            public UInt8 GSPrefix;
            public UInt8 ESPrefix;
            public UInt8 CSPrefix;
            public UInt8 DSPrefix;
            public UInt8 BranchTaken;
            public UInt8 BranchNotTaken;
            public REX_Struct REX;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct REX_Struct
        {
            public UInt8 W_;
            public UInt8 R_;
            public UInt8 X_;
            public UInt8 B_;
            public UInt8 state;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ARGTYPE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] ArgMnemonic;
            public Int32 ArgType;
            public Int32 ArgSize;
            public Int32 ArgPosition;
            public AccessMode AccessMode;
            public MEMORYTYPE Memory;
            public UInt32 SegmentReg;
            public ArgumentDetails Details
            {
                get { return (ArgumentDetails)(0xffff0000 & this.ArgType); }
            }
            public RegisterId RegisterId
            {
                get { return (RegisterId)(0x0000ffff & this.ArgType); }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MEMORYTYPE
        {
            public Int32 BaseRegister;
            public Int32 IndexRegister;
            public Int32 Scale;
            public Int64 Displacement;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct InternalDatas
        {
            public UIntPtr EIP_;
            public UInt64 EIP_VA;
            public UIntPtr EIP_REAL;
            public Int32 OriginalOperandSize;
            public Int32 OperandSize;
            public Int32 MemDecoration;
            public Int32 AddressSize;
            public Int32 MOD_;
            public Int32 RM_;
            public Int32 INDEX_;
            public Int32 SCALE_;
            public Int32 BASE_;
            public Int32 MMX_;
            public Int32 SSE_;
            public Int32 CR_;
            public Int32 DR_;
            public Int32 SEG_;
            public Int32 REGOPCODE;
            public UInt32 DECALAGE_EIP;
            public Int32 FORMATNUMBER;
            public Int32 SYNTAX_;
            public UInt64 EndOfBlock;
            public Int32 RelativeAddress;
            public UInt32 Architecture;
            public Int32 ImmediatSize;
            public Int32 NB_PREFIX;
            public Int32 PrefRepe;
            public Int32 PrefRepne;
            public UInt32 SEGMENTREGS;
            public UInt32 SEGMENTFS;
            public Int32 third_arg;
            public Int32 TAB_;
            public Int32 ERROR_OPCODE;
            public REX_Struct REX;
            public Int32 OutOfBlock;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Disasm(ref _Disasm instruction);
    }
}
