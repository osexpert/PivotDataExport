using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PivotDataExport;

public class Field<TRow> where TRow : class
{
	/// <summary>
	/// If not set, Name is used as caption
	/// TODO: not used
	/// </summary>
//		public string? Caption;

	public Area Area { get; set; }
	public string Name { get; set; } = null!;
	public SortOrder SortOrder { get; set; }
	public SortMode SortMode { get; set; }
	public int GroupIndex { get; set; }
	public GroupMode GroupMode { get; set; }


	/// <summary>
	/// Used to get aggregated value.
	/// The return value can be of type DisplayType, in case, GetDisplayValue does nothing.
	/// The return value can be of type DataType, in case, GetDisplayValue can convert from DataType to DisplayType.
	/// </summary>
	internal Func<IEnumerable<TRow>, object?> GetRowsValue = null!;

	/// <summary>
	/// User to get the value to group on (the value will be of type DataType)
	/// </summary>
	internal Func<TRow, object?> GetRowValue = null!;

	/// <summary>
	/// In: TData (DataType)
	/// Out: TDisp (DisplayType)
	/// </summary>
	internal Func<object?, object?> GetDisplayValue = o => o;

	// TODO: need both GroupComparer and SortComparer?
	internal IEqualityComparer<object?> GroupComparer = EqualityComparer<object?>.Default;

	internal IComparer<object?> SortComparer = Comparer<object?>.Default;

	/// <summary>
	/// Example: field being DateTime but the display type is DateOnly.
	/// </summary>
	public Type DisplayType { get; internal set; } = null!;

	// FIXME: kind of pointless...could simply used passed order
	//public int Index { get; set; }  // 0, 1, 2

	public Type DataType { get; internal set; } = null!;

	public DefaultValue? DisplayTypeDefaultValue { get; set; }

	internal object? GetDisplayTypeDefaultValue()
	{
		if (DisplayTypeDefaultValue != null)
			return DisplayTypeDefaultValue.Value;

		if (DisplayType == typeof(string))
			return "";

		if (DisplayType.IsValueType)
			return Activator.CreateInstance(DisplayType);

		return null;
	}

	internal TableColumn ToTableColumn()
	{
		return new()
		{
			Name = Name,
			//DataType = DataType, // displaytype???
			DataType = DisplayType,
			FieldArea = Area,
			SortOrder = SortOrder,
			GroupIndex = GroupIndex
		};
	}

	internal TableColumn ToTableColumn(string combName, object?[] groupVals)
	{
		return new()
		{
			Name = combName,
			FieldArea = Area,
			//DataType = DataType, // displaytype???
			DataType = DisplayType,
			GroupIndex = GroupIndex,
			SortOrder = SortOrder,
			GroupValues = groupVals
		};
	}

	public override string ToString()
	{
		return $"Name: {Name}, Area: {Area}";
	}

	public static List<Field<TRow>> CreateFieldsFromType()
	{
		return CreateFieldsFromProperties(TypeDescriptor.GetProperties(typeof(TRow)));
	}

	public static List<Field<TRow>> CreateFieldsFromProperties(IEnumerable<PropertyDescriptor> props)
	{
		// pd.ComponentType should be TRow
		return props.Select(pd => new Field<TRow>
		{
			Name = pd.Name,
			DataType = pd.PropertyType,
			DisplayType = pd.PropertyType,
			GetRowValue = row => pd.GetValue(row),
			GetRowsValue = pd.GetValue
		}).ToList();
	}

	public static List<Field<TRow>> CreateFieldsFromProperties(PropertyDescriptorCollection props)
	{
		return CreateFieldsFromProperties(props.Cast<PropertyDescriptor>());
	}

	public static List<Field<TRow>> CreateFieldsFromTypedList(ITypedList props)
	{
		return CreateFieldsFromProperties(props.GetItemProperties(null));
	}

	internal object? GetSortValue(object? v)
	{
		return SortMode == SortMode.DataValue ? v : GetDisplayValue(v);
	}

	internal object? GetGroupValue(object? v)
	{
		return GroupMode == GroupMode.DataValue ? v : GetDisplayValue(v);
	}

	HybridDictionary? _tags = null;

	public IDictionary Tags
	{
		get
		{
			if (_tags == null)
				_tags = new();
			return _tags;
		}
	}

}

public class DefaultValue
{
	public object? Value;

	public DefaultValue(object? value)
	{
		Value = value;			
	}
}

public class Field<TRow, TData> : Field<TRow, TData, TData> where TRow : class
{
	public Field(string fieldName, Func<TRow, TData> getRowValue, Func<IEnumerable<TData>, TData> getRowsValue) 
		: base(fieldName, getRowValue, getRowsValue)
	{
	}
	public Field(string fieldName, Func<TRow, TData> getRowValue, Func<IEnumerable<TData>, TData> getRowsValue, Func<TData, TData> getDisplayValue)
		: base(fieldName, getRowValue, getRowsValue, getDisplayValue)
	{
	}
}

public class Field<TRow, TData, TDisp> : Field<TRow> where TRow : class
{
	public Field(string fieldName, Func<TRow, TData> getRowValue, Func<IEnumerable<TData>, TDisp> getRowsValue)
	{
		Name = fieldName;
		GetRowValue = row => getRowValue(row);
		GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
		DataType = typeof(TData);
		DisplayType = typeof(TDisp);
	}

	public Field(string fieldName, Func<TRow, TData> getRowValue, Func<IEnumerable<TData>, TData> getRowsValue, Func<TData, TDisp> getDisplayValue)
	{
		Name = fieldName;
		GetRowValue = row => getRowValue(row);
		GetRowsValue = rows => getDisplayValue(getRowsValue(rows.Select(getRowValue)));
		GetDisplayValue = v => getDisplayValue((TData)v!);
		DataType = typeof(TData);
		DisplayType = typeof(TDisp);
	}
}

public enum Area
{
	Data = 0,
	Row = 1,
	Column = 2,
}

public enum SortOrder
{
	None = 0,
	Asc = 1,
	Desc = 2
}

public enum SortMode
{
	DataValue = 0,
	DisplayValue = 1
}
public enum GroupMode
{
	DataValue = 0,
	DisplayValue = 1
}
