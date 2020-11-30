# netcdu
.NET Curses Disk Usage

A small, multi-platform tool written in .NET Core, which allows you to view sizes of files and directories on your system in the form of a tree.

This tool uses a custom fork of the Terminal.Gui toolkit (https://github.com/migueldeicaza/gui.cs) as it's curses library.

# Statistics
An obvious weakness of this tool are directories with tons of small files.

Sample One: a node_modules folder. Total size: 336 MB (353237462 bytes). 38960 Files, 5380 Folders. Collection time: 28.623 seconds. Subsequent re-scans (F5 Refresh) of the same node_modules folder take between 1.5 to 1.7 seconds.

Sample Two: 8 files 25 GB each. Total size: 200 GB. Collection time: 0.013 seconds
