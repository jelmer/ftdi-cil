// 
// CIL bindings for the FTDI library
//
// Copyright (C) 2006 Jelmer Vernooij <jelmer@palmsens.com>
// Licensed under the GNU Lesser Public License
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace FTDI 
{
	public enum ChipType { TYPE_AM=0, TYPE_BM=1, TYPE_2232C=2 };
	public enum ParityType { NONE=0, ODD=1, EVEN=2, MARK=3, SPACE=4 };
	public enum StopBitsType { STOP_BIT_1=0, STOP_BIT_15=1, STOP_BIT_2=2 };
	public enum BitsType { BITS_7=7, BITS_8=8 };

	enum MpsseMode : uint {
	    BITMODE_RESET  = 0x00,
	    BITMODE_BITBANG= 0x01,
	    BITMODE_MPSSE  = 0x02,
	    BITMODE_SYNCBB = 0x04,
	    BITMODE_MCU    = 0x08,
	    BITMODE_OPTO   = 0x10
	};

	/* Port interface code for FT2232C */
	public enum Interface : uint {
	    INTERFACE_ANY = 0,
	    INTERFACE_A   = 1,
	    INTERFACE_B   = 2
	};

	/* Shifting commands IN MPSSE Mode*/
	[Flags] enum MPSSEShiftCmds {
		MPSSE_WRITE_NEG = 0x01,   /* Write TDI/DO on negative TCK/SK edge*/
		MPSSE_BITMODE   = 0x02,   /* Write bits, not bytes */
		MPSSE_READ_NEG  = 0x04,   /* Sample TDO/DI on negative TCK/SK edge */
		MPSSE_LSB       = 0x08,   /* LSB first */
		MPSSE_DO_WRITE  = 0x10,   /* Write TDI/DO */
		MPSSE_DO_READ   = 0x20,   /* Read TDO/DI */
		MPSSE_WRITE_TMS = 0x40    /* Write TMS/CS */
	};
	
	/* FTDI MPSSE commands */
	enum MPSSECommands {
		SET_BITS_LOW   = 0x80,
		/*BYTE DATA*/
		/*BYTE Direction*/
		SET_BITS_HIGH  = 0x82,
		/*BYTE DATA*/
		/*BYTE Direction*/
		GET_BITS_LOW   = 0x81,
		GET_BITS_HIGH  = 0x83,
		LOOPBACK_START = 0x84,
		LOOPBACK_END   = 0x85,
		TCK_DIVISOR    = 0x86,
		
		/* Commands in MPSSE and Host Emulation Mode */
		SEND_IMMEDIATE = 0x87,
		WAIT_ON_HIGH   = 0x88,
		WAIT_ON_LOW    = 0x89,

		/* Commands in Host Emulation Mode */
		READ_SHORT     = 0x90,
		/* Address_Low */
		READ_EXTENDED  = 0x91,
		/* Address High */
		/* Address Low  */
		WRITE_SHORT    = 0x92,
		/* Address_Low */
		WRITE_EXTENDED = 0x93,
		/* Address High */
		/* Address Low  */
	};


	[StructLayout(LayoutKind.Sequential)] struct ftdi_context {
		// USB specific
		IntPtr usb_dev;
		int usb_read_timeout;
		int usb_write_timeout;
	
		// FTDI specific
		ChipType type;
		int baudrate;
		byte bitbang_enabled;
		IntPtr readbuffer; /* byte * */
		uint readbuffer_offset;
		uint readbuffer_remaining;
		uint readbuffer_chunksize;
		uint writebuffer_chunksize;
	
		// FTDI FT2232C requirecments
		int iface;       // 0 or 1
		int index;       // 1 or 2
		// Endpoints
		int in_ep;
		int out_ep;      // 1 or 2
	
		/* 1: (default) Normal bitbang mode, 2: FT2232C SPI bitbang mode */
		byte bitbang_mode;
	
		// misc
		string error_str;
	};
	

	[StructLayout(LayoutKind.Sequential)] 
	public struct ftdi_eeprom {
		// init and build eeprom from ftdi_eeprom structure
		[DllImport("libftdi.so.0")] internal static extern int ftdi_eeprom_build(ref ftdi_eeprom eeprom, ref byte[] output);
		[DllImport("libftdi.so.0")] internal static extern void ftdi_eeprom_initdefaults(out ftdi_eeprom eeprom);

		int vendor_id;
		int product_id;
	
		int self_powered;
		int remote_wakeup;
		int BM_type_chip;
	
		int in_is_isochronous;
		int out_is_isochronous;
		int suspend_pull_downs;
	
		int use_serial;
		int change_usb_version;
		int usb_version;
		int max_power;
	
		string manufacturer;
		string product;
		string serial;
	};

	public class FTDIContext {
		private ftdi_context ftdi = new ftdi_context();

		[DllImport("libftdi.so.0")] internal static extern int ftdi_init(ref ftdi_context ftdi);
		[DllImport("libftdi.so.0")] internal static extern void ftdi_deinit(ref ftdi_context ftdi);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_open(ref ftdi_context ftdi, int vendor, int product);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_open_desc(ref ftdi_context ftdi, int vendor, int product, string description, string serial);

		private FTDIContext() {
			ftdi_init(ref ftdi);
		}

		public FTDIContext(int vendor, int product) : this() {
			int ret = ftdi_usb_open(ref ftdi, vendor, product);
			CheckRet(ret);
		}

		public FTDIContext(int vendor, int product, string description, string serial) : this() {
			int ret = ftdi_usb_open_desc(ref ftdi, vendor, product, description, serial);
			CheckRet(ret);
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_open_dev(ref ftdi_context ftdi, IntPtr dev);
		public FTDIContext(IntPtr dev) : this() {
			CheckRet(ftdi_usb_open_dev(ref ftdi, dev));
		}

		~FTDIContext() {
			ftdi_deinit(ref ftdi);
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_baudrate(ref ftdi_context ftdi, int baudrate);
		public int Baudrate { 
			set {
				CheckRet(ftdi_set_baudrate(ref ftdi, value));
			}
		}

		internal static void CheckRet(int ret) {
			/* FIXME: Throw exception */
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_reset(ref ftdi_context ftdi);

		public void Reset() {
			CheckRet(ftdi_usb_reset(ref ftdi));
		}


		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_close(ref ftdi_context ftdi);

		public void Close() {
			CheckRet(ftdi_usb_close(ref ftdi));
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_purge_buffers(ref ftdi_context ftdi);

		public void PurgeBuffers() {
			CheckRet(ftdi_usb_purge_buffers(ref ftdi));
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_data_set_chunksize(ref ftdi_context ftdi, uint chunksize);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_data_get_chunksize(ref ftdi_context ftdi, out uint chunksize);

		public uint ReadChunkSize {
			set { 
				CheckRet(ftdi_read_data_set_chunksize(ref ftdi, value)); 
			}
			get {
				uint chunksize;
				CheckRet(ftdi_read_data_get_chunksize(ref ftdi, out chunksize));
				 return chunksize;
			}
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_data_set_chunksize(ref ftdi_context ftdi, uint chunksize);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_data_get_chunksize(ref ftdi_context ftdi, out uint chunksize);
		public uint WriteChunkSize {
			set {
				CheckRet(ftdi_write_data_set_chunksize(ref ftdi, value));
			}
			get {
				uint chunksize;
				CheckRet(ftdi_write_data_get_chunksize(ref ftdi, out chunksize));
				return chunksize;
			}
		}


		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_latency_timer(ref ftdi_context ftdi, byte latency);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_get_latency_timer(ref ftdi_context ftdi, out byte latency);
		public byte LatencyTimer {
			set {
				CheckRet(ftdi_set_latency_timer(ref ftdi, value));
			}
			get {
				byte latency;
				CheckRet(ftdi_get_latency_timer(ref ftdi, out latency));
				return latency;
			}
		}

		[DllImport("libftdi.so.0")] internal static extern string ftdi_get_error_string(ref ftdi_context ftdi);


		public string ErrorString { 
			get {
				return ftdi_get_error_string(ref ftdi);
			}
		}

		// "eeprom" needs to be valid 128 byte eeprom (generated by the eeprom generator)
		// the checksum of the eeprom is valided
		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_eeprom(ref ftdi_context ftdi, ref byte[] eeprom);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_eeprom(ref ftdi_context ftdi, ref byte[] eeprom);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_erase_eeprom(ref ftdi_context ftdi);

		public void WriteEEPROM(ftdi_eeprom eeprom)
		{
			byte[] data = new byte[128];

			ftdi_eeprom.ftdi_eeprom_build(ref eeprom, ref data); 

			CheckRet(ftdi_write_eeprom(ref ftdi, ref data));
		}

		public void EraseEEPROM()
		{
			CheckRet(ftdi_erase_eeprom(ref ftdi));
		}

		public byte[] ReadEEPROM()
		{
			/* FIXME */
			return null;
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_data(ref ftdi_context ftdi, ref byte[] buf, int size);
		public void ReadData(ref byte[] buf, int size)
		{
			CheckRet(ftdi_read_data(ref ftdi, ref buf, size));
		}
		
		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_data(ref ftdi_context ftdi, ref byte[] buf, int size);
		public void WriteData(byte[] buf, int size)
		{
			CheckRet(ftdi_write_data(ref ftdi, ref buf, size));
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_interface(ref ftdi_context ftdi, Interface iface);
		public Interface Interface { 
			set {
				CheckRet(ftdi_set_interface(ref ftdi, value));
			}
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_pins(ref ftdi_context ftdi, out byte pins);
		public byte GetPins() {
			byte pins;
			CheckRet(ftdi_read_pins(ref ftdi, out pins));
			return pins;
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_enable_bitbang(ref ftdi_context ftdi, byte bitmask);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_disable_bitbang(ref ftdi_context ftdi);

		public void EnableBitBang(byte bitmask) {
			CheckRet(ftdi_enable_bitbang(ref ftdi, bitmask));
		}

		public void DisableBitBang() {
			CheckRet(ftdi_disable_bitbang(ref ftdi));
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_bitmode(ref ftdi_context ftdi, byte bitmask, byte mode);
		public void SetBitMode(byte bitmask, byte mode) {
			CheckRet(ftdi_set_bitmode(ref ftdi, bitmask, mode));
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_line_property(ref ftdi_context ftdi, BitsType bits, StopBitsType sbit, ParityType parity);
		public void SetLineProperty(BitsType bits, StopBitsType sbit, ParityType parity) {
			CheckRet(ftdi_set_line_property(ref ftdi, bits, sbit, parity));
		}

		[StructLayout(LayoutKind.Sequential)] internal unsafe struct ftdi_device_list {
	
			internal ftdi_device_list *next;
			internal IntPtr dev;
		};

		[DllImport("libftdi.so.0")] internal unsafe static extern int ftdi_usb_find_all(ref ftdi_context ftdi, ftdi_device_list **devlist, int vendor, int product);
		[DllImport("libftdi.so.0")] internal unsafe static extern void ftdi_list_free(ftdi_device_list **devlist);

		public static unsafe IntPtr[] GetDeviceList(int vendor, int product) {
			ArrayList ar = new ArrayList();
			ftdi_context ftdi = new ftdi_context();

			ftdi_device_list *devlist, d;
			ftdi_init(ref ftdi);

			CheckRet(ftdi_usb_find_all(ref ftdi, &devlist, vendor, product));

			for (d = devlist; d != null; d = d->next) {
				ar.Add(d->dev);
			}

			ftdi_deinit(ref ftdi);

			ftdi_list_free(&devlist);

			return (IntPtr[])ar.ToArray(typeof(IntPtr));
		}
	}
		
//		There is no wrapper for libusb at the moment, so this is pointless:
//	[DllImport("libftdi.so.0")] internal unsafe static extern void ftdi_set_usbdev (ref ftdi_context ftdi, usb_dev_handle *usbdev);
}
