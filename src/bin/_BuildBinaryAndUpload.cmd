SETLOCAL
SET BATCH_FILE_NAME=%0
SET BATCH_DIR_NAME=%0\..

for /f "usebackq tokens=*" %%i in (`"%BATCH_DIR_NAME%\..\..\submodules\IPA-DN-ThinLib\src\BuildFiles\Utility\vswhere.exe" -version [17.0^,18.0^) -sort -requires Microsoft.Component.MSBuild -find Common7\Tools\VsDevCmd.bat`) do (
    if exist "%%i" (
        call "%%i"
    )
)

echo on

cd /D "%~dp0"

rmdir /s /q c:\tmp\NativeWinApp_built_binary\
mkdir c:\tmp\NativeWinApp_built_binary\

C:\git\IPA-DN-Cores\Cores.NET\Dev.Tools\CompiledBin\WriteTimeStamp.exe > c:\tmp\NativeWinApp_built_binary\TimeStamp.txt

del %BATCH_DIR_NAME%\bin\BuildRelease.exe

msbuild /target:Rebuild /property:Configuration=Release,Platform=x64 "%BATCH_DIR_NAME%\..\IPA-DN-ThinLib-NativeWinApp-VS2022.sln"
IF ERRORLEVEL 1 GOTO LABEL_ERROR

%BATCH_DIR_NAME%\BuildRelease.exe /CMD:BuildHamcore
IF ERRORLEVEL 1 GOTO LABEL_ERROR

copy /y %BATCH_DIR_NAME%\NativeWinApp_x64.exe c:\tmp\NativeWinApp_built_binary\NativeWinApp_x64.exe
IF ERRORLEVEL 1 GOTO LABEL_ERROR

copy /y %BATCH_DIR_NAME%\hamcore.se2 c:\tmp\NativeWinApp_built_binary\hamcore.se2
IF ERRORLEVEL 1 GOTO LABEL_ERROR

S:\CommomDev\SE-DNP-CodeSignClientApp\SE-DNP-CodeSignClientApp_signed.exe SignDir c:\tmp\NativeWinApp_built_binary\ /CERT:SoftEtherEv /COMMENT:'NativeWinApp_x64"

cd /d c:\tmp\NativeWinApp_built_binary\

c:\tmp\NativeWinApp_built_binary\NativeWinApp_x64.exe Hello

IF NOT "%ERRORLEVEL%" == "0" GOTO L_END

call H:\Secure\230326_Upload_NativeWinApp\lts_upload_url_with_password.cmd

c:\windows\system32\curl.exe --insecure %lts_upload_url_with_password% -k -f -F "json=false" -F "getfile=false" -F "getdir=true" -F "file=@NativeWinApp_x64.exe" -F "file=@hamcore.se2" -F "file=@TimeStamp.txt"


echo ALL ok!

pause

EXIT 0


:LABEL_ERROR

echo Error occured: %ERRORLEVEL%

pause

EXIT %ERRORLEVEL%

