using System;
using RunGun.Core.Utility;

namespace RunGun.UnitTesting
{
	class Program {

		struct TestStruct {
			public int Integral { get; set; }
			public string Fuck { get; set; }
			public int AA { get; set; }
		}
		static void TestMemoryMan() {
			for (int i = 0; i < 10000; i++) {
				int correctInt = i * 6;
				TestStruct ass = new TestStruct {
					Integral = correctInt,
					Fuck = "ur mom lol allalal",
					AA = 420,
				};
				byte[] arr = ByteUtil.Serialize(ass);
				TestStruct reconstructed = ByteUtil.Deserialize<TestStruct>(arr);
				if (ass.Fuck != reconstructed.Fuck || ass.Integral != reconstructed.Integral || ass.AA != reconstructed.AA) {
					Console.WriteLine(String.Format("Mismatch: before {0}, after {1}", ass.Fuck, reconstructed.Fuck));
					return;
				}
			}
		}

		static void Main(string[] args) {
			TestMemoryMan();
			Console.WriteLine("Test Complete");
		}
	}
}
