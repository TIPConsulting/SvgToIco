using SvgToIco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;


string svgPath_Small;
string svgPath_Medium;
string svgPath_Large;

if (args.Length == 1)
{
    svgPath_Small = args[0];
    svgPath_Medium = args[0];
    svgPath_Large = args[0];
}
else if (args.Length == 3)
{
    svgPath_Small = args[0];
    svgPath_Medium = args[1];
    svgPath_Large = args[2];
}
else
{
    Console.Write("Small SVG (16, 20, 24, 32): ");
    svgPath_Small = Console.ReadLine();

    Console.Write("Medium SVG (48, 64, 96): ");
    svgPath_Medium = Console.ReadLine();

    Console.Write("Large SVG (128, 256): ");
    svgPath_Large = Console.ReadLine();
}

if (!File.Exists(svgPath_Small) || Path.GetExtension(svgPath_Small).ToLower() != ".svg")
{
    throw new Exception("Must use valid small SVG");
}
if (!File.Exists(svgPath_Medium) || Path.GetExtension(svgPath_Medium).ToLower() != ".svg")
{
    throw new Exception("Must use valid medium SVG");
}
if (!File.Exists(svgPath_Large) || Path.GetExtension(svgPath_Large).ToLower() != ".svg")
{
    throw new Exception("Must use valid large SVG");
}



var parentDir = Path.GetDirectoryName(svgPath_Large);
var bmpDir = Path.Combine(parentDir, Path.GetFileNameWithoutExtension(svgPath_Large) + "_ico_" + Guid.NewGuid().ToString().Split('-')[0]);
Directory.CreateDirectory(bmpDir);
var finalDestIco = Path.Combine(parentDir, Path.GetFileNameWithoutExtension(svgPath_Large) + ".ico");

var sizes = new[] { 16, 20, 24, 32, 48, 64, 96, 128, 256 };
var outputPaths = new List<string>();
void bmpGenerator(int size)
{
    Bitmap bmp;
    var s = new Size(size, size);
    if (size < 48)
    {
        bmp = SvgTools.GetBmpFromSvg(svgPath_Small, s);
    }
    else if (size < 128)
    {
        bmp = SvgTools.GetBmpFromSvg(svgPath_Medium, s);
    }
    else
    {
        bmp = SvgTools.GetBmpFromSvg(svgPath_Large, s);
    }
    var outPath = Path.Combine(bmpDir, $"{size}.bmp");
    bmp.Save(outPath);
    outputPaths.Add(outPath);
}
foreach (var size in sizes)
{
    bmpGenerator(size);
}

var imagesString = string.Join(" ", outputPaths.Select(x => $"\"{x}\""));

var p = Process.Start("magick ", $"convert {imagesString} {finalDestIco}");
p.WaitForExit();
//Directory.Delete(bmpDir, true);

//use magick tool to validate file:
//magick identify [path]