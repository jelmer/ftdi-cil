using FTDI;
using System;

class FTDITest {
	public static void Main() {
		IntPtr[] devices = FTDIContext.GetDeviceList(0x0403, 0x6001);

		if (devices.Length == 0) {
			Console.WriteLine("No devices found, exiting");
			return;
		}
			
		Console.WriteLine("{0} devices found", devices.Length);

		FTDIContext ftdi = new FTDIContext(devices[0]);

//		FTDIContext ftdi = new FTDIContext(0x0403, 0x6001);

		Console.WriteLine(ftdi);
	}
}
