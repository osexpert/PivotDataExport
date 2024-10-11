using System.ComponentModel;

namespace PivotDataExport
{
	public class Field<TRow>
	{
		public Area Area;

		public string Name = null!;

		public SortOrder SortOrder;

		public int GroupIndex;

		//public Func<object?, object?> GetRowValue = null!;

		/// <summary>
		/// Used to get aggregated value.
		/// The return value can be of type DisplayType, in case, GetDisplayValue does nothing.
		/// The return value can be of type DataType, in case, GetDisplayValue can convert from DataType to DisplayType.
		/// </summary>
		public Func<IEnumerable<TRow>, object?> GetRowsValue = null!;

		/// <summary>
		/// User to get the value to group on (the value will be of type DataType)
		/// 
		/// TODO: is this ever used for display??
		/// 
		/// </summary>
		public Func<TRow, object?> GetRowValue = null!;

		/// <summary>
		/// In: TProp (DataType)
		/// Out: TDisp (DisplayType)
		/// </summary>
		public Func<object?, object?> GetDisplayValue = o => o;

		// TODO: need both GroupComparer and SortComparer?
		public IEqualityComparer<object?> GroupComparer = EqualityComparer<object?>.Default;

		public IComparer<object?> SortComparer = Comparer<object?>.Default;

		//public Func<object?, object?> GetDisplayValue = (o) => o;

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

		public static List<Field<object>> CreateFieldsFromType<T>()
		{
			return CreateFieldsFromProperties(TypeDescriptor.GetProperties(typeof(T)));
		}

		public static List<Field<object>> CreateFieldsFromProperties(IEnumerable<PropertyDescriptor> props)
		{
			return props.Select(pd => new Field<object>
			{
				Name = pd.Name,
				DataType = pd.PropertyType,
				DisplayType = pd.PropertyType,
				GetRowValue = pd.GetValue,
				GetRowsValue = pd.GetValue,
			}).ToList();
		}

		public static List<Field<object>> CreateFieldsFromProperties(PropertyDescriptorCollection props)
		{
			return CreateFieldsFromProperties(props.Cast<PropertyDescriptor>());
		}

		public static List<Field<object>> CreateFieldsFromTypedList(ITypedList props)
		{
			return CreateFieldsFromProperties(props.GetItemProperties(null));
		}

		//public void SetGetRowsValue<TProp>(Func<IEnumerable<TRow>, TProp> getRowsValue)
		//{
		//	GetRowsValue = rows => getRowsValue(rows);//.Cast<TRow>());
		//}
		//public void SetGetRowValue<TProp>(Func<IEnumerable<TRow>, TProp> getRowValue)
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

	public class Field<TRow, TProp> : Field<TRow>
	{
		public Field(string fieldName, Func<TRow, TProp> getRowValue, Func<IEnumerable<TProp>, TProp> getRowsValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
			DataType = typeof(TProp);
			DisplayType = typeof(TProp);
		}
	}

	public class Field<TRow, TProp, TDisp> : Field<TRow>
	{
		public Field(string fieldName, Func<TRow, TProp> getRowValue, Func<IEnumerable<TProp>, TDisp> getRowsValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
			DataType = typeof(TProp);
			DisplayType = typeof(TDisp);
		}

		public Field(string fieldName, Func<TRow, TProp> getRowValue, Func<IEnumerable<TProp>, TProp> getRowsValue, Func<TProp, TDisp> getDisplayValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			GetRowsValue = rows => getDisplayValue(getRowsValue(rows.Select(getRowValue)));
			GetDisplayValue = v => getDisplayValue((TProp)v);
			DataType = typeof(TProp);
			DisplayType = typeof(TDisp);
		}

		public Field(string fieldName, Func<TRow, TProp> getRowValue, Func<IEnumerable<TProp>, TDisp> getRowsValue, Func<TProp, TDisp> getDisplayValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
			GetDisplayValue = v => getDisplayValue((TProp)v);
			DataType = typeof(TProp);
			DisplayType = typeof(TDisp);
		}

		public Field(string fieldName, Func<TRow, TProp> getRowValue, Func<TProp, TDisp> getDisplayValue, Func<IEnumerable<TDisp>, TDisp> getRowsValue)
		{
			Name = fieldName;
			GetRowValue = row => getRowValue(row);
			GetDisplayValue = v => getDisplayValue((TProp)v);
			GetRowsValue = rows => getRowsValue(rows.Select(getRowValue).Select(getDisplayValue));
			DataType = typeof(TProp);
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
}