using FTDI;
using System;

class FTDITest {
	public static void Main() {
		IntPtr[] devices = FTDIContext.GetDeviceList(0x0403, 0x4003);

		if (devices.Length == 0) {
			Console.WriteLine("No devices found, exiting");
			return;
		}
			
		Console.WriteLine("{0} devices found", devices.Length);

		FTDIContext ftdi = new FTDIContext(devices[0]);
		ftdi.Baudrate = 57600;
		ftdi.PurgeBuffers();

		Console.WriteLine(ftdi);
		byte[] b;
		int ret;
		do {
			b = new byte[1];
			ret = ftdi.ReadData(b, 1);
			Console.WriteLine("{0}", ret);
			Console.WriteLine("{0}", b[0].ToString());
		} while (ret > 0);

		b = new byte[] { 0x1 };
		ftdi.WriteData(b, b.Length);
		Console.WriteLine(ftdi);

		ftdi.Close();
	}
}
