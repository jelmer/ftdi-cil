MCS = mcs

all:: FTDI.dll

FTDI.dll: ftdi.cs
	$(MCS) /unsafe /t:library /out:$@ $^
