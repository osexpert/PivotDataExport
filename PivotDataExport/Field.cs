using System.ComponentModel;
using System.Xml.Linq;

namespace PivotDataExport
{
	public interface IField<TRow> //where TRow : class
	{
		Area Area { get; set; }
		SortOrder SortOrder { get; set; }
		string Name { get; set; }
		int GroupIndex { get; set; }
		IEqualityComparer<object?> GroupComparer { get; set; }

		IComparer<object?> SortComparer { get; set; }

		object? GetDisplayTypeDefaultValue();
		object? GetDisplayValue(object? value);
		object? GetRowsValue(IEnumerable<TRow> rows);
		object? GetRowValue(TRow row);

		TableColumn ToTableColumn();
		TableColumn ToTableColumn(string combName, object?[] objects);
	}

	public static class Field
	{
		public static Field<TRow, TProp, TProp> Create<TRow, TProp>(string fieldName, Func<TRow, TProp> getRowValue, Func<IEnumerable<TProp>, TProp> getRowsValue)
		{
			var f = new Field<TRow, TProp, TProp /* disp */>();
			f.Name = fieldName;
			f.GetRowValue = row => getRowValue(row);
			f.GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
			f.GetDisplayValue = v => v;
			f.DataType = typeof(TProp);
			f.DisplayType = typeof(TProp);
			return f;
		}

		public static Field<TRow, TProp, TDisp> Create<TRow, TProp, TDisp>(string fieldName,
			Func<TRow, TProp> getRowValue,
			Func<IEnumerable<TProp>, TProp> getRowsValue,
			Func<TProp, TDisp> getDisplayValue)
		{
			var f = new Field<TRow, TProp, TDisp /* disp */>();
			f.Name = fieldName;
			f.GetRowValue = row => getRowValue(row);
			f.GetRowsValue = rows => getDisplayValue(getRowsValue(rows.Select(getRowValue)));
			f.GetDisplayValue = v => getDisplayValue(v);
			f.DataType = typeof(TProp);
			f.DisplayType = typeof(TDisp);
			return f;
		}

		public static Field<TRow, TProp, TDisp> Create<TRow, TProp, TDisp>(string fieldName, 
			Func<TRow, TProp> getRowValue, 
			Func<IEnumerable<TProp>, TDisp> getRowsValue, 
			Func<TProp, TDisp> getDisplayValue)
		{
			var f = new Field<TRow, TProp, TDisp /* disp */>();
			f.Name = fieldName;
			f.GetRowValue = row => getRowValue(row);
			f.GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
			f.GetDisplayValue = v => getDisplayValue(v);
			f.DataType = typeof(TProp);
			f.DisplayType = typeof(TDisp);
			return f;
		}

		public static Field<TRow, TProp, TDisp> Create<TRow, TProp, TDisp>(string fieldName,
			Func<TRow, TProp> getRowValue,
			Func<TProp, TDisp> getDisplayValue,
			Func<IEnumerable<TDisp>, TDisp> getRowsValue)
		{
			var f = new Field<TRow, TProp, TDisp /* disp */>();
			f.Name = fieldName;
			f.GetRowValue = row => getRowValue(row);
			f.GetDisplayValue = v => getDisplayValue(v);
			f.GetRowsValue = rows => getRowsValue(rows.Select(getRowValue).Select(getDisplayValue));
			f.DataType = typeof(TProp);
			f.DisplayType = typeof(TDisp);
			return f;
		}
	}

	public class Field<TRow, TProp, TDisp> : IField<TRow>
	{
		public Area Area { get; set; }

		public string Name { get; set; } = null!;

		public SortOrder SortOrder { get; set; }

		public int GroupIndex { get; set; }

		//public Func<object?, object?> GetRowValue = null!;

		/// <summary>
		/// Used to get aggregated value.
		/// The return value can be of type DisplayType, in case, GetDisplayValue does nothing.
		/// The return value can be of type DataType, in case, GetDisplayValue can convert from DataType to DisplayType.
		/// </summary>
		public Func<IEnumerable<TRow>, object?> GetRowsValue = null!;

		/// <summary>
		/// User to get the value to group on (the value will be of type DataType)
		/// </summary>
		public Func<TRow, TProp> GetRowValue = null!;

		object? IField<TRow>.GetRowsValue(IEnumerable<TRow> rows) => GetRowsValue(rows);

		object? IField<TRow>.GetRowValue(TRow row) => GetRowValue(row);

		/// <summary>
		/// In: TProp (DataType)
		/// Out: TDisp (DisplayType)
		/// </summary>
		public Func<TProp, TDisp> GetDisplayValue = null!; //o => o;

		object? IField<TRow>.GetDisplayValue(object? value) => GetDisplayValue((TProp)value);
	

		// TODO: need both GroupComparer and SortComparer?
		public IEqualityComparer<object?> GroupComparer { get; set; } = EqualityComparer<object?>.Default;

		/// <summary>
		/// TODO: sort by display or data?
		/// </summary>
		public IComparer<object?> SortComparer { get; set; } = Comparer<object?>.Default;

		//public Func<object?, object?> GetDisplayValue = (o) => o;

		/// <summary>
		/// Currently unused. But imagine a field being DateTime but the display type is DateOnly.
		/// </summary>
		public Type DisplayType = null!;

		// FIXME: kind of pointless...could simply used passed order
		//public int Index { get; set; }  // 0, 1, 2

		public Type DataType = null!;

		public DefaultValue? DisplayTypeDefaultValue;

		internal Field()
		{ }

		public object? GetDisplayTypeDefaultValue()
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

		public TableColumn ToTableColumn()
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

		public TableColumn ToTableColumn(string combName, object?[] groupVals)
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

		public static List<IField<TRow>> CreateFieldsFromType<T>()
		{
			return CreateFieldsFromProperties(TypeDescriptor.GetProperties(typeof(T)));
		}

		public static List<IField<TRow>> CreateFieldsFromProperties(IEnumerable<PropertyDescriptor> props)
		{
			return props.Select(pd => new Field<TRow, object?, object?>()
			{
				Name = pd.Name,
				DataType = pd.PropertyType,
				DisplayType = pd.PropertyType,
				GetRowValue = row => pd.GetValue(row),
				GetRowsValue = pd.GetValue,
			}).Cast<IField<TRow>>().ToList();
		}

		public static List<IField<TRow>> CreateFieldsFromProperties(PropertyDescriptorCollection props)
		{
			return CreateFieldsFromProperties(props.Cast<PropertyDescriptor>());
		}

		public static List<IField<TRow>> CreateFieldsFromTypedList(ITypedList props)
		{
			return CreateFieldsFromProperties(props.GetItemProperties(null));
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

	//public class Field<TRow, TProp> : Field<TRow, TProp, TProp>
	//{
	//	public Field(string fieldName, Func<TRow, TProp> getRowValue, Func<IEnumerable<TProp>, TProp> getRowsValue) : base(fieldName, getRowValue, getRowsValue)
	//	{
	//		//Name = fieldName;
	//		//GetRowValue = row => getRowValue(row);
	//		//GetRowsValue = rows => getRowsValue(rows.Select(getRowValue));
	//		//DataType = typeof(TProp);
	//		//DisplayType = typeof(TProp);
	//	}
	//}

	//public class Field<TRow> : Field<TRow, object?, object?>
	//{
	//}

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