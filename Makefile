MCS = mcs

all:: FTDI.dll ftdi-test.exe

FTDI.dll: ftdi.cs
	$(MCS) /unsafe /t:library /out:$@ $^

ftdi-test.exe: ftdi-test.cs FTDI.dll
	$(MCS) /out:$@ /r:FTDI ftdi-test.cs
