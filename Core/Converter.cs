using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RunGun.Core
{
	class Converter
	{

        public interface IIntToByte
        {
            Int32 Int { get; set; }

            byte B0 { get; }
            byte B1 { get; }
            byte B2 { get; }
            byte B3 { get; }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IntToByteLE : IIntToByte
        {
            [FieldOffset(0)]
            public Int32 IntVal;

            [FieldOffset(0)]
            public byte b0;
            [FieldOffset(1)]
            public byte b1;
            [FieldOffset(2)]
            public byte b2;
            [FieldOffset(3)]
            public byte b3;

            public Int32 Int {
                get { return IntVal; }
                set { IntVal = value; }
            }

            public byte B0 => b0;
            public byte B1 => b1;
            public byte B2 => b2;
            public byte B3 => b3;
        }

        public byte[] GetByteData(int intValue) {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            return intBytes;
        }

        /*public byte[] GetByteData(float floatValue) {
            byte[] floatBytes = BitConverter.GetBytes(floatValue);
        }*/

    }
}
