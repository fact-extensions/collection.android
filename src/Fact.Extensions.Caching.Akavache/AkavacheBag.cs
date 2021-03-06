﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Akavache;
using System.Reactive.Linq;
using Fact.Extensions.Serialization;
using Fact.Extensions.Collection;
using Android.Util;

namespace Fact.Extensions.Caching
{
    public class AkavacheBag : ICache, ICacheAsync, IKeys<string>, IContainsKey<string>
    {
        const string TAG = nameof(AkavacheBag);
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
                yield return typeof(SlidingTimeExpiration);
                yield return typeof(AbsoluteTimeExpiration);
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
            return await serializationManager.DeserializeAsyncHelper(value, type);
        }

        public async Task SetAsync(string key, object value, Type type)
        {
            var valueByteArray = serializationManager.SerializeToByteArray(value, type);
            await blobCache.Insert(key, valueByteArray);
        }

        public void Set(string key, object value, Type type, params ICacheItemOption[] options)
        {
            //if (options.Any()) throw new InvalidOperationException("Cache options not supported yet");
            var valueByteArray = serializationManager.SerializeToByteArray(value, type);
            Log.Verbose(TAG, "Set phase 1");
            /*
            var setAsyncHelper = SetAsyncHelper(key, valueByteArray, type, options);
            Log.Info(TAG, "Set phase 1.1");
            var result = setAsyncHelper.GetAwaiter().GetResult();
            //setAsyncHelper.Wait();
            Log.Info(TAG, "Set phase 2");
            */
            // FIX: Pokey but functional way to do this.  Hard to test, which is why it's still this way
            var result = Task.Run(async () => await SetAsyncHelper(key, valueByteArray, type, options)).Result;
            if (result) return;
            blobCache.Insert(key, valueByteArray).Wait();
            Log.Verbose(TAG, "Set phase 3");
            //this[key, type] = value;
        }

        public void Remove(string key)
        {
            blobCache.Invalidate(key).Wait();
        }

        public bool TryGet(string key, Type type, out object value)
        {
            if(ContainsKey(key))
            {
                try
                {
                    value = Get(key, type);
                }
                catch(KeyNotFoundException e)
                {
                    // For race condition where key expires after ContainsKey says it's present
                    value = null;
                    return false;
                }
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }


        public async Task<Tuple<bool, object>> TryGetAsync(string key, Type type)
        {
            if (ContainsKey(key))
            {
                try
                {
                    return Tuple.Create(true, await GetAsync(key, type));
                }
                catch(KeyNotFoundException e)
                {
                    // For race condition where key expires after ContainsKey says it's present
                    // NOTE: I don't think it's usually a race condition, I think instead my code is malfunctioning
                    Log.Verbose(TAG, "Encountered race condition, key not actually present");
                    return Tuple.Create(false, (object)null);
                }
            }
            else
                return Tuple.Create(false, (object)null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueByteArray"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns>true if helper handled set operation</returns>
        async Task<bool> SetAsyncHelper(string key, byte[] valueByteArray, Type type, params ICacheItemOption[] options)
        {
            foreach (var option in options)
            {
                if (option is SlidingTimeExpiration)
                {
                    await blobCache.Insert(key, valueByteArray, ((SlidingTimeExpiration)option).Duration);
                    return true;
                }
                else if (option is AbsoluteTimeExpiration)
                {
                    await blobCache.Insert(key, valueByteArray, ((AbsoluteTimeExpiration)option).Expiry);
                    return true;
                }
            }

            return false;
        }

        public async Task SetAsync(string key, object value, Type type, params ICacheItemOption[] options)
        {
            var valueByteArray = serializationManager.SerializeToByteArray(value, type);
            if (await SetAsyncHelper(key, valueByteArray, type, options)) return;
            await blobCache.Insert(key, valueByteArray);
        }

        public async Task RemoveAsync(string key)
        {
            await blobCache.Invalidate(key);
        }

        public bool ContainsKey(string key)
        {
            var result = blobCache.GetCreatedAt(key).Wait();
            // FIX: Temporarily logging this, as I suspect it of always returning true
            Log.Verbose(TAG, $"ContainsKey: {key} = {result.HasValue}");
            return result.HasValue;
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
