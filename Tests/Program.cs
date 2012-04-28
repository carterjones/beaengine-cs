// This is free and unencumbered software released into the public domain.
namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices;
    using BeaEngineCS;

    class Program
    {
        static void Main(string[] args)
        {
            int dataSize = 0x1000;
            IntPtr data = Marshal.AllocHGlobal(dataSize);
            for (int i = 0; i < dataSize; ++i)
            {
                Marshal.WriteByte(IntPtr.Add(data, i), 0);
            }

            BeaEngine._Disasm inst = new BeaEngine._Disasm();
            inst.EIP = (UIntPtr)data.ToInt64();
            int len = BeaEngine.Disasm(ref inst);
            if (len == BeaEngine.UnknownOpcode)
            {
                Console.Error.WriteLine("Unknown opcode.");
            }
            else if (len == BeaEngine.OutOfRange)
            {
                Console.Error.WriteLine("Out of range.");
            }
            else
            {
                Console.WriteLine(inst.CompleteInstr);
            }
        }
    }
}
