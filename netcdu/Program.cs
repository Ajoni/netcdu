﻿using netcdu.Nodes;
using netcdu.Scanning;
using netcdu.Scanning.Strategies;
using NStack;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace netcdu
{
    class Program
    {
        private readonly static string[] Parameters = new[] { "-p", "-v", "-h", "help" };

        private const string MenuHelp = @"TAB and ENTER - Expand/Collapse directory node
F1 - Opens menu help dialog
F2 - Goes up one directory if possible and then scans it
F3 - Brings up a delete file/directory dialog
F4 - Closes the application
F5 - Refreshes the files and folders by re-scanning the current directory
";

        static void Main(string[] args)
        {
            // The working directory is the default scanned path.
            // This can be overridden with the -p parameter.
            var path = Directory.GetCurrentDirectory();

            if (args.Length > 0)
            {
                if (HandleParameters(args, ref path))
                {
                    return;
                }
            }

            var fs = new FileScanner(path, new DefaultGetFileSizeStrategy());

            Application.Init();
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.White, Color.Black);
            Colors.Base.HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black);
            var top = Application.Top;
            var window = new Window(new Rect(0, 1, top.Frame.Width, top.Frame.Height - 2), "netcdu");
            var tree = new TreeView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };
            tree.KeyPress += e => ExpandCollapse(e, tree);
            window.Add(tree);
            var statusBar = new StatusBar(new[] {
                new StatusItem(Key.F1, "~F1~ Help", Help ),
                new StatusItem(Key.F2, "~F2~ Go up",async() => await GoUp(window,fs,top,tree) ),
                new StatusItem(Key.F3, "~F3~ Delete", () => Delete(tree) ),
                new StatusItem(Key.F4, "~F4~ Exit", Application.RequestStop ),
                new StatusItem(Key.F5, "~F5~ Refresh", async() => await Refresh(window,fs,top,tree) ),
            });
            var menu = new MenuBar();

            top.Add(statusBar, window, menu);

            top.Ready += async () => await ScanFiles(window, fs, top, tree);
            Application.Run(top);

            Console.Clear();
        }

        private static bool HandleParameters(string[] args, ref string path)
        {
            // For now, netcdu will support only one parameter at a time, for simplicity's sake.
            var parameter = args[0];
            if (!Parameters.Contains(parameter))
            {
                PrintHelp($"Unrecognized parameter {parameter}");
                return true;
            }

            if (parameter == "-h" || parameter == "help")
            {
                PrintHelp();
                return true;
            }

            if (parameter == "-v")
            {
                PrintVersion();
                return true;
            }

            if (parameter == "-p")
            {
                if (args.Length != 2)
                {
                    PrintHelp($"Unexpected parameter count. Expected '-p' and a path, got the following parameters: [{string.Join(", ", args)}]");
                    return true;
                }

                var tmpPath = args[1]; // Could be a file or a directory
                var directoryExists = Directory.Exists(tmpPath);
                var fileExists = File.Exists(tmpPath);
                if (!directoryExists && !fileExists)
                {
                    Console.WriteLine($"Supplied path: '{tmpPath}' does not exist.");
                    return true;
                }

                if (directoryExists)
                {
                    path = tmpPath;
                    return false;
                }
                else // fileExists
                {
                    var parentFolder = Path.GetDirectoryName(tmpPath);
                    path = parentFolder;
                    return false;
                }
            }

            throw new Exception(); // Should never happen.
        }

        private static void PrintVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine($"netcdu, version: {version}");
        }

        private static void PrintHelp(string error = null)
        {
            if (error != null)
                Console.WriteLine(error);
            Console.WriteLine("netcdu - .NET Curses Disk Usage");
            Console.WriteLine("Parameters:");
            Console.WriteLine("-p, Path of directory to scan from. Example: netcdu -p C:\\");
            Console.WriteLine("\tIf this parameter is not supplied the working directory is used.");
            Console.WriteLine("\tIf a file is supplied, then it's parent directory will be used.");
            Console.WriteLine("-v, Prints the current version of the application. Example: netcdu -v");
            Console.WriteLine("-h|help, Prints this text. Example: netcdu -h");
            Console.WriteLine();
            Console.WriteLine("Menu help:");
            Console.Write(MenuHelp);
        }



        static void Help()
        {
            MessageBox.Query(50, 11, "Help", MenuHelp, "Ok");
        }

        static async Task GoUp(Window window, FileScanner fs, Toplevel top, TreeView tree)
        {
            if (!fs.GoUp())
                return;
            tree.Root = null;
            await ScanFiles(window, fs, top, tree);
        }

        static async Task Refresh(Window window, FileScanner fs, Toplevel top, TreeView tree)
        {
            fs.Reset(fs.Root.Path);
            tree.Root = null;
            await ScanFiles(window, fs, top, tree);
        }

        static void Delete(TreeView tv)
        {
            var index = MessageBox.Query(50, 7, "Delete", $"About to delete {tv.SelectedItem}", "Cancel", "Delete");
            if (index == 1)
            {
                ITreeViewItem nodeToDelete = tv.SelectedItem;
                if (File.Exists(nodeToDelete.ToString()))
                    File.Delete(nodeToDelete.ToString());
                else
                if (Directory.Exists(nodeToDelete.ToString()))
                    Directory.Delete(nodeToDelete.ToString(), true);
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

            window.Title = ((DirNode)fs.Root).Path;
            sw.Stop();
            sw = null;
            fs.Root.OrderBySizeDesc();
            tree.Root = fs.Root;
        }

        static void ExpandCollapse(View.KeyEventEventArgs e, TreeView tv)
        {
            if (e.KeyEvent.Key == Key.Enter)
            {
                tv.OnExpandOrCollapseSelectedItem();
                e.Handled = true;
            }
        }

    }
}
