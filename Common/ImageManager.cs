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
                    if (!m.Success) { return (RawFile?)null; }

                    try
                    {
                        return new RawFile
                        {
                            image = new Bitmap(path),
                            group = m.Groups["group"].Value.ToLower(),
                            number = int.Parse(m.Groups["number"].Value),
                            suffix = m.Groups["suffix"].Value
                        };
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(rf => rf != null)
                .ToList();

            if (initFiles.Count <= 0) { return false; }



            var groupFiles = initFiles.GroupBy(rf => new { rf.Value.group, rf.Value.number });
            foreach (var group in groupFiles)
            {
                var baseFile = group.FirstOrDefault(rf => string.IsNullOrEmpty(rf.Value.suffix));

                if (baseFile == null)
                {
                    foreach (var rf in group)
                    {
                        rf.Value.image?.Dispose();
                    }
                    continue;
                }

                var mergeFiles = group
                    .Where(rf => !string.IsNullOrEmpty(rf.Value.suffix))
                    .OrderBy(rf => rf.Value.suffix)
                    .ToList();

                var currFile = (RawFile)baseFile;
                foreach (var mf in mergeFiles)
                {
                    try
                    {
                        currFile.image
                            = MergeImage(currFile.image, mf.Value.image, true);
                    } 
                    finally
                    {
                        mf.Value.image?.Dispose();
                    }
                }
                rawFiles.Add(currFile);
            }

            return rawFiles.Any();
        }

        /// <summary>
        /// 2枚の画像を1枚に合成する（高品質変換）。重ね順の反転可能。<br/>
        /// 大きさが異なる場合、高幅それぞれ大きい方に合わせる。
        /// </summary>
        /// <param name="_in1">入力画像1（通常:前面）</param>
        /// <param name="_in2">入力画像2（通常:背面）</param>
        /// <param name="isInv">画像の重ね順を反転</param>
        /// <returns>合成画像</returns>
        public static Bitmap MergeImage(Bitmap _in1, Bitmap _in2, bool isInv = false)
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

            using (var result = new Bitmap(w, h, PixelFormat.Format32bppArgb))
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

                return result;
            }
        }

        /// <summary>
        /// 画像を指定の倍率で拡縮する（高品質変換）。
        /// </summary>
        /// <param name="_in">入力画像</param>
        /// <param name="scale">拡縮率</param>
        /// <returns>変更後の画像</returns>
        public static Bitmap ScaleImage(Bitmap _in, float scale)
        {
            if (_in == null)
            {
                throw new ArgumentNullException(nameof(_in), "入力画像が null です。");
            }

            int sw = Math.Max(1, (int)Math.Round(_in.Width * scale));
            int sh = Math.Max(1, (int)Math.Round(_in.Height * scale));

            using (var result = new Bitmap(sw, sh, PixelFormat.Format32bppArgb))
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

                return result;
            }
        }

        /// <summary>
        /// 画像を指定の倍率で拡縮する（高品質変換）。
        /// </summary>
        /// <param name="_in">入力画像</param>
        /// <param name="wScale">Width 拡縮率</param>
        /// <param name="hScale">Height 拡縮率</param>
        /// <returns>変更後の画像</returns>
        public static Bitmap ScaleImage(Bitmap _in, float wScale, float hScale)
        {
            if (_in == null)
            {
                throw new ArgumentNullException(nameof(_in), "入力画像が null です。");
            }

            int sw = Math.Max(1, (int)Math.Round(_in.Width * wScale));
            int sh = Math.Max(1, (int)Math.Round(_in.Height * hScale));

            var result = new Bitmap(sw, sh, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(result))
            {
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
        /// 画像を指定の倍率で拡縮する（高品質変換）。
        /// </summary>
        /// <param name="_in">入力画像</param>
        /// <param name="scale">Width/Height 拡縮率</param>
        /// <returns>変更後の画像</returns>
        public static Bitmap ScaleImage(Bitmap _in, SizeF scale)
        {
            return ScaleImage(_in, scale.Width, scale.Height);
        }

        /// <summary>
        /// 画像を指定の不透明度(α値)で出力する（高品質変換）。
        /// </summary>
        /// <param name="_in">入力画像</param>
        /// <param name="opacity">不透明度</param>
        /// <returns>変更後の画像</returns>
        public static Bitmap OpacityImage(Bitmap _in, float opacity)
        {
            if (_in == null)
            {
                throw new ArgumentNullException(nameof(_in), "入力画像が null です。");
            }

            using (var result = new Bitmap(_in.Width, _in.Height, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(result))
            {
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

                return result;
            }
        }
    }
}
