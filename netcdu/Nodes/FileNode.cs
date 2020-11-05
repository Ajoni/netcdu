using NStack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace netcdu.Nodes
{
    public class FileNode : INetcduNode
    {
        public string _nodeName { get; set; }
        private object data;

        public FileNode(string nodeName, long size)
        {
            _nodeName = nodeName;
            Data = size;
        }
        public object Data { get => data; set => data = value; }
        public ITreeViewItem Parent { get; set; }
        public int Count => 1;
        public bool IsExpanded { get; set; }
        public bool IsMarked { get; set; }
        public IList<ITreeViewItem> Children => Array.Empty<ITreeViewItem>();

        public void Render(TreeView container, ConsoleDriver driver, bool selected, int level, int col, int line, int width)
        {
            container.Move(col, line);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < level - 1; i++)
            {
                sb.Append("  ");
            }
            if (level > 0)
            {
                sb.Append('|');
                if (Children != null && Children.Count > 0)
                    if (IsExpanded)
                        sb.Append("^");
                    else
                        sb.Append('v');
                else
                    sb.Append('-');
            }

            sb.Append(_nodeName);

            RenderUstr(driver, sb.ToString(), col, line, width);
        }

        void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width)
        {
            int byteLen = ustr.Length;
            int used = 0;
            for (int i = 0; i < byteLen;)
            {
                (var rune, var size) = Utf8.DecodeRune(ustr, i, i - byteLen);
                var count = System.Rune.ColumnWidth(rune);
                if (used + count > width)
                    break;
                driver.AddRune(rune);
                used += count;
                i += size;
            }
            for (; used < width; used++)
            {
                driver.AddRune(' ');
            }
        }

        public IList ToList()
        {
            var list = new List<ITreeViewItem>();
            list.Add(this);
            return list;
        }
    }
}
