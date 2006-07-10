INSTALL = install
MCS = mcs
GACUTIL = gacutil
prefix = /usr

all:: FTDI.dll ftdi-test.exe

FTDI.dll: ftdi.cs AssemblyInfo.cs
	$(MCS) /unsafe /t:library /out:$@ $^

ftdi-test.exe: ftdi-test.cs FTDI.dll
	$(MCS) /out:$@ /r:FTDI ftdi-test.cs

install:: all
	$(GACUTIL) -i FTDI.dll
	$(INSTALL) -d $(prefix)/lib/ftdi
	$(INSTALL) FTDI.dll $(prefix)/lib/ftdi	
	$(INSTALL) ftdi-sharp.pc $(prefix)/lib/pkgconfig
