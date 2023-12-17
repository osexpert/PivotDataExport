using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
{
	/// <summary>
	/// Field is the display part of the system.
	/// Lets imagine we have a data source with 1 million PropertyColumn. Via some api, any of these can be selected for display, and this is done via a Field.
	/// So if I send in eg. 10 fields, only data for 10 PropertyColumn's will be fetched.
	/// So the number of Fields and PropertyColumn will always be matched in number.
	/// </summary>
	public class Field
	{
		public IEqualityComparer<object?> Comparer = EqualityComparer<object?>.Default;

		// FIXME: kind of pointless...could simply used passed order
		//public int Index { get; set; }  // 0, 1, 2

		public Type DataType;

		public string FieldName { get; set; }

		public FieldType FieldType { get; set; } // Group, Data, etc.?

		public Sorting Sorting;

		/// <summary>
		/// Not sure if sort index make sense...
		/// For groups, sorting should just follow the group index...
		/// If col grouping, then there data cols are replicated, so then what field are we talking about?
		/// Sorting need to be figured out..
		/// </summary>
		public int SortIndex;

		public int GroupIndex;

		internal TableColumn ToTableColumn()
		{
			return new()
			{
				Name = FieldName,
				DataType = DataType,
				FieldType = FieldType,
				Sorting = Sorting,
				SortIndex = SortIndex,
				GroupIndex = GroupIndex
			};

		}

		internal TableColumn ToTableColumn(string combNAme, object?[] groupVals)
		{
			return new()
			{
				Name = combNAme,
				FieldType = FieldType,
				DataType = DataType,
				GroupIndex = GroupIndex,
				SortIndex = SortIndex,
				Sorting = Sorting,
				GroupValues = groupVals
			};

		}

		// FUNC to get display text?
		// Compare\equal by value or text?

		// datavalue (groupin)
		// displayvalue
		// sorting value

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


	public enum Sorting
	{
		None = 0,
		Asc = 1,
		Desc = 2
	}

}
