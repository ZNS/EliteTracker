set doBuild=false
if "%2" == "DebugWithBower" set doBuild=true
if "%2" == "Release" set doBuild=true

if "%doBuild%" == "true" goto Build
goto:eof

:Build
set PATH=%1.bin;%PATH%
cd %~dp0
IF NOT EXIST %~dp0\node_modules\grunt call npm install
call bower install
call grunt copy
goto:eof