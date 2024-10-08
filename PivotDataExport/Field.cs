using System.ComponentModel;

namespace PivotDataTable
{
	public class Field
	{
		/// <summary>
		/// A pivot table has 3 distinct areas, Rows, Columns and Values. you can only place a measure in the Values section, you cannot place it in the ...
		/// </summary>
		public Area Area { get; set; } // Group, Data, etc.?

		public string Name { get; set; } = null!;

		public SortOrder SortOrder;

		public int GroupIndex;

		public Func<IEnumerable<object>, object?> GetValue = null!;


		// TODO: need both?
		public IEqualityComparer<object?> GroupComparer = EqualityComparer<object?>.Default;
		public IComparer<object?> SortComparer = Comparer<object?>.Default;

		public Func<object?, object?> GetDisplayValue => (o) => o;

		public Type DisplayType => DataType;

		// FIXME: kind of pointless...could simply used passed order
		//public int Index { get; set; }  // 0, 1, 2

		public Type DataType = null!;


		internal TableColumn ToTableColumn()
		{
			return new()
			{
				Name = Name,
				DataType = DataType,
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
				DataType = DataType,
				GroupIndex = GroupIndex,
				SortOrder = SortOrder,
				GroupValues = groupVals
			};

		}

		public override string ToString()
		{
			return $"Name: {Name}, Area: {Area}";
		}

		public static List<Field> CreateFieldsFromType<T>()
		{
			return CreateFieldsFromProperties(TypeDescriptor.GetProperties(typeof(T)));
		}

		public static List<Field> CreateFieldsFromProperties(IEnumerable<PropertyDescriptor> props)
		{
			return props.Select(pd => new Field { Name = pd.Name, DataType = pd.PropertyType, GetValue = pd.GetValue }).ToList();
		}

		public static List<Field> CreateFieldsFromProperties(PropertyDescriptorCollection props)
		{
			return CreateFieldsFromProperties(props.Cast<PropertyDescriptor>());
		}

		public static List<Field> CreateFieldsFromTypedList(ITypedList props)
		{
			return CreateFieldsFromProperties(props.GetItemProperties(null));
		}

		public void SetGetValue<TRow, TProp>(Func<IEnumerable<TRow>, TProp> getValue)
		{
			GetValue = rows => getValue(rows.Cast<TRow>());
		}
	}

	public class Field<TRow, TProp> : Field
	{
		public Field(string fieldName, Func<IEnumerable<TRow>, TProp> getValue)
		{
			Name = fieldName;
			GetValue = rows => getValue(rows.Cast<TRow>());
			DataType = typeof(TProp);
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

