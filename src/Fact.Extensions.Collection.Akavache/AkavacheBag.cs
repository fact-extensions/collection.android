using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Akavache;
using System.Reactive.Linq;

namespace Fact.Extensions.Collection.Akavache
{
    public class AkavacheBag : IBag, IBagAsync, IKeys<string>
        //, ITryGetter
    {
        readonly string key_prefix;
        readonly IBlobCache blobCache;

        public AkavacheBag(IBlobCache blobCache, string key_prefix)
        {
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
                //blobCache.Insert(key, )
                throw new NotImplementedException();
            }
        }

        public object Get(string key, Type type)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetAsync(string key, Type type)
        {
            //blobCache.Get(key).Subscribe(x => x.)
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
