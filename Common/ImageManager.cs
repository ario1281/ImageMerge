using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
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
        public int number;
        public string suffix;
    }

    internal class ImageManager
    {
        public static bool FileAnalysis(string dir, out List<RawFile?> rawFiles)
        {
            rawFiles = new List<RawFile?>();

            var initFiles = Directory.GetFiles(dir, "*.png")
                .Select(path =>
                {
                    var name = Path.GetFileName(path);
                    Regex regex
                        = new Regex(
                            @"^(?<group>[a-z])(?<number>\d{3})(?<suffix>j)?\.png$",
                            RegexOptions.IgnoreCase
                        );
                    var m = regex.Match(name);
                    if (!m.Success) return (RawFile?)null;

                    return new RawFile
                    {
                        image = new Bitmap(path),
                        group = m.Groups["group"].Value.ToLower(),
                        number = int.Parse(m.Groups["number"].Value),
                        suffix = m.Groups["suffix"].Value
                    };
                })
                .Where(x => x != null)
                .ToList();

            if (!initFiles.Any())
            {
                return false;
            }


            var groupFiles = initFiles.GroupBy(rf => new { rf.Value.group, rf.Value.number });

            foreach (var group in groupFiles)
            {
                string grpKey = $"{group.Key.group}{group.Key.number}";
                RawFile baseFile = (RawFile)group.FirstOrDefault(rf => string.IsNullOrEmpty(rf.Value.suffix));

                if (string.IsNullOrEmpty(baseFile.suffix) == false)
                {
                    Console.WriteLine($"警告: 基本ファイルが見つかりません: {grpKey}。このグループはスキップされます。");
                    foreach (var dispose in group)
                    {
                        dispose.Value.image?.Dispose();
                    }
                    continue;
                }

                var currBaseFile = baseFile;

                var mergeFiles = group
                    .Where(rf => !string.IsNullOrEmpty(rf.Value.suffix))
                    .OrderBy(rf => rf.Value.suffix)
                    .ToList();

                foreach (var mergeFile in mergeFiles)
                {
                    currBaseFile.image = MergeImage(currBaseFile.image, mergeFile.Value.image, true);
                }
                rawFiles.Add(currBaseFile);
            }

            return rawFiles.Any();
        }

        public static Image DrawImage(List<RawFile> rawFiles)
        {
            if (rawFiles == null || rawFiles.Count <= 0)
            {
                return new Bitmap("");
            }

            var image = rawFiles[1].image;
            var size = image != null ? new Size(image.Width, image.Height) : new Size(1, 1);
            var outImg = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

            foreach (var rawFile in rawFiles)
            {
                outImg = MergeImage(outImg, rawFile.image);
            }

            return outImg;
        }

        public static void SaveImage(Image img, string outDir, IProgress<int> progress = null)
        {
            Directory.CreateDirectory(outDir);

            string baseName = "output";
            string ext = ".png";
            string filePath = Path.Combine(outDir, baseName + ext);

            int cnt = 0;

            do
            {
                filePath = Path.Combine(outDir, $"{baseName}_{cnt:000}{ext}");
                cnt++;
            }
            while (File.Exists(filePath));

            img.Save(filePath, ImageFormat.Png);
        }

        private static Bitmap MergeImage(Image _in1, Image _in2, bool isInv = false, float scale = 1f, float opacity = 1f)
        {
            _in1 = _in1 ?? new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            _in2 = _in2 ?? new Bitmap(1, 1, PixelFormat.Format32bppArgb);

            var baseImg = !isInv ? _in1 : _in2;
            var mergeImg = isInv ? _in1 : _in2;

            int w = Math.Max(baseImg.Width, mergeImg.Width);
            int h = Math.Max(baseImg.Height, mergeImg.Height);
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            // calc scale
            int mw = Math.Max(1, (int)Math.Round(mergeImg.Width * scale));
            int mh = Math.Max(1, (int)Math.Round(mergeImg.Height * scale));

            using (var g = Graphics.FromImage(result))
            using (var smi = new Bitmap(mergeImg, new Size(mw, mh)))
            {
                // quality seting
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(baseImg, 0, 0, baseImg.Width, baseImg.Height);

                // setting alpha
                float alpha = Math.Max(0f, Math.Min(1f, opacity));
                var cm = new ColorMatrix();
                cm.Matrix33 = alpha;

                using (var attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(cm);

                    g.DrawImage(
                        smi,
                        new Rectangle(0, 0, smi.Width, smi.Height),
                        0, 0, smi.Width, smi.Height,
                        GraphicsUnit.Pixel,
                        attrs
                    );
                }
            }

            return result;
        }
    }
}
