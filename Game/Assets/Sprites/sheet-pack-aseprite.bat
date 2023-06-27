@ECHO OFF
setlocal enabledelayedexpansion
for %%f in (..\*_0.png) do (
	set newname=%%~nf
	echo newname: %new_name%
	echo %%f
)