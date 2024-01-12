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
				.Where(f => f.SortOrder != SortOrder.None)
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
				.Where(f => f.SortOrder != SortOrder.None)
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


//}
// else...a mode for output 1:1?
//{
//	foreach (var l in _list)
//	{
//		var r = res.NewRow();

//		foreach (Field dataField in GetDataFields())
//		{
//			var getter = _props[dataField.FieldName];
//			// TODO: get value from multiple ROWS
//			var theValue = getter.GetValue(l.Yield());

//			r[dataField.FieldName] = theValue;
//		}
//	}
//}

//public class PathElement
//{
//	public string Name;
//	public object Value;
//}

//public static PathElement[] SplitPathName(string str)
//{
//	if (!str.StartsWith('/'))
//		throw new ArgumentException("Must start with '/'");

//	var parts = str.Split('/');
//	// make sure first part is empty

//	PathElement[] res = new PathElement[parts.Length - 1];



//}

#if false
		public class DissectedPropertyName
		{
			public KeyValuePair<string, string?>[]? KeyValues;
			public string? FinalKey;
		}

		public static bool IsKeyValuePropertyName(string propName)
		{
			return propName.StartsWith('/');
		}

		/// <summary>
		/// return false if the property name is not a keyValue property name (does not start with "/")
		/// </summary>
		/// <param name="propName"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static DissectedPropertyName DissectKeyValuePropertyName(string propName)
		{
			//list = null!;

			if (propName.StartsWith('/'))
			{
				var parts = propName.Split('/');

				// make sure first part is empty

				//KeyValuePair<string, object?>[] res = new KeyValuePair<string, object?>[parts.Length - 1];

				//DissectedName res = new();

				List<KeyValuePair<string, string?>> ilist = new();
				string? finaleKey = null;

				for (int i = 0; i < parts.Length; i++)
				{
					var keyVal = parts[i].Split(':');
					if (keyVal.Length == 0)
						throw new Exception();
					else if (keyVal.Length == 1) // only ok for the last ele
					{
						if (i < parts.Length)
							throw new Exception("Key alone only valid for last element");
						finaleKey = Unescape(keyVal[0]);
					}
					else if (keyVal.Length == 2)
					{
						ilist.Add(new KeyValuePair<string, string?>(Unescape(keyVal[0]), keyVal[1] == "?" ? null : Unescape(keyVal[1])));
					}
					else
						throw new Exception("more than 2 parts");
				}

				return new DissectedPropertyName { FinalKey = finaleKey, KeyValues = ilist.Any() ? ilist.ToArray() : null };
				//return true;
			}

			//return false;
			throw new FormatException("Not a keyValue property name (does not start with '/')");
		}
#endif

#if false
using System.ComponentModel;

namespace PivotDataTable
{

	class RowList<TRow> : List<TRow>, ITypedList
	{
		PropertyDescriptorCollection _props;

		public RowList(PropertyDescriptorCollection props)
		{
			_props = props;
		}

		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) => _props;
		public string GetListName(PropertyDescriptor[] listAccessors) => string.Empty;
	}

	//public abstract class PropertyColumn : PropertyDescriptor
	//{
	//	public PropertyColumn(string name, Attribute[] attrs) : base(name, attrs)
	//	{
	//	}
	//}

	/// <summary>
	/// rename DataField ?
	/// FieldProperty? _props
	/// FieldData _datas
	/// FieldStore _Stores
	/// _props
	/// _datas
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	/// <typeparam name="TProp"></typeparam>
	public class Property<TRow, TProp> : PropertyDescriptor
	{
		readonly Func<IEnumerable<TRow>, TProp> _getValue;

		public Property(string fieldName, Func<IEnumerable<TRow>, TProp> getValue)
			: base(fieldName, null)
		{
			_getValue = getValue;
		}

		public Property(string fieldName)
			: base(fieldName, null)
		{
			_getValue = GetValue;
		}

		public override object? GetValue(object? component)
		{
			if (component is IEnumerable<TRow> rows)
			{
				return _getValue(rows);
			}
			else
				throw new InvalidOperationException("wrong component type");
		}

		protected virtual TProp GetValue(IEnumerable<TRow> rows)
			=> throw new InvalidOperationException("GetValue(IEnumerable<TRow> rows) must be overridden");

		public override Type PropertyType => typeof(TProp);

		public override void ResetValue(object component)
		{
			// Not relevant.
		}

		public override void SetValue(object? component, object? value) => throw new NotImplementedException();

		public override bool ShouldSerializeValue(object component) => true;
		public override bool CanResetValue(object component) => false;

		public override Type ComponentType => typeof(IEnumerable<TRow>);
		public override bool IsReadOnly => true;


	}


}

#endif

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

#if false

namespace PivotDataTable
{
	public class GroupingKey<T> : IEquatable<GroupingKey<T>>
	{
		public T[] Groups { get; init; }
		static EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

		public GroupingKey(T[] groups)
		{
			Groups = groups;
		}

		public override int GetHashCode()
		{
			var hc = new HashCode();
			foreach (var g in Groups)
				hc.Add(g);
			return hc.ToHashCode();
		}

		public override bool Equals(object? other)
		{
			return Equals(other as GroupingKey<T>);
		}

		public bool Equals(GroupingKey<T>? other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (Groups.Length != other.Groups.Length)
				return false;

			for (int i = 0; i < Groups.Length; i++)
			{
				if (!equalityComparer.Equals(Groups[i], other.Groups[i]))
					return false;
			}

			return true;
		}

		public override string ToString()
		{
			string[] array = new string[Groups.Length];
			for (int i = 0; i < Groups.Length; i++)
				array[i] = $"Group{i} = {Groups[i]}";

			return $"{{ {string.Join(", ", array)} }}";
		}
	}
}

#endif

#if false
		private List<object?[]> SortRows(List<object?[]> rows, List<TableColumn> tableCols)
		{
			var sortFields = _data.fields
				.Where(f => f.FieldArea != Area.Column) // SortOrder col groups mean SortOrder the columns themself (the labels)
				.Where(f => f.SortOrder != SortOrder.None)
				.OrderBy(f => f.GroupIndex);

			if (sortFields.Any())
			{
				IOrderedEnumerable<object?[]> sorter = null!;
				foreach (var sf in sortFields)
				{
					// TODO lookup idx from filedname
					var sortCol = tableCols.Single(tc => tc.Name == sf.FieldName);
					var idx = tableCols.IndexOf(sortCol);

					if (sorter == null)
						sorter = sf.SortOrder == SortOrder.Asc ? rows.OrderBy(r => r[idx], sf.SortComparer) : rows.OrderByDescending(r => r[idx], sf.SortComparer);
					else
						sorter = sf.SortOrder == SortOrder.Asc ? sorter.ThenBy(r => r[idx], sf.SortComparer) : sorter.ThenByDescending(r => r[idx], sf.SortComparer);
				}
				rows = sorter.ToList();
			}

			return rows;
		}
#endif


//public void Sort(GroupedData<TRow> data)
//{
//	//			data.rowFieldsInGroupOrder
//	var comparer = Comparer<object>.Default;
//	foreach (var grpLevel in data.allRowGroups)
//	{
//		var first = grpLevel.First();
//		if (first.Field.SortOrder != SortOrder.None)
//		{
//			bool asc = first.Field.SortOrder == SortOrder.Asc;
//			grpLevel.Sort((a, b) => asc ? comparer.Compare(a.Key, b.Key) : comparer.Compare(b.Key, a.Key));
//		}
//	}
//}