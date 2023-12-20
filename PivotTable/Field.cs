using System.ComponentModel;

namespace osexpert.PivotTable
{
	/// <summary>
	/// Field is the display part of the system.
	/// Lets imagine we have a data source with 1 million PropertyColumn. Via some api, any of these can be selected for display, and this is done via a Field.
	/// So if I send in eg. 10 fields, only data for 10 PropertyColumn's will be fetched.
	/// So the number of Fields and PropertyColumn will always be matched in number.
	/// </summary>
	public class Field
	{
		// TODO: need both?
		public IEqualityComparer<object?> GroupComparer = EqualityComparer<object?>.Default;
		public IComparer<object?> SortComparer = Comparer<object?>.Default;

		public Func<object?, object?> GetDisplayValue => (o) => o;
		

		// FIXME: kind of pointless...could simply used passed order
		//public int Index { get; set; }  // 0, 1, 2

		public Type DataType = null!;

		public string FieldName { get; set; } = null!;

		public FieldType FieldType { get; set; } // Group, Data, etc.?

		public SortOrder SortOrder;

		public int GroupIndex;

		internal TableColumn ToTableColumn()
		{
			return new()
			{
				Name = FieldName,
				DataType = DataType,
				FieldType = FieldType,
				SortOrder = SortOrder,
				GroupIndex = GroupIndex
			};

		}

		internal TableColumn ToTableColumn(string combName, object?[] groupVals)
		{
			return new()
			{
				Name = combName,
				FieldType = FieldType,
				DataType = DataType,
				GroupIndex = GroupIndex,
				SortOrder = SortOrder,
				GroupValues = groupVals
			};

		}

		// FUNC to get display text?
		// Compare\equal by value or text?

		// datavalue (groupin)
		// displayvalue
		// SortOrder value

		// TOTAL value stored here?

		// Should we cache the value for every row?
		//internal int idx;



		public static List<Field> CreateFieldsFromType<T>()
		{
			return typeof(T).GetProperties().Select(pd => new Field { FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		}

		public static List<Field> CreateFieldsFromProperties(IEnumerable<PropertyDescriptor> props)
		{
			return props.Select(pd => new Field { FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		}


	}




	public class Field<T> : Field
	{
		public Field()
		{
			DataType = typeof(T);
		}
	}

	public enum FieldType
	{
		Data = 0,
		RowGroup = 1,
		ColGroup = 2,
	}


	public enum SortOrder
	{
		None = 0,
		Asc = 1,
		Desc = 2
	}

}

