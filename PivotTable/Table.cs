using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PivotExpert
{
	public class Table<TRow>
	{
		public List<TableColumn> RowGroups { get; set; }
		public List<TableColumn> ColumnGroups { get; set; }
		public List<TableColumn> Columns { get; set; }

		public List<TRow> Rows { get; set; }
//		public TRow? GrandTotalRow { get; set; }

		//public void ChangeTypeToName(bool fullName = false)
		//{
		//	foreach (var c in this.Columns)
		//	{
		//		if (c.DataType is Type t)
		//			c.DataType = fullName ? (t.FullName ?? t.Name) : t.Name;
		//	}
		//	foreach (var c in this.ColumnGroups)
		//	{
		//		if (c.DataType is Type t)
		//			c.DataType = fullName ? (t.FullName ?? t.Name) : t.Name;
		//	}
		//}

	}

	public class TableColumn
	{
		public string Name { get; set; }

		[JsonIgnore]
		public Type DataType { get; set; }

		public string TypeName => DataType.Name;

		public FieldType FieldType { get; set; }
		public int GroupIndex { get; set; }

		public Sorting Sorting { get; set; }
		public int SortIndex { get; set; }

		public object?[]? GroupValues { get; set; }
	}

}
