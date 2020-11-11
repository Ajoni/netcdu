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
        static void Main(string[] args)
        {
            var path = args[0];
            var fs = new FileScanner(path, new DefaultGetFileSizeStrategy());

            Application.Init();
            var top = Application.Top;
            var window = new Window(new Rect(0, 1, top.Frame.Width, top.Frame.Height - 1), "netcdu");
            var label = new TextField("Scan in progress...") { ReadOnly = true, Width = top.Frame.Width };
            var tree = new TreeView
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 4,
                Height = Dim.Fill() - 4,
                AllowsMarking = true,
                AllowsMultipleSelection = false
            };
            window.Add(tree);

            var statusBar = new StatusBar(new [] {
            new StatusItem(Key.F1, "~F1~ Help", Help ),
            new StatusItem(Key.F2, "~F2~ Go up",async() => await GoUp(label,fs,top,tree) ),
            new StatusItem(Key.F3, "~F3~ Delete", () => Delete(tree) ),
            new StatusItem(Key.F4, "~F4~ Exit", Application.RequestStop ),
            });
            var menu = new MenuBar();
            
            top.Add(statusBar, window, menu);

            top.Ready += async()=>await ScanFiles(label,fs,top, tree);
            Application.Run(top);
        }

        static void Help()
        {
            MessageBox.Query(50, 7, "Help", 
@"
F1 - Brings you here
F2 - Goes up one directory if possible and then scans it
F3 - Brings up a delete file/dir dialog
F4 - Bye bye
", "Ok");
        }

        static async Task GoUp(TextField label, FileScanner fs, Toplevel top, TreeView tree)
        {
            if(!fs.GoUp())
                return;

            await ScanFiles(label, fs, top,tree);
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
                tv.SetNeedsDisplay();
            }
        }

        static async Task ScanFiles(TextField label, FileScanner fs, Toplevel top, TreeView tree)
        {
            top.Add(label);
            var sw = new Stopwatch();
            sw.Start();
            await foreach (var item in fs.ContinueScan())
            {
                if (sw.ElapsedMilliseconds > 200)
                {
                    await Task.Delay(1);
                    label.Text = item;
                    sw.Restart();
                }
            }

            top.Remove(label);
            sw.Stop();
            sw = null;
            fs.Root.OrderBySizeDesc();
            tree.Root = fs.Root;
        }

    }
}
