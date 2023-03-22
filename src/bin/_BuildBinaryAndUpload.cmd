SETLOCAL
SET BATCH_FILE_NAME=%0
SET BATCH_DIR_NAME=%0\..

for /f "usebackq tokens=*" %%i in (`"%BATCH_DIR_NAME%\..\submodules\IPA-DN-ThinLib\src\BuildFiles\Utility\vswhere.exe" -version [17.0^,18.0^) -sort -requires Microsoft.Component.MSBuild -find Common7\Tools\VsDevCmd.bat`) do (
    if exist "%%i" (
        call "%%i"
    )
)

echo on

cd /D "%~dp0"

rmdir /s /q c:\tmp\nativeutilapp_built_binary\
mkdir c:\tmp\nativeutilapp_built_binary\

C:\git\IPA-DN-Cores\Cores.NET\Dev.Tools\CompiledBin\WriteTimeStamp.exe > c:\tmp\nativeutilapp_built_binary\TimeStamp.txt

del %BATCH_DIR_NAME%\bin\BuildRelease.exe

msbuild /target:Clean /property:Configuration=Debug "%BATCH_DIR_NAME%\BuildRelease\BuildRelease.csproj"
IF ERRORLEVEL 1 GOTO LABEL_ERROR

msbuild /target:Rebuild /property:Configuration=Debug "%BATCH_DIR_NAME%\BuildRelease\BuildRelease.csproj"
IF ERRORLEVEL 1 GOTO LABEL_ERROR

msbuild /target:Rebuild /property:Configuration=Release,Platform=x64 "%BATCH_DIR_NAME%\NativeUtilApp\NativeUtilApp.vcxproj"
IF ERRORLEVEL 1 GOTO LABEL_ERROR

bin\BuildRelease.exe /CMD:BuildHamcore
IF ERRORLEVEL 1 GOTO LABEL_ERROR

copy /y bin\NativeUtilApp_x64.exe c:\tmp\nativeutilapp_built_binary\NativeUtilApp_x64.exe
IF ERRORLEVEL 1 GOTO LABEL_ERROR

S:\CommomDev\SE-DNP-CodeSignClientApp\SE-DNP-CodeSignClientApp_signed.exe SignDir c:\tmp\nativeutilapp_built_binary\ /CERT:SoftEtherEv /COMMENT:'NativeUtilApp_x64"

cd /d c:\tmp\nativeutilapp_built_binary\

c:\tmp\nativeutilapp_built_binary\NativeUtilApp_x64.exe Hello

IF NOT "%ERRORLEVEL%" == "0" GOTO L_END

rem call H:\Secure\220623_Upload_CoresLib_DevTest\lts_upload_url_with_password.cmd

rem c:\windows\system32\curl.exe --insecure %lts_upload_url_with_password% -k -f -F "json=false" -F "getfile=false" -F "getdir=true" -F "file=@Dev.Test.Win.x86_64.exe" -F "file=@Dev.Test.Win.aarch64.exe" -F "file=@Dev.Test.Linux.x86_64" -F "file=@Dev.Test.Linux.aarch64" -F "file=@TimeStamp.txt"


echo ALL ok!

pause

EXIT 0


:LABEL_ERROR

echo Error occured: %ERRORLEVEL%

pause

EXIT %ERRORLEVEL%

