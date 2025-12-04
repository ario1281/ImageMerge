using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.ImageSharp.Point;

namespace ImageMerge.Common
{
    internal class ImageManager
    {
        // 再利用ポイント
        private static readonly Point m_zeroPoint = new Point(0, 0);
        private static readonly Regex m_regex = new Regex(
                @"^([a-z]+)(\d{3})([a-z]*)\.png$",
                RegexOptions.IgnoreCase
            );

        public struct RawFile {
            public string path { get; set; }
            public string group { get; set; }
            public int number { get; set; }
            public bool hasSuffix { get; set; }
        };

        public static bool FileAnalysis(string dir, out List<RawFile?> rawFiles)
        {
            rawFiles = Directory.GetFiles(dir, "*.png")
                .Select(static path =>
                {
                    var name = Path.GetFileName(path);
                    var m = m_regex.Match(name);
                    if (!m.Success) return (RawFile?)null;

                    return new RawFile {
                        path      = path,
                        group     = m.Groups[1].Value.ToLower(),
                        number    = int.Parse(m.Groups[2].Value),
                        hasSuffix = !string.IsNullOrEmpty(m.Groups[3].Value)
                    };
                })
                .Where(x => x != null)
                .ToList();

            return rawFiles.Any();
        }

        /// <summary>
        /// dir 内の画像を全通り合成して outDir に保存する（超高速版）
        /// </summary>
        public static void MergeImagesByPrefix(
            string dir,
            string outDir,
            IProgress<int>? progress = null,
            CancellationToken? cancelToken = null)
        {
            // 入力チェック
            if (!Directory.Exists(dir)) { throw new DirectoryNotFoundException(dir); }

            var regex = new Regex(
                @"^([a-z]+)(\d{3})([a-z]*)\.png$",
                RegexOptions.IgnoreCase
            );

            Directory.CreateDirectory(outDir);

            #region ファイル解析

            var rawFiles = new List<RawFile?>(); 

            var groups = rawFiles
                .Select(x => x.Value.group)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            #endregion

            #region 画像キャッシュ（統一 Rgba32）

            var imageCache = new ConcurrentDictionary<string, Image<Rgba32>>();
            foreach (var f in rawFiles)
            {
                imageCache[f.Value.path] = Image.Load<Rgba32>(f.Value.path);
            }

            #endregion

            #region 番号マップ作成（透明番号を追加）

            var numberMap = groups.ToDictionary(
                g => g,
                g =>
                {
                    var nums = rawFiles
                        .Where(x => x.Value.group == g)
                        .Select(x => x.Value.number)
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList();

                    int transparent = nums.Any() ? nums.Max() + 1 : 1;
                    nums.Add(transparent);

                    return nums;
                }
            );

            #endregion

            #region オーバーラップ作成

            var overlayMap = rawFiles
                .GroupBy(f => f.Value.group)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(f => f.Value.number)
                        .ToDictionary(
                            n => n.Key,
                            n => n.OrderBy(f => f.Value.hasSuffix ? 1 : 0)
                                   .ThenBy(f => f.Value.hasSuffix)
                                   .Select(f => f.Value.path)
                                   .ToList()
                        )
                );

            if (!overlayMap.ContainsKey("a"))
            {
                throw new Exception("a グループが見つかりません");
            }

            #endregion

            #region 全組み合わせ

            var combos = CartesianProduct(numberMap);

            int total = numberMap.Values
                .Select(v => v.Count)
                .Aggregate(1, (acc, x) => acc * x);
            if (total == 0) { return; }

            int completed = 0;

            #endregion

            #region 保存キュー（I/O 専用スレッド）

            var saveQueue = new BlockingCollection<(byte[] pngBytes, string outPath)>(
                boundedCapacity: Math.Min(1000, Math.Max(10, Environment.ProcessorCount * 10))
            );

            var saveTask = Task.Run(() =>
            {
                foreach (var item in saveQueue.GetConsumingEnumerable())
                {
                    if (cancelToken?.IsCancellationRequested ?? false)
                        break;

                    try
                    {
                        File.WriteAllBytes(item.outPath, item.pngBytes);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(
                            $"Save error {item.outPath}: {ex.Message}"
                        );
                    }
                }
            });

            #endregion

            #region 並列合成処理

            var groupsExceptA = groups.Where(g => g != "a").ToList();

            var pngEncoder = new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestSpeed
            };

            var po = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
                CancellationToken = cancelToken ?? CancellationToken.None
            };

            try
            {
                Parallel.ForEach(combos.Select((combo, idx) => new { combo, idx }), po, item =>
                {
                    if (po.CancellationToken.IsCancellationRequested)
                        return;

                    var combo = item.combo;

                    // a の画像をロード
                    int aNum = combo["a"];
                    if (!overlayMap["a"].TryGetValue(aNum, out var aPaths))
                        return;

                    string aPath = aPaths.First();

                    // 作業用キャンバス
                    using var canvas = imageCache[aPath].CloneAs<Rgba32>();

                    // --- ほかのグループを重ねる ---
                    foreach (var g in groupsExceptA)
                    {
                        int num = combo[g];

                        if (!overlayMap.TryGetValue(g, out var perNumber)) continue;
                        if (!perNumber.TryGetValue(num, out var ovPaths)) continue;

                        foreach (var ovPath in ovPaths)
                        {
                            using var ov = imageCache[ovPath].CloneAs<Rgba32>();
                            canvas.Mutate(ctx =>
                                ctx.DrawImage(
                                    ov,
                                    m_zeroPoint,
                                    new GraphicsOptions
                                    {
                                        AlphaCompositionMode = PixelAlphaCompositionMode.Src,
                                        BlendPercentage = 1f,
                                        Antialias = false
                                    }
                                )
                            );
                        }
                    }

                    // --- PNG encode → 保存キュー ---
                    using var ms = new MemoryStream();
                    canvas.Save(ms, pngEncoder);

                    var bytes = ms.ToArray();
                    string outFile = Path.Combine(
                        outDir,
                        $"{Path.GetFileName(dir)}_{item.idx:000000}.png"
                    );

                    saveQueue.Add((bytes, outFile), po.CancellationToken);

                    // 進捗通知
                    int done = Interlocked.Increment(ref completed);
                    progress?.Report((int)((double)done / total * 100));
                });
            }
            finally
            {
                // 完了・中断時の処理
                saveQueue.CompleteAdding();

                try { saveTask.Wait(); } catch { /* ignore */ }

                foreach (var kv in imageCache)
                {
                    try { kv.Value.Dispose(); } catch { }
                }
            }

            #endregion
        }

        // -------------------------------------------------------------
        // デカルト積（group -> numbers）を展開
        // -------------------------------------------------------------
        private static IEnumerable<Dictionary<string, int>> CartesianProduct(Dictionary<string, List<int>> numberMap)
        {
            // group と numberList の配列化（順序固定）
            var groups = numberMap.Keys.ToList();
            var numberLists = groups.Select(g => numberMap[g]).ToList();

            int depth = groups.Count;
            int[] indices = new int[depth];

            while (true)
            {
                // 現在の indices に基づき辞書を1つ yield
                var dict = new Dictionary<string, int>(depth);
                for (int i = 0; i < depth; i++)
                {
                    dict[groups[i]] = numberLists[i][indices[i]];
                }

                yield return dict;

                // 次の組み合わせに進む
                int pos = depth - 1;
                while (pos >= 0)
                {
                    indices[pos]++;
                    if (indices[pos] < numberLists[pos].Count) { break; }

                    indices[pos] = 0;
                    pos--;
                }

                if (pos < 0) { break; }
            }
        }
    }
}
