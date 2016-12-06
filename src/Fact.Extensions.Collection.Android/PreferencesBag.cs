using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Preferences;
using Android.Content;
using System.Reflection;
using Fact.Extensions.Serialization;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// TODO: Change this and other related classes to "PreferenceBag" dropping the plural, to maintain
    /// parity with Android API
    /// </remarks>
    public class PreferencesBag : IBag
    {
        readonly ISharedPreferences preferences;
        readonly ISerializationManager serializationManager;

        public PreferencesBag(ISharedPreferences preferences)
        {
            this.preferences = preferences;
        }

        ISharedPreferencesEditor Editor
        {
            get { return preferences.Edit(); }
        }

        public object this[string key, Type type]
        {
            set
            {
                var edit = Editor;
                var typeCode = Type.GetTypeCode(type);
                switch(typeCode)
                {
                    case TypeCode.String:
                        edit.PutString(key, (string) value);
                        break;

                    case TypeCode.Int16:
                    case TypeCode.Int32:
                        edit.PutInt(key, (int)value);
                        break;

                    case TypeCode.Int64:
                        edit.PutLong(key, (long)value);
                        break;

                    case TypeCode.Boolean:
                        edit.PutBoolean(key, (bool)value);
                        break;
                }
                edit.Commit();
            }
        }

        public object Get(string key, Type type)
        {
            var typeCode = Type.GetTypeCode(type);

            switch(typeCode)
            {
                case TypeCode.Boolean: return preferences.GetBoolean(key, false);
                case TypeCode.String: return preferences.GetString(key, null);
                case TypeCode.Int16:
                case TypeCode.Int32:
                    return preferences.GetInt(key, 0);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
