REM Platform options: "x86", "x64", "x64"
REM /p:Configuration="Debug" or "Release"




REM ************* Begin x86 *********************************************

"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%CD%\..\WarSetup.sln" /toolsversion:4.0 /p:Configuration="Release";Platform="x86" /t:clean;rebuild /m:4


del .\warsetup-fork-dot-net-4-x86 /Q
mkdir .\warsetup-fork-dot-net-4-x86
mkdir .\warsetup-fork-dot-net-4-x86\Licenses
mkdir .\warsetup-fork-dot-net-4-x86\CustomActions

copy ..\WarSetup\bin\x86\Release\WarPackager.exe .\warsetup-fork-dot-net-4-x86\WarPackager.exe /Y
copy ..\WarSetup\bin\x86\Release\WarPackager.exe.config .\warsetup-fork-dot-net-4-x86\WarPackager.exe.config /Y
copy "..\WarSetup\CustomActions\WarSetupPlugin.dll" ".\warsetup-fork-dot-net-4-x86\CustomActions\WarSetupPlugin.dll" /Y
copy "..\WarSetup\CustomActions\WarSetupPlugin.exp" ".\warsetup-fork-dot-net-4-x86\CustomActions\WarSetupPlugin.exp" /Y
copy "..\WarSetup\CustomActions\WarSetupPlugin.lib" ".\warsetup-fork-dot-net-4-x86\CustomActions\WarSetupPlugin.lib" /Y
copy "..\WarSetup\CustomActions\WarSetupPluginD.dll" ".\warsetup-fork-dot-net-4-x86\CustomActions\WarSetupPluginD.dll" /Y
copy "..\WarSetup\CustomActions\WarSetupPluginD.ilk" ".\warsetup-fork-dot-net-4-x86\CustomActions\WarSetupPluginD.ilk" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\Academic Free License (AFL) 3.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\Academic Free License (AFL) 3.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\Apache Software License 1.1.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\Apache Software License 1.1.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\Apache Software License 2.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\Apache Software License 2.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\Common Public License Version 1.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\Common Public License Version 1.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\Eclipse Public License  1.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\Eclipse Public License  1.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\GNU General Public License (GPL) 2.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\GNU General Public License (GPL) 2.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\GNU General Public License (GPL) 3.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\GNU General Public License (GPL) 3.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\GNU Lesser General Public License (LGPL) 2.1.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\GNU Lesser General Public License (LGPL) 2.1.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\GNU Lesser General Public License (LGPL) 3.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\GNU Lesser General Public License (LGPL) 3.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\IBM Public License Version 1.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\IBM Public License Version 1.0.rtf" /Y
copy "..\WarSetup\bin\x86\Release\Licenses\Open Software License (OSL)  3.0.rtf" ".\warsetup-fork-dot-net-4-x86\Licenses\Open Software License (OSL)  3.0.rtf" /Y

7za.exe a warsetup-fork-dot-net-4-x86.zip warsetup-fork-dot-net-4-x86\

REM ************* End x86 *********************************************
