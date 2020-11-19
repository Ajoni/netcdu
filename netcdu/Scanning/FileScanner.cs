using netcdu.Nodes;
using netcdu.Scanning.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Terminal.Gui.Views;

namespace netcdu.Scanning
{
    public class FileScanner
    {
        private string _root;
        private readonly IGetFileSizeStrategy _getFileSizeStrategy;
        private DirNode _rootNode;
        private List<string> _prevRoots;

        public INetcduNode Root => _rootNode;

        public FileScanner(string root, IGetFileSizeStrategy getFileSizeStrategy)
        {
            _getFileSizeStrategy = getFileSizeStrategy;
            Reset(root);
        }

        public async IAsyncEnumerable<string> ContinueScan()
        {
            foreach (var dir in Directory.EnumerateDirectories(_root).Except(_prevRoots))
            {
                if (DirNotReadable(dir))
                    continue;
                await foreach (var nodePath in ContinueScanRec(dir, _rootNode))
                    yield return nodePath;
            }

            foreach (var file in Directory.EnumerateFiles(_root))
            {
                var size = _getFileSizeStrategy.GetSize(file);
                _rootNode.Children.Add(new FileNode(file, size) { Parent = _rootNode });
                _rootNode.Data = (long) _rootNode.Data + size;
            }

            yield return _root;
        }

        private async IAsyncEnumerable<string> ContinueScanRec(string root, DirNode parent)
        {
            var dirNode = new DirNode(root);
            parent.Children.Add(dirNode);
            dirNode.Parent = parent;

            foreach (var dir in Directory.EnumerateDirectories(root))
            {
                if (DirNotReadable(dir))
                    continue;
                await foreach (var nodePath in ContinueScanRec(dir, dirNode))
                    yield return nodePath;
            }
            foreach (var file in Directory.EnumerateFiles(root))
            {
                var size = _getFileSizeStrategy.GetSize(file);
                dirNode.Children.Add(new FileNode(file, size) { Parent = dirNode });
                dirNode.Data = (long)dirNode.Data + size;
            }
            parent.Data = (long)parent.Data + (long)dirNode.Data;

            yield return root;
        }

        public bool GoUp()
        {
            var rootDirInfo = new DirectoryInfo(_root);
            if (rootDirInfo.Parent == null)
                return false;

            _root = rootDirInfo.Parent.FullName;
            var prevRootNode = _rootNode;
            _rootNode = new DirNode(_root, new List<ITreeViewItem>{ prevRootNode });
            _rootNode.Data = prevRootNode.Data;
            _prevRoots.Add(rootDirInfo.FullName);

            return true;
        }
        
        bool DirNotReadable(string path)
        {
            try
            {
                Directory.GetDirectories(path);
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        internal void Reset(string root)
        {
            _root = root;
            _rootNode = new DirNode(root);
            _prevRoots = new List<string> { _root };
        }
    }
}
