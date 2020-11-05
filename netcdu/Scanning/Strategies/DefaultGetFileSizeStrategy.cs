using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netcdu.Scanning.Strategies
{
    public interface IGetFileSizeStrategy
    {
        Task<long> GetSize(string path);
    }

    public class DefaultGetFileSizeStrategy : IGetFileSizeStrategy
    {
        public Task<long> GetSize(String path)
        {
            throw new NotImplementedException();
        }
    }
}
