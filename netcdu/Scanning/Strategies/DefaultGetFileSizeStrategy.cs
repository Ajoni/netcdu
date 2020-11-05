namespace netcdu.Scanning.Strategies
{
    public class DefaultGetFileSizeStrategy : IGetFileSizeStrategy
    {
        public long GetSize(string path)
        {
            return new System.IO.FileInfo(path).Length;
        }
    }
}
