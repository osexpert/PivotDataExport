using System.ComponentModel;

namespace PivotDataExport
{
	public class Field<TRow>
	{
		public Area Area;
		public string Name = null!;
		public SortOrder SortOrder;
		public SortMode SortMode;
		public int GroupIndex;
		public GroupMode GroupMode;

		/// <summary>
		/// Used to get aggregated value.
		/// The return value can be of type DisplayType, in case, GetDisplayValue does nothing.
		/// The return value can be of type DataType, in case, GetDisplayValue can convert from DataType to DisplayType.
		/// </summary>
		public Func<IEnumerable<TRow>, object?> GetRowsValue = null!;

		/// <summary>
		/// User to get the value to group on (the value will be of type DataType)
		/// </summary>
		public Func<TRow, object?> GetRowValue = null!;

		/// <summary>
		/// In: TData (DataType)
		/// Out: TDisp (DisplayType)
		/// </summary>
		public Func<object?, object?> GetDisplayValue = o => o;

		// TODO: need both GroupComparer and SortComparer?
		public IEqualityComparer<object?> GroupComparer = EqualityComparer<object?>.Default;

		public IComparer<object?> SortComparer = Comparer<object?>.Default;

		/// <summary>
		/// Currently unused. But imagine a field being DateTime but the display type is DateOnly.
		/// </summary>
		public Type DisplayType = null!;

		// FIXME: kind of pointless...could simply used passed order
		//public int Index { get; set; }  // 0, 1, 2

		public Type DataType = null!;

		public DefaultValue? DisplayTypeDefaultValue;

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

		//public bool DisplayViaGetRowsValue => DisplayType != DataType;

		//internal object? GetDisplayValue(object? key)
		//{
		//	if (DisplayType != DataType)
		//	{
		//		// hmmm...we don't have the rows here...
		//		return GetRowsValue();
		//	}
		//	else
		//	{
		//		return key;
		//	}
		//}

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
			return props.Select(pd => new Field<TRow>
			{
				Name = pd.Name,
				DataType = pd.PropertyType,
				DisplayType = pd.PropertyType,
				GetRowValue = row => pd.GetValue(row),
				GetRowsValue = pd.GetValue,
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

		public  object? GetSortValue(object? v)
		{
			return SortMode == SortMode.DataValue ? v : GetDisplayValue(v);
		}

		internal object? GetGroupValue(object? v)
		{
			return GroupMode == GroupMode.DataValue ? v : GetDisplayValue(v);
		}

		//public void SetGetRowsValue<TData>(Func<IEnumerable<TRow>, TData> getRowsValue)
		//{
		//	GetRowsValue = rows => getRowsValue(rows);//.Cast<TRow>());
		//}
		//public void SetGetRowValue<TData>(Func<IEnumerable<TRow>, TData> getRowValue)
		//{
		//	GetRowValue = row => getRowValue(row);//.Cast<TRow>());
		//}
	}

	public class DefaultValue
	{
		public object? Value;

		public DefaultValue(object? value)
		{
			Value = value;			
		}
	}

	public class Field<TRow, TData> : Field<TRow>
	{
		public Field(string fieldName, Func<TRow, TData> getRowValue, Func<IEnumerable<TData>, TData> getRowsValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
			DataType = typeof(TData);
			DisplayType = typeof(TData);
		}
	}

	public class Field<TRow, TData, TDisp> : Field<TRow>
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
			GetDisplayValue = v => getDisplayValue((TData)v);
			DataType = typeof(TData);
			DisplayType = typeof(TDisp);
		}

		public Field(string fieldName, Func<TRow, TData> getRowValue, Func<IEnumerable<TData>, TDisp> getRowsValue, Func<TData, TDisp> getDisplayValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
			GetDisplayValue = v => getDisplayValue((TData)v);
			DataType = typeof(TData);
			DisplayType = typeof(TDisp);
		}

		public Field(string fieldName, Func<TRow, TData> getRowValue, Func<TData, TDisp> getDisplayValue, Func<IEnumerable<TDisp>, TDisp> getRowsValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			// FIXME: here we capture getRowValue/getDisplayValue when we should have used dynamic GetRowValue/GetDisplayValue...
			// So if GetRowValue/GetDisplayValue is changed after this point, GetRowsValue won't get the message...
			GetRowsValue = rows => getRowsValue(rows.Select(getRowValue).Select(getDisplayValue));
			GetDisplayValue = v => getDisplayValue((TData)v);
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
}