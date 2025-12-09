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
                string _key = $"{group.Key.group}{group.Key.number}";
                RawFile baseFile = (RawFile)group.FirstOrDefault(rf => string.IsNullOrEmpty(rf.Value.suffix));

                if (!string.IsNullOrEmpty(baseFile.suffix))
                {
                    Console.WriteLine($"警告: 基本ファイルが見つかりません: {_key}。このグループはスキップされます。");
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
            var result = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

            if (rawFiles != null && rawFiles.Count > 0)
            {
                var img = rawFiles[1].image;
                var size = img != null ? new Size(img.Width, img.Height) : new Size(1, 1);

                result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

                foreach (var rawFile in rawFiles)
                {
                    result = MergeImage(result, rawFile.image);
                }
            }

            return result;
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

        /// <summary>
        /// 2枚の画像を1枚に合成する。重ね順の反転可能。<br/>
        /// 大きさが異なる場合、高幅それぞれ大きい方に合わせる。
        /// </summary>
        /// <param name="_in1">入力画像1（通常:前面）</param>
        /// <param name="_in2">入力画像2（通常:背面）</param>
        /// <param name="isInv">画像の重ね順を反転</param>
        /// <returns>合成画像（Bitmap）</returns>
        private static Bitmap MergeImage(Bitmap _in1, Bitmap _in2, bool isInv = false)
        {
            if (_in1 == null || _in2 == null)
            {
                string err = "";
                err += _in1 == null ? $"{nameof(_in1)} & " : "";
                err += _in2 == null ? $"{nameof(_in2)}" : "";
                throw new ArgumentNullException(err, "入力画像が null です。");
            }

            var frnt = isInv ? _in2 : _in1;
            var back = isInv ? _in1 : _in2;

            int w = Math.Max(frnt.Width, back.Width);
            int h = Math.Max(frnt.Height, back.Height);

            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(result))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(frnt, 0, 0, frnt.Width, frnt.Height);

                g.DrawImage(
                    back,
                    new Rectangle(0, 0, back.Width, back.Height),
                    0, 0, back.Width, back.Height,
                    GraphicsUnit.Pixel
                );
            }

            return result;
        }

        /// <summary>
        /// 画像を指定の倍率で拡縮します（高品質変換）。
        /// </summary>
        /// <param name="_in">入力画像</param>
        /// <param name="scale">拡縮率</param>
        /// <returns>リサイズ後画像</returns>
        private static Bitmap ScaleImage(Bitmap _in, float scale)
        {
            if (_in == null)
            {
                throw new ArgumentNullException(nameof(_in), "入力画像が null です。");
            }
            // calc scale
            int sw = Math.Max(1, (int)Math.Round(_in.Width * scale));
            int sh = Math.Max(1, (int)Math.Round(_in.Height * scale));

            var result = new Bitmap(sw, sh, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(result))
            {
                // quality seting
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(
                    _in,
                    new Rectangle(0, 0, sw, sh),
                    0, 0, _in.Width, _in.Height,
                    GraphicsUnit.Pixel
                );
            }

            return result;
        }

        /// <summary>
        /// 画像を指定の不透明度で出力します（高品質変換）。
        /// </summary>
        /// <param name="_in">入力画像</param>
        /// <param name="opacity">不透明度</param>
        /// <returns>リサイズ後画像</returns>
        private static Bitmap OpacityImage(Image _in, float opacity)
        {
            if (_in == null)
            {
                throw new ArgumentNullException(nameof(_in), "入力画像が null です。");
            }

            var result = new Bitmap(_in.Width, _in.Height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(result))
            {
                // quality seting
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float alpha = Math.Max(0f, Math.Min(1f, opacity));
                var cm = new ColorMatrix();
                cm.Matrix33 = alpha;

                using (var attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(cm);

                    g.DrawImage(
                        _in,
                        new Rectangle(0, 0, _in.Width, _in.Height),
                        0, 0, _in.Width, _in.Height,
                        GraphicsUnit.Pixel,
                        attrs
                    );
                }
            }

            return result;
        }
    }
}
