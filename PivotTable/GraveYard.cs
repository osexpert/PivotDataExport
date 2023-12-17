#if false

		private List<KeyValueClass<TRow>> GetFullRows_NestedDict(Field[] dataFields,
			Field[] rowFieldsInGroupOrder,
			List<Group<TRow>> lastRowGroups,
			List<Group<TRow>> lastColGroups, /* sorted */
			List<Group<TRow>> firstColGroups, /* not sorted */
			List<TableColumn> tableCols)
		{
			List<KeyValueClass<TRow>> rows = new();

			var colFieldsInSortOrder = _data.fields.Where(f => f.FieldType == FieldType.ColGroup)
				.Where(f => f.Sorting != Sorting.None)
				.OrderBy(f => f.SortIndex).ToArray();

			Dictionary<Group<TRow>, string> groupNameLookup = new();

			foreach (var lastRowGroup in lastRowGroups)
			{
				KeyValueClass<TRow> row = new KeyValueClass<TRow>();
				rows.Add(row);

				var current = lastRowGroup;
				do
				{
					row.Add(current.Field.FieldName, current.Key);
					current = current.ParentGroup;
				} while (current != null && !current.IsRoot);


				foreach (var idata in SortColGroups(lastRowGroup.IntersectData, colFieldsInSortOrder, ele => ele.Key))
				{
					var lastColGroup = idata.Key;
					var values = idata.Value;

					KeyValueClass<TRow> sub_row = new KeyValueClass<TRow>();
					foreach (var z in dataFields.Zip(values))
					{
						sub_row.Add(z.First.FieldName, z.Second);
					}

					string grpNamePath = GetGroupNamePath(lastColGroup, groupNameLookup);
					row.Add(grpNamePath, sub_row);
				}

				//					Stack<WrapperObjNested>

				// this produce one row in the table
				//foreach (var lastColGrp in lastColGroups)
				//{
				//	var startIdx = grpStartIdx[lastColGrp];

				//	if (lastRowGroup.IntersectData.TryGetValue(lastColGrp, out var values))
				//	{

				//		KeyValueClass sub_row = GetCreateSubRow(lastColGrp, row);

				//		foreach (var z in dataFields.Zip(values))
				//		{
				//			sub_row.Add(z.First.FieldName, z.Second);
				//		}

				//		// write values
				//		//Array.Copy(values, 0, row, startIdx, values.Length);

				//		//							colGrp.ParentGroup

				//		// push this on the parent somehow...

				//		//WrapperObjNested nrow = new WrapperObjNested(aRow, tableCols);


				//		//							aRow[startIdx] = nrow;
				//	}
				//}

				//WrapperObjNested row = new WrapperObjNested(aRow, tableCols);
				//rows.Add(row);
			}

			return rows;
		}

#endif

#if false
		private string GetGroupNamePath(Group<TRow> lastColGroup, Dictionary<Group<TRow>, string> dict)
		{
			if (!dict.TryGetValue(lastColGroup, out var name))
			{

				Stack<TableGroup> tgs = new();

				var current = lastColGroup;
				do
				{
					tgs.Push(new TableGroup
					{
						Name = current.Field.FieldName,
						//DataType = parent.Field.DataType,
						Value = current.Key
					});

					current = current.ParentGroup;
				} while (current != null && !current.IsRoot);

				//			foreach (var dataField in dataFields)
				{
					// /fdfd:34/gfgfg:fdfd/gfgfgfggf
					name = string.Join('/', tgs.Select(tg => $"{DataPath.Escape(tg.Name)}:{DataPath.Escape(Convert.ToString(tg.Value) ?? string.Empty)}"));
					//var combNAme = $"/{middle}/{Escape(dataField.FieldName)}";

					//					tablecols_after.Add(dataField.ToTableColumn(combNAme, tgs.Select(tg => tg.Value).ToArray()));
				}

				dict.Add(lastColGroup, name);
			}
			return name;
		}
#endif

#if false
		/// <summary>
		/// I think this one nests just one level
		/// </summary>
		/// <returns></returns>
		public Table<KeyValueClass<TRow>> GetTable_NestedDict()
		{
			var lastRowGroups = _data.allRowGroups.Last();
			var firstColGroups = _data.allColGroups.First();
			var lastColGroups = _data.allColGroups.Last();

			var colFieldsInSortOrder = _data.fields.Where(f => f.FieldType == FieldType.ColGroup)
				.Where(f => f.Sorting != Sorting.None)
				.OrderBy(f => f.SortIndex).ToArray();

			var lastColGroupsSorted = SortColGroups(lastColGroups, colFieldsInSortOrder, ele => ele).ToList();

			// create a new GetFullRows that create nested objects

			var tableCols = CreateTableCols(_data.dataFields, _data.rowFieldsInGroupOrder, lastColGroupsSorted);
			//			rowsss = SortRowsNew(rowsss, tableCols);

			Table<KeyValueClass<TRow>> t = new Table<KeyValueClass<TRow>>();

			//	
			//	
			//			t.Rows = toRows(rowsss, tableCols);

			var rowsss = GetFullRows_NestedDict(_data.dataFields, _data.rowFieldsInGroupOrder, lastRowGroups,
				lastColGroupsSorted, firstColGroups, tableCols);

			t.Rows = rowsss;
			t.Columns = tableCols;
			t.RowGroups = _data.rowFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();
			t.ColumnGroups = _data.colFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();

			return t;
		}
#endif


#if false
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
{


	internal class NullableDict<K, V> : IDictionary<K, V>
	{
		Dictionary<K, V> dict = new Dictionary<K, V>();
		V nullValue = default(V);
		bool hasNull = false;

		public NullableDict()
		{
		}

		public void Add(K key, V value)
		{
			if (key == null)
				if (hasNull)
					throw new ArgumentException("Duplicate key");
				else
				{
					nullValue = value;
					hasNull = true;
				}
			else
				dict.Add(key, value);
		}

		public bool ContainsKey(K key)
		{
			if (key == null)
				return hasNull;
			return dict.ContainsKey(key);
		}

		public ICollection<K> Keys
		{
			get
			{
				if (!hasNull)
					return dict.Keys;

				List<K> keys = dict.Keys.ToList();
				keys.Add(default(K));
				return new ReadOnlyCollection<K>(keys);
			}
		}

		public bool Remove(K key)
		{
			if (key != null)
				return dict.Remove(key);

			bool oldHasNull = hasNull;
			hasNull = false;
			return oldHasNull;
		}

		public bool TryGetValue(K key, out V value)
		{
			if (key != null)
				return dict.TryGetValue(key, out value);

			value = hasNull ? nullValue : default(V);
			return hasNull;
		}

		public ICollection<V> Values
		{
			get
			{
				if (!hasNull)
					return dict.Values;

				List<V> values = dict.Values.ToList();
				values.Add(nullValue);
				return new ReadOnlyCollection<V>(values);
			}
		}

		public V this[K key]
		{
			get
			{
				if (key == null)
					if (hasNull)
						return nullValue;
					else
						throw new KeyNotFoundException();
				else
					return dict[key];
			}
			set
			{
				if (key == null)
				{
					nullValue = value;
					hasNull = true;
				}
				else
					dict[key] = value;
			}
		}

		public void Add(KeyValuePair<K, V> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			hasNull = false;
			dict.Clear();
		}

		public bool Contains(KeyValuePair<K, V> item)
		{
			if (item.Key != null)
				return ((ICollection<KeyValuePair<K, V>>)dict).Contains(item);
			if (hasNull)
				return EqualityComparer<V>.Default.Equals(nullValue, item.Value);
			return false;
		}

		public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<K, V>>)dict).CopyTo(array, arrayIndex);
			if (hasNull)
				array[arrayIndex + dict.Count] = new KeyValuePair<K, V>(default(K), nullValue);
		}

		public int Count
		{
			get { return dict.Count + (hasNull ? 1 : 0); }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(KeyValuePair<K, V> item)
		{
			V value;
			if (TryGetValue(item.Key, out value) && EqualityComparer<V>.Default.Equals(item.Value, value))
				return Remove(item.Key);
			return false;
		}

		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			if (!hasNull)
				return dict.GetEnumerator();
			else
				return GetEnumeratorWithNull();
		}

		private IEnumerator<KeyValuePair<K, V>> GetEnumeratorWithNull()
		{
			yield return new KeyValuePair<K, V>(default(K), nullValue);
			foreach (var kv in dict)
				yield return kv;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
#endif

#if false
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
{
	class WrapperObjNested : IDictionary<string, object?>
	{
		public object?[] _row;
		public List<TableColumn> _tcols;

		public WrapperObjNested(object?[] row, List<TableColumn> tcols)
		{
			_row = row;
			_tcols = tcols;
		}

		public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
		{
			var a = _row.Zip(_tcols).Select(a => new KeyValuePair<string, object?>(a.Second.Name, a.First));
			return a.GetEnumerator();
		}


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

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

}
#endif