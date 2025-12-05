using ImageMerge.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageMerge
{
    public partial class MergeForm : Form
    {
        public MergeForm()
        {
            InitializeComponent();
        }

        private string m_dirPath = "";

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
                await Task.Run(() => ImageManager.SaveImage(pbPreview.Image, dst, progress));
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
            pbPreview.Image = ImageManager.DrawImage(ucComboList.RawList);
        }
    }
}
