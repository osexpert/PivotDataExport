using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
{
	/// <summary>
	/// Pro: write dynamically to json since IDictionary
	/// Pro: backed by a list, so it is ordered, even if IDictionary
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class KeyValueList<T> : IDictionary<string, object?>
		where T : class
	{
		public Group<T> Group = null!;

		List<KeyValuePair<string, object?>> _list = new();

		public object? this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ICollection<string> Keys => throw new NotImplementedException();
		public ICollection<object?> Values => throw new NotImplementedException();
		public int Count => throw new NotImplementedException();
		public bool IsReadOnly => throw new NotImplementedException();
		public void Add(KeyValuePair<string, object?> item) => throw new NotImplementedException();
		public void Clear() => throw new NotImplementedException();
		public bool Contains(KeyValuePair<string, object?> item) => throw new NotImplementedException();
		public bool ContainsKey(string key) => throw new NotImplementedException();
		public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => throw new NotImplementedException();
		public bool Remove(string key) => throw new NotImplementedException();
		public bool Remove(KeyValuePair<string, object?> item) => throw new NotImplementedException();
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value) => throw new NotImplementedException();
		void IDictionary<string, object?>.Add(string key, object? value) => throw new NotImplementedException();

		public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() 
		{
			return _list.GetEnumerator();
		}

		internal void Add(string fieldName, object? key)
		{
			var kv = new KeyValuePair<string, object?>(fieldName, key);
			_list.Add(kv);
		}

		//internal KeyValueClass<T> GetOrCreate(Group<T> grp)
		//{
		//	if (Group == null)
		//		throw new Exception();
		//	if (Group == grp)
		//		return this;

		//}

		internal KeyValuePair<string, object?>? Last()
		{
			return _list.Last();
		}






















		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

}
