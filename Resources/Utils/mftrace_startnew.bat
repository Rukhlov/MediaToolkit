::SET _NT_SYMBOL_PATH=D:\SymbolCache
::SET /p pid="Set process id: " 
"%PROGRAMFILES(x86)%\Windows Kits\10\bin\x86\mftrace.exe" -es -k all -l 16 -o trace_MFPlayerTest.log -v "C:\Users\Alexander\source\repos\MFPlayerTest\Debug\MFPlayerTest.exe"