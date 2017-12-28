
REM "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" "%CD%\..\WarSetup.sln" /toolsversion:4.0 /p:Configuration="Release";Platform="x86" /t:clean;rebuild /m:4

"C:\Program Files (x86)\MajorSilence\War Setup Fork\WarPackager.exe" -i "..\Setup\WarSetup.warsetup" -c -e


