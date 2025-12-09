using ImageMerge.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageMerge
{
    public partial class MergeForm : Form
    {
        private static string m_outName = "out";

        private string m_dirPath = "";

        public MergeForm()
        {
            InitializeComponent();
        }

        private void BtnSrc_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    m_dirPath = dialog.SelectedPath;
                    lblDirName.Text = Path.GetFileName(m_dirPath);
                    ucComboList.UpdateComboList(m_dirPath);

                    DrawImage();
                }
            }
        }
        private async void BtnStart_Click(object sender, EventArgs e)
        {
            string src = m_dirPath;
            string dst = $"{AppContext.BaseDirectory}\\output";

            if (string.IsNullOrWhiteSpace(src))
            {
                MessageBox.Show("フォルダを指定してください。");
                return;
            }

            btnStart.Enabled = false;
            lblStatus.Text = "準備中...";
            progressBar1.Value = 0;

            var progress = new Progress<int>(value =>
            {
                progressBar1.Value = value;
                lblStatus.Text = $"{value}% 完了";
            });

            try
            {
                await Task.Run(() => SaveImage((Bitmap)pbPreview.Image, dst, progress));
                lblStatus.Text = "完了！";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー");
                lblStatus.Text = "エラー";
            }
            finally
            {
                btnStart.Enabled = true;
            }
        }
        private void ucComboList_ComboListChanged(object sender, EventArgs e)
        {
            DrawImage();
        }

        private void DrawImage()
        {
            var rawFiles = ucComboList.RawList;

            if (rawFiles == null || rawFiles.Count <= 0)
            {
                return;
            }

            var img = rawFiles[0].image;
            var size = img != null ? img.Size : new Size(1, 1);

            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            foreach (var rawFile in rawFiles)
            {
                result = ImageManager.MergeImage(result, rawFile.image);
            }

            pbPreview.Image = result;
        }

        private static void SaveImage(Bitmap img, string outDir, IProgress<int> progress = null)
        {
            Directory.CreateDirectory(outDir);
            string filePath = "";

            int cnt = 0;
            do
            {
                filePath = Path.Combine(outDir, $"{m_outName}_{cnt:000}.png");
                cnt++;
            }
            while (File.Exists(filePath));

            img.Save(filePath, ImageFormat.Png);
        }
    }
}
