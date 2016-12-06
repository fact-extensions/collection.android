using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;

namespace Fact.Extensions.Collection.Interceptor
{
    public static class ISharedPreference_Extensions
    {
        /// <summary>
        /// Using an interceptor, presents sharedPreferences as a strongly-typed interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sharedPreferences"></param>
        /// <returns></returns>
        public static T ToInterface<T>(this ISharedPreferences sharedPreferences)
            where T : class
        {
            var bag = new PreferenceBag(sharedPreferences);
            return bag.ToInterface<T>();
        }
    }
}
