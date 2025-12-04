using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMerge.Common;

using RawFile = ImageMerge.Common.ImageManager.RawFile;

namespace ImageMerge.Common
{
    public partial class ucComboList : UserControl
    {
        public event EventHandler<EventArgs> ComboListChanged;
        public class ComboItem
        {
            public string? Display { get; set; } // 表示用
            public object? Value { get; set; }   // 実際の値

            public override string? ToString()
            {
                return Display; // ComboBox には Display を表示
            }
        }

        private List<ComboBox> m_comboList = new List<ComboBox>();
        private List<RawFile> m_rawList = new List<RawFile>();

        private int m_width;
        private int m_height;

        public List<RawFile> RawList => m_rawList;

        public ucComboList()
        {
            InitializeComponent();

            m_width = this.Width;
            m_height = this.Height;
        }

        public void UpdateComboList(string dir)
        {
            if (!ImageManager.FileAnalysis(dir, out var rawFiles))
            {
                return;
            }

            m_comboList.Clear();
            m_rawList.Clear();
            this.Controls.Clear();

            var groups = rawFiles
                .Select(x => x.Value.group)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            foreach (var group in groups.Select((val, idx) => (val, idx)))
            {
                var cb = new ComboBox();

                cb.DropDownStyle = ComboBoxStyle.DropDownList;

                cb.Items.Add("");
                rawFiles.Select(x => x.Value)
                    .Where(x => x.group == group.val)
                    .OrderBy(x => x.number)
                    .ToList()
                    .ForEach(x =>
                    {
                        var item = new ComboItem
                        {
                            Display = $"{x.group}{x.number:000}.png",
                            Value = rawFiles,
                        };
                        cb.Items.Add(item);
                    });

                // Set default selection
                cb.Top = 3 + group.idx * 35;
                cb.Left = 0;
                cb.Width = 120;

                cb.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

                this.Controls.Add(cb);
                m_comboList.Add(cb);

                m_rawList.Add(new RawFile());
            }

            this.Height = m_height * (m_comboList.Count + 1);
        }

        private void ComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sender is not ComboBox cb) { return; }

            int index = m_comboList.IndexOf(cb);
            if (index < 0) { return; }

            if (cb.SelectedItem is ComboItem item)
            {
                m_rawList[index]
                    = item.Value as RawFile? ?? new RawFile();
            }
            else
            {
                m_rawList[index] = new RawFile();
            }

            OnComboListChanged(e);
        }

        protected virtual void OnComboListChanged(EventArgs e)
        {
            ComboListChanged?.Invoke(this, e);
        }
    }
}