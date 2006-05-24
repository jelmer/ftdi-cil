using FTDI;
using System;

class FTDITest {
	public static void Main() {
		IntPtr[] devices = FTDIContext.GetDeviceList(0x0403, 0x6001);

		Console.WriteLine("{0} devices found", devices.Length);
	}
}
