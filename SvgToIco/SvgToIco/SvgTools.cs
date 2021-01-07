using Svg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SvgToIco
{
    /// <summary>
    /// SVG helper tools
    /// </summary>
    public class SvgTools
    {
        private static readonly ConcurrentDictionary<BmpCacheIdentifier, Bitmap> _bmpCacheMap = new();


        private static SvgDocument AdjustSize(SvgDocument document, in Size maxSize)
        {
            var calcWidth = (int)((document.Width / (double)document.Height) * maxSize.Height);
            var calcHeight = (int)((document.Height / (double)document.Width) * maxSize.Width);

            //in theory, this should maximize the size of the image for the constrained space
            if (calcWidth <= maxSize.Width)
            {
                document.Width = calcWidth;
                document.Height = maxSize.Height;
            }
            else if (calcHeight <= maxSize.Height)
            {
                document.Width = maxSize.Width;
                document.Height = calcHeight;
            }
            else
            {
                throw new InvalidOperationException("Unable to calculate valid image size");
            }

            return document;
        }

        /// <summary>
        /// Get processed SVG document that applies scaling factor and modifier
        /// </summary>
        /// <param name="FilePath">SVG file</param>
        /// <param name="MaximumSize">Bounding size fit box</param>
        /// <param name="ScalingFactor">Scale factor applied after setting bounding box size.  Range: [0, 10]</param>
        /// <param name="SvgPaintModifier">Modifier function applied to every svg element in the document</param>
        /// <returns></returns>
        public static SvgDocument GetSvgDocument(string FilePath, in Size MaximumSize, double ScalingFactor = 1.0, Action<SvgElement> SvgPaintModifier = null)
        {
            if (FilePath is null)
            {
                throw new ArgumentNullException(nameof(FilePath));
            }

            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException($"{nameof(FilePath)} path not found", FilePath);
            }

            if (double.IsNaN(ScalingFactor) || double.IsInfinity(ScalingFactor))
            {
                throw new ArgumentException($"{nameof(ScalingFactor)} must be between 0 and 10 (inclusive)");
            }


            if (ScalingFactor < 0 || ScalingFactor > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(ScalingFactor), $"{nameof(ScalingFactor)} must be positive, between 0 and 10 (inclusive)");
            }

            try
            {
                SvgDocument document = SvgDocument.Open(FilePath);
                var doc = AdjustSize(document, in MaximumSize);
                doc.Width = (int)(doc.Width * ScalingFactor);
                doc.Height = (int)(doc.Height * ScalingFactor);


                if (SvgPaintModifier is not null)
                {
                    Stack<SvgElement> elementStack = new();
                    foreach (SvgElement item in doc.Children)
                    {
                        elementStack.Push(item);
                    }

                    while (elementStack.Count > 0)
                    {
                        var itm = elementStack.Pop();
                        SvgPaintModifier?.Invoke(itm);
                        foreach (SvgElement child in itm.Children)
                        {
                            elementStack.Push(child);
                        }
                    }
                }

                return doc;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to perform required operation with SVG file [{FilePath}]", ex);
            }
        }


        /// <summary>
        /// Convert SVG to bmp image while applying scale and tranform modifiers.
        /// Bitmaps are cached when possible to prevent duplicate resource use.
        /// </summary>
        /// <param name="FilePath">SVG file</param>
        /// <param name="MaximumSize">Bounding size fit box</param>
        /// <param name="ScalingFactor">Scale factor applied after setting bounding box size.  Range: [0, 10]</param>
        /// <param name="SvgPaintModifier">Modifier function applied to every svg element in the document</param>
        /// <returns></returns>
        /// <remarks>
        /// Caching Policy: 
        /// BMP cache is used if the file matches and the final output size is the same for multiple SVG documents.
        /// Cache is ignored when using SvgPaintModifier argument
        /// </remarks>
        public static Bitmap GetBmpFromSvg(string FilePath, in Size MaximumSize, double ScalingFactor = 1.0, Action<SvgElement> SvgPaintModifier = null)
        {
            SvgDocument document = GetSvgDocument(FilePath, in MaximumSize, ScalingFactor, SvgPaintModifier);
            //dont cache images with modifiers
            //we cant guarantee that modifiers are pure
            //so caching them could cause bad reads if the result varies based on state variables
            if (SvgPaintModifier is not null)
            {
                return document.Draw();
            }

            var id = new BmpCacheIdentifier()
            {
                FilePath = FilePath,
                FinalSize = new Size((int)document.Width.Value, (int)document.Height.Value)
            };

            var img = _bmpCacheMap.GetOrAdd(id, (key) => document.Draw());
            return img;
        }

        private class BmpCacheIdentifier
        {
            public string FilePath { get; set; }
            public Size FinalSize { get; set; }
            public override bool Equals(object obj)
            {
                if (obj is BmpCacheIdentifier bmp2)
                {
                    return this.GetHashCode() == bmp2.GetHashCode();
                }
                else
                    return false;
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(FilePath, FinalSize);
            }
        }

    }
}
