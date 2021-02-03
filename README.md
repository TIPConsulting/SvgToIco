# About

This tool takes an SVG file and creates a high-quality ICO file with 9 sizes

16, 20, 24, 32, 48, 64, 96, 128, 256

The tool resizes the SVG for each size and generates a BMP.
The BMPs are then all added to an ICO file using [ImageMagick](https://imagemagick.org/index.php).
This process offers better quality than simply generating an ICO from the ImageMagick CLI.

## Use

You can either supply the SVG path as a startup argument or as text input when the program launches.
The ICO will be calculated and saved next to the source image with the same name (but ico ext).

### Single ICO file

`$ {path}/SvgToIco.exe "path/to/my/IcoFile.ico"`

### Progressive ICO file

`$ {path}/SvgToIco.exe "path/to/my/IcoFile_Small.ico" "path/to/my/IcoFile_Medium.ico" "path/to/my/IcoFile_Large.ico"`

## Prereqs

You must have [ImageMagick](https://imagemagick.org/index.php) installed and added to your system path.
