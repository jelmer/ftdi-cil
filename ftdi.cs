// 
// CIL bindings for the FTDI library
//
// Copyright (C) 2006 Jelmer Vernooij <jelmer@palmsens.com>
// Licensed under the GNU Lesser Public License
//

using System;
using System.Runtime.InteropServices;

namespace FTDI 
{
	enum ChipType { TYPE_AM=0, TYPE_BM=1, TYPE_2232C=2 };
	enum ParityType { NONE=0, ODD=1, EVEN=2, MARK=3, SPACE=4 };
	enum StopBitsType { STOP_BIT_1=0, STOP_BIT_15=1, STOP_BIT_2=2 };
	enum BitsType { BITS_7=7, BITS_8=8 };

	enum MpsseMode {
	    BITMODE_RESET  = 0x00,
	    BITMODE_BITBANG= 0x01,
	    BITMODE_MPSSE  = 0x02,
	    BITMODE_SYNCBB = 0x04,
	    BITMODE_MCU    = 0x08,
	    BITMODE_OPTO   = 0x10
	};

	/* Port interface code for FT2232C */
	enum Interface {
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
	
//	[StructLayout(LayoutKind.Sequential)] internal struct ftdi_device_list {
//		ftdi_device_list *next;
//		IntPtr dev;
//	};
	
	[StructLayout(LayoutKind.Sequential)] internal struct ftdi_eeprom {
		// init and build eeprom from ftdi_eeprom structure
		[DllImport("libftdi.so.0")] internal static extern int  ftdi_eeprom_build(ref ftdi_eeprom eeprom, ref byte[] output);
		[DllImport("libftdi.so.0")] internal static extern void ftdi_eeprom_initdefaults(ref ftdi_eeprom eeprom);

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

	class FTDIContext {
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

		~FTDIContext() {
			ftdi_deinit(ref ftdi);
		}

		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_baudrate(ref ftdi_context ftdi, int baudrate);
		public int Baudrate { 
			set {
				CheckRet(ftdi_set_baudrate(ref ftdi, value));
			}
		}

		private void CheckRet(int ret) {
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
	}
	
	internal class Native {
		/* Value Low */
		/* Value HIGH */ /*rate is 12000000/((1+value)*2) */
		int DIV_VALUE(int rate) {
			return (rate > 6000000)?0:((6000000/rate -1) > 0xffff)? 0xffff: (6000000/rate -1);
		}
		
		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_interface(ref ftdi_context ftdi, Interface iface);
	
	//	[DllImport("libftdi.so.0")] internal static extern void ftdi_set_usbdev (ftdi_context *ftdi, usb_dev_handle *usbdev);
		
//		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_find_all(ref ftdi_context ftdi, ftdi_device_list **devlist, int vendor, int product);
//		[DllImport("libftdi.so.0")] internal static extern void ftdi_list_free(ftdi_device_list **devlist);
		
		[DllImport("libftdi.so.0")] internal static extern int ftdi_usb_open_dev(ref ftdi_context ftdi, IntPtr dev);
		
	
		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_line_property(ref ftdi_context ftdi, BitsType bits, StopBitsType sbit, ParityType parity);
	
		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_data(ref ftdi_context ftdi, ref byte[] buf, int size);

		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_data_set_chunksize(ref ftdi_context ftdi, uint chunksize);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_data_get_chunksize(ref ftdi_context ftdi, out uint chunksize);
	
		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_data(ref ftdi_context ftdi, ref byte[] buf, int size);

		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_data_set_chunksize(ref ftdi_context ftdi, uint chunksize);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_data_get_chunksize(ref ftdi_context ftdi, out uint chunksize);
	
		[DllImport("libftdi.so.0")] internal static extern int ftdi_enable_bitbang(ref ftdi_context ftdi, byte bitmask);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_disable_bitbang(ref ftdi_context ftdi);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_bitmode(ref ftdi_context ftdi, byte bitmask, byte mode);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_pins(ref ftdi_context ftdi, out byte pins);
	
		[DllImport("libftdi.so.0")] internal static extern int ftdi_set_latency_timer(ref ftdi_context ftdi, byte latency);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_get_latency_timer(ref ftdi_context ftdi, out byte latency);
	
	
		// "eeprom" needs to be valid 128 byte eeprom (generated by the eeprom generator)
		// the checksum of the eeprom is valided
		[DllImport("libftdi.so.0")] internal static extern int ftdi_read_eeprom(ref ftdi_context ftdi, ref byte[] eeprom);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_write_eeprom(ref ftdi_context ftdi, ref byte[] eeprom);
		[DllImport("libftdi.so.0")] internal static extern int ftdi_erase_eeprom(ref ftdi_context ftdi);
	
		[DllImport("libftdi.so.0")] internal static extern string ftdi_get_error_string(ref ftdi_context ftdi);
	}
}
