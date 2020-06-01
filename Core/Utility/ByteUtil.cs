using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RunGun.Core.Utility
{
	/*
	 * So do not fear, for I am with you; do not be dismayed, for I am your God. 
	 * I will strengthen you and help you; I will uphold you with my righteous right hand.
	 */
	// the filtration of the custom serialize would indefinitely search the intricacy of the problem
	// https://codereview.stackexchange.com/questions/122969/udp-server-design-and-performance?rq=1 holy fuck

	public static class ByteUtil {

		[Conditional("g")]
		public static void DumpNum(byte[] data) {
			Console.Write("Data: ");
			for (int i = 0; i < data.Length; i++) {
				Console.Write("{0} ", data[i]);
			}
			Console.WriteLine("");
		}
		[Conditional("f")]
		public static void DumpHex(byte[] data) {
			Console.Write("Data: ");
			for (int i = 0; i < data.Length; i++) {
				Console.Write("{0:X2} ", data[i]);
			}
			Console.WriteLine("");
		}

		public static byte[] Serialize<T>(T s) {
			int size = Marshal.SizeOf(typeof(T));
			IntPtr ptr = Marshal.AllocHGlobal(size);
			try {
				
				var array = new byte[size];
				
				Marshal.StructureToPtr(s, ptr, true);
				Marshal.Copy(ptr, array, 0, size);
				return array;
			} catch (Exception e) {
				Console.WriteLine("Exception occured with " + typeof(T).ToString());
				throw e;
			} finally {
				Marshal.FreeHGlobal(ptr);
			}
			
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T Deserialize<T>(byte[] array){
			int size = Marshal.SizeOf(typeof(T));
			IntPtr ptr = Marshal.AllocHGlobal(size);
			try {
				
				Marshal.Copy(array, 0, ptr, size);
				T s = (T)Marshal.PtrToStructure(ptr, typeof(T));
				
				return s;

			} catch (Exception e) {
				Console.WriteLine("Incorrectly formatted packet: " + typeof(T).ToString());
				ByteUtil.DumpNum(array);
				throw e;
			} finally {
				Marshal.FreeHGlobal(ptr);
			}
		}

		// inserts all of ins[x] at packet[x+index]
		public static void Put(int index, byte[] ins, ref byte[] packet) {
			for (int i = 0; i < ins.Length; i++) {
				packet[index + i] = ins[i];
			}
		}
	}
}
