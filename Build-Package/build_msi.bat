
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%CD%\..\WarSetup.sln" /toolsversion:4.0 /p:Configuration="Release";Platform="x86" /t:clean;rebuild /m:4

"C:\Program Files (x86)\MajorSilence\War Setup Fork\WarPackager.exe" -i "..\Setup\WarSetup.warsetup" -c -e


