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
            if (args.Length == 0)
            {
                Console.WriteLine("netcdu requires a single path arg pointing to a directory");
                return;
            }

            var path = args[0];
            var fs = new FileScanner(path, new DefaultGetFileSizeStrategy());

            Application.Init();
            var top = Application.Top;
            var window = new Window(new Rect(0, 1, top.Frame.Width, top.Frame.Height - 1), "netcdu");
            var tree = new TreeView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill() - 4,
                Height = Dim.Fill() - 4,
                AllowsMarking = true,
                AllowsMultipleSelection = false
            };
            window.Add(tree);
            var statusBar = new StatusBar(new [] {
            new StatusItem(Key.F1, "~F1~ Help", Help ),
            new StatusItem(Key.F2, "~F2~ Go up",async() => await GoUp(window,fs,top,tree) ),
            new StatusItem(Key.F3, "~F3~ Delete", () => Delete(tree) ),
            new StatusItem(Key.F4, "~F4~ Exit", Application.RequestStop ),
            });
            var menu = new MenuBar();
            
            top.Add(statusBar, window, menu);

            top.Ready += async()=>await ScanFiles(window,fs,top, tree);
            Application.Run(top);

            Console.Clear();
        }

        static void Help()
        {
            MessageBox.Query(50, 7, "Help", 
@"
TAB - expand/shrink dir node
F1 - Brings you here
F2 - Goes up one directory if possible and then scans it
F3 - Brings up a delete file/dir dialog
F4 - Bye bye
", "Ok");
        }

        static async Task GoUp(Window window, FileScanner fs, Toplevel top, TreeView tree)
        {
            if(!fs.GoUp())
                return;
            tree.Root = null;
            await ScanFiles(window, fs, top,tree);
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

        static async Task ScanFiles(Window window, FileScanner fs, Toplevel top, TreeView tree)
        {
            top.Add(window);
            var sw = new Stopwatch();
            sw.Start();
            await foreach (var item in fs.ContinueScan())
            {
                if (sw.ElapsedMilliseconds > 200)
                {
                    await Task.Delay(1);
                    window.Title = $"Scan in progress... {item}";
                    sw.Restart();
                }
            }

            window.Title = "netcdu";
            sw.Stop();
            sw = null;
            fs.Root.OrderBySizeDesc();
            tree.Root = fs.Root;
        }

    }
}
