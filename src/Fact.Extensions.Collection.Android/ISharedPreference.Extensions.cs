using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Fact.Extensions.Serialization;

namespace Fact.Extensions.Collection.Interceptor
{
    public static class ISharedPreference_Extensions
    {
        // Not using this while we iron out dependency nightmares
#if UNUSED
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
#endif

        /// <summary>
        /// Wrap shared prefs into a bag interface
        /// </summary>
        /// <param name="sharedPreferences"></param>
        /// <param name="serializationManager"></param>
        /// <returns></returns>
        public static PreferenceBag AsBag(this ISharedPreferences sharedPreferences, ISerializationManager serializationManager)
        {
            var bag = new PreferenceBag(sharedPreferences, serializationManager);
            return bag;
        }
    }
}
