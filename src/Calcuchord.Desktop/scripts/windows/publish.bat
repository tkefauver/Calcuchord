rmdir /S /Q out
dotnet publish ..\..\Calcuchord.Desktop.csproj -r win-x64 -c Release -o out
rename "out\*.exe" Calcuchord.exe
makensis .\installer.nsi
