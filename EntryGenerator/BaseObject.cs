using System;
using System.Collections.Generic;

namespace EntryGenerator
{
    [Serializable]
    public abstract class BaseObject : PropertyNotifier
    {
        private readonly IDictionary<string, object> _mValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public T GetValue<T>(string key)
        {
            object value = GetValue(key);
            return (value is T value1) ? value1 : default;
        }

        private object GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            return _mValues.ContainsKey(key) ? _mValues[key] : null;
        }

        public void SetValue(string key, object value)
        {
            if (!_mValues.ContainsKey(key))
            {
                _mValues.Add(key, value);
            }
            else
            {
                _mValues[key] = value;
            }
            OnPropertyChanged(key);
        }
    }
}
