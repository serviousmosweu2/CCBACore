using System;
using System.Threading.Tasks;

namespace CCBA.Integrations.DataTransformation.Interfaces
{
    public interface IRedisService
    {
        public Task<string> getData(string cacheKey);

        public Task<string> GetData(string cacheKey);

        public Task RemoveData(string cacheKey);

        public Task SetData(string cacheKey, string cacheValue, TimeSpan? expiry = null);
    }
}