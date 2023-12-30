#if false
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace PivotDataTable
{
	public class KeyValueZipList : IDictionary<string, object?>
	{
		object?[] _row;
		List<TableColumn> _tcols;

		public KeyValueZipList(object?[] row, List<TableColumn> tcols)
		{
			_row = row;
			_tcols = tcols;
		}

		public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
		{
			var a = _row.Zip(_tcols).Select(a => new KeyValuePair<string, object?>(a.Second.Name, a.First));
			return a.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		public object? this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ICollection<string> Keys => throw new NotImplementedException();
		public ICollection<object?> Values => throw new NotImplementedException();
		public int Count => throw new NotImplementedException();
		public bool IsReadOnly => throw new NotImplementedException();
		public void Add(string key, object? value) => throw new NotImplementedException();
		public void Add(KeyValuePair<string, object?> item) => throw new NotImplementedException();
		public void Clear() => throw new NotImplementedException();
		public bool Contains(KeyValuePair<string, object?> item) => throw new NotImplementedException();
		public bool ContainsKey(string key) => throw new NotImplementedException();
		public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => throw new NotImplementedException();
		public bool Remove(string key) => throw new NotImplementedException();
		public bool Remove(KeyValuePair<string, object?> item) => throw new NotImplementedException();
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value) => throw new NotImplementedException();


	}


}

#endif