using netcdu.Nodes;
using netcdu.Scanning.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using Terminal.Gui.Views;

namespace netcdu.Scanning
{
    public class FileScanner
    {
        private readonly string _root;
        private readonly IGetFileSizeStrategy _getFileSizeStrategy;
        private DirNode _rootNode;

        public INetcduNode Root => _rootNode;

        public FileScanner(string root, IGetFileSizeStrategy getFileSizeStrategy)
        {
            _root = root;
            _getFileSizeStrategy = getFileSizeStrategy;
            _rootNode = new DirNode(root);
        }

        public async IAsyncEnumerable<string> ContinueScan()
        {
            foreach (var dir in Directory.EnumerateDirectories(_root))
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
            }

            foreach (var child in _rootNode.Children)
                _rootNode.Data = (long)_rootNode.Data + (long)child.Data;

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
    }
}
