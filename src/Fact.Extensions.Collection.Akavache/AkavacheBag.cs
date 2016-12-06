using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Akavache;
using System.Reactive.Linq;
using Fact.Extensions.Serialization;

namespace Fact.Extensions.Collection
{
    public class AkavacheBag : IBag, IBagAsync, IKeys<string>
        //, ITryGetter
    {
        readonly string key_prefix;
        readonly IBlobCache blobCache;
        readonly ISerializationManager serializationManager;

        public AkavacheBag(IBlobCache blobCache, string key_prefix = null)
        {
            this.serializationManager = new Fact.Extensions.Serialization.Newtonsoft.JsonSerializationManager();
            this.blobCache = blobCache;
            this.key_prefix = key_prefix;
        }

        /*
        public T this[string key]
        {
            get
            {
                return blobCache.GetObject<T>(key_prefix + key).Wait();
            }

            set
            {
                blobCache.InsertObject(key_prefix + key, value);
            }
        }*/


        public IEnumerable<string> Keys
        {
            get
            {
                var result = from n in blobCache.GetAllKeys().Wait()
                             where n.StartsWith(key_prefix)
                             select n.Substring(key_prefix.Length);

                return result;
            }
        }

#if UNUSED
        /// <returns></returns>
        public bool TryGetValue(string key, out object value)
        {
            /*
            var rawKeys = blobCache.GetAllKeys().Wait();
        
            return this.TryGetValue(rawKeys, key, out value);*/

            // go back to raw one once we get prefixed indexer in place
            // FIX: Not fully tested, debugger wiggin out
            //return this.TryGetValue(Keys, key, out value);
        }
#endif

        public object this[string key, Type type]
        {
            set
            {
                var valueByteArray = serializationManager.SerializeToByteArray(value, type);
                blobCache.Insert(key, valueByteArray).Wait();
            }
        }

        public object Get(string key, Type type)
        {
            var valueByteArray = blobCache.Get(key).Wait();
            return serializationManager.Deserialize(valueByteArray, type);
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            var value = await blobCache.Get(key);
            return serializationManager.Deserialize(value, type);
        }

        public async Task SetAsync(string key, object value, Type type)
        {
            var valueByteArray = serializationManager.SerializeToByteArray(value, type);
            await blobCache.Insert(key, valueByteArray);
        }
    }

    public static class BlobCache_Extensions
    {
        public static AkavacheBag ToBag(this Akavache.IBlobCache blobCache)
        {
            return new AkavacheBag(blobCache);
        }
    }
}
