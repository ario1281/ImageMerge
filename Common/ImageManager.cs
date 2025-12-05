using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace ImageMerge.Common
{
    public struct RawFile
    {
        public Bitmap image;
        public string group;
        public int    number;
        public string suffix;
    }

    internal class ImageManager
    {
        private static readonly Regex m_regex
            = new Regex(
                @"^(?<group>[a-z])(?<num>\d{3})(?<suffix>j)?\.png$",
                RegexOptions.IgnoreCase
            );

        public static bool FileAnalysis(string dir, out List<RawFile?> rawFiles)
        {
            rawFiles = Directory.GetFiles(dir, "*.png")
                .Select(path =>
                {
                    var name = Path.GetFileName(path);
                    var m = m_regex.Match(name);
                    if (!m.Success) return (RawFile?)null;

                    return new RawFile
                    {
                        image  = new Bitmap(path),
                        group  = m.Groups["group"].Value.ToLower(),
                        number = int.Parse(m.Groups["num"].Value),
                        suffix = m.Groups["suffix"].Value
                    };
                })
                .Where(x => x != null)
                .ToList();

            return rawFiles.Any();
        }

        public static Image DrawImage(List<RawFile> rawFiles)
        {
            var outImg = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

            foreach (var rawFile in rawFiles)
            {
                MergeImage(outImg, rawFile.image);
            }

            return outImg;
        }

        public static void SaveImage(Image img, string outDir, IProgress<int> progress = null)
        {
            Directory.CreateDirectory(outDir);
            img.Save(outDir, ImageFormat.Png);
        }

        private static Image MergeImage(in Image @base, in Image merge, float scale = 1f, float opacity = 1f)
        {
            var result = new Bitmap(@base.Width, @base.Height, PixelFormat.Format32bppArgb);

            // calc scale
            int width = Math.Max(1, (int)Math.Round(merge.Width * scale));
            int height = Math.Max(1, (int)Math.Round(merge.Height * scale));

            using (var g = Graphics.FromImage(result))
            using (var _merge = new Bitmap(merge, new Size(width, height)))
            {
                // quality seting
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(@base, 0, 0, @base.Width, @base.Height);

                // setting alpha
                float alpha = Math.Max(0f, Math.Min(1f, opacity));
                var cm = new ColorMatrix
                {
                    Matrix00 = 1f,
                    Matrix11 = 1f,
                    Matrix22 = 1f,
                    Matrix33 = alpha,
                    Matrix44 = 1f
                };

                using (var attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(cm);

                    var destRect = new Rectangle(0, 0, _merge.Width, _merge.Height);

                    g.DrawImage(
                        _merge,
                        destRect,
                        0, 0, _merge.Width, _merge.Height,
                        GraphicsUnit.Pixel,
                        attrs
                    );
                }
            }

            return result;
        }
    }
}
