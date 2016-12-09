﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Akavache;
using System.Reactive.Linq;
using Fact.Extensions.Serialization;
using Fact.Extensions.Collection;

namespace Fact.Extensions.Caching
{
    public class AkavacheBag : ICache, ICacheAsync, IKeys<string>, IContainsKey<string>
    {
        readonly string key_prefix;
        readonly IBlobCache blobCache;
        readonly ISerializationManager serializationManager;

        public AkavacheBag(IBlobCache blobCache, string key_prefix = null)
        {
            this.serializationManager = new Serialization.Newtonsoft.JsonSerializationManager();
            this.blobCache = blobCache;
            this.key_prefix = key_prefix;
        }

        /// <summary>
        /// Beware, according to akavache documentation this value may not always be 100% accurate
        /// </summary>
        /// <remarks>
        /// https://github.com/akavache/Akavache/blob/develop/src/Akavache/Portable/IBlobCache.cs
        /// </remarks>
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

        public IEnumerable<Type> SupportedOptions
        {
            get
            {
                throw new NotImplementedException();
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

        public void Set(string key, object value, Type type, params ICacheItemOption[] options)
        {
            if (options.Any()) throw new InvalidOperationException("Cache options not supported yet");
            // TODO: actually utilize options
            this[key, type] = value;
        }

        public void Remove(string key)
        {
            blobCache.Invalidate(key).Wait();
        }

        public bool TryGet(string key, Type type, out object value)
        {
            if(ContainsKey(key))
            {
                value = Get(key, type);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public async Task SetAsync(string key, object value, Type type, params ICacheItemOption[] options)
        {
            if (options.Any()) throw new InvalidOperationException("Cache options not supported yet");

            var valueByteArray = serializationManager.SerializeToByteArray(value, type);
            await blobCache.Insert(key, valueByteArray);
        }

        public async Task RemoveAsync(string key)
        {
            await blobCache.Invalidate(key);
        }

        public bool ContainsKey(string key)
        {
            return blobCache.GetCreatedAt(key).Wait().HasValue;
        }
    }

    public static class BlobCache_Extensions
    {
        public static AkavacheBag ToBag(this IBlobCache blobCache)
        {
            return new AkavacheBag(blobCache);
        }
    }
}