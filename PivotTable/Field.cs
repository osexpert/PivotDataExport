using System.ComponentModel;

namespace osexpert.PivotTable
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
	}

	public class Field<TRow, TProp> : Field
	{






		//public static List<Field> CreateFieldsFromType<T>()
		//{
		//	return typeof(T).GetProperties().Select(pd => new Field { FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		//}

		//public static List<Field> CreateFieldsFromProperties(IEnumerable<PropertyDescriptor> props)
		//{
		//	return props.Select(pd => new Field { FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		//}
		public Field(string fieldName, Func<IEnumerable<TRow>, TProp> getValue)
		{
			Name = fieldName;
			GetValue = rows => getValue(rows.Cast<TRow>());
			DataType = typeof(TProp);
		}
	}

	///// <summary>
	///// Field is the display part of the system.
	///// Lets imagine we have a data source with 1 million PropertyColumn. Via some api, any of these can be selected for display, and this is done via a Field.
	///// So if I send in eg. 10 fields, only data for 10 PropertyColumn's will be fetched.
	///// So the number of Fields and PropertyColumn will always be matched in number.
	///// </summary>
	//public class Field<TRow, TData> : Field<TRow>
	//{




	//	// FUNC to get display text?
	//	// Compare\equal by value or text?

	//	// datavalue (groupin)
	//	// displayvalue
	//	// SortOrder value

	//	// TOTAL value stored here?

	//	// Should we cache the value for every row?
	//	//internal int idx;



	//	//public static List<Field> CreateFieldsFromType<T>()
	//	//{
	//	//	return typeof(T).GetProperties().Select(pd => new Field { FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
	//	//}

	//	//public static List<Field> CreateFieldsFromProperties(IEnumerable<PropertyDescriptor> props)
	//	//{
	//	//	return props.Select(pd => new Field { FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
	//	//}


	//}




	//public class Field<T> : Field
	//{
	//	public Field()
	//	{
	//		DataType = typeof(T);
	//	}
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

