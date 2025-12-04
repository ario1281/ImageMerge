

namespace ImageMerge
{
    partial class MergeForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSrc = new Button();
            btnStart = new Button();
            progressBar1 = new ProgressBar();
            lblStatus = new Label();
            pbPreview = new PictureBox();
            lblDirName = new Label();
            panel1 = new Panel();
            ucComboList1 = new ImageMerge.Common.ucComboList();
            ((System.ComponentModel.ISupportInitialize)pbPreview).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSrc
            // 
            btnSrc.Location = new Point(16, 12);
            btnSrc.Name = "btnSrc";
            btnSrc.Size = new Size(86, 23);
            btnSrc.TabIndex = 1;
            btnSrc.Text = "ファイル選択";
            btnSrc.Click += BtnSrc_Click;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(789, 463);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 4;
            btnStart.Text = "Save";
            btnStart.Click += BtnStart_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(300, 463);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(310, 23);
            progressBar1.TabIndex = 5;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(626, 463);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(154, 23);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "待機中";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pbPreview
            // 
            pbPreview.BackColor = Color.Silver;
            pbPreview.Location = new Point(14, 41);
            pbPreview.Name = "pbPreview";
            pbPreview.Size = new Size(686, 408);
            pbPreview.TabIndex = 7;
            pbPreview.TabStop = false;
            // 
            // lblDirName
            // 
            lblDirName.AutoSize = true;
            lblDirName.Location = new Point(109, 16);
            lblDirName.Name = "lblDirName";
            lblDirName.Size = new Size(0, 15);
            lblDirName.TabIndex = 8;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(ucComboList1);
            panel1.Location = new Point(706, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(172, 437);
            panel1.TabIndex = 10;
            // 
            // ucComboList1
            // 
            ucComboList1.Location = new Point(9, 8);
            ucComboList1.Name = "ucComboList1";
            ucComboList1.Size = new Size(140, 30);
            ucComboList1.TabIndex = 11;
            ucComboList1.ComboListChanged += ucComboList1_ComboListChanged;
            // 
            // MergeForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(890, 499);
            Controls.Add(panel1);
            Controls.Add(lblDirName);
            Controls.Add(pbPreview);
            Controls.Add(btnSrc);
            Controls.Add(btnStart);
            Controls.Add(progressBar1);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = new Size(390, 240);
            Name = "MergeForm";
            Text = "Image Merger";
            ((System.ComponentModel.ISupportInitialize)pbPreview).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnSrc;
        private Button btnStart;
        private ProgressBar progressBar1;
        private Label lblStatus;
        private PictureBox pbPreview;
        private Label lblDirName;
        private Panel panel1;
        private Common.ucComboList ucComboList1;
    }
}