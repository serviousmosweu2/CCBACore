/*using CCBA.Integrations.DataTransformation.Interfaces;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace CCBA.Integrations.DataTransformation.Services
{
    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redisConnection;

        public RedisService(string redisConnectionString)
        {
            // Initialize Redis connection
            _redisConnection = ConnectionMultiplexer.Connect(redisConnectionString, options => { options.ReconnectRetryPolicy = new ExponentialRetry(100); });
        }

        [Obsolete("Use GetData method")]
        public async Task<string> getData(string cacheKey)
        {
            return await _redisConnection.GetDatabase().StringGetAsync(cacheKey);
        }

        /// <summary>
        /// Get the string value for the corresponding cache key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<string> GetData(string cacheKey)
        {
            return await _redisConnection.GetDatabase().StringGetAsync(cacheKey);
        }

        /// <summary>
        /// Set the string value for the corresponding cache key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="expiry"></param>
        public async Task SetData(string cacheKey, string cacheValue, TimeSpan? expiry = null)
        {
            // Set expiry to 1 day if not specified
            expiry ??= TimeSpan.FromDays(1);

            await _redisConnection.GetDatabase().StringSetAsync(cacheKey, cacheValue, expiry);
        }

        /// <summary>
        /// Remove the cache key and its value from the cache
        /// </summary>
        /// <param name="cacheKey"></param>
        public async Task RemoveData(string cacheKey)
        {
            await _redisConnection.GetDatabase().KeyDeleteAsync(cacheKey);
        }
    }
}*/