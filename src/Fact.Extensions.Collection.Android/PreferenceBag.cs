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
using System.Diagnostics;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// NOTE: Seems perhaps I should instead use preferences.All directly.  If it's more efficient,
    /// I will
    /// </remarks>
    public class PreferenceBag : IBag, 
        INotifyPropertyChanged, 
        IRemover, 
        IContainsKey<string>,
        IKeys<string>
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

        ISerializationManager SerializationManager
        {
            get
            {
                Debug.Assert(serializationManager != null, "Serialization Manager not set for this object, but needed for this operation");
                return serializationManager;
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

        public PreferenceBag(ISharedPreferences preferences, ISerializationManager serializationManager = null)
        {
            this.serializationManager = serializationManager;
            this.preferences = preferences;
        }

        ISharedPreferencesEditor Editor
        {
            get { return preferences.Edit(); }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return preferences.All.Keys;
            }
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

                    case TypeCode.Single:
                        edit.PutFloat(key, (float)value);
                        break;

                    case TypeCode.Boolean:
                        edit.PutBoolean(key, (bool)value);
                        break;

                    case TypeCode.Object:
                        serializationManager.SerializeToString(value, type);
                        break;

                    default:
                        throw new NotSupportedException();
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
                case TypeCode.Single: return preferences.GetFloat(key, 0);
                case TypeCode.Int64: return preferences.GetLong(key, 0);
                case TypeCode.Int16:
                case TypeCode.Int32:
                    return preferences.GetInt(key, 0);

                case TypeCode.Object:
                {
                    var stringValue = preferences.GetString(key, null);
                    return serializationManager.Deserialize(stringValue, type);
                }

                default:
                    throw new NotSupportedException();
            }
        }

        public void Remove(string key)
        {
            var edit = Editor;
            edit.Remove(key);
            edit.Commit();
        }


        public bool ContainsKey(string key)
        {
            return preferences.Contains(key);
        }
    }
}
