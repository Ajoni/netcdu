using netcdu.Nodes;
using netcdu.Scanning;
using netcdu.Scanning.Strategies;
using NStack;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace netcdu
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var path = @"E:\dump";
            Application.Init();
            var top = Application.Top;
            var window = new Window(new Rect(0, 1, top.Frame.Width, top.Frame.Height - 1), "netcdu");

            var fs = new FileScanner(path, new DefaultGetFileSizeStrategy());

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
            var label = new TextField("Scan in progress...") { ReadOnly = true, Width = Dim.Fill() - 4 };
            top.Add(statusBar, window, menu, label);
            top.Ready += async () =>
            {
                var sw = new Stopwatch();
                sw.Start();
                await foreach (var item in fs.ContinueScan())
                {
                    if(sw.ElapsedMilliseconds>200)
                    {
                        await Task.Delay(1);
                        label.Text = item;
                        sw.Restart();
                    }
                }
                label.Text = string.Empty;
                sw.Stop();
                sw = null;
                fs.Root.OrderBySizeDesc();
            };
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
