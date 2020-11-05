using NStack;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace netcdu.Nodes
{
    public class DirNode : INetcduNode
    {
        public string _nodeName { get; set; }
        private object data;
        private readonly string path;
        private List<ITreeViewItem> _children;

        public DirNode(string path, List<ITreeViewItem> children = null)
        {
            _nodeName = path.GetFileNameFromPath();
            this.path = path;
            _children = children ?? new List<ITreeViewItem>();
            foreach (var child in _children)
                child.Parent = this;
            Data = 0L;
        }

        public long Size { get; private set; }
        public object Data { get => data; set => data = value; }
        public ITreeViewItem Parent { get; set; }
        public int Count => _children.Count;
        public bool IsExpanded { get; set; }
        public bool IsMarked { get; set; }
        public IList<ITreeViewItem> Children => _children;

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

            sb.Append($"  {((long)Data).LongToStringSize()}  {_nodeName}");

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
            if (_children == null)
                return list;

            foreach (ITreeViewItem item in _children)
            {
                foreach (ITreeViewItem childItem in item.ToList())
                {
                    list.Add(childItem);
                }
            }
            return list;
        }

        public void OrderBySizeDesc()
        {
            _children = _children.OrderByDescending(x => (long)x.Data).ToList();
            foreach (INetcduNode child in _children)
                child.OrderBySizeDesc();
        }

        public override string ToString()
        {
            return path;
        }

        public void Delete()
        {
            var parent = (DirNode)Parent;
            parent.Children.Remove(this);
            parent.RecalculateSize();
        }

        public void RecalculateSize()
        {
            var size = 0L;
            foreach (var child in Children)
                size += (long)child.Data;
            Size = size;

            if(Parent!=null)
                ((DirNode)Parent).RecalculateSize();
        }
    }
}
