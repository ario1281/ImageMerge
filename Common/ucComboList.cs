using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Tar;
using System.Linq;
using System.Windows.Forms;

namespace ImageMerge.Common
{
    public partial class ucComboList : UserControl
    {
        public event EventHandler<EventArgs> ComboListChanged;
        public class ComboItem
        {
            public string Display { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Display;
            }
        }

        private List<ComboBox> m_comboList = new List<ComboBox>();
        private List<RawFile> m_rawList = new List<RawFile>();

        private int m_width;
        private int m_height;

        public List<RawFile> RawList
        {
            get { return m_rawList; }
            private set;
        }

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

                cb.Items.Add(new ComboItem {
                    Display = "",
                    Value = new RawFile(),
                });
                rawFiles.Select(x => x.Value)
                    .Where(x => x.group == group.val)
                    .OrderBy(x => x.number)
                    .ToList()
                    .ForEach(rf =>
                    {
                        var item = new ComboItem
                        {
                            Display = $"{rf.group}{rf.number:000}.png",
                            Value = rf,
                        };
                        cb.Items.Add(item);
                    });

                // Set default selection
                cb.Top = 3 + group.idx * 35;
                cb.Left = 0;
                cb.Width = 120;

                cb.SelectedIndex = 1;

                cb.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

                this.Controls.Add(cb);
                m_rawList.Add(SetRawFile((ComboItem)cb.SelectedItem));
                m_comboList.Add(cb);
            }

            this.Height = m_height * (m_comboList.Count + 1);
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is not ComboBox cb) { return; }

            int idx = m_comboList.IndexOf(cb);
            if (idx < 0) { return; }

            m_rawList[idx] = SetRawFile((ComboItem)cb.SelectedItem);

            OnComboListChanged(e);
        }

        private RawFile SetRawFile(ComboItem item)
        {
            if (item != null && item.Value is RawFile rawFile)
            {
                return rawFile;
            }

            return new RawFile();
        }

        protected virtual void OnComboListChanged(EventArgs e)
        {
            ComboListChanged.Invoke(this, e);
        }
    }
}
