using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RunGun.Core.Utility
{
	// https://codereview.stackexchange.com/questions/122969/udp-server-design-and-performance?rq=1 holy fuck
	public static class ByteUtil
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <returns></returns>
		public static byte[] Serialize<T>(T s) {
			try {
				var size = Marshal.SizeOf(typeof(T));
				var array = new byte[size];
				var ptr = Marshal.AllocHGlobal(size);
				Marshal.StructureToPtr(s, ptr, true);
				Marshal.Copy(ptr, array, 0, size);
				Marshal.FreeHGlobal(ptr);
				return array;
			} catch (Exception e) {
				Console.WriteLine("Exception occured with " + typeof(T).ToString());
				throw e;
			}
			
		}

		static void Dump(byte[] data) {
			Console.Write("D: ");
			for (int i = 0; i < data.Length; i++) {
				Console.Write("{0} ", data[i]);
			}
			Console.WriteLine("");
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T Deserialize<T>(byte[] array){
			try {
				var size = Marshal.SizeOf(typeof(T));
				var ptr = Marshal.AllocHGlobal(size);
				Marshal.Copy(array, 0, ptr, size);
				var s = (T)Marshal.PtrToStructure(ptr, typeof(T));
				Marshal.FreeHGlobal(ptr);
				return s;

			} catch (Exception e) {
				Console.WriteLine("Incorrectly formatted packet: " + typeof(T).ToString());
				Dump(array);
				throw e;
			}	
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="ins"></param>
		/// <param name="packet"></param>
		// inserts all of ins[x] at packet[x+index]
		public static void Put(int index, byte[] ins, ref byte[] packet) {
			for (int i = 0; i < ins.Length; i++) {
				packet[index + i] = ins[i];
			}
		}
	}
}
