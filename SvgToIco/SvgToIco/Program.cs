using SvgToIco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;


string svgPath;
if (args.Length == 1)
{
    svgPath = args[0];
}
else
{
    svgPath = Console.ReadLine();
}

if (!File.Exists(svgPath) || Path.GetExtension(svgPath).ToLower() != ".svg")
{
    throw new Exception("Must use valid SVG");
}
var parentDir = Path.GetDirectoryName(svgPath);
var bmpDir = Path.Combine(parentDir, Path.GetFileNameWithoutExtension(svgPath) + "_ico_" + Guid.NewGuid().ToString().Split('-')[0]);
Directory.CreateDirectory(bmpDir);
var finalDestIco = Path.Combine(parentDir, Path.GetFileNameWithoutExtension(svgPath) + ".ico");

var sizes = new[] { 16, 20, 24, 32, 48, 64, 96, 128, 256 };
var outputPaths = new List<string>();
void bmpGenerator(int size)
{
    var bmp = SvgTools.GetBmpFromSvg(svgPath, new Size(size, size));
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
Directory.Delete(bmpDir, true);
