SET /p pid="Set process id: " 
"%PROGRAMFILES(x86)%\Windows Kits\8.0\bin\x86\mftrace.exe" -a %pid% -es -k all -l 16 -o trace_%pid%.log -v