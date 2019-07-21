using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxShared.Containers
{
	/// <summary>
	/// Uses a two dictionaries to give fast access to the values by keys and keys by their values
	/// All keys and values have to be unique as they both represent the key to each other
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	public class BiDictionary<T1, T2>
	{
		public BiDictionary()
		{}

		public BiDictionary(Dictionary<T1, T2> keysToValue)
		{
			KeyToValue = keysToValue;
			foreach (var keyValue in keysToValue)
			{
				ValueToKey.Add(keyValue.Value, keyValue.Key);
			}
		}

		public BiDictionary(Dictionary<T2, T1> valuesToKey)
		{
			ValueToKey = valuesToKey;
			foreach (var valueKey in valuesToKey)
			{
				KeyToValue.Add(valueKey.Value, valueKey.Key);
			}
		}

		public void Add(T1 key, T2 value)
		{
			KeyToValue.Add(key, value);
			ValueToKey.Add(value, key);
		}

		public bool TryGet(T1 key, out T2 value)
		{
			return KeyToValue.TryGetValue(key, out value);
		}

		public bool TryGet(T2 value, out T1 key)
		{
			return ValueToKey.TryGetValue(value, out key);
		}

		public bool ContainsKey(T1 key)
		{
			return KeyToValue.ContainsKey(key);
		}

		public bool ContainsValue(T2 value)
		{
			return ValueToKey.ContainsKey(value);
		}

		public T2 this[T1 key]
		{
			get
			{
				return KeyToValue[key];
			}
			set
			{
				T2 oldValue = KeyToValue[key];
				if (!oldValue.Equals(value))
				{
					ValueToKey.Remove(oldValue);
					KeyToValue[key] = value;
					ValueToKey[value] = key;
				}
			}
		}

		public T1 this[T2 valueKey]
		{
			get { return ValueToKey[valueKey]; }
			set
			{
				T1 oldValue = ValueToKey[valueKey];
				if (!oldValue.Equals(value))
				{
					KeyToValue.Remove(oldValue);
					ValueToKey[valueKey] = value;
					KeyToValue[value] = valueKey;
				}
			}
		}

		public void Remove(T1 key)
		{
			if (KeyToValue.TryGetValue(key, out T2 value))
			{
				KeyToValue.Remove(key);
				ValueToKey.Remove(value);
			}
		}

		public void Remove(T2 value)
		{
			if (ValueToKey.TryGetValue(value, out T1 key))
			{
				ValueToKey.Remove(value);
				KeyToValue.Remove(key);
			}
		}

		[JsonProperty]
		public Dictionary<T1, T2> KeyToValue { get; private set; } = new Dictionary<T1, T2>();
		[JsonProperty]
		public Dictionary<T2, T1> ValueToKey { get; private set; } = new Dictionary<T2, T1>();
	}
}
