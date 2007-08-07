prefix = /usr
INSTALL = install
XBUILD = xbuild
GACUTIL = gacutil
destdir = $(prefix)/lib/mono/ftdi

all:: FTDI.dll ftdi-test.exe

FTDI.dll: ftdi-cil.csproj ftdi.cs AssemblyInfo.cs
	$(XBUILD) ftdi-cil.csproj

ftdi-test.exe: ftdi-test.cs FTDI.dll ftdi-test.csproj
	$(XBUILD) ftdi-test.csproj

install:: all
	$(GACUTIL) -i FTDI.dll
	$(INSTALL) -d $(destdir)
	$(INSTALL) FTDI.dll $(destdir)
	$(INSTALL) ftdi-sharp.pc $(prefix)/lib/pkgconfig
