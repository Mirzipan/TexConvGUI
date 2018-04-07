# TexConvGUI
Simple GUI window for a more comfortable usage of texconv utility, inspired by [convert-to-dds-util](https://github.com/frkn/convert-to-dds-util)

## Requirements
This tool needs texconv.exe in the same directory to run. You can either use the included version, or download a newer one from [DirectXTex](https://github.com/Microsoft/DirectXTex/releases).

## Usage
1. Select files you want to convert, either by clicking the button, or by drag'n'drop. Any DDS files will be converted to PNG, and any non-DDS files will be converted to DDS, using the specified options.
2. Select output directory, it is set to your Desktop by default.
3. Click 'convert' button.
4. If there are any issues converting some of the files, there will be an error message per each failed file.
5. When everything has either failed, or been converted, a message will be displayed to let you know.

## Executable
If you don't feel like compiling this yourself, you can grab the .exe [here](https://github.com/Mirzipan/TexConvGUI/blob/master/TexConvGUI/bin/x64/Release/TexConvGUI.exe).
