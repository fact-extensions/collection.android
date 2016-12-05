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
}
