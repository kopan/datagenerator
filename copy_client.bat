@echo off

xcopy Assets\DataScripts\*.cs %1\Assets\2_Script\DataScripts\ /e /h /k /y
xcopy resources\xeData\*.xml %1\Assets\StreamingAssets\xeData\ /e /h /k /y
xcopy resources\Text\*.* %1\Assets\StreamingAssets\Text\ /e /h /k /y