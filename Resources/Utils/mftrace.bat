SET /p pid="Set process id: " 
"%PROGRAMFILES(x86)%\Windows Kits\10\bin\x86\mftrace.exe" -a %pid% -es -k all -l 16 -o -v trace_%pid%.txt