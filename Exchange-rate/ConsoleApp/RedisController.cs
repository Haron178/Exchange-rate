using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using StackExchange.Redis;

namespace ConsoleApp
{
    public class RedisController : IDisposable
    {
        private static String Host = "localhost";
        private static List<string> ListKey;
        private readonly IDatabase RedDb;
        private readonly ConnectionMultiplexer RedisCon;
        private static TimeSpan Expire = TimeSpan.FromMinutes(20);
        public RedisController(string host, List<string> listKey, TimeSpan expire)
        {
            ListKey = listKey;
            try 
            {
                RedisCon = ConnectionMultiplexer.Connect(host);
                RedDb = RedisCon.GetDatabase();
            } 
            catch (Exception ex)
            { 
                throw new Exception(ex.Message); 
            }

        }
        public RedisController(List<string> listKeys) : this(Host, listKeys, Expire) {}

        public async void SetItemAsync(string key, string value)
        {
            try 
            {
                await RedDb.StringSetAsync(key, value, Expire);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<RedisValue> GetItemAsync(string key)
        {
            if (RedDb.KeyExists(key))
            {
                return await RedDb.StringGetAsync(key);
            }
            else
            {
                throw new ArgumentException("Key not found");
            }
        }

        public RedisValue GetItem(string key)
        {
            if (RedDb.KeyExists(key))
            {
                return RedDb.StringGet(key);
            }
            else
            {
                throw new ArgumentException("Key not found");
            }
        }

        public void SetItem(string key, String value)
        {
            try 
            {  
                RedDb.StringSet(key, value, Expire); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }

        }

        public List<RedisValue> GetListByKeys(IEnumerable<string> keys)
        {
            var result = new List<RedisValue>();
            try
            {
                foreach (string key in keys)
                {
                    result.Add(GetItem(key));
                }
                return result;
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        public void SetList(IEnumerable<string> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            var strBuild = new StringBuilder();
            foreach(var item in list)
            {
                strBuild.Append(item);
                strBuild.Append("\n");
            }
            SetItem(ListKey.Last(), strBuild.Remove(strBuild.Length - 1, 1).ToString());
        }

        public void Dispose()
        {
            RedisCon?.Close();
            RedisCon?.Dispose();
        }
    }
}
