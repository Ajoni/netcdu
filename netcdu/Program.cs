using netcdu.Nodes;
using netcdu.Scanning;
using netcdu.Scanning.Strategies;
using System;
using System.IO;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace netcdu
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            var top = Application.Top;
            var window = new Window(new Rect(0, 1, top.Frame.Width, top.Frame.Height - 1), "netcdu");

            var fs = new FileScanner(@"E:\dump\dupa", new DefaultGetFileSizeStrategy());
            foreach (var item in fs.ContinueScan())
            {
                //Console.WriteLine(item);
            }

            fs.Root.OrderBySizeDesc();
            var tree = new TreeView(fs.Root)
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 4,
                Height = Dim.Fill() - 4,
                AllowsMarking = true,
                AllowsMultipleSelection = false
            };
            window.Add(tree);
            var statusBar = new StatusBar(new StatusItem[] {
            new StatusItem(Key.F1, "~F1~ Help", () => Help() ),
            new StatusItem(Key.F2, "~F2~ Delete", () => Delete(tree) ),
            });
            var menu = new MenuBar();
            top.Add(statusBar,window,menu);

            Application.Run(top);
        }

        static void Help()
        {
            MessageBox.Query(50, 7, "Help", "This is a small help\nBe kind.", "Ok");
        }

        static void Delete(TreeView tv)
        {
            var index = MessageBox.Query(50, 7, "Delete", $"About to delete {tv.SelectedItem}", "Cancel", "Delete");
            if (index == 1)
            {
                ITreeViewItem nodeToDelete = tv.SelectedItem;
                File.Delete(nodeToDelete.ToString());
                ((INetcduNode)nodeToDelete).Delete();

                tv.SelectedItem = nodeToDelete.Parent;
            }
        }
    }
}
