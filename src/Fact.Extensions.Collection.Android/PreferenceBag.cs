using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

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
    /// </remarks>
    public class PreferenceBag : IBag, INotifyPropertyChanged
    {
        readonly ISharedPreferences preferences;
        readonly ISerializationManager serializationManager;
        // Make listener a "has a" instead of an "is a" so that Java object memory management
        // is only necessary when actually listening for changes
        Listener listener;

        event PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (listener == null)
                {
                    listener = new Listener();
                    listener.ValueChanged += Listener_ValueChanged;
                    preferences.RegisterOnSharedPreferenceChangeListener(listener);
                }

                propertyChanged += value;
            }
            remove
            {
                propertyChanged -= value;
                
                if (propertyChanged == null) // If no targets left, deallocate our Java listener
                {
                    preferences.UnregisterOnSharedPreferenceChangeListener(listener);
                    listener = null;
                }
            }
        }

        private void Listener_ValueChanged(string key)
        {
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }

        public class Listener : Java.Lang.Object, 
            ISharedPreferencesOnSharedPreferenceChangeListener
        {
            public event Action<string> ValueChanged;

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                ValueChanged?.Invoke(key);    
            }
        }

        public PreferenceBag(ISharedPreferences preferences)
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
