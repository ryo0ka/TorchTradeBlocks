@echo off
cmd 

set PLUGINS_DIRS="C://torches/torch-general/Plugins" "C://torches/torch-earthmoon/Plugins"
set TORCH_EXE_PATHS="C://torches/torch-general/Torch.Server.exe" "C://torches/torch-earthmoon/Torch.Server.exe"
echo "plugins output path: %PLUGINS_DIRS%, exe: %TORCH_EXE_PATHS%"

REM Prerequisites:
REM * This batch file is executed in the build folder;
REM * This batch file is located in the project folder;
REM * Manifest file is located in the project folder (same as this batch file);
REM * DLL name equals to the project name;
REM * 7za (7-Zip commandline tool) is installed and the path is set.
REM * cmdow (ritchielawrence.github.io/cmdow) is installed and the path is set.

set PROJ_NAME=%1
set SOLUTION_DIR=%2
set BUILD_DIR=%CD%\
set PROJ_DIR=%~dp0
echo "build path: %BUILD_DIR%, project path: %PROJ_DIR%, project name: %PROJ_NAME%"

set MANIFEST_PATH="%PROJ_DIR%manifest.xml"

TorchPluginPackager ^
    -n %PROJ_NAME% ^
    -m %MANIFEST_PATH% ^
    -b %BUILD_DIR% ^
    -r "%SOLUTION_DIR%TorchBinaries" "%SOLUTION_DIR%GameBinaries" ^
    -o %PLUGINS_DIRS% ^
    -e "Nexus.dll"

REM Restarts the torch app. Comment out below if you don't want it.

taskkill /f /im "Torch.Server.exe" /fi "memusage gt 2"
taskkill /f /im "Torch.Server.exe" /fi "memusage gt 2"
taskkill /f /im "Torch.Server.exe" /fi "memusage gt 2"

for %%p in (%TORCH_EXE_PATHS%) do cmdow /run %%p

echo "done"