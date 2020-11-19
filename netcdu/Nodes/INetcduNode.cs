using Terminal.Gui.Views;

namespace netcdu.Nodes
{
    public interface INetcduNode : ITreeViewItem
    {
        string _nodeName { get; set; }
        string Path { get; }
        void OrderBySizeDesc();
        void Delete();
    }
}
