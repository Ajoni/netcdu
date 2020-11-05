namespace netcdu.Scanning.Strategies
{
    public interface IGetFileSizeStrategy
    {
        long GetSize(string path);
    }
}