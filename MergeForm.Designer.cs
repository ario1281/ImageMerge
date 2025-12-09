namespace ImageMerge
{
    partial class MergeForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSrc = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pbPreview = new System.Windows.Forms.PictureBox();
            this.lblDirName = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ucComboList = new ImageMerge.Common.ucComboList();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSrc
            // 
            this.btnSrc.Location = new System.Drawing.Point(16, 12);
            this.btnSrc.Name = "btnSrc";
            this.btnSrc.Size = new System.Drawing.Size(86, 23);
            this.btnSrc.TabIndex = 1;
            this.btnSrc.Text = "ファイル選択";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(789, 463);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "Save";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(300, 463);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(310, 23);
            this.progressBar1.TabIndex = 5;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(626, 463);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(150, 20);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbPreview
            // 
            this.pbPreview.BackColor = System.Drawing.Color.Silver;
            this.pbPreview.Location = new System.Drawing.Point(14, 41);
            this.pbPreview.Name = "pbPreview";
            this.pbPreview.Size = new System.Drawing.Size(686, 408);
            this.pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbPreview.TabIndex = 7;
            this.pbPreview.TabStop = false;
            // 
            // lblDirName
            // 
            this.lblDirName.AutoSize = true;
            this.lblDirName.Location = new System.Drawing.Point(109, 16);
            this.lblDirName.Name = "lblDirName";
            this.lblDirName.Size = new System.Drawing.Size(0, 12);
            this.lblDirName.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.ucComboList);
            this.panel1.Location = new System.Drawing.Point(706, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(172, 437);
            this.panel1.TabIndex = 10;
            // 
            // ucComboList
            // 
            this.ucComboList.Location = new System.Drawing.Point(9, 8);
            this.ucComboList.Name = "ucComboList";
            this.ucComboList.Size = new System.Drawing.Size(140, 30);
            this.ucComboList.TabIndex = 11;
            // 
            // MergeForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(890, 499);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblDirName);
            this.Controls.Add(this.pbPreview);
            this.Controls.Add(this.btnSrc);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(390, 240);
            this.Name = "MergeForm";
            this.Text = "Image Merger";
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnSrc;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox pbPreview;
        private System.Windows.Forms.Label lblDirName;
        private System.Windows.Forms.Panel panel1;
        private Common.ucComboList ucComboList;
    }
}

