MCS = mcs

all:: FTDI.dll

FTDI.dll: ftdi.cs
	$(MCS) /t:library /out:$@ $^
