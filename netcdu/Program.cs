using netcdu.Nodes;
using netcdu.Scanning;
using netcdu.Scanning.Strategies;
using System;
using Terminal.Gui.Views;

namespace netcdu
{
    class Program
    {
        static void Main(string[] args)
        {
            var fs = new FileScanner(@"C:\Users\adam5\Downloads\studia\st2\praca mag", new DefaultGetFileSizeStrategy());
            foreach (var item in fs.ContinueScan())
            {
                //Console.WriteLine(item);
            }
            var root = fs.Root;
            prynt(root);
            Console.ReadLine();
        }

        private static void prynt(INetcduNode item)
        {
            Console.WriteLine($"{item._nodeName} {((long)item.Data).LongToStringSize()}");
            foreach (var child in item.Children)
            {
                prynt((INetcduNode)child);
            }
        }
    }
}
