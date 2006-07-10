prefix = /usr
INSTALL = install
MCS = mcs
GACUTIL = gacutil
destdir = $(prefix)/lib/mono/ftdi

all:: FTDI.dll ftdi-test.exe

FTDI.dll: ftdi.cs AssemblyInfo.cs
	$(MCS) /unsafe /t:library /out:$@ $^

ftdi-test.exe: ftdi-test.cs FTDI.dll
	$(MCS) /out:$@ /r:FTDI ftdi-test.cs

install:: all
	$(GACUTIL) -i FTDI.dll
	$(INSTALL) -d $(destdir)
	$(INSTALL) FTDI.dll $(destdir)
	$(INSTALL) ftdi-sharp.pc $(prefix)/lib/pkgconfig
