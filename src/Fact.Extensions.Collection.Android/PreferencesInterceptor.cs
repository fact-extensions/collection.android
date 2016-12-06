using Fact.Extensions.Collection.Interceptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using System.Reflection;

using Android.App;
using Android.Preferences;
using Android.Content;

namespace Fact.Extensions.Collection
{
    public class PreferencesInterceptor : PropertyInterceptor
    {
        readonly ISharedPreferences preferences;

        public PreferencesInterceptor(ISharedPreferences preferences)
        {
            this.preferences = preferences;
        }

        //global::Android.Preferences.P
        protected override object Get(IInvocation invocation, PropertyInfo prop)
        {
            throw new NotImplementedException();
        }

        protected override void Set(IInvocation invocation, PropertyInfo prop, object value)
        {
            throw new NotImplementedException();
        }
    }


    public class BagInterceptor : NamedPropertyInterceptor
    {
        readonly IBag bag;

        // FIX: Needs cleanup, definitely redundant and sloppy
        protected override string ResolveName(System.Reflection.PropertyInfo prop)
        {
            if (resolveName != null)
                return resolveName(prop);

            return nameResolver != null ? nameResolver.ResolveName(prop) : prop.Name;
        }

        public BagInterceptor(IBag bag, ResolveNameDelegate resolveName = null)
            : base(null, null, resolveName)
        {
            this.bag = bag;
        }

        protected override object Get(IInvocation invocation, PropertyInfo prop)
        {
            return bag.Get(ResolveName(prop), prop.PropertyType);
        }

        protected override void Set(IInvocation invocation, PropertyInfo prop, object value)
        {
            bag.Set(ResolveName(prop), value, prop.PropertyType);
        }
    }

    public static class IBag_Extensions
    {
        // FIX: temporarily in use until we move this into actual interceptor lib
        internal static readonly ProxyGenerator Proxy = new ProxyGenerator();

        public static T ToInterface<T>(this IBag bag)
            where T: class
        {
            var interceptor = new BagInterceptor(bag);
            return Proxy.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }
    }


    public static class ISharedPreference_Extensions
    {
        /// <summary>
        /// Using an interceptor, presents sharedPreferences as a strongly-typed interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sharedPreferences"></param>
        /// <returns></returns>
        public static T ToInterface<T>(this ISharedPreferences sharedPreferences)
            where T: class
        {
            var bag = new PreferencesBag(sharedPreferences);
            return bag.ToInterface<T>();
        }
    }
}
